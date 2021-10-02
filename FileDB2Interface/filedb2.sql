drop table if exists filepersons;
drop table if exists filelocations;
drop table if exists filetags;
drop table if exists files;
drop table if exists persons;
drop table if exists locations;
drop table if exists tags;

create table files(
    id integer primary key autoincrement not null,
    path text unique not null, /* Format: path/to/file/filename */
    description text,
    datetime varchar(19), /* Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS */
    position text /* Format: <latitude> <longitude> */
);

create table persons(
    id integer primary key autoincrement not null,
    firstname text not null,
    lastname text not null,
    description text,
    dateofbirth varchar(10), /* Format: YYYY-MM-DD */
    profilefileid integer references files(id) on delete set null
);

create table locations(
    id integer primary key autoincrement not null,
    name text unique not null,
    description text,
    position text /* Format: <latitude> <longitude> */
);

create table tags(
    id integer primary key autoincrement not null,
    name text unique not null
);

create table filepersons(
    fileid integer references files(id) on delete cascade,
    personid integer references persons(id) on delete cascade,
    primary key(fileid, personid)
);

create table filelocations(
    fileid integer references files(id) on delete cascade,
    locationid integer references locations(id) on delete cascade,
    primary key(fileid, locationid)
);

create table filetags(
    fileid integer references files(id) on delete cascade,
    tagid integer references tags(id) on delete cascade,
    primary key(fileid, tagid)
);
