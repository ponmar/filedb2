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
        private const string ChangesFilePath = "CHANGES.txt";
        private readonly string LicensesJsonFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "licenses.json");

        public string DownloadLink => Utils.ApplicationDownloadUrl;

        public string Changes => File.Exists(ChangesFilePath) ? File.ReadAllText(ChangesFilePath) : "Not deployed";

        public string Heading => $"About {Utils.ApplicationName} version {Utils.GetVersionString()}";

        public ObservableCollection<LicenseFileFormatDto> Licenses { get; } = new();

        public AboutViewModel()
        {
            var licensesJson = File.ReadAllText(LicensesJsonFilePath);
            var licenses = JsonConvert.DeserializeObject<List<LicenseFileFormatDto>>(licensesJson);
            licenses.ForEach(x => Licenses.Add(x));
        }
    }
}
