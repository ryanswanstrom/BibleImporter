using Microsoft.WindowsAzure.Storage.Table;

namespace BibleImporter
{
    class BibleVerseEntity : TableEntity
    {
        public BibleVerseEntity(string Book, string OsisID)
        {
            this.PartitionKey = Book;
            this.RowKey = OsisID;
        }
        public BibleVerseEntity() { }

        public string Book { get; set; }
        public string Text { get; set; }
        public int Chapter { get; set; }
        public int Verse { get; set; }

        public override string ToString()
        {
            return $"{this.RowKey} - {this.PartitionKey} {this.Chapter}:{this.Verse} => {this.Text}";
        }
    }
}
