using System.IO;

namespace FileDB.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        public string DownloadLink => "https://drive.google.com/drive/folders/1GyZpdDcMdUOlvvtwtKUuylazoy7XaIcm?usp=sharing";

        public string Changes => File.ReadAllText("CHANGES.txt");

        public AboutViewModel()
        {
        }
    }
}
