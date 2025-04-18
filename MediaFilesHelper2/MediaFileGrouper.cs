﻿using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace MediaFilesHelper2;

public class FileInfo
{
    public string FilePath { get; }
    public DateTime? DateTime { get; }
    public DateOnly? Date => DateTime is not null ? DateOnly.FromDateTime(DateTime.Value) : null;
    public FileMetadata Metadata { get; }

    public string? DestinationPath { get; set; }

    public FileInfo(string filePath, FileMetadata metadata)
    {
        FilePath = filePath;
        Metadata = metadata;
        DateTime = DatabaseParsing.ParseFilesDatetime(metadata.Datetime);
    }
}

public class MediaFileGrouper
{
    private readonly string directory;

    public MediaFileGrouper(string directory)
    {
        this.directory = directory;
    }

    public List<FileInfo> ListFiles()
    {
        var filenames = Directory.GetFiles(directory, "*.*");
        var result = new List<FileInfo>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        var filesystemAccess = new FilesystemAccess(new FileSystem(), loggerFactory, "not used");

        foreach (var filename in filenames)
        {
            var fileMetadata = filesystemAccess.ParseFileMetadata(filename);
            if (fileMetadata is not null)
            {
                result.Add(new FileInfo(filename, fileMetadata));
            }
        }

        return result;
    }

    public void GroupImagesByDate(List<FileInfo> files)
    {
        foreach (var file in files.Where(x => x.DateTime is not null))
        {
            var groupPath = GetFileGroupName(file);
            var numImagesInGroup = Directory.Exists(groupPath) ? Directory.GetFiles(groupPath).Count() : 0;
            numImagesInGroup += files.Count(x => x.Date == file.Date);

            if (numImagesInGroup > 1)
            {
                file.DestinationPath = Path.Combine(groupPath, Path.GetFileName(file.FilePath));
            }
        }
    }

    public void MoveFiles(List<FileInfo> files)
    {
        foreach (var file in files.Where(x => x.DestinationPath is not null))
        {
            if (!Directory.Exists(file.DestinationPath))
            {
                var groupDirPath = Path.GetDirectoryName(file.DestinationPath)!;
                Directory.CreateDirectory(groupDirPath);
            }
            File.Move(file.FilePath, file.DestinationPath!);
        }
    }

    private string GetFileGroupName(FileInfo fileInfo)
    {
        string subDir = fileInfo.DateTime!.Value.ToString("yyyy-MM-dd") + " - todo";
        return Path.Combine(Path.GetDirectoryName(fileInfo.FilePath)!, subDir);
    }
}
