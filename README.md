# FileDB

## About

This is a project for storing, maintaining and presenting meta-data (persons, locations and tags) for files. A typical use case is to make a picture and video collection searchable and for running slideshows with extra information.

FileDB provides a desktop application and ways for exporting data to 3rd party applications. Releases are currently only published for Windows, but it is possible to build and run the application in Linux.

Key features:

* Own your data: No data is shared with other parties
* Control your data: All data is stored in an SQLite database and may be converted for future use depending on your future needs (no lock-in effect)
* Search capabilities: Find files given a basic search criterion. Combine criteria for creating an advanced search
* The built-in file browser supports showing extra data such as person age
* Database and search result export to simplify 3rd party software integrations

## Main Application

The FileDB main application is the GUI towards the internal database that stores the file meta-data. This application includes file browsing, meta-data editing and advanced search features.

A [demo](https://github.com/ponmar/filedb2_demo/) with some images and meta-data is available to showcase the FileDB potential.

### Search Filters

The following search filters are available. Multiple filters can be added and combined using AND, OR, or XOR operators.

| Filter | Description |
|--------|-------------|
| All Files | Returns all files in the database |
| Annual Date | Filter by day and month (optionally a range), ignoring the year — useful for finding files from recurring calendar dates |
| Combine | Combine two file ID lists using intersection, union, or difference operations |
| Date | Filter by a specific date or date range |
| Directory | Filter by directory path within the file collection |
| File List | Filter by an explicit list of file IDs (can be negated) |
| File Type | Filter by file type (e.g., image, video) |
| Location | Filter by assigned location (can be negated) |
| No Date/Time | Files that have no date/time metadata |
| Num Persons | Filter by the number of persons tagged in the file |
| Person | Filter by a specific person (can be negated) |
| Person Age | Filter by the age of tagged persons at the time of the file (age range) |
| Person Group | Filter by a group of persons, with an option to allow additional persons in the file |
| Person Profile Files | Files that are used as a person's profile picture |
| Person Sex | Filter by the sex of tagged persons |
| Position | Filter by GPS position — specify a coordinate and a search radius in meters |
| Random | Return a random selection of files |
| Season | Filter by season (spring, summer, autumn, winter) |
| Tag | Filter by a specific tag (can be negated) |
| Tags | Filter by a group of tags, with an option to allow additional tags in the file |
| Text | Search by text in file descriptions, and optionally in person names/descriptions, location names/descriptions, and tag names |
| Text file content | Search within the actual content of files in the database — specify file extensions (e.g. `.txt .md`) and a search string; toggle case-sensitive matching |
| Time | Filter by time-of-day range |
| Uncategorized | Files that have no persons, locations or tags |

### Prerequisites

* A collection of files
* It is strongly recommended to have a backup procedure for your file collection (although FileDB itself does not intentionally modify your files)

### 3rd Party Software Integrations

FileDB gives you the possibility to export the internal database and file searches to your own applications. The following export formats are supported:

* **JSON** — full meta-data export
* **HTML** — files and meta-data as a browsable HTML page (images stored alongside)
* **Self-contained HTML** — single HTML file with images embedded (base64)
* **M3U** — playlist for media players

## Getting Started

1. Create an empty file in your files collection root directory with the `.FileDB` extension (e.g. `MyFiles.FileDB`). Open the file with the FileDB application.
2. Create a database for your files collection and adjust your settings (see Tools and Settings tabs)
3. Add your files (see Files tab)
4. Create persons, locations and tags (see Update tabs)
5. Add persons, locations and tags to your files (see Search tab)
6. Find your wanted pictures and run slideshows!

## Documentation

The release contains the following documentation:

* This README
* [Changelog](CHANGES.txt)
* Main application About page

## Backup and Restore

### Backup

Database backups are automatically created when adding new files. Manual backup can be created via the Tools tab. The backup files are stored in the same directory as the .FileDB and .db files.

The .db files and the .FileDB file should be backed up with the same procedure as for the files in your collection of files.

### Restore

1. Copy the current database file (FileDB.db) to a backup location
2. Replace the .db file with the wanted backup file available in the same directory as the .db and .FileDB file

## Developer Information

### About

The main application is a C# [Avalonia UI](https://avaloniaui.net/) desktop application. Accessing the internal database and files are done via the FileDBInterface project. Dapper is used for all database access.

### Building from Source

1. Clone GIT repository
2. Checkout wanted branch
3. Build FileDB.slnx solution
4. Start the FileDB.Desktop project with a .FileDB file as command line argument (the [demo](https://github.com/ponmar/filedb2_demo/) can be used)

### Contribute

So far this project has been developed by me, Pontus Markström, and I would love to see that change both regarding number of developers and users. Feel free to contribute with pull requests towards the dev branch!

### Branch Strategy

* master: branch used for official releases in combination with vX.Y version tags
* dev: development branch, merged to master when ready for new release

## Licenses

This project uses the [MIT license](LICENSE.txt).

See licenses for used NuGet packages at the About page in the FileDB application.
