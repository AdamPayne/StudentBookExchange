using Bookstore.WebApp.Entities;
using ImageResizer;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace Bookstore.WebApp.Services
{
    public class ImageProcessingService
    {
        private HttpServerUtilityBase server;

        public ImageProcessingService(HttpServerUtilityBase server)
        {
            this.server = server;
        }

        public void ProcessImage(Stream imageStream, BookListing listing)
        {
            // Resize images
            var thumbnailBytes = this.ResizeImageForThumbnail(imageStream);
            var newImageBytes = this.ResizeImageForFullImage(imageStream);
            var imageName = Guid.NewGuid().ToString();

            //var imagePath = this.GetImagesPath() + imageName + ".jpg";
            //var thumbnailPath = this.GetImagesPath() + imageName + "_thumb.jpg";

            // Write images to disk
            //Directory.CreateDirectory(this.GetImagesPath());
            //File.WriteAllBytes(imagePath, newImageBytes);
            //File.WriteAllBytes(thumbnailPath, thumbnailBytes);

            string connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Create container
            CloudBlobContainer container = blobClient.GetContainerReference("images");
            container.CreateIfNotExists();

            // Change access to Blob
            BlobContainerPermissions permissions = new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            // Get blob reference
            CloudBlockBlob thumnailBlob = container.GetBlockBlobReference(imageName + "_thumb.jpg");
            CloudBlockBlob imageBlob = container.GetBlockBlobReference(imageName + ".jpg");

            // Read image bytes
            thumnailBlob.UploadFromByteArray(thumbnailBytes, 0, thumbnailBytes.Length);
            imageBlob.UploadFromByteArray(newImageBytes, 0, newImageBytes.Length);
            // Update BookListing with image name
            listing.SetImage(imageName);
        }

        public byte[] ResizeImageForThumbnail(Stream imageStream)
        {
            var thumbnailResizeSettings = new ResizeSettings();
            thumbnailResizeSettings.Format = "jpg";
            thumbnailResizeSettings.MaxHeight = 500;
            thumbnailResizeSettings.MaxWidth = 300;
            thumbnailResizeSettings.Quality = 80;

            var bytes = this.ResizeImage(imageStream, thumbnailResizeSettings);

            return bytes;
        }

        public byte[] ResizeImageForFullImage(Stream imageStream)
        {
            var newImageSettings = new ResizeSettings();
            newImageSettings.Format = "jpg";
            newImageSettings.MaxHeight = 800;
            newImageSettings.MaxWidth = 1200;
            newImageSettings.Quality = 80;

            var bytes = this.ResizeImage(imageStream, newImageSettings);

            return bytes;
        }

        private string GetImagesPath()
        {
            var dataPath = ConfigurationManager.AppSettings["DataFolderRelativePath"];
            var imagesPath = "~/" + dataPath + "images/";
            return this.server.MapPath(imagesPath);
        }

        private byte[] ResizeImage(Stream imageStream, ResizeSettings settings)
        {
            imageStream.Position = 0;

            var resizedImageStream = new MemoryStream();
            ImageBuilder.Current.Build(imageStream, resizedImageStream, settings, false);

            return resizedImageStream.ToArray();
        }
    }
}