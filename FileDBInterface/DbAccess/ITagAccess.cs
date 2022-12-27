using System.Collections.Generic;
using FileDBShared.Model;

namespace FileDBInterface.DbAccess;

public interface ITagAccess
{
    public IEnumerable<TagModel> GetTags();
    public int GetTagCount();
    public TagModel GetTagById(int id);
    public bool HasTagId(int id);
    public void InsertTag(TagModel tag);
    public void UpdateTag(TagModel tag);
    public void DeleteTag(int id);
}
