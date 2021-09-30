using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class TrolleyLogin : Trolley
    {
        public byte[] Login { get; set; }
        public PassengerPoint Passengers { get; set; }
        public Int32 LonFixed { get; set; }
        public Int32 LatFixed { get; set; }
    }
}
