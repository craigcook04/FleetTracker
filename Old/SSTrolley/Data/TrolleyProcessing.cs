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

            RoutePoint trolleyPoint = CalculateClosestPoint(routePoints, trolley);

            // Calculate the direction of travel along the route
            int nextIndex;
            int prevIndex;

            if (trolleyPoint.Index == 0)
                nextIndex = prevIndex = 1;
            else if (trolleyPoint.Index == routePoints.Count - 1)
                nextIndex = prevIndex = trolleyPoint.Index - 1;
            else
            {
                nextIndex = trolleyPoint.Index + 1;
                prevIndex = trolleyPoint.Index - 1;
            }

            double nextHead = CalculateHeadingFromPoints(trolley, routePoints[nextIndex]);
            double prevHead = CalculateHeadingFromPoints(trolley, routePoints[prevIndex]);
            double trolleyHead = trolley.Heading;

            double nextDif = Math.PI - Math.Abs((Math.Abs(nextHead - trolleyHead) % (Math.PI * 2)) - Math.PI);
            double prevDif = Math.PI - Math.Abs((Math.Abs(prevHead - trolleyHead) % (Math.PI * 2)) - Math.PI);

            double distance = 0;
            double timeOffset = 0;
            
            {
                StopPoint curStop = stopPoints.FirstOrDefault(s => s.RoutePointId == routePoints[trolleyPoint.Index].Id);
                if (curStop != null)
                    stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = 0, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = timeOffset, DepartTime = GetDepartTimeForStop(now.AddMinutes(timeOffset), estTimeZone, curStop, delays) });
            }

            if (nextDif < prevDif)
            { // Direction of travel is towards higher indicies
                for (int i = trolleyPoint.Index + 1; i < routePoints.Count; i++)
                { // Run from current position to end
                    distance += HaversineDistanceFromPoints(routePoints[i - 1], routePoints[i]);

                    StopPoint curStop = stopPoints.FirstOrDefault(s => s.RoutePointId == routePoints[i].Id);
                    if (curStop != null)
                    {
                        double curTime = ((distance / 1000) / trolley.AverageSpeed * 60) + timeOffset;
                        stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = distance, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = curTime, DepartTime = GetDepartTimeForStop(now.AddMinutes(curTime), estTimeZone, curStop, delays) });
                        timeOffset += curStop.TimeDelay;
                    }
                }
                distance *= 2;
                timeOffset *= 2;
                for (int i = trolleyPoint.Index - 1; i >= 0; i--)
                { // Run from current position to other end for the return journey
                    distance += HaversineDistanceFromPoints(routePoints[i + 1], routePoints[i]);
                    StopPoint curStop = stopPoints.FirstOrDefault(s => s.RoutePointId == routePoints[i].Id);
                    if (curStop != null)
                    {
                        double curTime = ((distance / 1000) / trolley.AverageSpeed * 60) + timeOffset;
                        stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = distance, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = curTime, DepartTime = GetDepartTimeForStop(now.AddMinutes(curTime), estTimeZone, curStop, delays) });
                        timeOffset += curStop.TimeDelay;
                    }
                }
            }
            else
            { // Direction of travel is towards lower indicies
                for (int i = trolleyPoint.Index - 1; i >= 0; i--)
                { // Run from current position to end
                    distance += HaversineDistanceFromPoints(routePoints[i + 1], routePoints[i]);
                    StopPoint curStop = stopPoints.FirstOrDefault(s => s.RoutePointId == routePoints[i].Id);
                    if (curStop != null)
                    {
                        double curTime = ((distance / 1000) / trolley.AverageSpeed * 60) + timeOffset;
                        stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = distance, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = curTime, DepartTime = GetDepartTimeForStop(now.AddMinutes(curTime), estTimeZone, curStop, delays) });
                        timeOffset += curStop.TimeDelay;
                    }
                }
                distance *= 2;
                timeOffset *= 2;
                for (int i = trolleyPoint.Index + 1; i < routePoints.Count; i++)
                { // Run from current position to other end for the return journey
                    distance += HaversineDistanceFromPoints(routePoints[i - 1], routePoints[i]);
                    StopPoint curStop = stopPoints.FirstOrDefault(s => s.RoutePointId == routePoints[i].Id);
                    if (curStop != null)
                    {
                        double curTime = ((distance / 1000) / trolley.AverageSpeed * 60) + timeOffset;
                        stopTrolleyInfo.Add(new StopPointTrolleyInfo { DistanceFromTrolley = distance, StopId = curStop.Id, TrolleyId = trolley.Id, TimeFromTrolley = curTime, DepartTime = GetDepartTimeForStop(now.AddMinutes(curTime), estTimeZone, curStop, delays) });
                        timeOffset += curStop.TimeDelay;
                    }
                }
            }
            
            {
                StopPoint closestStop = CalculateClosestPoint(stopPoints, trolley);
                StopPointTrolleyInfo lastInfo = stopTrolleyInfo.Last();
                if (closestStop.Id == lastInfo.StopId)
                {
                    DateTime time = GetDepartTimeForStop(now, estTimeZone, closestStop, delays);
                    if (time != DateTime.MinValue)
                    {
                        lastInfo.DistanceFromTrolley = 0;
                        lastInfo.TimeFromTrolley = 0;
                        lastInfo.DepartTime = time;
                    }
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
