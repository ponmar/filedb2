using System.Collections.Generic;
using System.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public interface IFilterViewModel : INotifyDataErrorInfo
{
    IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess);
}
