using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bookstore.WebApp.Entities
{
    public class BookListingTableEntity : TableEntity
    {
        public BookListingTableEntity(string id)
        {
            this.PartitionKey = "books";
            this.RowKey = id;
        }

        public BookListingTableEntity()
        {

        }

        public string Title
        {
            get;
            set;
        }

        public string ImageId
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public double Price
        {
            get;
            set;
        }

        public DateTimeOffset PublishedOn
        {
            get;
            set;
        }
    }
}