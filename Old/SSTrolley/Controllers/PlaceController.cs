using Microsoft.AspNetCore.Mvc;
using SSTrolley.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Controllers
{
    [Produces("application/json")]
    [Route("api/place")]
    public class PlaceController : Controller
    {
        TrolleyContext context;
        public PlaceController(TrolleyContext context)
        {
            this.context = context;
        }

        // GET: api/place
        [HttpGet]
        public IEnumerable<int> GetIds()
        {
            return context.Places.Select(p => p.Id);
        }

        // GET: api/place/:id
        [HttpGet("{id:int}")]
        public IActionResult GetPlace(int id)
        {
            try
            {
                return Json(context.Places.First(h => h.Id == id));
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        // GET: api/place/all
        [HttpGet("all")]
        public IEnumerable<Place> GetAll()
        {
            return context.Places;
        }
    }
}