﻿using CommunityToolkit.Mvvm.ComponentModel;
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
        public List<IFolder> Folders { get; } = [];

        public string Path => parent is not null ? parent.Path + "/" + Name : Name;

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
        private List<IFolder> folders = [];

        [ObservableProperty]
        private IFolder? selectedFolder;

        public string? SelectedDirectoryPath { get; private set; }

        private readonly IDbAccessProvider dbAccessProvider;

        public BrowseDirectoriesViewModel(IDbAccessProvider dbAccessProvider)
        {
            this.dbAccessProvider = dbAccessProvider;
            ReloadFolders();
        }

        private void ReloadFolders()
        {
            var root = new Folder(RootFolderName);

            Folders.Clear();
            Folders.Add(root);

            foreach (var directoryPath in dbAccessProvider.DbAccess.GetDirectories())
            {
                var pathParts = directoryPath.Split("/");

                if (pathParts.Length > 0)
                {
                    var currentFolder = root;

                    foreach (var pathPart in pathParts)
                    {
                        var subFolder = currentFolder.Folders.FirstOrDefault(x => x.Name == pathPart);
                        if (subFolder is null)
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

            if (SelectedFolder is not null)
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
