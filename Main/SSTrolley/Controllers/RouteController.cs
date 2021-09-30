using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSTrolley.Models;

namespace SSTrolley.Controllers
{
    [Produces("application/json")]
    [Route("api/route")]
    public class RouteController : Controller
    {
        TrolleyContext context;
        public RouteController(TrolleyContext context)
        {
            this.context = context;
        }

        // GET: api/route
        [HttpGet]
        public IEnumerable<int> GetIds()
        {
            return context.Stops.Select(r => r.RouteId).Distinct();
        }

        // GET: api/route/:id
        [HttpGet("{id:int}")]
        public IActionResult GetRoute(int id)
        {
            IQueryable<StopPoint> route = context.Stops.Where(r => r.RouteId == id);
            if (route.Count() == 0)
                return NotFound();
            return Json(route);
        }

        // GET: api/route/all
        [HttpGet("all")]
        public IEnumerable<StopPoint> GetAll()
        {
            return context.Stops;
        }

        // GET: api/route/:id/trolleys
        [HttpGet("{id:int}/trolleys")]
        public IEnumerable<int> GetTrolleys(int id)
        {
            return context.Trolleys.Where(t => t.RouteId == id).Select(t => t.Id);
        }

        // GET: api/route/:id/trolleys/full
        [HttpGet("{id:int}/trolleys/full")]
        public IEnumerable<Trolley> GetTrolleysFull(int id)
        {
            return context.Trolleys.Where(t => t.RouteId == id);
        }
    }
}