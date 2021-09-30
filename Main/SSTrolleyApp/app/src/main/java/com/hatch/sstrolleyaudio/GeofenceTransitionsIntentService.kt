package com.hatch.sstrolleyaudio

import android.app.IntentService
import android.content.Intent
import android.util.Log
import android.location.Location
import android.media.MediaPlayer
import android.os.Handler
import android.os.Looper
import android.widget.Toast
import com.google.android.gms.location.Geofence
import com.google.android.gms.location.GeofencingEvent

import kotlinx.android.synthetic.main.activity_maps.*

class GeofenceTransitionsIntentService: IntentService("GeoTrIntentService")
{
    companion object {
        private const val LOG_TAG = "GeoTrIntentService"

        private var mAudioPlayer: MediaPlayer? = null

    }

    override fun onHandleIntent(intent: Intent?) {


        val geofencingEvent = GeofencingEvent.fromIntent(intent)
        if(geofencingEvent.hasError()){
            val errorMessage = GeofenceErrorMessages.getErrorString(this,
                geofencingEvent.errorCode)
            Log.e("GEOFENCEERROR", errorMessage)
            return
        }

        handleEvent(geofencingEvent)
    }

    private fun handleEvent(event: GeofencingEvent) {

        val handle = Handler(Looper.getMainLooper())

        if (event.geofenceTransition == Geofence.GEOFENCE_TRANSITION_ENTER) {
            handle.post {
                Toast.makeText(applicationContext, "GEOFENCE ENTERED", Toast.LENGTH_LONG).show()
            }
        }
        else if(event.geofenceTransition == Geofence.GEOFENCE_TRANSITION_EXIT) {

            handle.post {
                Toast.makeText(applicationContext, "GEOFENCE EXITED", Toast.LENGTH_LONG).show()
            }

            var trigGeofence: Geofence= event.triggeringGeofences.first()

            //get request ID
            var intString: String = trigGeofence.requestId
            var stopIndex: Int = intString.toInt()

            var stopLocation = Location(event.triggeringLocation)
            stopLocation.longitude = stopPoints[stopIndex-1].longitude
            stopLocation.latitude = stopPoints[stopIndex-1].latitude
            var bearingFromTrig = stopLocation.bearingTo(event.triggeringLocation)



            when(stopIndex)
            {
                6, 7 -> if(bearingFromTrig in -90.0..90.0){
                    playMp3(stopPoints[stopIndex-1].firstAudio!!)
                }
                else{
                    playMp3(stopPoints[stopIndex-1].secAudio!!)
                }

                3, 4 -> if(bearingFromTrig in -135.0..45.0){
                    playMp3(stopPoints[stopIndex-1].firstAudio!!)
                }
                else{
                    playMp3(stopPoints[stopIndex-1].secAudio!!)
                }

                5, 8 -> if(bearingFromTrig in -45.0..135.0){
                    playMp3(stopPoints[stopIndex-1].firstAudio!!)
                }
                else{
                    playMp3(stopPoints[stopIndex-1].secAudio!!)
                }
                else -> playMp3(stopPoints[stopIndex-1].firstAudio!!)
            }
        }
    }

    fun playMp3(audioFileName: String)
    {
        Thread{
            if (mAudioPlayer != null){
                mAudioPlayer?.stop()
                mAudioPlayer?.reset()
            }
            mAudioPlayer = MediaPlayer.create(applicationContext, applicationContext.resources.getIdentifier(audioFileName, "raw", applicationContext.packageName))
            mAudioPlayer?.start()
        }.start()
    }
}