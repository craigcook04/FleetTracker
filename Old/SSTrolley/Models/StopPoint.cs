using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class StopPoint : Point
    {
        public int Id { get; set; }
        public int StopNumber { get; set; }
        public int RouteId { get; set; }
        public int RoutePointId { get; set; }
        public double Longitude { get; set; } = -1;
        public double Latitude { get; set; } = -1;
        public double TimeDelay { get; set; } = 0;
        public string Name { get; set; }
        public string Address { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
