using PersonalMovieLibrary__.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public class MovieController : Controller
{
    public ActionResult Index(string sortOrder)
    {
        string movieFolder = Server.MapPath("~/Content/MovieLibrary/");
        string[] files = Directory.GetFiles(movieFolder);
        List<Movie> movies = new List<Movie>();

        foreach (string file in files)
        {
            movies.Add(new Movie
            {
                Title = Path.GetFileNameWithoutExtension(file),
                ThumbnailPath = Url.Content("~/Content/MovieLibrary/" + Path.GetFileName(file)),
                AverageRating = GetAverageRating(Path.GetFileNameWithoutExtension(file))
            });
        }

        switch (sortOrder)
        {
            case "title_asc":
                movies = movies.OrderBy(m => m.Title).ToList();
                break;
            case "title_desc":
                movies = movies.OrderByDescending(m => m.Title).ToList();
                break;
            case "rating_high":
                movies = movies.OrderByDescending(m => m.AverageRating).ToList();
                break;
            case "rating_low":
                movies = movies.OrderBy(m => m.AverageRating).ToList();
                break;
        }

        ViewBag.CurrentSort = sortOrder;
        return View(movies);
    }

    private double GetAverageRating(string title)
    {
        var ratings = Session["Ratings"] as Dictionary<string, List<int>>;
        if (ratings != null && ratings.ContainsKey(title) && ratings[title].Count > 0)
        {
            return ratings[title].Average();
        }
        return 0;
    }

    public ActionResult AddToWatchlist(string title)
    {
        var watchlist = Session["Watchlist"] as List<WatchlistItem> ?? new List<WatchlistItem>();
        if (!watchlist.Any(m => m.Title == title))
            watchlist.Add(new WatchlistItem { Title = title, UserRating = 0 });

        Session["Watchlist"] = watchlist;
        TempData["Message"] = $"{title} added to your Watchlist!";
        return RedirectToAction("Index");
    }

    public ActionResult RemoveFromWatchlist(string title)
    {
        var watchlist = Session["Watchlist"] as List<WatchlistItem>;
        if (watchlist != null)
        {
            watchlist.RemoveAll(m => m.Title == title);
            Session["Watchlist"] = watchlist;
        }
        TempData["Message"] = $"{title} removed from your Watchlist!";
        return RedirectToAction("Watchlist");
    }

    public ActionResult RateMovie(string title, int rating)
    {
        if (rating < 1 || rating > 5)
        {
            TempData["Message"] = "Invalid rating!";
            return RedirectToAction("Index");
        }

        var ratings = Session["Ratings"] as Dictionary<string, List<int>> ?? new Dictionary<string, List<int>>();
        if (!ratings.ContainsKey(title))
            ratings[title] = new List<int>();

        ratings[title].Add(rating);
        Session["Ratings"] = ratings;
        TempData["Message"] = $"You rated {title} {rating} stars!";
        return RedirectToAction("Index");
    }

    public ActionResult Watchlist()
    {
        var watchlist = Session["Watchlist"] as List<WatchlistItem> ?? new List<WatchlistItem>();
        return View(watchlist);
    }

    [HttpGet]
    public ActionResult UploadMovie()
    {
        return View();
    }

    [HttpPost]
    public ActionResult UploadMovie(HttpPostedFileBase file)
    {
        if (file != null && file.ContentLength > 0)
        {
            string path = Server.MapPath("~/Content/MovieLibrary/");
            string filename = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
            {
                file.SaveAs(Path.Combine(path, filename + extension));
                TempData["Message"] = $"{filename} uploaded successfully!";
            }
            else
            {
                TempData["Message"] = "Only JPG and PNG files are allowed!";
            }
        }
        else
        {
            TempData["Message"] = "No file selected!";
        }
        return RedirectToAction("UploadMovie");
    }
}
