using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;

namespace FileDB.ViewModels.Search.Filters;

public record TagForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class TagViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<TagForSearch> tags = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private TagForSearch? selectedTag;

    [ObservableProperty]
    private bool negate;

    private readonly ITagsRepository tagsRepo;
    private readonly IFileSelector fileSelector;
    private readonly IDatabaseAccessProvider databaseAccessProvider;

    public TagViewModel(ITagsRepository tagsRepo, IFileSelector fileSelector, IDatabaseAccessProvider databaseAccessProvider)
    {
        this.tagsRepo = tagsRepo;
        this.fileSelector = fileSelector;
        this.databaseAccessProvider = databaseAccessProvider;
        ReloadTags();
        this.RegisterForEvent<TagsUpdated>((x) => ReloadTags());
        TrySelectTagFromSelectedFile();

        ValidateAllProperties();
    }

    private void TrySelectTagFromSelectedFile()
    {
        if (fileSelector.SelectedFile is not null)
        {
            var tagsInFile = databaseAccessProvider.DbAccess.GetTagsFromFile(fileSelector.SelectedFile.Id);
            if (tagsInFile.Any())
            {
                var firstTagFromFile = tagsInFile.First();
                SelectedTag = Tags.First(x => x.Id == firstTagFromFile.Id);
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
        Negate ? new WithoutTagFilter(SelectedTag) : new TagFilter(SelectedTag);
}
