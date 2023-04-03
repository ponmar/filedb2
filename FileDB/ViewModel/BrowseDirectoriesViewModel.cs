using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.ViewModel
{
    public interface IFolder
    {
        string Name { get; }
        List<IFolder> Folders { get; }
        string Path { get; }
    }

    public class Folder : IFolder
    {
        private readonly IFolder? parent;
        public string Name { get; }
        public List<IFolder> Folders { get; } = new();

        public string Path => parent != null ? parent.Path + "/" + Name : Name;

        public Folder(string name, IFolder? parent = null)
        {
            Name = name;
            this.parent = parent;
        }
    }

    public partial class BrowseDirectoriesViewModel : ObservableObject
    {
        private const string RootFolderName = "root";

        [ObservableProperty]
        private List<IFolder> folders = new();

        [ObservableProperty]
        private IFolder? selectedFolder;

        public string? SelectedDirectoryPath { get; private set; }

        private readonly IDbAccessRepository dbAccessRepository;

        public BrowseDirectoriesViewModel(IDbAccessRepository dbAccessRepository)
        {
            this.dbAccessRepository = dbAccessRepository;
            ReloadFolders();
        }

        private void ReloadFolders()
        {
            var root = new Folder(RootFolderName);

            Folders.Clear();
            Folders.Add(root);

            foreach (var file in dbAccessRepository.DbAccess.GetFiles())
            {
                var directoryEndIndex = file.Path.LastIndexOf("/");
                if (directoryEndIndex == -1)
                {
                    // This fils is in the root directory
                    continue;
                }

                var directoryPath = file.Path.Substring(0, directoryEndIndex);
                var directories = directoryPath.Split("/");

                if (directories.Length > 0)
                {
                    var currentFolder = root;

                    foreach (var pathPart in directories)
                    {
                        var subFolder = currentFolder.Folders.FirstOrDefault(x => x.Name == pathPart);
                        if (subFolder == null)
                        {
                            subFolder = new Folder(pathPart, currentFolder);
                            currentFolder.Folders.Add(subFolder);
                        }

                        currentFolder = (Folder)subFolder;
                    }
                }
            }

            OnPropertyChanged(nameof(Folders));
        }

        [RelayCommand]
        private void SelectFolder()
        {
            Events.Send<CloseModalDialogRequest>();

            if (SelectedFolder != null)
            {
                var path = SelectedFolder.Path;
                if (path.StartsWith(RootFolderName))
                {
                    path = path.Substring(RootFolderName.Length);
                }
                if (path.StartsWith("/"))
                {
                    path = path.Substring("/".Length);
                }
                SelectedDirectoryPath = path;
            }
        }
    }
}
