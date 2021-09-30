using SSTrolley.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSTrolley.Data
{
    public static class DbInitializer
    {
        public static void Initialize(TrolleyContext context)
        {
            context.Database.EnsureCreated();

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                if (!context.Trolleys.Any() && !context.Stops.Any() && !context.RoutePoints.Any())
                {
                    byte[] arr = new byte[16];
                    for (byte i = 0; i < arr.Length; i++)
                        arr[i] = i;

                    byte[] salt;
                    salt = Trolley.GenerateSalt(rng);
                    byte[] hash = Trolley.GenerateHash(arr, salt);

                    Trolley trolley = new Trolley { Id = 1, RouteId = 0, AverageSpeed = 35, Hash = hash, Salt = salt, Longitude = -81.379172, Latitude = 44.491690, LastLongitude = -81.379018, LastLatitude = 44.491937, MaxPassengers = 30, TotalDistance = 15840000, TotalPassengers = 17000 };
                    trolley.Heading = TrolleyProcessing.CalculateHeadingFromLonLat(trolley.LastLongitude, trolley.LastLatitude, trolley.Longitude, trolley.Latitude);
                    context.Trolleys.Add(trolley);

                    StopPoint[] points = new StopPoint[]
                    {
                        new StopPoint { RouteId = 0, StopNumber = 1, TimeDelay = 2, Name = "Ivings Dr. (South Port Elgin)", Latitude = 44.426758, Longitude = -81.398684, Address = "Ivings Dr. & Goderich St.", AdditionalInfo = "The beginning and end to the Trolley route, at the south end of town." },
                        new StopPoint { RouteId = 0, StopNumber = 2, TimeDelay = 2, Name = "Downtown Port Elgin", Latitude = 44.4361560, Longitude = -81.3871670, Address = "Bricker St. & Green St.", AdditionalInfo = "The charming downtown area offers exceptional shopping opportunities with a wide array of stores, specialty shops and dining options." },
                        new StopPoint { RouteId = 0, StopNumber = 3, TimeDelay = 3, Name = "Port Elgin Beach", OnRouteTwice = true, Latitude = 44.443149, Longitude = -81.401851, Address = "Harbour St. & Mill St.", AdditionalInfo = "The Port Elgin Main Beach is complete with a harbour, restaurants, playground, paved promenade, and more. It is a delightful destination for a stroll any time of the day." },
                        new StopPoint { RouteId = 0, StopNumber = 4, TimeDelay = 2, Name = "Port Elgin Splash Pad", OnRouteTwice = true, Latitude = 44.4484040, Longitude = -81.4043960, Address = "84 McVicar St., Port Elgin", AdditionalInfo = "Saugeen Shores has two fully accessible splash pads (the other in Southampton) suitable for children of all ages and abilities. The splash pads are free of charge for all residents and visitors to use." },
                        new StopPoint { RouteId = 0, StopNumber = 5, TimeDelay = 2, Name = "Pegasus Trail", OnRouteTwice = true, Latitude = 44.465917, Longitude = -81.392695, Address = "Miramichi Bay Rd & Pegasus Trail", AdditionalInfo = "This, approximately 6km paved trail connects Port Elgin to Southampton and offers a great experience for bikers, walkers, roller bladers, and joggers along the scenic Lake Huron shoreline." },
                        new StopPoint { RouteId = 0, StopNumber = 6, TimeDelay = 2, Name = "Huron St & South St.", OnRouteTwice = true, Latitude = 44.4786610, Longitude = -81.3846360, Address = "Huron St & South St.", AdditionalInfo = "Huron St & South St." },
                        new StopPoint { RouteId = 0, StopNumber = 7, TimeDelay = 2, Name = "Huron St & Beach Rd", OnRouteTwice = true, Latitude = 44.4881530, Longitude = -81.3818210, Address = "Huron St & Beach Rd", AdditionalInfo = "The Long Dock Park and Playground is a great place for families and young children down by the dunes, which is also the home of the only fast-food outlet on the beach." },
                        new StopPoint { RouteId = 0, StopNumber = 8, TimeDelay = 3, Name = "Huron St & High St", OnRouteTwice = true, Latitude = 44.4970080, Longitude = -81.3749230, Address = "Huron St & High St", AdditionalInfo = "Visit the Southampton Main Beach, stroll the Southampton Beach Sidewalk or the charming downtown area. Beautiful views of Lake Huron and Chantry Island home to an Imperial Lighthouse." },
                        new StopPoint { RouteId = 0, StopNumber = 9, TimeDelay = 2, Name = "Bruce County & Cultural Centre", Latitude = 44.496498, Longitude = -81.368072, Address = "33 Victoria St N, Southampton", AdditionalInfo = "The museum features an extensive history of Bruce County and all that it entails. Discover local marine heritage, Bevans General Store, and featured family-friendly exhibits." },
                        new StopPoint { RouteId = 0, StopNumber = 10, TimeDelay = 2, Name = "Port Elgin Tourist Camp", Latitude = 44.4381, Longitude = -81.39604, Address = "594 Bruce St., Saugeen Shores", AdditionalInfo = "Centrally Located in Port Elgin. Camp sites available, seasonally, weekly, daily." }
                    };

                    context.Stops.AddRange(points);
                    context.SaveChanges();

                    context.TrolleyDelays.AddRange(new TrolleyDelay[]
                    {
                        new TrolleyDelay { TrolleyId = trolley.Id, StopId = points[0].Id, DelayStart = new TimeSpan(18, 00, 00), DelayEnd = new TimeSpan(10, 00, 00) },
                        new TrolleyDelay { TrolleyId = trolley.Id, StopId = points[7].Id, DelayStart = new TimeSpan(13, 15, 00), DelayEnd = new TimeSpan(14, 00, 00) }
                    });
                    context.SaveChanges();

                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SSTrolley.SSTrolley.src.assets.ssroute.gpx"))
                    {
                        IEnumerable<XElement> elementPoints = XElement.Load(stream).Descendants().Where(d => d.Name.LocalName == "trkpt");
                        int index = 0;
                        context.RoutePoints.AddRange(elementPoints.Select(p => new RoutePoint { Longitude = Convert.ToDouble(p.Attribute("lon").Value), Latitude = Convert.ToDouble(p.Attribute("lat").Value), Index = index++, RouteId = 0 }));
                    }
                    context.SaveChanges();

                    foreach (StopPoint stop in context.Stops)
                    {
                        //flag set in database initialization whether stop point should be included on route twice
                        if(stop.OnRouteTwice == true)
                        {
                            List<RoutePoint> closestPoints = TrolleyProcessing.ClosestPointsToPoint(context.RoutePoints, stop);
                            if (closestPoints.Count() == 1)
                            {
                                stop.RoutePointId = closestPoints.First().Id;
                            }
                            else if (closestPoints.Count() >= 2)
                            {
                                stop.RoutePointId = closestPoints.First().Id;

                                stop.RoutePointId2 = closestPoints[1].Id;
                            }
                        }
                        else
                        {
                            stop.RoutePointId = TrolleyProcessing.CalculateClosestPoint(context.RoutePoints, stop).Id;
                        }
                    }
                    context.SaveChanges();

                    TrolleyProcessing.CalculateRoutePointInfo(trolley, context);
                }

                if (!context.Places.Any())
                {
                    //context.Places.Add(new Place { Latitude = 44.436491, Longitude = -81.388971, Name = "Farmers Market", Icon = "place.png", Address = "626 Goderich St, Port Elgin, ON", AdditionalInfo = "Located right in the middle of Port Elgin's downtown, you will find an amazing selection of fresh local produce, unique local artisans and yummy eats. The market is on every Wednesday between 9am and 2pm throughout the Summer." });
                    context.SaveChanges();
                }

                if (!context.Users.Any())
                {
                    context.Users.Add(new User("trolleyadmin", "ssTrolley1234!", rng));
                    context.SaveChanges();
                }
            }
        }
    }
}
