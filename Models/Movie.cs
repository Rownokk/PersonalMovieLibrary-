using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonalMovieLibrary__.Models
{
    public class Movie
    {
        public string Title { get; set; }
        public string ThumbnailPath { get; set; }
        public double AverageRating { get; set; }
    }
}