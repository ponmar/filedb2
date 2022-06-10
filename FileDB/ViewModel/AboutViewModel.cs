using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FileDB.ViewModel
{
    public record LicenseFileFormatDto(string PackageName, string PackageVersion, string PackageUrl, string LicenseType);

    public class AboutViewModel
    {
        public string DownloadLink => "https://drive.google.com/drive/folders/1GyZpdDcMdUOlvvtwtKUuylazoy7XaIcm?usp=sharing";

        public string Changes => File.Exists("CHANGES.txt") ? File.ReadAllText("CHANGES.txt") : "Not deployed";

        public string Heading => $"About {Utils.ApplicationName} version {ReleaseInformation.VersionString}";

        public ObservableCollection<LicenseFileFormatDto> Licenses { get; } = new();

        public AboutViewModel()
        {
            var licensesJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", "licenses.json"));
            var licenses = JsonConvert.DeserializeObject<List<LicenseFileFormatDto>>(licensesJson);
            licenses.ForEach(x => Licenses.Add(x));
        }
    }
}
