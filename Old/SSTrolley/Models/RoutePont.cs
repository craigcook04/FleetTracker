﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class RoutePoint : Point
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int Index { get; set; }
    }
}
