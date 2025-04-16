using FileDB.Dialogs;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModels;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.File;
using FileDB.ViewModels.Search.Filters;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System;
using System.IO;
using System.IO.Abstractions;

namespace FileDB;

public static class Bootstrapper
{
    private static readonly ILoggerFactory loggerFactory = CreateLoggerFactory();

    public static void Bootstrap()
    {
        ServiceLocator.RegisterSingleton<IFileSystem, FileSystem>();
        ServiceLocator.RegisterSingleton<INotifierFactory, NotifierFactory>();
        ServiceLocator.RegisterSingleton<ISpeeker, Speeker>();
        ServiceLocator.RegisterSingleton<DateObserver>();
        ServiceLocator.RegisterSingleton<IClipboardService, ClipboardService>();

        ServiceLocator.RegisterSingleton<DatabaseCache>([
            typeof(IPersonsRepository),
            typeof(ILocationsRepository),
            typeof(ITagsRepository),
        ]);

        ServiceLocator.RegisterSingleton<AboutViewModel>();
        ServiceLocator.RegisterSingleton<IDialogs, Dialogs.Dialogs>();

        ServiceLocator.RegisterSingleton<Model.Model>([
            typeof(INotificationHandling),
            typeof(INotificationsRepository),
            typeof(IConfigProvider),
            typeof(IDatabaseAccessProvider),
            typeof(IFilesystemAccessProvider),
            typeof(IConfigUpdater),
        ]);

        ServiceLocator.RegisterSingleton<MainViewModel>();
        ServiceLocator.RegisterSingleton<ResultViewModel>([
            typeof(ResultViewModel),
            typeof(ISearchResultRepository),
            typeof(IFileSelector),
        ]);
        ServiceLocator.RegisterSingleton<FileViewModel>();
        ServiceLocator.RegisterSingleton<NotificationsViewModel>();
        ServiceLocator.RegisterSingleton<SettingsViewModel>();
        ServiceLocator.RegisterSingleton<LocationsViewModel>();
        ServiceLocator.RegisterSingleton<PersonsViewModel>();
        ServiceLocator.RegisterSingleton<ViewModels.TagsViewModel>();
        ServiceLocator.RegisterSingleton<BirthdaysViewModel>();
        ServiceLocator.RegisterSingleton<RipViewModel>();
        ServiceLocator.RegisterSingleton<ToolsViewModel>();
        ServiceLocator.RegisterSingleton<FilesViewModel>();
        ServiceLocator.RegisterSingleton<CriteriaViewModel>();
        ServiceLocator.RegisterSingleton<FileCategorizationViewModel>();
        ServiceLocator.RegisterTransient<AddLocationViewModel>();
        ServiceLocator.RegisterTransient<AddPersonViewModel>();
        ServiceLocator.RegisterTransient<AddTagViewModel>();

        ServiceLocator.RegisterSingleton<IImageLoader, ImageLoader>();

        ServiceLocator.RegisterTransient<ExportSearchResultViewModel>();
        ServiceLocator.RegisterTransient<BrowseSubDirectoriesViewModel>();

        ServiceLocator.RegisterTransient<FilterSelectionViewModel>();
        ServiceLocator.RegisterTransient<AnnualDateViewModel>();
        ServiceLocator.RegisterTransient<DateViewModel>();
        ServiceLocator.RegisterTransient<NoMetaDataViewModel>();
        ServiceLocator.RegisterTransient<AllFilesViewModel>();
        ServiceLocator.RegisterTransient<NoDateTimeViewModel>();
        ServiceLocator.RegisterTransient<TextViewModel>();
        ServiceLocator.RegisterTransient<FileListViewModel>();
        ServiceLocator.RegisterTransient<FileTypeViewModel>();
        ServiceLocator.RegisterTransient<PersonViewModel>();
        ServiceLocator.RegisterTransient<PersonAgeViewModel>();
        ServiceLocator.RegisterTransient<PersonSexViewModel>();
        ServiceLocator.RegisterTransient<PersonGroupViewModel>();
        ServiceLocator.RegisterTransient<LocationViewModel>();
        ServiceLocator.RegisterTransient<PositionViewModel>();
        ServiceLocator.RegisterTransient<SeasonViewModel>();
        ServiceLocator.RegisterTransient<NumPersonsViewModel>();
        ServiceLocator.RegisterTransient<TagViewModel>();
        ServiceLocator.RegisterTransient<ViewModels.Search.Filters.TagsViewModel>();
        ServiceLocator.RegisterTransient<TimeViewModel>();

        ServiceLocator.RegisterSingleton(loggerFactory);
    }

    public static void StartServices()
    {
        ServiceLocator.Resolve<DateObserver>();
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });

            var logfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $" {Utils.ApplicationName}.log");
            builder.AddFile(logfilePath);
        });
    }
}
