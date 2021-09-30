using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class StopPointTrolleyInfo
    {
        public int Id { get; set; }
        public int StopId { get; set; }
        public int TrolleyId { get; set; }
        public double DistanceFromTrolley { get; set; }
        public double TimeFromTrolley { get; set; }
        public DateTime DepartTime { get; set; } = DateTime.MinValue;
    }
}
