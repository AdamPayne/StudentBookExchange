﻿using Bookstore.WebApp.Entities;
using Bookstore.WebApp.Models;
using Bookstore.WebApp.Services;
using System.Web.Mvc;

namespace Bookstore.WebApp.Controllers
{
    public class BooksController : Controller
    {
        public ActionResult Index()
        {
            var bookService = new BookListingService(this.Server);
            var books = bookService.GetBookListingModels();

            return View(books);
        }

        [HttpGet]
        public ActionResult Submit()
        {
            return View();
        }

        [ActionName("Submit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitPost(BookListingSubmissionModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var bookService = new BookListingService(this.Server);
            var newListing = BookListing.New(model.BookTitle, model.BookDescription, model.Price);

            if (model.BookImage != null)
            {
                var imageService = new ImageProcessingService(this.Server);
                imageService.ProcessImage(model.BookImage.InputStream, newListing);
            }

            bookService.SaveNewListing(newListing);

            return this.RedirectToAction("Index", "Books");
        }
        
    }
}