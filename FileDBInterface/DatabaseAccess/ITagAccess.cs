using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.DatabaseAccess;

public interface ITagAccess
{
    IEnumerable<TagModel> GetTags();
    int GetTagCount();
    TagModel GetTagById(int id);
    bool HasTagId(int id);
    int InsertTag(TagModel tag);
    void UpdateTag(TagModel tag);
    void DeleteTag(int id);
}
