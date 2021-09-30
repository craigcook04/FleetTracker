using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public interface Point
    {
        double Longitude { get; set; }
        double Latitude { get; set; }
    }
}
