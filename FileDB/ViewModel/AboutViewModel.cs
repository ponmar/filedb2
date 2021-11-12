using System.IO;

namespace FileDB.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        public string DownloadLink => "https://drive.google.com/drive/folders/1GyZpdDcMdUOlvvtwtKUuylazoy7XaIcm?usp=sharing";

        // TODO: update path when changes included in release
        public string Changes => File.ReadAllText(@"C:\repos\filedb2\docs\CHANGES.txt");

        public AboutViewModel()
        {
        }
    }
}
