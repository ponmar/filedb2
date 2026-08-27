using FileDB.Extensions;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using FileDBInterface.Validators;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace FileDB.Export.SearchResult;

public static class HtmlExportUtils
{
    /// <summary>Serializes bounding boxes to a JS array literal, e.g. [{name:`Alice`,x:0.1,y:0.2,w:0.3,h:0.4}].</summary>
    public static string BuildBboxesJs(ExportedFile file, IEnumerable<PersonModel> allPersons)
    {
        if (file.BoundingBoxes.Count == 0) return "[]";
        var items = file.BoundingBoxes.Select(b =>
        {
            var person = allPersons.FirstOrDefault(p => p.Id == b.PersonId);
            var name = EscapeJsTemplateLiteral(person?.FullName ?? string.Empty);
            string F(double v) => v.ToString("G6", CultureInfo.InvariantCulture);
            return $"{{name:`{name}`,x:{F(b.X)},y:{F(b.Y)},w:{F(b.Width)},h:{F(b.Height)}}}";
        });
        return $"[{string.Join(",", items)}]";
    }

    private static string EscapeJsTemplateLiteral(string value) =>
        value.Replace("\\", "\\\\").Replace("`", "\\`").Replace("${", "\\${");


    public static string BuildMetaHtml(RichExportData data, ExportedFile file, string? locationLink)
    {
        var pictureDateText = string.Empty;
        if (file.Datetime is not null)
        {
            pictureDateText = HtmlExporter.CreateExportedFileDatetime(file.Datetime) ?? string.Empty;
        }

        var pictureDescription = string.Empty;
        if (file.Description is not null)
        {
            if (pictureDateText != string.Empty)
                pictureDescription += ": ";
            var htmlDescription = HttpUtility.HtmlEncode(file.Description);
            htmlDescription = htmlDescription.Replace(FileModelValidator.DescriptionLineEnding, "<br>");
            pictureDescription += htmlDescription;
        }

        var text = string.Empty;
        if (pictureDateText.Length > 0 || pictureDescription.Length > 0)
            text = $"<p>{pictureDateText}{pictureDescription}</p>";

        if (file.PersonIds.Count > 0)
        {
            var fileDateTime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
            var persons = data.Persons.Where(x => file.PersonIds.Contains(x.Id));
            var personParts = persons
                .Select(p => (
                    Display: FileTextOverlayCreator.GetPersonText(p, file.Datetime),
                    Tooltip: FileTextOverlayCreator.GetPersonDetailsText(p, fileDateTime)))
                .OrderBy(x => x.Display)
                .Select(x => $"<span title=\"{HttpUtility.HtmlEncode(x.Tooltip)}\">{HttpUtility.HtmlEncode(x.Display)}</span>");
            text += $"<p>&#128578; {string.Join(", ", personParts)}</p>";
        }

        if (file.LocationIds.Count > 0)
        {
            var locations = data.Locations.Where(x => file.LocationIds.Contains(x.Id)).OrderBy(x => x.Name);
            var locationParts = locations.Select(l =>
            {
                var link = Utils.CreatePositionLink(l.Position, locationLink);
                return link is not null
                    ? $"<a href=\"{link}\">{HttpUtility.HtmlEncode(l.Name)}</a>"
                    : HttpUtility.HtmlEncode(l.Name);
            });
            text += $"<p>&#127968; {string.Join(", ", locationParts)}</p>";
        }

        if (file.TagIds.Count > 0)
        {
            var tags = data.Tags.Where(x => file.TagIds.Contains(x.Id)).OrderBy(x => x.Name);
            var tagsStr = string.Join(", ", tags.Select(t => HttpUtility.HtmlEncode(t.Name)));
            text += $"<p>&#128278; {tagsStr}</p>";
        }

        var filePositionLink = Utils.CreatePositionLink(file.Position, locationLink);
        if (filePositionLink is not null)
        {
            text += $"<p>&#x1F6F0; <a href=\"{filePositionLink}\">{HttpUtility.HtmlEncode(file.Position)}</a></p>";
        }

        return text;
    }

    public static string BuildViewerHtml(RichExportData data, string slidesJs)
    {
        var header = $"<!-- {HttpUtility.HtmlEncode(Utils.ApplicationName)} {HttpUtility.HtmlEncode(data.FileDBVersion)} {HttpUtility.HtmlEncode(data.ExportDateTime.ToDateAndTime())} {HttpUtility.HtmlEncode(data.ApplicationProjectUrl)} -->";
        var title = HttpUtility.HtmlEncode(data.Name);
        return $$"""
{{header}}
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>{{title}}</title>
<style>
:root { --overlay-scale: 1; }
* { box-sizing: border-box; margin: 0; padding: 0; }
body { background: #000; color: #fff; font-family: sans-serif; overflow: hidden; }
#viewer { position: relative; width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; }
#slide-img { max-width: 100%; max-height: 100%; object-fit: contain; display: block; }
#slide-card { color: #ccc; font-size: 1.2em; text-align: center; padding: 2em; display: none; }
#meta { position: absolute; top: 0.5em; left: 0.8em; background: rgba(0,0,0,0.5); padding: 2px 8px; border-radius: 4px; pointer-events: auto; font-size: calc(0.9em * var(--overlay-scale)); }
#meta a { color: #adf; }
#counter { position: absolute; top: 0.5em; right: 0.8em; background: rgba(0,0,0,0.5); padding: 2px 8px; border-radius: 4px; font-size: calc(0.9em * var(--overlay-scale)); }
#scale-indicator { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: rgba(0,0,0,0.5); color: #fff; padding: 2px 8px; border-radius: 4px; font-size: calc(0.9em * var(--overlay-scale)); pointer-events: none; display: none; z-index: 1000; }
#controls { position: fixed; bottom: 0.8em; left: 50%; transform: translateX(-50%); background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; gap: 0.5em; padding: 0.35em; border-radius: 4px; opacity: 0; transition: opacity 0.3s; z-index: 500; font-size: calc(1em * var(--overlay-scale)); }
#controls.visible { opacity: 1; }
#controls.pinned { opacity: 1; }
#controls button { background: rgba(255,255,255,0.1); border: 1px solid rgba(255,255,255,0.3); color: #fff; border-radius: 4px; cursor: pointer; font-size: 1em; width: 2.2em; height: 2.2em; padding: 0; display: inline-flex; align-items: center; justify-content: center; }
#controls .group-sep { margin-left: 0.8em; }
#controls button:hover { background: rgba(255,255,255,0.25); }
#controls button.active { background: rgba(80,160,255,0.45); border-color: #5af; }
#speed-select { background: #222; border: 1px solid rgba(255,255,255,0.3); color: #fff; border-radius: 4px; font-size: 1em; cursor: pointer; height: 2.2em; padding: 0 0.4em; }
#speed-select option { background: #222; color: #fff; }
#bbox-overlay { position: absolute; inset: 0; pointer-events: none; }
.bbox { position: absolute; box-sizing: border-box; border: 2px solid transparent; }
.bbox.hovered { border-color: #00c040; filter: drop-shadow(0 0 4px rgba(0,0,0,0.8)); }
.bbox-label { display: none; position: absolute; top: calc(100% + 3px); left: 50%; transform: translateX(-50%); white-space: nowrap; color: #fff; font-size: 0.75em; background: rgba(0,0,0,0.7); padding: 1px 5px; border-radius: 3px; pointer-events: none; text-shadow: 0 1px 2px #000; }
.bbox.hovered .bbox-label { display: block; }
</style>
</head>
<body>
<div id="viewer">
  <img id="slide-img" src="" alt=""/>
  <div id="bbox-overlay"></div>
  <div id="slide-card"></div>
  <div id="meta"></div>
  <div id="counter"></div>
  <div id="scale-indicator"></div>
</div>
<div id="controls">
  <button id="btn-prev" title="Previous (PageUp, ArrowLeft)">&#x2B60;</button>
  <button id="btn-next" title="Next (PageDown, ArrowRight)">&#x2B62;</button>
  <button id="btn-prev-dir" class="group-sep" title="Previous directory (Ctrl+PageUp)">&#x21C7;</button>
  <button id="btn-next-dir" title="Next directory (Ctrl+PageDown)">&#x21C9;</button>
  <button id="btn-first" class="group-sep" title="First (Home)">&#x21E4;</button>
  <button id="btn-last" title="Last (End)">&#x21E5;</button>
  <button id="btn-play" class="group-sep" title="Play / Pause (Space)">&#9654;</button>
  <select id="speed-select" title="Slideshow speed">
    <option value="2000">2 s</option>
    <option value="5000" selected>5 s</option>
    <option value="10000">10 s</option>
    <option value="30000">30 s</option>
  </select>
  <button id="btn-random" title="Random (R)">&#128256;</button>
  <button id="btn-repeat" title="Repeat (T)">&#128257;</button>
  <button id="btn-font-dec" class="group-sep" title="Decrease font size (ArrowDown)">A&#8722;</button>
  <button id="btn-font-inc" title="Increase font size (ArrowUp)">A+</button>
</div>
<script>
{{slidesJs}}
let current = 0;
let overlayScale = parseFloat(sessionStorage.getItem('overlayScale') || '1');
let slideshowTimer = null;
let isPlaying = false;
let isRandom = sessionStorage.getItem('random') === '1';
let isRepeat = sessionStorage.getItem('repeat') === '1';

const controls = document.getElementById('controls');
const btnPlay = document.getElementById('btn-play');
const btnRandom = document.getElementById('btn-random');
const btnRepeat = document.getElementById('btn-repeat');
const speedSelect = document.getElementById('speed-select');

// Restore persisted speed
const savedSpeed = sessionStorage.getItem('speed');
if (savedSpeed) {
  const opt = speedSelect.querySelector(`option[value="${savedSpeed}"]`);
  if (opt) opt.selected = true;
}

function updateToggleButtons() {
  btnRandom.classList.toggle('active', isRandom);
  btnRepeat.classList.toggle('active', isRepeat);
}
updateToggleButtons();

function showSlide(n) {
  if (slides.length === 0) return;
  current = ((n % slides.length) + slides.length) % slides.length;
  const s = slides[current];
  const img = document.getElementById('slide-img');
  const card = document.getElementById('slide-card');
  if (s.isPicture) {
    img.src = s.src; img.alt = s.alt; img.style.display = 'block'; card.style.display = 'none';
  } else {
    img.style.display = 'none'; card.textContent = s.alt; card.style.display = 'block';
  }
  document.getElementById('meta').innerHTML = s.meta;
  document.getElementById('counter').textContent = (current + 1) + ' / ' + slides.length;
  createBboxOverlays();
}

function createBboxOverlays() {
  const overlay = document.getElementById('bbox-overlay');
  overlay.innerHTML = '';
  const s = slides[current];
  if (!s.isPicture || !s.bboxes || s.bboxes.length === 0) return;
  const img = document.getElementById('slide-img');
  // Wait for image to be sized (may need natural size after load)
  function build() {
    const imgRect = img.getBoundingClientRect();
    const viewerRect = document.getElementById('viewer').getBoundingClientRect();
    if (imgRect.width === 0 || imgRect.height === 0) return;
    overlay.innerHTML = '';
    for (const bbox of s.bboxes) {
      const div = document.createElement('div');
      div.className = 'bbox';
      div.style.left = (imgRect.left - viewerRect.left + bbox.x * imgRect.width) + 'px';
      div.style.top = (imgRect.top - viewerRect.top + bbox.y * imgRect.height) + 'px';
      div.style.width = (bbox.w * imgRect.width) + 'px';
      div.style.height = (bbox.h * imgRect.height) + 'px';
      const label = document.createElement('span');
      label.className = 'bbox-label';
      label.textContent = bbox.name;
      div.appendChild(label);
      overlay.appendChild(div);
    }
  }
  if (img.complete && img.naturalWidth > 0) { build(); }
  else { img.addEventListener('load', build, { once: true }); }
}

window.addEventListener('resize', () => { if (slides[current]?.isPicture) createBboxOverlays(); });

document.getElementById('viewer').addEventListener('mousemove', e => {
  const bboxDivs = document.querySelectorAll('.bbox');
  const viewerRect = document.getElementById('viewer').getBoundingClientRect();
  const mx = e.clientX - viewerRect.left;
  const my = e.clientY - viewerRect.top;
  bboxDivs.forEach(div => {
    const x = parseFloat(div.style.left);
    const y = parseFloat(div.style.top);
    const w = parseFloat(div.style.width);
    const h = parseFloat(div.style.height);
    div.classList.toggle('hovered', mx >= x && mx <= x + w && my >= y && my <= y + h);
  });
});

document.getElementById('viewer').addEventListener('mouseleave', () => {
  document.querySelectorAll('.bbox.hovered').forEach(div => div.classList.remove('hovered'));
});

function nextSlide() {
  if (isRandom) {
    let next;
    do { next = Math.floor(Math.random() * slides.length); } while (slides.length > 1 && next === current);
    showSlide(next);
  } else {
    const isLast = current === slides.length - 1;
    if (isLast && !isRepeat) { stopSlideshow(); return; }
    showSlide(current + 1);
  }
}

function move(delta) {
  stopSlideshow();
  const n = current + delta;
  if (n < 0 || n >= slides.length) return;
  showSlide(n);
}

function prevDirectory() {
  stopSlideshow();
  const currentDir = slides[current].dir;
  for (let i = current - 1; i >= 0; i--) {
    if (slides[i].dir !== currentDir) { showSlide(i); return; }
  }
}

function nextDirectory() {
  stopSlideshow();
  const currentDir = slides[current].dir;
  for (let i = current + 1; i < slides.length; i++) {
    if (slides[i].dir !== currentDir) { showSlide(i); return; }
  }
}

function startSlideshow() {
  if (slides.length === 0) return;
  isPlaying = true;
  btnPlay.innerHTML = '&#9646;&#9646;';
  btnPlay.title = 'Play / Pause (Space)';
  const delay = parseInt(speedSelect.value, 10);
  slideshowTimer = setInterval(nextSlide, delay);
  showControls(true);
}

function stopSlideshow() {
  if (!isPlaying) return;
  isPlaying = false;
  btnPlay.innerHTML = '&#9654;';
  btnPlay.title = 'Play / Pause (Space)';
  clearInterval(slideshowTimer);
  slideshowTimer = null;
}

function togglePlay() {
  if (isPlaying) stopSlideshow(); else startSlideshow();
}

btnPlay.addEventListener('click', togglePlay);
document.getElementById('btn-first').addEventListener('click', () => { stopSlideshow(); showSlide(0); });
document.getElementById('btn-prev').addEventListener('click', () => move(-1));
document.getElementById('btn-next').addEventListener('click', () => move(1));
document.getElementById('btn-prev-dir').addEventListener('click', prevDirectory);
document.getElementById('btn-next-dir').addEventListener('click', nextDirectory);
document.getElementById('btn-last').addEventListener('click', () => { stopSlideshow(); showSlide(slides.length - 1); });

function toggleRandom() {
  isRandom = !isRandom;
  sessionStorage.setItem('random', isRandom ? '1' : '0');
  updateToggleButtons();
}

function toggleRepeat() {
  isRepeat = !isRepeat;
  sessionStorage.setItem('repeat', isRepeat ? '1' : '0');
  updateToggleButtons();
}

btnRandom.addEventListener('click', toggleRandom);
btnRepeat.addEventListener('click', toggleRepeat);

speedSelect.addEventListener('change', () => {
  sessionStorage.setItem('speed', speedSelect.value);
  if (isPlaying) { stopSlideshow(); startSlideshow(); }
});

document.getElementById('btn-font-dec').addEventListener('click', () => adjustOverlayScale(-0.1));
document.getElementById('btn-font-inc').addEventListener('click', () => adjustOverlayScale(0.1));

// Show controls on hover near bottom or during slideshow
let hideTimer = null;
let hasPointerOrTouch = false;
let controlsPinned = false;
function setControlsPinned(pinned) {
  controlsPinned = pinned;
  controls.classList.toggle('pinned', pinned);
  if (pinned) {
    clearTimeout(hideTimer);
    controls.classList.add('visible');
  }
}
function markPointerOrTouchInput() {
  hasPointerOrTouch = true;
  if (controlsPinned) setControlsPinned(false);
}
function scheduleHideControls(delay) {
  clearTimeout(hideTimer);
  if (controlsPinned) return;
  hideTimer = setTimeout(() => controls.classList.remove('visible'), delay);
}
function showControls(temporary) {
  controls.classList.add('visible');
  if (temporary && !controlsPinned) {
    scheduleHideControls(2500);
  }
}
document.addEventListener('mousemove', e => {
  markPointerOrTouchInput();
  if (e.clientY > window.innerHeight - 80) showControls(true);
  else scheduleHideControls(800);
});
controls.addEventListener('mouseenter', () => { clearTimeout(hideTimer); controls.classList.add('visible'); });
controls.addEventListener('mouseleave', () => scheduleHideControls(800));

function adjustOverlayScale(delta) {
  overlayScale = Math.max(0.5, Math.min(2.0, overlayScale + delta));
  document.documentElement.style.setProperty('--overlay-scale', overlayScale.toString());
  sessionStorage.setItem('overlayScale', overlayScale.toString());
  showScaleIndicator(Math.round(overlayScale * 100) + '%');
  showControls(true);
}

let scaleIndicatorTimer = null;
function showScaleIndicator(text) {
  const indicator = document.getElementById('scale-indicator');
  indicator.textContent = text;
  indicator.style.display = 'block';
  clearTimeout(scaleIndicatorTimer);
  scaleIndicatorTimer = setTimeout(() => { indicator.style.display = 'none'; }, 2500);
}

document.documentElement.style.setProperty('--overlay-scale', overlayScale.toString());

document.addEventListener('keydown', e => {
  if (!hasPointerOrTouch && !controlsPinned) setControlsPinned(true);
  if (e.key === ' ' || e.key === 'Enter' || e.key === 'NumpadEnter' || e.key === 'MediaPlayPause') { togglePlay(); e.preventDefault(); }
  else if (e.key === 'ArrowUp') { adjustOverlayScale(0.1); e.preventDefault(); }
  else if (e.key === 'ArrowDown') { adjustOverlayScale(-0.1); e.preventDefault(); }
  else if (e.key === 'ArrowRight' || (e.key === 'PageDown' && !e.ctrlKey) || e.key === 'MediaTrackNext') move(1);
  else if (e.key === 'ArrowLeft' || (e.key === 'PageUp' && !e.ctrlKey) || e.key === 'MediaTrackPrevious') move(-1);
  else if (e.key === 'PageDown' && e.ctrlKey) { nextDirectory(); e.preventDefault(); }
  else if (e.key === 'PageUp' && e.ctrlKey) { prevDirectory(); e.preventDefault(); }
  else if (e.key === 'Home') { stopSlideshow(); showSlide(0); }
  else if (e.key === 'End') { stopSlideshow(); showSlide(slides.length - 1); }
  else if (e.key === 'r' || e.key === 'R') toggleRandom();
  else if (e.key === 't' || e.key === 'T') toggleRepeat();
});

let tx = 0, ty = 0, singleTouch = false;
document.addEventListener('touchstart', e => {
  markPointerOrTouchInput();
  if (e.touches.length === 1) { tx = e.touches[0].clientX; ty = e.touches[0].clientY; singleTouch = true; }
  else { singleTouch = false; }
}, { passive: true });
document.addEventListener('touchend', e => {
  if (!singleTouch) return;
  const dx = e.changedTouches[0].clientX - tx;
  const dy = e.changedTouches[0].clientY - ty;
  if (Math.abs(dy) > 40 && Math.abs(dy) > Math.abs(dx)) { adjustOverlayScale(dy < 0 ? 0.1 : -0.1); }
  else if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy)) { move(dx < 0 ? 1 : -1); }
  else if (Math.abs(dx) < 10 && Math.abs(dy) < 10) { showControls(true); }
});

showSlide(0);
</script>
</body>
</html>
""";
    }
}
