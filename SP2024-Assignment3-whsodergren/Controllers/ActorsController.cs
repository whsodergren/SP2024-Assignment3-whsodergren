using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class ActorsController : Controller
    {

        public async Task<IActionResult> GetActorPhoto(int id)
        {
            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }
            var imageData = actor.ActorImage;

            return File(imageData, "image/jpg");
        }

        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            ActorDetailsVM actorDetailsVM = new ActorDetailsVM();
            actorDetailsVM.actor = actor;

            var queryText = actor.Name;
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
            actorDetailsVM.Sentiment = avgResult.ToString() + ", " + GetSentimentDescription(avgResult);

            actorDetailsVM.RedditPosts = redditPosts;

            return View(actorDetailsVM);

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

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBHyperlink")] Actor actor, IFormFile ActorImage)
        {
            ModelState.Remove(nameof(actor.ActorImage));

            if (ModelState.IsValid)
            {
                if (ActorImage != null && ActorImage.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await ActorImage.CopyToAsync(memoryStream);
                    actor.ActorImage = memoryStream.ToArray();
                }
                else
                {
                    actor.ActorImage = new byte[0];
                }
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBHyperlink")] Actor actor, IFormFile ActorImage)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(actor.ActorImage));

            Actor existingActor = _context.Actor.AsNoTracking().FirstOrDefault(m => m.Id == id);

            if (ActorImage != null && ActorImage.Length > 0)
            {
                var memoryStream = new MemoryStream();
                await ActorImage.CopyToAsync(memoryStream);
                actor.ActorImage = memoryStream.ToArray();
            }
            //grab EXISTING photo from DB in case user didn't upload a new one. Otherwise, the actor will have the photo overwritten with empty
            else if (existingActor != null)
            {
                actor.ActorImage = existingActor.ActorImage;
            }
            else
            {
                actor.ActorImage = new byte[0];
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
