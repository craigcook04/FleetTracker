using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using SSTrolley.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace SSTrolley.Controllers
{
    [Produces("application/json")]
    [Route("api/ad")]
    public class AdController : Controller
    {
        const string AD_PATH = "./wwwroot/assets/images/ads";

        private readonly IMemoryCache cache;

        public AdController(IMemoryCache cache)
        {
            this.cache = cache;
        }

        // GET: api/ad/random
        [HttpGet("random")]
        public IActionResult Random([FromQuery(Name = "count")] int count)
        {
            var files = new DirectoryInfo(AD_PATH).GetFiles().Where(f => !f.Name.EndsWith(".txt")).ToArray();
            if (count > files.Length) return StatusCode(StatusCodes.Status400BadRequest);

            var range = Enumerable.Range(0, files.Length).ToList();
            var random = new Random();

            int n = range.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = range[k];
                range[k] = range[n];
                range[n] = value;
            }

            return Json(range.Take(count).Select(i => new AdInfo
            {
                Name = files[i].Name,
                URL = cache.GetOrCreate("URL_" + files[i].Name, (e) =>
                {
                    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                    string url = "";
                    try
                    {
                        url = System.IO.File.ReadAllText(Path.Combine(files[i].Directory.FullName, Path.GetFileNameWithoutExtension(files[i].Name) + ".txt"));
                    }
                    catch (FileNotFoundException) { }
                    return url;
                })
            }));
        }

        // GET: api/ad/list
        [HttpGet("list")]
        [Authorize]
        public IActionResult List()
        {
            try
            {
                return Json(new DirectoryInfo(AD_PATH).GetFiles().Where(f => !f.Name.EndsWith(".txt")).Select(f => new AdInfo
                {
                    Name = f.Name,
                    URL = cache.GetOrCreate("URL_" + f.Name, (e) =>
                    {
                        e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                        string url = "";
                        try
                        {
                            url = System.IO.File.ReadAllText(Path.Combine(f.Directory.FullName, Path.GetFileNameWithoutExtension(f.Name) + ".txt"));
                        }
                        catch (FileNotFoundException) { }
                        return url;
                    })
                }));
            }
            catch (FileNotFoundException)
            {
                return StatusCode(500);
            }
        }

        // PUT: api/ad/update
        [HttpPut("update")]
        [Authorize]
        public IActionResult Update([FromBody]AdInfo adInfo)
        {
            var invalids = Path.GetInvalidFileNameChars();
            string newName = String.Join("_", adInfo.Name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            newName = Path.GetFileNameWithoutExtension(newName) + ".txt";

            System.IO.File.WriteAllText(Path.Combine(AD_PATH, newName), adInfo.URL);

            return Ok();
        }

        // POST: api/ad/delete
        [HttpPost("delete")]
        [Authorize]
        public IActionResult Delete([FromBody]AdInfo adInfo)
        {
            var invalids = Path.GetInvalidFileNameChars();
            string newName = String.Join("_", adInfo.Name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            try
            {
                System.IO.File.Delete(Path.Combine(AD_PATH, newName));
            }
            catch (FileNotFoundException)
            {
                return NotFound(newName);
            }

            return Ok();
        }

        // PUT: api/ad/upload
        [HttpPut("upload")]
        [Authorize]
        public IActionResult Upload(IFormFileCollection fileUpload)
        {
            if (fileUpload == null)
            {
                return StatusCode(422);
            }

            foreach (IFormFile f in fileUpload)
            {
                using (FileStream stream = new FileStream(Path.Combine(AD_PATH, f.FileName), FileMode.Create))
                {
                    f.CopyTo(stream);
                }
            }

            return Ok();
        }
    }
}