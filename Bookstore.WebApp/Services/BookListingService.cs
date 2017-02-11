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
            var imagePath = ConfigurationManager.AppSettings["DataFolderRelativePath"] + "images/";

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
            var listingPath = this.GetListingsPath();
            Directory.CreateDirectory(this.GetListingsPath());
            var allListringFiles = Directory.GetFiles(listingPath, "*.json");

            foreach (var listingFile in allListringFiles)
            {
                var jsonText = File.ReadAllText(listingFile);
                var listing = JsonConvert.DeserializeObject<BookListing>(jsonText);

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
            var listingsPath = this.GetListingsPath();
            var filePath = listingsPath + newListing.Id + ".json";
            Directory.CreateDirectory(listingsPath);

            var json = JsonConvert.SerializeObject(newListing);
            File.WriteAllText(filePath, json);
        }
    }
}