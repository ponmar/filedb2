using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;
using System.Collections.ObjectModel;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public record TagForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class TagViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<TagForSearch> tags = [];

    [ObservableProperty]
    private TagForSearch? selectedTag;

    [ObservableProperty]
    private bool negate;

    private readonly ITagsRepository tagsRepository;

    public TagViewModel(ITagsRepository tagsRepository) : base(FilterType.Tag)
    {
        this.tagsRepository = tagsRepository;
        ReloadTags();
        this.RegisterForEvent<TagsUpdated>((x) => ReloadTags());
    }

    private void ReloadTags()
    {
        Tags.Clear();
        foreach (var tag in tagsRepository.Tags)
        {
            Tags.Add(new(tag.Id, tag.Name));
        }
    }

    protected override IFilesFilter DoCreate() =>
        Negate ? new FilterWithoutTag(SelectedTag) : new FilterTag(SelectedTag);
}
