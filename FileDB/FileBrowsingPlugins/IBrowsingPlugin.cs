using FileDBInterface.Model;

namespace FileDB.FileBrowsingPlugins;

public interface IBrowsingPlugin
{
    void FileLoaded(FilesModel file);
}
