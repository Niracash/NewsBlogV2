using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlog.Data;
using NewsBlog.Models;
using NewsBlog.ViewModels;
using Newtonsoft.Json.Linq;

namespace NewsBlog.Controllers
{
    public class PageController : Controller
    {
        private readonly AppDbContext _db;

        //weather api
        private readonly string apiKey = "ff24c72efe3238fcc138631c67bcc428";
        private readonly string city = "copenhagen";

        public PageController(AppDbContext db)
        {
            _db = db;
        }
        //public IActionResult Weather()
        //{
        //    return View();
        //}
        public async Task<IActionResult> About()
        {
            var page = await _db.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
            var pageViewModel = new PageViewModel()
            {
                Title = page!.Title,
                Description = page.Description,
            };
            return View(pageViewModel);
        }
        public async Task<IActionResult> Contact()
        {
            var page = await _db.Pages!.FirstOrDefaultAsync(x => x.Slug == "contact");
            var pageViewModel = new PageViewModel()
            {
                Title = page!.Title,
                Description = page.Description,
            };
            return View(pageViewModel);
        }

        public async Task<IActionResult> Weather()
        {
            Weather weather = await GetWeatherData();
            return View(weather);
        }

        private async Task<Weather> GetWeatherData()
        {
            string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            string responseBody = await response.Content.ReadAsStringAsync();

            // Log the response for debugging purposes
            //System.IO.File.WriteAllText("weatherApiResponse.txt", responseBody);

            JObject weatherJson = JObject.Parse(responseBody);

            Weather weather = new Weather
            {
                Description = weatherJson["weather"]?[0]?["description"]?.ToString() ?? "N/A",
                Temperature = float.Parse(weatherJson["main"]?["temp"]?.ToString() ?? "0"),
                Humidity = int.Parse(weatherJson["main"]?["humidity"]?.ToString() ?? "0"),
                WindSpeed = float.Parse(weatherJson["wind"]?["speed"]?.ToString() ?? "0")
            };

            return weather;
        }
    }
}