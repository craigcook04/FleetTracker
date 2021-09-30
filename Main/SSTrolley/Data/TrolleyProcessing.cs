using SSTrolley.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSTrolley.Data
{
    public static class TrolleyProcessing
    {
        public static double HaversineDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double R = 6371e3;
            double l1 = lat1 * Math.PI / 180.0;
            double l2 = lat2 * Math.PI / 180.0;
            double dLa = (lat2 - lat1) * Math.PI / 180.0;
            double dLo = (lon2 - lon1) * Math.PI / 180.0;

            double a = Math.Sin(dLa / 2) * Math.Sin(dLa / 2) + Math.Cos(l1) * Math.Cos(l2) * Math.Sin(dLo / 2) * Math.Sin(dLo / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public static double HaversineDistanceFromPoints(Point p1, Point p2)
        {
            return HaversineDistance(p1.Longitude, p1.Latitude, p2.Longitude, p2.Latitude);
        }

        public static double CalculateHeadingFromLonLat(double lon1, double lat1, double lon2, double lat2)
        {
            lon1 *= (Math.PI / 180.0);
            lat1 *= (Math.PI / 180.0);
            lon2 *= (Math.PI / 180.0);
            lat2 *= (Math.PI / 180.0);

            double dLon = (lon2 - lon1);

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

            return Math.Atan2(y, x);
        }

        public static double CalculateHeadingFromPoints(Point p1, Point p2)
        {
            return CalculateHeadingFromLonLat(p1.Longitude, p1.Latitude, p2.Longitude, p2.Latitude);
        }

        public static T CalculateClosestPoint<T>(IEnumerable<T> points, Point point) where T : Point
        {
            T closestPoint = points.First();
            double closestDistance = HaversineDistanceFromPoints(closestPoint, point);
            foreach (T p in points.Skip(1))
            {
                double distance = HaversineDistanceFromPoints(p, point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = p;
                }
            }
            return closestPoint;
        }
 
        public static List<T> ClosestPointsToPoint<T>(IEnumerable<T> points, Point point) where T : Point
        {
            List<T> pointList = new List<T>();

            double smallestDistance = HaversineDistanceFromPoints(point, points.First());
            pointList.Add(points.First());

            foreach (T p in points.Skip(1))
            {
                double distance = HaversineDistanceFromPoints(p, point);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    pointList.Clear();
                    pointList.Add(p);
                }
                else if(distance == smallestDistance)
                {
                    pointList.Add(p);
                }
            }

            return pointList;
        }

        public static void CalculateRoutePointInfo(Trolley trolley, TrolleyContext context)
        {
            List<RoutePoint> routePoints = context.RoutePoints.Where(p => p.RouteId == trolley.RouteId).OrderBy(r => r.Index).ToList();
            List<StopPoint> stopPoints = context.Stops.Where(p => p.RouteId == trolley.RouteId).ToList();
            List<StopPointTrolleyInfo> stopTrolleyInfo = new List<StopPointTrolleyInfo>(stopPoints.Count());
            List<TrolleyDelay> delays = context.TrolleyDelays.Where(d => d.TrolleyId == trolley.Id).ToList();

            TimeZoneInfo estTimeZone;
            try
            {
                estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
            }
            catch (TimeZoneNotFoundException)
            {
                estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }
            DateTime utcNow = DateTime.UtcNow;
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, estTimeZone);

            /*
             * Finds the closest point on the route to the Trolley's current location
             */
            RoutePoint trolleyPoint = null;
            List<RoutePoint> closestPoints = ClosestPointsToPoint(routePoints, trolley);
            if (closestPoints.Count() == 1)
            {
                trolleyPoint = closestPoints.First();
            }
            else if (closestPoints.Count() > 1)
            {
                trolleyPoint = closestPoints.First();

                int nextInd;
                if (trolleyPoint.Index == routePoints.Count - 1){nextInd = 0;}
                else{nextInd = trolleyPoint.Index + 1;}

                double pointHeading = CalculateHeadingFromPoints(trolley, routePoints[nextInd]);
                double headingDiff = Math.PI - Math.Abs((Math.Abs(pointHeading - trolley.Heading) % (Math.PI * 2)) - Math.PI);

                foreach (RoutePoint p in closestPoints.Skip(1))
                {
                    if (p.Index == routePoints.Count - 1) { nextInd = 0; }
                    else { nextInd = p.Index + 1; }

                    double testHeading = CalculateHeadingFromPoints(trolley, routePoints[nextInd]);
                    double testHeadingDiff = Math.PI - Math.Abs((Math.Abs(testHeading - trolley.Heading) % (Math.PI * 2)) - Math.PI);
                    if (testHeadingDiff < headingDiff)
                    {
                        trolleyPoint = p;
                    }
                }
            }
            else
            {//There is no closest points to the trolley
                //This will not happen unless there are no points
            }

            double distance = 0;
            double timeOffset = 0;
            {
                StopPoint curStop = stopPoints.FirstOrDefault(s => (s.RoutePointId == routePoints[trolleyPoint.Index].Id || s.RoutePointId2 == routePoints[trolleyPoint.Index].Id));
                if (curStop != null)
                    stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = 0, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = timeOffset, DepartTime = GetDepartTimeForStop(now.AddMinutes(timeOffset), estTimeZone, curStop, delays) });
            }

            //Starts from trolley's current position and iterates to end of route
            for (int i = trolleyPoint.Index; i < routePoints.Count-1; i++)
            {
                distance += HaversineDistanceFromPoints(routePoints[i], routePoints[i+1]);

                StopPoint curStop = stopPoints.FirstOrDefault(s => (s.RoutePointId == routePoints[i].Id || s.RoutePointId2 == routePoints[i].Id));
                if (curStop != null)
                {
                    double curTime = ((distance / 1000) / trolley.AverageSpeed * 60) + timeOffset;
                    if (!stopTrolleyInfo.Select((s) => s.StopId).Contains(curStop.Id))
                        stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = distance, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = curTime, DepartTime = GetDepartTimeForStop(now.AddMinutes(curTime), estTimeZone, curStop, delays) });
                    timeOffset += curStop.TimeDelay;
                }
            }
            
            //Starts from beginning of route and iterates to trolley's current position
            for (int i = 0; i < trolleyPoint.Index; i++)
            { 
                distance += HaversineDistanceFromPoints(routePoints[i], routePoints[i+1]);

                StopPoint curStop = stopPoints.FirstOrDefault(s => (s.RoutePointId == routePoints[i].Id || s.RoutePointId2 == routePoints[i].Id));
                if (curStop != null)
                {
                    double curTime = ((distance / 1000) / trolley.AverageSpeed * 60) + timeOffset;
                    if (!stopTrolleyInfo.Select((s) => s.StopId).Contains(curStop.Id))
                        stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = distance, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = curTime, DepartTime = GetDepartTimeForStop(now.AddMinutes(curTime), estTimeZone, curStop, delays) });
                    timeOffset += curStop.TimeDelay;
                }
            }

            using (var transaction = context.Database.BeginTransaction())
            {
                context.StopTrolleyInfo.RemoveRange(context.StopTrolleyInfo.Where(r => r.TrolleyId == trolley.Id));
                context.StopTrolleyInfo.AddRange(stopTrolleyInfo);

                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return;
                }
                transaction.Commit();
            }
        }

        static DateTime GetDepartTimeForStop(DateTime now, TimeZoneInfo timeZone, StopPoint stop, IEnumerable<TrolleyDelay> delays)
        {
            TrolleyDelay delay = delays.FirstOrDefault(d => d.StopId == stop.Id);
            if (delay != null)
            {
                TimeSpan curTime = now.TimeOfDay;
                if ((delay.DelayStart <= delay.DelayEnd) ? (curTime >= delay.DelayStart && curTime < delay.DelayEnd) : (curTime >= delay.DelayStart || curTime < delay.DelayEnd))
                {
                    DateTime departTime = now.Date.Add(delay.DelayEnd);
                    if (departTime < now) departTime.AddDays(1);
                    return TimeZoneInfo.ConvertTimeToUtc(departTime, timeZone);
                }
            }

            return DateTime.MinValue;
        }
    }
}
