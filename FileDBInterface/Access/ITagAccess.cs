using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.Access
{
    public interface ITagAccess
    {
        public IEnumerable<TagModel> GetTags();
        public int GetTagCount();
        public TagModel GetTagById(int id);
        public bool HasTagId(int id);
        public void InsertTag(string name);
        public void UpdateTag(int id, string name);
        public void DeleteTag(int id);
    }
}
