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
}
