using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class TrolleyDelay
    {
        public int Id { get; set; }
        public int TrolleyId { get; set; }
        public int StopId { get; set; }
        public TimeSpan DelayStart { get; set; }
        public TimeSpan DelayEnd { get; set; }
    }
}
