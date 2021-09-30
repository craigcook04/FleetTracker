using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class Place
    {
        public int Id { get; set; }
        public double Longitude { get; set; } = -1;
        public double Latitude { get; set; } = -1;
        public string Name { get; set; }
        public string Address { get; set; }
        public string AdditionalInfo { get; set; }
        public string Icon { get; set; }
    }
}
