package com.hatch.sstrolleyaudio

class TrolleyStop(latitude: Double, longitude: Double, stopNumber: Int, stopName: String, firstAudio: String?, secAudio: String?)
{
    var latitude = latitude
    var longitude = longitude
    var stopNumber = stopNumber
    var stopName = stopName
    var firstAudio: String? = firstAudio
    var secAudio: String? = secAudio
}

val stopPoints = listOf<TrolleyStop>(
    TrolleyStop(44.426758, -81.398684, 1, "Ivings Dr. (South Port Elgin)", "a1stop2", null),
    TrolleyStop(44.4361560, -81.3871670, 2, "Downtown Port Elgin", "a2stop3", null),
    TrolleyStop(44.443149, -81.401851,3, "Port Elgin Beach", "a3stop4", "a15stop16"),
    TrolleyStop(44.4484040, -81.4043960, 4, "Port Elgin Splash Pad", "a4stop5", "a14stop15"),
    TrolleyStop(44.465917, -81.392695, 5, "Pegasus Trail", "a5stop6", "a13stop14"),
    TrolleyStop(44.4786610, -81.3846360, 6, "Huron St & South St.", "a6stop7", "a12stop13"),
    TrolleyStop(44.4881530, -81.3818210, 7, "Huron St & Beach Rd", "a7stop8", "a11stop12"),
    TrolleyStop(44.4970080, -81.3749230, 8, "Huron St & High St", "a8stop9", "a10stop11"),
    TrolleyStop(44.496498, -81.368072, 9 , "Bruce County & Cultural Centre", "a9stop10", null),
    TrolleyStop(44.4381, -81.39604, 10, "Port Elgin Tourist Camp", "a16stop17", null)
)

val GEOFENCE_RADIUS = 90.0f