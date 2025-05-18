# README

## About

This is a project for storing, maintaining and presenting meta-data (persons, locations and tags) for files. A typical use case is to make a picture and video collection searchable and for running slideshows with extra information.

FileDB provides a desktop application and ways for exporting data to 3rd party applications. Releases are currently only published for Windows, but it is possible to build and run the application in Linux.

FileDB pros:

* Own your data: No data is shared with other parties
* Control your data: All data is stored in an SQLite database and may be converted for future use depending on your future needs (no lock-in effect). The FileDB application has support for exporting to JSON format.
* Search cababilities: Find files given a basic search criteria. Combine basic search criterias for creating an advanced search criteria
* The built-in file browser supports showing extra data such as person age
* 3rd party software integrations

FileDB cons:

* Categorizing files is a bit time consuming. However, the main application tries to make this process as simple as possible. Competing cloud services usually identifies persons automatically with face recognition, but that feature is not implemented in FileDB.

### Main Application

The FileDB main application is the GUI towards the internal database that stores the file meta-data. This application includes file browsing, meta-data editing and advanced search features.

A [demo](https://github.com/ponmar/filedb2_demo/) with some images and meta-data is available to show case the FileDB potential.

### Prerequisites

* A collection of files
* It is strongly recommended to have a backup procedure for your file collection (although FileDB itself does not intentionally modify your files)

### 3rd Party Software Integrations

FileDB gives you the possibility to export the internal database and file searches to your own applications.

## Getting Started

1. Create an empty file in your files collection root directory and name it 'MyFiles.FileDB'. Open the file with the FileDB application.
2. Create a database for your files collection and adjust your settings (see Tools tab)
3. Add your files (see Files tab)
4. Create persons, locations and tags (see Update tabs)
5. Add persons, locations and tags to your files (see Search tab)
6. Find your wanted pictures and run slideshows!

## Documentation

The release contains the following documentation:

* This README
* [Changelog](CHANGES.txt)
* Main application About page

## Developer Information

### About

The main application is a C# [Avalonia UI](https://avaloniaui.net/) desktop application. Accessing the internal database and files are done via the FileDBInterface project. Dapper is used for all database access.

### Building from Source

1. Clone GIT repository
2. Checkout wanted branch
3. Build FileDB.sln solution
4. Start the FileDB.Desktop project with a .FileDB file as command line argument (the [demo](https://github.com/ponmar/filedb2_demo/) can be used)

### Contribute

So far this project has been developed by me, Pontus Markström, and I would love to see that change both regarding number of developers and users. Feel free to contribute with pull requests towards the dev branch!

### Branch Strategy

* master: branch used for official releases in combination with vX.Y version tags
* dev: development branch, merged to master when ready for new release

## Licenses

This project uses the [MIT license](LICENSE.txt).

See licenses for used NuGet packages at the About page in the FileDB application.
