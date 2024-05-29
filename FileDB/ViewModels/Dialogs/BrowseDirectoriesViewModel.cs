using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDB.ViewModels.Dialogs
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
            Directories.Clear();
            var dirs = DirectoryTreeCreator.Build(dbAccessProvider.DbAccess.GetDirectories());
            dirs.ForEach(Directories.Add);
        }

        [RelayCommand]
        private void SelectDirectory()
        {
            if (SelectedDirectory is not null)
            {
                var path = SelectedDirectory.Path;
                SelectedDirectoryPath = path;
                Messenger.Send<CloseModalDialogRequest>();
            }
        }
    }

    public class DirectoryTreeCreator
    {
        public static List<Directory> Build(IEnumerable<string> directoryPaths)
        {
            var result = new List<Directory>();

            foreach (var directoryPath in directoryPaths)
            {
                var pathParts = directoryPath.Split("/");

                if (pathParts.Length > 0)
                {
                    Directory? currentDirectory = null;

                    bool isTopLevel = true;
                    foreach (var pathPart in pathParts)
                    {
                        var subDirectory = currentDirectory?.Directories.FirstOrDefault(x => x.Name == pathPart);
                        if (subDirectory is null)
                        {
                            subDirectory = new Directory(pathPart, currentDirectory);
                            if (isTopLevel)
                            {
                                result.Add(subDirectory);
                            }
                            if (currentDirectory is null)
                            {
                                currentDirectory = subDirectory;
                            }
                            else
                            {
                                currentDirectory.Directories.Add(subDirectory!);
                            }
                        }

                        currentDirectory = subDirectory;
                        isTopLevel = false;
                    }
                }
            }

            return result;
        }
    }
}
