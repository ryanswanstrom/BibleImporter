using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace BibleImporter
{
    class Program
    {
        static string STORAGE_ACCT_NAME = "";
        static string STORAGE_ACCT_KEY = "";
        /*
         * Read in a Bible and store in Azure Table Storage
         */
        static void Main(string[] args)
        {
            Console.WriteLine("Getting Started");
            Console.WriteLine("Reading in the books of the Bible");
            Dictionary<string, string> dictionary = readBibleBooks();
            
            storeBible(dictionary);
            Console.WriteLine("BOOM!");
            Console.ReadLine();
        }

        static Dictionary<string, string> readBibleBooks()
        {
            Dictionary<string,string> dictionary = new Dictionary<string, string>();
            string book, bookKey;

            // Read the file and display it line by line.
            System.IO.StreamReader bibleBooks =
               new System.IO.StreamReader("../../bibleBooks.txt");
            System.IO.StreamReader bibleBooksKey =
               new System.IO.StreamReader("../../bibleBooksOsisKey.txt");
            while ((book = bibleBooks.ReadLine()) != null
                && (bookKey = bibleBooksKey.ReadLine()) != null)
            {
                //Console.WriteLine($"{bookKey} - {book}");
                dictionary.Add(bookKey.Trim(), book.Trim());
            }

            bibleBooks.Close();
            bibleBooksKey.Close();
            return dictionary;
        }

        /*
         * That is the bible to be read in
         * https://raw.githubusercontent.com/matt-cook/bible/master/en/web.xml
         */
        static void storeBible(Dictionary<string,string> dictionary)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse
   ($"DefaultEndpointsProtocol=https;AccountName={STORAGE_ACCT_NAME};AccountKey={STORAGE_ACCT_KEY}");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("BibleVerses");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            var count = 0;
            string prevBook = "none";
            String bibleURL = "https://raw.githubusercontent.com/matt-cook/bible/master/en/web.xml";
            XmlTextReader reader = new XmlTextReader(bibleURL);
            reader.MoveToContent();
            while (reader.Read())
            {
                //loop through
                
                switch(reader.Name)
                {
                    // find each book of the bible and store it
                    case "div":
                        if (String.Equals(reader.GetAttribute("type"),"book"))
                        {
                            string key = reader.GetAttribute("osisID");
                            Console.WriteLine($"{key} {dictionary[key]}");
                        }
                        break;
                    // then find each verse and store it
                    case "verse":
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string osisId = reader.GetAttribute("osisID"); ;
                            string[] vals = osisId.Split('.');
                            string book = dictionary[vals[0]];
                            if (!book.Equals(prevBook))
                            {
                                if (batchOperation.Count > 0)
                                {
                                    table.ExecuteBatch(batchOperation);
                                    batchOperation.Clear();
                                }
                                prevBook = book;
                            }
                            BibleVerseEntity bv = new BibleVerseEntity(book, osisId);
                            bv.Text = reader.ReadElementContentAsString();
                            int chap, ver = 0;

                            if (Int32.TryParse(vals[1], out chap) 
                                && Int32.TryParse(vals[2], out ver))
                            {
                                bv.Chapter = chap;
                                bv.Verse = ver;
                                batchOperation.InsertOrReplace(bv);
                                if (++count % 50 == 0 && batchOperation.Count > 0)
                                {
                                    Console.WriteLine($"Writing to batch {batchOperation.Count}");
                                    table.ExecuteBatch(batchOperation);
                                    Console.WriteLine("Batch Executed");
                                    batchOperation.Clear();
                                }

                            } else
                            {
                                Console.WriteLine($"Not a valid verse number format: {osisId}");
                            }
                            
                            
                        }
                        break;
                }
                
                
            }
            // Execute the batch operation.
            table.ExecuteBatch(batchOperation);
            Console.WriteLine($"{count} verses added to Table Storage.");


        }
    }
}
