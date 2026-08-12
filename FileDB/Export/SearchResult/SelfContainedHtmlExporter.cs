using FileDB.Extensions;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Web;

namespace FileDB.Export.SearchResult;

public interface ISelfContainedHtmlExporter
{
    void Export(RichExportData data, string destinationFilePath);
}

public class SelfContainedHtmlExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider, IConfigProvider configProvider) : ISelfContainedHtmlExporter
{
    public void Export(RichExportData data, string destinationFilePath)
    {
        var existingFiles = data.Files
            .Where(x => fileSystem.File.Exists(filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(x.OriginalPath)))
            .ToList();

        // Build JS slides array with embedded base64 images
        var slidesJs = new StringBuilder();
        slidesJs.AppendLine("const slides = [");
        for (int i = 0; i < existingFiles.Count; i++)
        {
            var file = existingFiles[i];
            var metaHtml = HtmlExportUtils.BuildMetaHtml(data, file, configProvider.Config.LocationLink);
            var isPicture = file.FileType == FileType.Picture;
            string srcValue;
            if (isPicture)
            {
                var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                var imageBytes = fileSystem.File.ReadAllBytes(sourceFilePath);
                var base64 = Convert.ToBase64String(imageBytes);
                var mime = GetImageMimeType(file.OriginalPath);
                srcValue = $"data:{mime};base64,{base64}";
            }
            else
            {
                srcValue = string.Empty;
            }
            var alt = Path.GetFileName(file.OriginalPath);
            slidesJs.AppendLine($"  {{ src: {ToJsString(srcValue)}, alt: {ToJsString(alt)}, meta: {ToJsString(metaHtml)}, isPicture: {(isPicture ? "true" : "false")} }}{(i < existingFiles.Count - 1 ? "," : "")}");
        }
        slidesJs.AppendLine("];");

        var html = BuildHtmlDocument(data, slidesJs.ToString());
        fileSystem.File.WriteAllText(destinationFilePath, html);
    }

    private static string ToJsString(string value) =>
        "`" + value.Replace("\\", "\\\\").Replace("`", "\\`").Replace("${", "\\${") + "`";

    private static string BuildHtmlDocument(RichExportData data, string slidesJs) => $$"""
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<title>{{HttpUtility.HtmlEncode(data.Name)}}</title>
<style>
* { box-sizing: border-box; margin: 0; padding: 0; }
body { background: #000; color: #fff; font-family: sans-serif; overflow: hidden; }
#viewer { position: relative; width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; }
#slide-img { max-width: 100%; max-height: 100%; object-fit: contain; display: block; }
#slide-card { color: #ccc; font-size: 1.2em; text-align: center; padding: 2em; display: none; }
#meta { position: absolute; top: 0.5em; left: 0.8em; background: rgba(0,0,0,0.5); padding: 2px 8px; border-radius: 4px; pointer-events: auto; }
#meta a { color: #adf; }
#counter { position: absolute; top: 0.5em; right: 0.8em; background: rgba(0,0,0,0.5); padding: 2px 8px; border-radius: 4px; font-size: 0.9em; }
.nav { position: absolute; top: 50%; transform: translateY(-50%); background: rgba(0,0,0,0.35); border: none; color: #fff; font-size: 2.5em; cursor: pointer; padding: 0.1em 0.3em; border-radius: 4px; user-select: none; }
.nav:hover { background: rgba(0,0,0,0.7); }
#btn-prev { left: 0.3em; }
#btn-next { right: 0.3em; }
#footer { position: fixed; bottom: 0; right: 0; font-size: 0.7em; color: #555; padding: 2px 6px; pointer-events: none; }
</style>
</head>
<body>
<div id="viewer">
  <img id="slide-img" src="" alt=""/>
  <div id="slide-card"></div>
  <div id="meta"></div>
  <div id="counter"></div>
  <button class="nav" id="btn-prev" onclick="move(-1)">&#8249;</button>
  <button class="nav" id="btn-next" onclick="move(1)">&#8250;</button>
</div>
<p id="footer">{{HttpUtility.HtmlEncode(Utils.ApplicationName)}} {{HttpUtility.HtmlEncode(data.FileDBVersion)}} {{HttpUtility.HtmlEncode(data.ExportDateTime.ToDateAndTime())}} <a href="{{data.ApplicationProjectUrl}}">{{data.ApplicationProjectUrl}}</a></p>
<script>
{{slidesJs}}
let current = 0;
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
function move(delta) { showSlide(current + delta); }
document.addEventListener('keydown', e => {
  if (e.key === 'ArrowRight' || e.key === 'PageDown') move(1);
  else if (e.key === 'ArrowLeft' || e.key === 'PageUp') move(-1);
  else if (e.key === 'Home') showSlide(0);
  else if (e.key === 'End') showSlide(slides.length - 1);
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
  if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy)) move(dx < 0 ? 1 : -1);
});
showSlide(0);
</script>
</body>
</html>
""";

    private static string GetImageMimeType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream",
        };
}

