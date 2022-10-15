import sqlite3
import argparse


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

    print("Upgrading database for '" + args.database + "'...")
    dbCon = sqlite3.connect(args.database)
    cur = dbCon.cursor()
    
    version2_to_version3(cur)
    
    dbCon.close()
    
    print("Finished")
    
if __name__ == "__main__":
    main()
