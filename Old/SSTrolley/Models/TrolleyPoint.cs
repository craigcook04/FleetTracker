using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class TrolleyPoint : Point
    {
        public int Id { get; set; }
        public int TrolleyId { get; set; }
        public double Longitude { get; set; } = -1;
        public double Latitude { get; set; } = -1;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
