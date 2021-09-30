SSTrolley
=========
Introduction
------------
This web application is designed to present the GPS location and other real time details of the Saugeen Shores Trolley service.   
Developed using ASP.NET Core (C#).   
Donated by [Hatch](https://www.hatch.com/)
 
Requirements
------------
Trolley Controller requires the following software:
[npm](https://www.npmjs.com/get-npm)
[.NET Core SDK 2.0 or later](https://www.microsoft.com/net/download/all)

[Visual Studio 2017 version 15.7 or later with the ASP.NET Core and web development workload](https://www.visualstudio.com/downloads/)   
OR  
[Visual Studio Code](https://code.visualstudio.com/download)  
[C# for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)  

Instructions getting started with .NET Core can be found [here](https://www.microsoft.com/net/learn/get-started/)

Build
-----
The .NET Core SDK is used to build the web application (as detailed above) and the two scripts (SSTrolley/install.bat & SSTrolley/build.bat) are used to insure that all the required node packages are installed and the Angular application is built.

Setup
-----
Kestrel (the ASP.NET Core web server) is capable of serving this applicaiton to the user and handling all API requests, however, the page itself can be served by another application (such as Apache) with API requests being forwarded to Kestrel.
More details can be found [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/?view=aspnetcore-2.1&tabs=aspnetcore2x).

To run on the production server, modify `<base href="/">` in `index.html` to `<base href="/map/">`. It must be the former to work in testing and the angular project must be re built after changing.
In addition, two lines near the end of `ConfigureServices` in `Startup.cs` starting with `var connection = Confi...` must be commented out and the line following those two must be put back in.

The connection string must be stored in the user's local secrets file which can be accessed by right clicking the project (not the solution) and selecting *Manage User Secrets*. It must formatted as followes.

<pre>
{
    ...
    "ConnectionStrings": {
        "trolleyConnectionString": "<i>Connection String</i>"
    }
    ...
}
</pre>

Changing/Creating a Route
-------------------------
The Route stops are hardcoded in the /Data/DbInitializer.cs file and the route is contained in /SSTrolley/src/assets/ssroute.gpx. This route is a standard file type and for it to work with the routing logic it must be a loop (first and last points in the route are the same).
The OnRouteTwice flag must be set in order for the app to correctly recognize that the stop should be reached twice in a single iteration of the route. This functions best if the point (or both points) on the route and the stop location have the exact same coordinates.

Connection String
-----------------


    Server=tcp:sstrolley.database.windows.net,1433;Initial Catalog=SSTrolleyDemo;Persist Security Info=False;User ID=trolleyadmin;Password=Trolley1234!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;

Deploying To Production
-----------------------
To access the production server contact Owen at Websmart (https://www.websmart.ca/).
To connect to the server, an outside connection from the Hatch network is required as the connection is blocked on the Hatch network.
The production server has a system process 'SSTrolley' which can be started with `systemctl start sstrolley` and stopped with `systemctl stop sstrolley`. Stop before changing files then start again afterwards. 
All credentials required *are* in trolleypass.txt if needed.

API Endpoints
-------------
All API Endpoints return/accept [JSON](https://www.w3schools.com/js/js_json_intro.asp) encoded data
###TrolleyController [api/trolley]
**GetIds() -> IEnumerable<int> [GET: api/trolley]**
Returns a list of all available trolley IDs

**GetTrolley(int id) -> Trolley [GET: api/trolley/:id]**   
Returns detailed information about a specific trolley or error 404 if a trolley matching **:id** could not be found

**GetALL() -> IEnumerable<Trolley> [GET: /api/trolley/all]**   
Returns detailed information about all available trolleys

**GetStops(int id) -> IEnumerable<StopPoint> [GET: /api/trolley/:id/stops]**   
Returns all the trolley specific stop info for all the stops on the trolley's current route or error 404 if a trolley matching **:id** could not be found

**GetStopsInfo(int id) -> IEnumerable<StopPoint> [GET: /api/trolley/:id/stopsinfo]**   
Returns all the stops on the trolley's current route or error 404 if a trolley matching **:id** could not be found

**GetService(int id) -> bool [GET: /api/trolley/:id/service]**   
Return a boolean representing the service state of the trolley or error 404 if a trolley matching **:id** could not be found

**GetServiceString(int id) -> string [GET: /api/trolley/:id/servicestring]**   
Return a string to be displayed if the trolley is out of service or error 404 if a trolley matching **:id** could not be found

**Post([FromBody]TrolleyLogin value) [POST: /api/trolley] <Requires Login ID>**   
Updates the current position and heading of the trolley

**SetService(int id, [FromBody]bool inService) [PUT: /api/trolley/:id/service] <Requires Jwt Authorization>**   
Updates the current service status of the trolley

**SetService(int id, [FromBody]string inService) [PUT: /api/trolley/:id/servicestring] <Requires Jwt Authorization>**   
Updates the current service string of the trolley

###RouteController [api/route]
**GetIds() -> IEnumerable<int> [GET: api/route]**   
Returns a list of all available route IDs

**GetRoute(int id) -> IEnumerable<StopPoint> [GET: api/route/:id]**   
Returns a list of all avaliable stop points for route **:id** or error 404 if a route matching **:id** could not be found

**GetALL() -> IEnumerable<StopPoint> [GET: /api/route/all]**   
Returns a list of all avaliable stop points

**GetTrolleys(int id) -> IEnumerable<int> [GET: /api/route/:id/trolleys]**   
Returns a list of all trolley IDs that follow route **:id**

**GetTrolleysFull(int id) -> IEnumerable<int> [GET: /api/route/:id/trolleys/full]**   
Returns a detailed list of all trolleys that follow route **:id**

###PlaceController [api/place]
**GetIds() -> IEnumerable<int> [GET: api/place]**
Returns a list of all available place IDs

**GetPlace(int id) -> Place [GET: api/place/:id]**
Returns the place represented by **:id** or error 404 if a place matching **:id** could not be found

**GetALL() -> IEnumerable<Place> [GET: /api/place/all]**   
Returns a list of all avaliable places

###AdController [api/ad]
**Random() -> IEnumerable<string> [GET: api/ad/random]**   
Returns name of two random ads

**List() -> IEnumerable<AdInfo> [GET: api/ad/list] <Requires Jwt Authorization>**   
Return the name and link url of all the avaliable ads

**Update([FromBody]AdInfo adInfo) [PUT: api/ad/update] <Requires Jwt Authorization>**   
Updates the link url for the provided ad

**Delete([FromBody]AdInfo adInfo) [POST: api/ad/delete] <Requires Jwt Authorization>**   
Removes the provided ad

**Upload(IFormFileCollection fileUpload) [PUT: api/ad/upload] <Requires Jwt Authorization>**   
Creates a new ad from the provided image(s)

###AdminController [api/admin]
**Check() [GET: api/admin/check] <Requires Jwt Authorization>**   
Returns status code 200 if the user provides a valid Jwt authorization tolken

**Login([FromBody]UserLogin userLogin) -> string [POST: api/admin/login]**   
Returns a Jwt authorization tolken valid for 30 minutes if the provided username and password match a valid user
