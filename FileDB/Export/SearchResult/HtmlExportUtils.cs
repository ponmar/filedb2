using FileDB.Extensions;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using FileDBInterface.Validators;
using System.Linq;
using System.Web;

namespace FileDB.Export.SearchResult;

public static class HtmlExportUtils
{
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
            var tags = data.Tags.Where(x => file.TagIds.Contains(x.Id));
            var tagsStr = FileTextOverlayCreator.GetTagsText(tags, ", ");
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
#scale-indicator { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: rgba(0,0,0,0.8); color: #fff; padding: 1em 2em; border-radius: 8px; font-size: 2em; pointer-events: none; display: none; z-index: 1000; }
#controls { position: fixed; bottom: 0; left: 0; right: 0; background: rgba(0,0,0,0.75); display: flex; align-items: center; justify-content: center; gap: 0.5em; padding: 0.4em 0.8em; opacity: 0; transition: opacity 0.3s; z-index: 500; }
#controls.visible { opacity: 1; }
#controls button { background: rgba(255,255,255,0.1); border: 1px solid rgba(255,255,255,0.3); color: #fff; padding: 0.25em 0.6em; border-radius: 4px; cursor: pointer; font-size: 1em; }
#controls button:hover { background: rgba(255,255,255,0.25); }
#controls button.active { background: rgba(80,160,255,0.45); border-color: #5af; }
#speed-select { background: rgba(255,255,255,0.1); border: 1px solid rgba(255,255,255,0.3); color: #fff; padding: 0.25em 0.4em; border-radius: 4px; font-size: 1em; cursor: pointer; }
#speed-select option { background: #222; }
</style>
</head>
<body>
<div id="viewer">
  <img id="slide-img" src="" alt=""/>
  <div id="slide-card"></div>
  <div id="meta"></div>
  <div id="counter"></div>
  <div id="scale-indicator"></div>
</div>
<div id="controls">
  <button id="btn-first" title="First">&#x21E4;</button>
  <button id="btn-prev" title="Previous">&#x2B60;</button>
  <button id="btn-play" title="Play / Pause">&#9654;</button>
  <button id="btn-next" title="Next">&#x2B62;</button>
  <button id="btn-last" title="Last">&#x21E5;</button>
  <button id="btn-random" title="Random">&#128256;</button>
  <button id="btn-repeat" title="Repeat">&#128257;</button>
  <select id="speed-select" title="Slideshow speed">
    <option value="2000">2 s</option>
    <option value="5000" selected>5 s</option>
    <option value="10000">10 s</option>
    <option value="30000">30 s</option>
  </select>
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
}

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

function move(delta) { stopSlideshow(); showSlide(current + delta); }

function startSlideshow() {
  if (slides.length === 0) return;
  isPlaying = true;
  btnPlay.innerHTML = '&#9646;&#9646;';
  btnPlay.title = 'Pause';
  const delay = parseInt(speedSelect.value, 10);
  slideshowTimer = setInterval(nextSlide, delay);
}

function stopSlideshow() {
  if (!isPlaying) return;
  isPlaying = false;
  btnPlay.innerHTML = '&#9654;';
  btnPlay.title = 'Play';
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
document.getElementById('btn-last').addEventListener('click', () => { stopSlideshow(); showSlide(slides.length - 1); });

btnRandom.addEventListener('click', () => {
  isRandom = !isRandom;
  sessionStorage.setItem('random', isRandom ? '1' : '0');
  updateToggleButtons();
});

btnRepeat.addEventListener('click', () => {
  isRepeat = !isRepeat;
  sessionStorage.setItem('repeat', isRepeat ? '1' : '0');
  updateToggleButtons();
});

speedSelect.addEventListener('change', () => {
  sessionStorage.setItem('speed', speedSelect.value);
  if (isPlaying) { stopSlideshow(); startSlideshow(); }
});

// Show controls on hover near bottom or during slideshow
let hideTimer = null;
function showControls(temporary) {
  controls.classList.add('visible');
  if (temporary) {
    clearTimeout(hideTimer);
    hideTimer = setTimeout(() => { if (!isPlaying) controls.classList.remove('visible'); }, 2500);
  }
}
document.addEventListener('mousemove', e => {
  if (e.clientY > window.innerHeight - 80) showControls(true);
  else if (!isPlaying) controls.classList.remove('visible');
});
controls.addEventListener('mouseenter', () => { clearTimeout(hideTimer); controls.classList.add('visible'); });
controls.addEventListener('mouseleave', () => {
  if (!isPlaying) hideTimer = setTimeout(() => controls.classList.remove('visible'), 800);
});

function adjustOverlayScale(delta) {
  overlayScale = Math.max(0.5, Math.min(2.0, overlayScale + delta));
  document.documentElement.style.setProperty('--overlay-scale', overlayScale.toString());
  sessionStorage.setItem('overlayScale', overlayScale.toString());
  showScaleIndicator(Math.round(overlayScale * 100) + '%');
}

function showScaleIndicator(text) {
  const indicator = document.getElementById('scale-indicator');
  indicator.textContent = text;
  indicator.style.display = 'block';
  setTimeout(() => { indicator.style.display = 'none'; }, 1000);
}

document.documentElement.style.setProperty('--overlay-scale', overlayScale.toString());

document.addEventListener('keydown', e => {
  if (e.key === ' ') { togglePlay(); e.preventDefault(); }
  else if (e.key === 'ArrowUp') { adjustOverlayScale(0.1); e.preventDefault(); }
  else if (e.key === 'ArrowDown') { adjustOverlayScale(-0.1); e.preventDefault(); }
  else if (e.key === 'ArrowRight' || e.key === 'PageDown') move(1);
  else if (e.key === 'ArrowLeft' || e.key === 'PageUp') move(-1);
  else if (e.key === 'Home') { stopSlideshow(); showSlide(0); }
  else if (e.key === 'End') { stopSlideshow(); showSlide(slides.length - 1); }
});

let tx = 0, ty = 0, singleTouch = false;
document.addEventListener('touchstart', e => {
  if (e.touches.length === 1) { tx = e.touches[0].clientX; ty = e.touches[0].clientY; singleTouch = true; }
  else { singleTouch = false; }
}, { passive: true });
document.addEventListener('touchend', e => {
  if (!singleTouch) return;
  const dx = e.changedTouches[0].clientX - tx;
  const dy = e.changedTouches[0].clientY - ty;
  if (Math.abs(dy) > 40 && Math.abs(dy) > Math.abs(dx)) { adjustOverlayScale(dy < 0 ? 0.1 : -0.1); }
  else if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy)) { move(dx < 0 ? 1 : -1); }
});

showSlide(0);
</script>
</body>
</html>
""";
    }
}
