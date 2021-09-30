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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Reflection;
using SSTrolley.Data;

namespace SSTrolley.Controllers
{
    [Produces("application/json")]
    [Route("api/admin")]
    public class AdminController : Controller
    {
        readonly TrolleyContext context;
        readonly IConfiguration config;
        public AdminController(TrolleyContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
        }

        // POST: api/admin/login
        [HttpPost("login")]
        public IActionResult Login([FromBody]UserLogin userLogin)
        {
            User user = context.Users.FirstOrDefault(u => u.Username == userLogin.Username);
            if (user == null)
                return StatusCode(403);

            if (!user.CheckLogin(userLogin.Password))
                return StatusCode(403);

            return Json(BuildToken(user));
        }

        // GET: api/admin/check
        [HttpGet("check")]
        [Authorize]
        public IActionResult Check()
        {
            return Ok();
        }

        [HttpGet("trolley_location_data.csv")]
        [Produces("text/csv")]
        [Authorize]
        public IActionResult GetLocationData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Trolley ID, Longitude, Latitude, Date, Time, Passengers, Arrived, Departed\r\n");
            short passengers = 0;
            List<PassengerPoint> passengerPoints = context.Passengers.ToList();
            foreach (TrolleyPoint p in context.Locations)
            {
                PassengerPoint passengerPoint = passengerPoints.FirstOrDefault(a => a.PointId == p.Id);
                short arrived = 0;
                short departed = 0;
                if (passengerPoint != null)
                {
                    passengers = passengerPoint.Passengers;
                    arrived = passengerPoint.Arrived;
                    departed = passengerPoint.Departed;
                }

                sb.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}\r\n",
                    p.TrolleyId,
                    p.Longitude,
                    p.Latitude,
                    p.Timestamp.ToShortDateString(),
                    p.Timestamp.ToShortTimeString(),
                    passengers,
                    arrived,
                    departed);
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "trolley_location_data.csv");
        }

        private string BuildToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(config["Jwt:Issuer"],
              config["Jwt:Issuer"],
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class UserLogin
        {
            public string Username;
            public string Password;
        }

        //GET: api/admin/stopInfo
        [HttpGet("stopInfo/{stop}")]
        //[Authorize]
        public string GetStopInfo(string stop)
        {
            if (stop == null) throw new ArgumentNullException(nameof(stop));

            Console.WriteLine(HttpContext.Request.Headers["startDate"] + ' ' + HttpContext.Request.Headers["endDate"]);

            //Converting dates and setting times
            DateTime startDate = Convert.ToDateTime(HttpContext.Request.Headers["startDate"]);
            DateTime endDate = Convert.ToDateTime(HttpContext.Request.Headers["endDate"]);

            TimeSpan st = new TimeSpan(0, 0, 0);
            startDate = startDate.Date + st;

            TimeSpan et = new TimeSpan(23, 59, 0);
            endDate = endDate.Date + et;


            Console.WriteLine(stop + " : " + startDate + " - " + endDate);

            //query database using parameters
            var join = from loc in context.Locations
                       join pas in context.Passengers on loc.Id equals pas.PointId
                       where loc.Timestamp >= startDate && loc.Timestamp <= endDate
                       select new
                       {
                            loc.Id,
                            loc.TrolleyId,
                            loc.Longitude,
                            loc.Latitude,
                            loc.Timestamp,
                            pas.Arrived,
                            pas.Departed,
                            pas.Passengers
                       };


            foreach( var test in join)
            {
                Console.WriteLine(test.Id + " " + test.Timestamp + " " +  test.Passengers + " +" + test.Arrived + " -" + test.Departed);
            }


            var joinJson = Newtonsoft.Json.JsonConvert.SerializeObject(join);

            //get all stop points for trolley
            //var fenceDistance = 0.00225000225;
            var id = 0;
            var stopPoints = context.Stops.Where(r => r.RouteId == id);

            /*
            //check which are within ~500m of Stop Points
            if (stop == "All")
            {
                foreach (StopPoint stopPoint in stopPoints)
                {
                    foreach (var point in join)
                    {
                        if (point.Latitude < stopPoint.Latitude + fenceDistance && point.Latitude > stopPoint.Latitude - fenceDistance)
                        {
                            if (point.Longitude < stopPoint.Longitude + fenceDistance && point.Longitude > stopPoint.Longitude - fenceDistance)
                            {
                                listOfPoints.Add(point);
                            }
                        }
                    }
                    listOfPointLists.Add(listOfPoints);
                    listOfPoints = new List<>();
                }
            }
            else
            {
            }
            */

            return joinJson;

        }

        /*
        //: api/admin/updatestops
        [HttpPut("updatestops")]
        //[Authorize]
        public void UpdateStops()
        {
            var context = this.context;

            //update points
            StopPoint[] points = new StopPoint[]
            {
                new StopPoint { RouteId = 0, StopNumber = 1, TimeDelay = 2, Name = "Ivings Dr. (South Port Elgin)", Latitude = 44.426758, Longitude = -81.398684, Address = "Ivings Dr. & Goderich St.", AdditionalInfo = "The beginning and end to the Trolley route, at the south end of town." },
                new StopPoint { RouteId = 0, StopNumber = 2, TimeDelay = 2, Name = "Downtown Port Elgin", Latitude = 44.4361560, Longitude = -81.3871670, Address = "Bricker St. & Green St.", AdditionalInfo = "The charming downtown area offers exceptional shopping opportunities with a wide array of stores, specialty shops and dining options." },
                new StopPoint { RouteId = 0, StopNumber = 3, TimeDelay = 3, Name = "Port Elgin Beach", OnRouteTwice = true, Latitude = 44.443149, Longitude = -81.401851, Address = "Harbour St. & Mill St.", AdditionalInfo = "The Port Elgin Main Beach is complete with a harbour, restaurants, playground, paved promenade, and more. It is a delightful destination for a stroll any time of the day." },
                new StopPoint { RouteId = 0, StopNumber = 4, TimeDelay = 2, Name = "Port Elgin Splash Pad", OnRouteTwice = true, Latitude = 44.4484040, Longitude = -81.4043960, Address = "84 McVicar St., Port Elgin", AdditionalInfo = "Saugeen Shores has two fully accessible splash pads (the other in Southampton) suitable for children of all ages and abilities. The splash pads are free of charge for all residents and visitors to use." },
                new StopPoint { RouteId = 0, StopNumber = 5, TimeDelay = 2, Name = "Pegasus Trail", OnRouteTwice = true, Latitude = 44.465917, Longitude = -81.392695, Address = "Miramichi Bay Rd & Pegasus Trail", AdditionalInfo = "This, approximately 6km paved trail connects Port Elgin to Southampton and offers a great experience for bikers, walkers, roller bladers, and joggers along the scenic Lake Huron shoreline." },
                new StopPoint { RouteId = 0, StopNumber = 6, TimeDelay = 2, Name = "Huron St & South St.", OnRouteTwice = true, Latitude = 44.4786610, Longitude = -81.3846360, Address = "Huron St & South St.", AdditionalInfo = "Huron St & South St." },
                new StopPoint { RouteId = 0, StopNumber = 7, TimeDelay = 2, Name = "Huron St & Beach Rd", OnRouteTwice = true, Latitude = 44.4881530, Longitude = -81.3818210, Address = "Huron St & Beach Rd", AdditionalInfo = "The Long Dock Park and Playground is a great place for families and young children down by the dunes, which is also the home of the only fast-food outlet on the beach." },
                new StopPoint { RouteId = 0, StopNumber = 8, TimeDelay = 3, Name = "Huron St & High St", OnRouteTwice = true, Latitude = 44.4970080, Longitude = -81.3749230, Address = "Huron St & High St", AdditionalInfo = "Visit the Southampton Main Beach, stroll the Southampton Beach Sidewalk or the charming downtown area. Beautiful views of Lake Huron and Chantry Island home to an Imperial Lighthouse." },
                new StopPoint { RouteId = 0, StopNumber = 9, TimeDelay = 2, Name = "Bruce County & Cultural Centre", Latitude = 44.496498, Longitude = -81.368072, Address = "33 Victoria St N, Southampton", AdditionalInfo = "The museum features an extensive history of Bruce County and all that it entails. Discover local marine heritage, Bevans General Store, and featured family-friendly exhibits." },
                new StopPoint { RouteId = 0, StopNumber = 10, TimeDelay = 2, Name = "Port Elgin Tourist Camp", Latitude = 44.4381, Longitude = -81.39604, Address = "594 Bruce St., Saugeen Shores", AdditionalInfo = "Centrally Located in Port Elgin. Camp sites available, seasonally, weekly, daily." }
            };

            context.Stops.AddRange(points);
            context.SaveChanges();

            //update route line
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SSTrolley.SSTrolley.src.assets.ssroute.gpx"))
            {
                IEnumerable<XElement> elementPoints = XElement.Load(stream).Descendants().Where(d => d.Name.LocalName == "trkpt");
                int index = 0;
                context.RoutePoints.AddRange(elementPoints.Select(p => new RoutePoint { Longitude = Convert.ToDouble(p.Attribute("lon").Value), Latitude = Convert.ToDouble(p.Attribute("lat").Value), Index = index++, RouteId = 0 }));
            }
            context.SaveChanges();

            //populate info for stops
            foreach (StopPoint stop in context.Stops)
            {
                //flag set in database initialization whether stop point should be included on route twice
                if (stop.OnRouteTwice == true)
                {
                    List<RoutePoint> closestPoints = TrolleyProcessing.ClosestPointsToPoint(context.RoutePoints, stop);
                    if (closestPoints.Count() == 1)
                    {
                        stop.RoutePointId = closestPoints.First().Id;
                    }
                    else if (closestPoints.Count() >= 2)
                    {
                        stop.RoutePointId = closestPoints.First().Id;

                        stop.RoutePointId2 = closestPoints[1].Id;
                    }
                }
                else
                {
                    stop.RoutePointId = TrolleyProcessing.CalculateClosestPoint(context.RoutePoints, stop).Id;
                }
            }
            context.SaveChanges();
        }
        */
    }
}