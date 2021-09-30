using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSTrolley.Data;
using SSTrolley.Models;

namespace SSTrolley.Controllers
{
    [Produces("application/json")]
    [Route("api/trolley")]
    public class TrolleyController : Controller
    {
        TrolleyContext context;
        public TrolleyController(TrolleyContext context)
        {
            this.context = context;
        }

        // GET: api/trolley
        [HttpGet]
        public IEnumerable<int> GetIds()
        {
            return context.Trolleys.Select(t => t.Id);
        }

        // GET: api/trolley/:id
        [HttpGet("{id:int}")]
        public IActionResult GetTrolley(int id)
        {
            try
            {
                return Json(context.Trolleys.First(h => h.Id == id));
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        // GET: api/trolley/all
        [HttpGet("all")]
        public IEnumerable<Trolley> GetAll()
        {
            return context.Trolleys;
        }

        // POST: api/trolley
        [HttpPost]
        public IActionResult Post([FromBody]TrolleyLogin value)
        {
            if (value == null || value.Login == null)
                return StatusCode(403);

            Trolley trolley;
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    trolley = context.Trolleys.First(h => h.Id == value.Id);
                }
                catch (InvalidOperationException)
                {
                    transaction.Rollback();
                    return NotFound();
                }

                if (!trolley.CheckLogin(value.Login))
                {
                    transaction.Rollback();
                    return StatusCode(403);
                }

                trolley.LastLongitude = trolley.Longitude;
                trolley.LastLatitude = trolley.Latitude;

                // Scale down the longitude and latitude if the trolley sends fixed point values, this is to increase percision as the hardware in the trolley only supports 32 bit floats
                if (value.Longitude == 0)
                    trolley.Longitude = value.LonFixed / 10000000.0;
                else
                    trolley.Longitude = value.Longitude;

                if (value.Latitude == 0)
                    trolley.Latitude = value.LatFixed / 10000000.0;
                else
                    trolley.Latitude = value.Latitude;

                if (value.Heading == -1)
                    trolley.Heading = TrolleyProcessing.CalculateHeadingFromLonLat(trolley.LastLongitude, trolley.LastLatitude, trolley.Longitude, trolley.Latitude);
                else
                    trolley.Heading = value.Heading * Math.PI / 180.0;

                trolley.TotalDistance += TrolleyProcessing.HaversineDistance(trolley.Longitude, trolley.Latitude, trolley.LastLongitude, trolley.LastLatitude);

                TrolleyPoint point = new TrolleyPoint { TrolleyId = trolley.Id, Longitude = trolley.Longitude, Latitude = trolley.Latitude };
                context.Locations.Add(point);
                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                if (value.Passengers != null)
                {
                    value.Passengers.PointId = point.Id;
                    value.Passengers.Id = 0;
                    context.Passengers.Add(value.Passengers);
                    trolley.PassengerCount = value.Passengers.Passengers;
                    trolley.TotalPassengers += value.Passengers.Arrived;
                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                transaction.Commit();
            }
            
            TrolleyProcessing.CalculateRoutePointInfo(trolley, context);

            return Ok();
        }

        // GET: api/trolley/:id/stops
        [HttpGet("{id:int}/stops")]
        public IActionResult GetStops(int id)
        {
            Trolley trolley = context.Trolleys.FirstOrDefault(t => t.Id == id);
            if (trolley == null)
                return NotFound();
            return Json(context.Stops.Where(r => r.RouteId == trolley.RouteId));
        }

        // GET: api/trolley/:id/stopsinfo
        [HttpGet("{id:int}/stopsinfo")]
        public IActionResult GetStopsInfo(int id)
        {
            Trolley trolley = context.Trolleys.FirstOrDefault(t => t.Id == id);
            if (trolley == null)
                return NotFound();
            return Json(context.StopTrolleyInfo.Where(r => r.TrolleyId == trolley.Id));
        }

        // GET: api/trolley/:id/service
        [HttpGet("{id:int}/service")]
        public IActionResult GetService(int id)
        {
            Trolley trolley = context.Trolleys.FirstOrDefault(t => t.Id == id);
            if (trolley == null)
                return NotFound();

            return Json(trolley.InService);
        }

        // PUT: api/trolley/:id/service
        [HttpPut("{id:int}/service")]
        [Authorize]
        public IActionResult SetService(int id, [FromBody]Data<bool> inService)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                Trolley trolley = context.Trolleys.FirstOrDefault(t => t.Id == id);
                if (trolley == null)
                    return NotFound();

                trolley.InService = inService.Value;

                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                transaction.Commit();
            }

            return Ok();
        }

        // GET: api/trolley/:id/servicestring
        [HttpGet("{id:int}/servicestring")]
        public IActionResult GetServiceString(int id)
        {
            Trolley trolley = context.Trolleys.FirstOrDefault(t => t.Id == id);
            if (trolley == null)
                return NotFound();

            string serviceString = trolley.ServiceString;
            if (serviceString == null || serviceString == "")
                serviceString = Trolley.DefaultServiceString;

            return Json(serviceString);
        }

        // PUT: api/trolley/:id/servicestring
        [HttpPut("{id:int}/servicestring")]
        [Authorize]
        public IActionResult SetServiceString(int id, [FromBody]Data<string> serviceString)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                Trolley trolley = context.Trolleys.FirstOrDefault(t => t.Id == id);
                if (trolley == null)
                    return NotFound();

                trolley.ServiceString = serviceString.Value;

                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                transaction.Commit();
            }

            return Ok();
        }
    }
}
