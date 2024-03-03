using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SP2024_Assignment3_whsodergren.Data;
using SP2024_Assignment3_whsodergren.Models;
using VaderSharp2;

namespace SP2024_Assignment3_whsodergren.Controllers
{
    public class MoviesController : Controller
    {
        public async Task<IActionResult> GetMoviePhoto(int id)
        {
            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            var imageData = movie.MovieImage;

            return File(imageData, "image/jpg");
        }

        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            MovieDetailsVM movieDetailsVM = new MovieDetailsVM();
            movieDetailsVM.movie = movie;

            var queryText = movie.Title;
            var json = "";
            using (WebClient wc = new WebClient())
            {
                //fake like you are a "real" web browser
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                json = wc.DownloadString("https://www.reddit.com/search.json?limit=100&q=" + HttpUtility.UrlEncode(queryText));
            }
            var textToExamine = new List<string>();
            JsonDocument doc = JsonDocument.Parse(json);

            // Navigate to the "data" object
            JsonElement dataElement = doc.RootElement.GetProperty("data");

            // Navigate to the "children" array
            JsonElement childrenElement = dataElement.GetProperty("children");
            foreach (JsonElement child in childrenElement.EnumerateArray())
            {
                if (child.TryGetProperty("data", out JsonElement data))
                {
                    if (data.TryGetProperty("selftext", out JsonElement selftext))
                    {
                        string selftextValue = selftext.GetString();
                        if (!string.IsNullOrEmpty(selftextValue)) { textToExamine.Add(selftextValue); }
                        else if (data.TryGetProperty("title", out JsonElement title)) //use title if text is empty
                        {
                            string titleValue = title.GetString();
                            if (!string.IsNullOrEmpty(titleValue)) { textToExamine.Add(titleValue); }
                        }
                    }
                }
            }

            var analyzer = new SentimentIntensityAnalyzer();
            int validResults = 0;
            double resultsTotal = 0;
            List<RedditPost> redditPosts = new List<RedditPost>();

            foreach (string textValue in textToExamine)
            {
                var results = analyzer.PolarityScores(textValue);
                if (results.Compound != 0)
                {
                    resultsTotal += results.Compound;
                    validResults++;

                    redditPosts.Add(new RedditPost
                    {
                        Text = textValue,
                        Sentiment = Math.Round(results.Compound, 2)
                    });
                }
            }

            double avgResult = Math.Round(resultsTotal / validResults, 2);
            movieDetailsVM.Sentiment = avgResult.ToString() + ", " + GetSentimentDescription(avgResult);

            movieDetailsVM.RedditPosts = redditPosts;

            return View(movieDetailsVM);

            static string GetSentimentDescription(double sentiment)
            {
                if (sentiment >= 0.8)
                    return "Very Positive";
                else if (sentiment >= 0.6)
                    return "Positive";
                else if (sentiment >= 0.4)
                    return "Slightly Positive";
                else if (sentiment >= -0.2 && sentiment <= 0.2)
                    return "Neutral";
                else if (sentiment >= -0.4)
                    return "Slightly Negative";
                else if (sentiment >= -0.6)
                    return "Negative";
                else
                    return "Very Negative";
            }

        }


        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,YearOfRelease,IMDBHyperlink")] Movie movie, IFormFile MovieImage)
        {
            ModelState.Remove(nameof(movie.MovieImage));

            if (ModelState.IsValid)
            {
                if (MovieImage != null && MovieImage.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await MovieImage.CopyToAsync(memoryStream);
                    movie.MovieImage = memoryStream.ToArray();
                }
                else
                {
                    movie.MovieImage = new byte[0];
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,YearOfRelease,IMDBHyperlink")] Movie movie, IFormFile MovieImage)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(movie.MovieImage));

            Actor existingActor = _context.Actor.AsNoTracking().FirstOrDefault(m => m.Id == id);

            if (MovieImage != null && MovieImage.Length > 0)
            {
                var memoryStream = new MemoryStream();
                await MovieImage.CopyToAsync(memoryStream);
                movie.MovieImage = memoryStream.ToArray();
            }
            //grab EXISTING photo from DB in case user didn't upload a new one. Otherwise, the actor will have the photo overwritten with empty
            else if (existingActor != null)
            {
                movie.MovieImage = existingActor.ActorImage;
            }
            else
            {
                movie.MovieImage = new byte[0];
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
