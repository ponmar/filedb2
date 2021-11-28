﻿using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Sorters
{
    public class FilesByPathSorter : IComparer<FilesModel>
    {
        public int Compare(FilesModel x, FilesModel y)
        {
            return x.Path.CompareTo(y.Path);
        }
    }
}