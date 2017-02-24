# BibleImporter
Imports all 31,102 bible verses into Azure Table storage.

It reads the file from [Matt Cook's bible repository](https://github.com/matt-cook/bible).
The actual file used is the [Web version](https://raw.githubusercontent.com/matt-cook/bible/master/en/web.xml) which is in the public domain.


# To Use
In [Program.cs](BibleImporter/Program.cs), provide your storage account name and storage account key.
 
```C#
static string STORAGE_ACCT_NAME = "";
static string STORAGE_ACCT_KEY = "";
```

# Data Dictionary for Table Storage

Below are the details for what gets inserted into Table Storage. Each
Bible verse gets an entry.

*  **PartitionKey**: Book of the Bible (ex. Titus, 1 Chronicles, 1 Samuel)
*  **RowKey**: The OsisID of the verse (ex. 1Sam.7.12, Zeph.3.8)
*  **Timestamp**: The time the data was inserted, this gets automatically added
*  **Chapter**: An int representing the number of the Chapter with a Book
*  **Verse**: An int representing the number of the verse
*  **Text**: The actual text of the verse
