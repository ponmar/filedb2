import sqlite3
import argparse
import os.path
import sys


def version2_to_version3(cur):
    try:
        addColumn = "ALTER TABLE files ADD COLUMN Orientation integer"
        cur.execute(addColumn)
    except:
        # Assume column already added
        pass


def main():
    parser = argparse.ArgumentParser(description='A tool for upgrading the format of the FileDB database.')
    parser.add_argument('--database', help='Path to database file', default='filedb.db')
    args = parser.parse_args()

    if not os.path.isfile(args.database):
        print('No such database: ' + args.database)
        sys.exit(1)
    
    print("Upgrading database '" + args.database + "'...")
    dbCon = sqlite3.connect(args.database)
    cur = dbCon.cursor()
    
    version2_to_version3(cur)
    
    dbCon.close()
    
    print("Finished")


if __name__ == "__main__":
    main()
