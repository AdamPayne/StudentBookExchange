using Bookstore.WebApp.Entities;
using Bookstore.WebApp.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Bookstore.WebApp.Services
{
    public class BookListingService
    {
        private HttpServerUtilityBase server;

        public BookListingService(HttpServerUtilityBase server)
        {
            this.server = server;
        }

        public IEnumerable<BookListingModel> GetBookListingModels()
        {
            var listings = this.GetBookListings().OrderByDescending(l => l.PublishedOn);
            var books = new List<BookListingModel>();
            var imagePath = "http://127.0.0.1:10000/devstoreaccount1/" + "images/";

            foreach (var listing in listings)
            {
                // Create model for page
                var viewModel = BookListingModel.FromListing(listing);

                // Set image URLs
                if (listing.ImageId != null)
                {
                    viewModel.ImageThumbnailUrl = imagePath + listing.ImageId + "_thumb.jpg";
                    viewModel.ImageFullUrl = imagePath + listing.ImageId + ".jpg";
                }

                books.Add(viewModel);
            }

            return books;
        }

        private IEnumerable<BookListing> GetBookListings()
        {
            var listings = new List<BookListing>();

            // Connect
            string connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = account.CreateCloudTableClient();

            // Get table
            CloudTable table = tableClient.GetTableReference("bookListing");
            table.CreateIfNotExists();

            // Generate query
            TableQuery<BookListingTableEntity> query = new TableQuery<BookListingTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "books"));

            var listingsEntity = table.ExecuteQuery(query);

            foreach (var listingEntity in listingsEntity)
            {
                BookListing listing = new BookListing(listingEntity.RowKey, listingEntity.Title, listingEntity.Description, listingEntity.Price, listingEntity.PublishedOn, listingEntity.ImageId);
                listings.Add(listing);
            }

            return listings;
        }

        private string GetListingsPath()
        {
            var dataPath = ConfigurationManager.AppSettings["DataFolderRelativePath"];
            var listingsPath = "~/" + dataPath + "listings/";
            return this.server.MapPath(listingsPath);
        }

        private string GetImagesPath()
        {
            var dataPath = ConfigurationManager.AppSettings["DataFolderRelativePath"];
            var imagesPath = "~/" + dataPath + "images/";
            return this.server.MapPath(imagesPath);
        }

        public void SaveNewListing(BookListing newListing)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = account.CreateCloudTableClient();

            // Create table
            CloudTable table = tableClient.GetTableReference("bookListing");
            table.CreateIfNotExists();

            //Create entity to insert
            BookListingTableEntity entity = new BookListingTableEntity(newListing.Id);
            entity.Description = newListing.Description;
            entity.ImageId = newListing.ImageId;
            entity.Price = newListing.Price;
            entity.PublishedOn = newListing.PublishedOn;
            entity.Title = newListing.Title;

            // Create table operation
            TableOperation insertOperation = TableOperation.Insert(entity);

            // Execute operation
            table.Execute(insertOperation);

            //var listingsPath = this.GetListingsPath();
            //var filePath = listingsPath + newListing.Id + ".json";
            //Directory.CreateDirectory(listingsPath);

            //var json = JsonConvert.SerializeObject(newListing);
            //File.WriteAllText(filePath, json);
        }
    }
}