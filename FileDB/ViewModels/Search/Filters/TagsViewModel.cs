using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDB.ViewModels.Search.Filters;

public partial class TagsViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<TagForSearch> tags = [];

    [ObservableProperty]
    private ObservableCollection<TagForSearch> selectedTags = [];

    [ObservableProperty]
    private bool allowOtherTags;

    private readonly ITagsRepository tagsRepo;
    private readonly IFileSelector fileSelector;
    private readonly IDatabaseAccessProvider databaseAccessProvider;

    public TagsViewModel(ITagsRepository tagsRepo, IFileSelector fileSelector, IDatabaseAccessProvider databaseAccessProvider)
    {
        this.tagsRepo = tagsRepo;
        this.fileSelector = fileSelector;
        this.databaseAccessProvider = databaseAccessProvider;
        ReloadTags();
        this.RegisterForEvent<TagsUpdated>((x) => ReloadTags());
        TrySelectTagsFromSelectedFile();
    }

    private void TrySelectTagsFromSelectedFile()
    {
        if (fileSelector.SelectedFile is not null)
        {
            var tagsInFile = databaseAccessProvider.DbAccess.GetTagsFromFile(fileSelector.SelectedFile.Id);
            if (tagsInFile.Any())
            {
                SelectedTags.Clear();
                foreach (var tagInFile in tagsInFile)
                {
                    SelectedTags.Add(Tags.First(x => x.Id == tagInFile.Id));
                }
            }
        }
    }

    private void ReloadTags()
    {
        Tags.Clear();
        foreach (var tag in tagsRepo.Tags)
        {
            Tags.Add(new(tag.Id, tag.Name));
        }
    }

    public IFilesFilter CreateFilter() =>
        new TagsFilter(SelectedTags, AllowOtherTags);
}
