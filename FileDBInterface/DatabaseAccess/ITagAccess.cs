using System.Collections.Generic;
using FileDBShared.Model;

namespace FileDBInterface.DatabaseAccess;

public interface ITagAccess
{
    IEnumerable<TagModel> GetTags();
    int GetTagCount();
    TagModel GetTagById(int id);
    bool HasTagId(int id);
    void InsertTag(TagModel tag);
    void UpdateTag(TagModel tag);
    void DeleteTag(int id);
}
