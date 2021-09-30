using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class TrolleyContext : DbContext
    {
        public TrolleyContext(DbContextOptions<TrolleyContext> options) : base(options) { }

        public DbSet<Trolley> Trolleys { get; set; }
        public DbSet<TrolleyPoint> Locations { get; set; }
        public DbSet<StopPoint> Stops { get; set; }
        public DbSet<StopPointTrolleyInfo> StopTrolleyInfo { get; set; }
        public DbSet<PassengerPoint> Passengers { get; set; }
        public DbSet<RoutePoint> RoutePoints { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<TrolleyDelay> TrolleyDelays { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
