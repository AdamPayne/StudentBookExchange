﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bookstore.WebApp.Models
{
    public class BookListingModel
    {
        public string BookId
        {
            get;
            set;
        }

        public string BookTitle
        {
            get;
            set;
        }

        public string BookDescription
        {
            get;
            set;
        }

        public double Price
        {
            get;
            set;
        }

        public string ImageThumbnailUrl
        {
            get;
            set;
        }

        public string ImageFullUrl
        {
            get;
            set;
        }
    }
}