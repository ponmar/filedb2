using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDBAvalonia.ViewModels.Dialogs
{
    public class Directory
    {
        private readonly Directory? parent;
        public string Name { get; }
        public ObservableCollection<Directory> Directories { get; } = [];

        public string Path => parent is not null ? parent.Path + "/" + Name : Name;

        public Directory(string name, Directory? parent = null)
        {
            Name = name;
            this.parent = parent;
        }
    }

    public partial class BrowseSubDirectoriesViewModel : ObservableObject
    {
        private const string RootDirectoryName = "root";

        [ObservableProperty]
        private List<Directory> directories = [];

        [ObservableProperty]
        private Directory? selectedDirectory;

        public string? SelectedDirectoryPath { get; private set; }

        private readonly IDatabaseAccessProvider dbAccessProvider;

        public BrowseSubDirectoriesViewModel(IDatabaseAccessProvider dbAccessProvider)
        {
            this.dbAccessProvider = dbAccessProvider;
            ReloadDirectoryTree();
        }

        private void ReloadDirectoryTree()
        {
            var root = new Directory(RootDirectoryName);

            Directories.Clear();
            Directories.Add(root);

            foreach (var directoryPath in dbAccessProvider.DbAccess.GetDirectories())
            {
                var pathParts = directoryPath.Split("/");

                if (pathParts.Length > 0)
                {
                    var currentDirectory = root;

                    foreach (var pathPart in pathParts)
                    {
                        var subDirectory = currentDirectory.Directories.FirstOrDefault(x => x.Name == pathPart);
                        if (subDirectory is null)
                        {
                            subDirectory = new Directory(pathPart, currentDirectory);
                            currentDirectory.Directories.Add(subDirectory);
                        }

                        currentDirectory = subDirectory;
                    }
                }
            }
        }

        [RelayCommand]
        private void SelectDirectory()
        {
            if (SelectedDirectory is not null)
            {
                var path = SelectedDirectory.Path;
                if (path.StartsWith(RootDirectoryName))
                {
                    path = path.Substring(RootDirectoryName.Length);
                }
                if (path.StartsWith("/"))
                {
                    path = path.Substring("/".Length);
                }
                SelectedDirectoryPath = path;

                Messenger.Send<CloseModalDialogRequest>();
            }
        }
    }
}
