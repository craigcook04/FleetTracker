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
    }
}