using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class PassengerPoint
    {
        public int Id { get; set; }
        public short Arrived { get; set; }
        public short Departed { get; set; }
        public short Passengers { get; set; }
        public int PointId { get; set; }
    }
}
