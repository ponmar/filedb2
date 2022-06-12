# README #

## About ##

This is a project for storing, maintaining and presenting meta-data (persons, locations and tags) for files. A typical use case is to make a picture and video collection searchable and for running slideshows with extra information.

FileDB provides a main application, a MAUI based app and ways for exporting data to 3rd party applications.

FileDB pros:

* Own your data: No data is shared with other parties
* Control your data: All data is stored in an SQLite database and may be converted for future use depending on your future needs (no lock-in effect). The FileDB application has support for exporting to JSON format.
* Search cababilities: Find files given a basic search criteria. Combine basic search criterias for creating an advanced search criteria
* File browser supports showing extra data such as person age
* 3rd party software integrations: the internal database and file searches can be exported

FileDB cons:

* Categorizing files is a bit time consuming. However, the main application tries to make this process as simple as possible. Competing cloud services usually identifies persons automatically with face recognition, but that feature is not implemented in FileDB.

### Main Application ###

The FileDB main application is the GUI towards the internal database that stores the file meta-data. It is a C# WPF application. The application uses one configuration file that, for example, points out the file collection root directory.

A demo configuration with some images and meta-data is available to show case the FileDB potential.

### MAUI App ###

A .NET MAUI app is being developed for visualizing exported FileDB data.

### 3rd Party Application Integrations ###

FileDB gives you the possibility to export FileDB data to your own applications.

## Getting Started ##

### Prerequisites ###

Note that it is recommended to have a backup procedure for your file collection before running FileDB (although FileDB itself does not modify your files).

FileDB prerequisites:

- .NET6

### Downloads ###

Download official releases [here](https://drive.google.com/drive/folders/1GyZpdDcMdUOlvvtwtKUuylazoy7XaIcm).

## Documentation ##

The release contains the following documentation:

* This README
* [Changelog](CHANGES.txt)
* Main application About page

## Developer Information ##

### Building from Source ###

1. Clone GIT repository
2. Checkout wanted branch
3. Open FileDB.sln in Visual Studio 2022
4. Disable FileDBApp project if not wanted (it requires .NET MAUI)
5. Build solution
6. Set FileDB project as startup project
7. Start project

### Contribute ###

So far this project has been developed by me, Pontus Markstrom, and I would love to see that change both regarding number of developers and users. Feel free to contribute!

### Branch Strategy ###

* master: branch used for official releases in combination with vX.Y version tags
* dev: development branch, merged to master when ready for new release
* feature/*: feature-branches merged to dev via pull-requests

## Licenses ##

This project uses the [MIT license](LICENSE.txt).

See licenses for used NuGet packages at the About page in the FileDB application.
