#Introduction
This is the SSTrolley Android App intended to be a companion audio guide played on the trolley from an android device. It relies on geofence triggers to play associated audio files when leaving the designated stops on the trolley route.

#Locations
The audio files for each stop are located in *app\src\res\raw*

The route is hardcoded in the file *app\src\java\com\hatch\sstrolleyaudio\StopLocations.kt*
This is where the audio files are associated with the stops. Each stop can have up to 2 audio files associated with it. The logic for deciding which file is played is in *app\src\java\com\hatch\sstrolleyaudio\GeofenceTransitionsIntentService.kt* and in its current iteration is based on the heading of the trolley.

#Deployment
The app can be compiled in to a .APK by using the Build->Build Bundle(s) / APK (s) -> Build Apk(s) menu. The file produced can then be installed locally on the Android device.
