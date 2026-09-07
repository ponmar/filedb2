using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Notifiers;
using FileDB.Services;
using FileDB.ViewModels;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.File;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess.SQLite;
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
        ServiceLocator.RegisterSingleton<IDatabaseAccessFactory, DatabaseAccessFactory>();
        ServiceLocator.RegisterSingleton<IFilesystemAccessFactory, FilesystemAccessFactory>();
        ServiceLocator.RegisterSingleton<IFilesWritePermissionCheckerFactory, FilesWritePermissionCheckerFactory>();
        ServiceLocator.RegisterSingleton<IApplicationStartupService, ApplicationStartupService>();
        ServiceLocator.RegisterSingleton<IProcessUtils, ProcessUtils>();
        ServiceLocator.RegisterSingleton<INotifierFactory, NotifierFactory>();
        ServiceLocator.RegisterSingleton<ISpeeker, Speeker>();
        ServiceLocator.RegisterSingleton<IDateTimeProvider, DateTimeProvider>();
        ServiceLocator.RegisterSingleton<DateObserver>();
        ServiceLocator.RegisterSingleton<IClipboardService, ClipboardService>();

        ServiceLocator.RegisterSingleton<DatabaseCache>(
            typeof(IPersonsRepository),
            typeof(ILocationsRepository),
            typeof(ITagsRepository));

        ServiceLocator.RegisterSingleton<IFilesWritePermissionChecker, FilesWritePermissionChecker>();

        ServiceLocator.RegisterSingleton<HelpViewModel>();
        ServiceLocator.RegisterSingleton<IDialogs, Dialogs.Dialogs>();

        ServiceLocator.RegisterSingleton<Model.Model>(
            typeof(IConfigProvider),
            typeof(IDatabaseAccessProvider),
            typeof(IFilesystemAccessProvider),
            typeof(IConfigUpdater));

        ServiceLocator.RegisterSingleton<NotificationRepository>(
            typeof(INotificationRepository),
            typeof(INotificationManagement));

        ServiceLocator.RegisterSingleton<MainViewModel>();
        ServiceLocator.RegisterSingleton<ResultViewModel>(
            typeof(ResultViewModel),
            typeof(ISearchResultRepository),
            typeof(ISearchResultRepositoryManagement),
            typeof(IFileSelector));
        ServiceLocator.RegisterSingleton<FileViewModel>();
        ServiceLocator.RegisterSingleton<NotificationsViewModel>();
        ServiceLocator.RegisterSingleton<SettingsViewModel>();
        ServiceLocator.RegisterSingleton<UpdateLocationsViewModel>();
        ServiceLocator.RegisterSingleton<UpdatePersonsViewModel>();
        ServiceLocator.RegisterSingleton<UpdateTagsViewModel>();
        ServiceLocator.RegisterSingleton<BirthdaysViewModel>();
        ServiceLocator.RegisterSingleton<RipViewModel>();
        ServiceLocator.RegisterSingleton<ToolsViewModel>();
        ServiceLocator.RegisterSingleton<FilesViewModel>();
        ServiceLocator.RegisterSingleton<CriteriaViewModel>(
            typeof(CriteriaViewModel),
            typeof(ICriteriaViewModel));
        ServiceLocator.RegisterSingleton<IBoundingBoxRotator, BoundingBoxRotator>();
        ServiceLocator.RegisterSingleton<IFileRotator, FileRotator>();
        ServiceLocator.RegisterSingleton<FileCategorizationViewModel>();
        ServiceLocator.RegisterTransient<AddLocationViewModel>();
        ServiceLocator.RegisterTransient<AddPersonViewModel>();
        ServiceLocator.RegisterTransient<AddTagViewModel>();

        ServiceLocator.RegisterSingleton<IImageLoader, ImageLoader>();
        ServiceLocator.RegisterSingleton<IDatabaseCreator, SqLiteDatabaseCreator>();
        ServiceLocator.RegisterSingleton<IFileBackup, FileBackup>();
        ServiceLocator.RegisterSingleton<IDatabaseMigrationStartupCoordinator, DatabaseMigrationStartupCoordinator>();

        ServiceLocator.RegisterTransient<ExportSearchResultViewModel>();

        ServiceLocator.RegisterTransient<ISearchResultExportDataBuilder, SearchResultExportDataBuilder>();
        ServiceLocator.RegisterTransient<IFilesExporter, FilesExporter>();
        ServiceLocator.RegisterTransient<IM3uExporter, M3uExporter>();
        ServiceLocator.RegisterTransient<IHtmlExporter, HtmlExporter>();
        ServiceLocator.RegisterTransient<ISelfContainedHtmlExporter, SelfContainedHtmlExporter>();
        ServiceLocator.RegisterTransient<IJsonExporter, JsonExporter>();

        ServiceLocator.RegisterTransient<FilesExportTabViewModel>();
        ServiceLocator.RegisterTransient<M3uExportTabViewModel>();
        ServiceLocator.RegisterTransient<HtmlExportTabViewModel>();
        ServiceLocator.RegisterTransient<SelfContainedHtmlExportTabViewModel>();
        ServiceLocator.RegisterTransient<JsonExportTabViewModel>();

        ServiceLocator.RegisterTransient<FilterSelectionViewModel>();
        ServiceLocator.RegisterTransient<AnnualDateViewModel>();
        ServiceLocator.RegisterTransient<DateViewModel>();
        ServiceLocator.RegisterTransient<DirectoryViewModel>();
        ServiceLocator.RegisterTransient<RandomViewModel>();
        ServiceLocator.RegisterTransient<CombineViewModel>();
        ServiceLocator.RegisterTransient<UncategorizedViewModel>();
        ServiceLocator.RegisterTransient<AllFilesViewModel>();
        ServiceLocator.RegisterTransient<NoDateTimeViewModel>();
        ServiceLocator.RegisterTransient<TextViewModel>();
        ServiceLocator.RegisterTransient<FileListViewModel>();
        ServiceLocator.RegisterTransient<FileTypeViewModel>();
        ServiceLocator.RegisterTransient<PersonViewModel>();
        ServiceLocator.RegisterTransient<PersonAgeViewModel>();
        ServiceLocator.RegisterTransient<PersonSexViewModel>();
        ServiceLocator.RegisterTransient<PersonGroupViewModel>();
        ServiceLocator.RegisterTransient<PersonProfileFilesViewModel>();
        ServiceLocator.RegisterTransient<LocationViewModel>();
        ServiceLocator.RegisterTransient<PositionViewModel>();
        ServiceLocator.RegisterTransient<SeasonViewModel>();
        ServiceLocator.RegisterTransient<NumPersonsViewModel>();
        ServiceLocator.RegisterTransient<TagViewModel>();
        ServiceLocator.RegisterTransient<TagsViewModel>();
        ServiceLocator.RegisterTransient<TimeViewModel>();
        ServiceLocator.RegisterTransient<TextFileContentViewModel>();
        ServiceLocator.RegisterTransient<PersonTimelineViewModel>();

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
