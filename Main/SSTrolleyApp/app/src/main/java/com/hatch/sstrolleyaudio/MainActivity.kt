package com.hatch.sstrolleyaudio

import android.app.Application
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.util.Log
import androidx.core.content.ContextCompat
import com.google.android.gms.location.Geofence
import com.google.android.gms.location.GeofencingClient
import com.google.android.gms.location.LocationServices

import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.GoogleMap
import com.google.android.gms.maps.OnMapReadyCallback
import com.google.android.gms.maps.SupportMapFragment
import com.google.android.gms.maps.model.LatLng
import com.google.android.gms.maps.model.MarkerOptions
import android.Manifest
import android.app.NotificationChannel
import android.app.NotificationManager
import android.graphics.Color
import android.location.Criteria
import android.location.Location
import android.location.LocationListener
import android.location.LocationManager
import android.net.sip.SipSession
import android.widget.Toast
import androidx.core.app.ActivityCompat
import com.google.android.gms.location.GeofencingRequest
import com.google.android.gms.maps.model.CircleOptions

import kotlinx.android.synthetic.main.activity_maps.*
class MainActivity : AppCompatActivity(), OnMapReadyCallback {

    private lateinit var mMap: GoogleMap

    private lateinit var locationManager: LocationManager

    lateinit var geofencingClient: GeofencingClient

    var isMapReady = false

    //Constants
    val MY_LOCATION_REQUEST_CODE = 1400

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_maps)

        val mapFragment = supportFragmentManager
            .findFragmentById(R.id.map) as SupportMapFragment
        mapFragment.getMapAsync(this)

        locationManager = getSystemService(Context.LOCATION_SERVICE) as LocationManager

        //ask for location permissions
        if(ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED){
            ActivityCompat.requestPermissions(
                this,
                arrayOf(Manifest.permission.ACCESS_FINE_LOCATION),
                MY_LOCATION_REQUEST_CODE
            )
        }
        else{
            permissionsReady()
        }
    }

    override fun onRequestPermissionsResult(requestCode: Int, permissions: Array<out String>, grantResults: IntArray) {
        //super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == MY_LOCATION_REQUEST_CODE)
        {
            permissionsReady()
        }
    }

    fun permissionsReady()
    {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED) {

            geofencingClient = GeofencingClient(this)
            var newGeofence: Geofence
            for (stop in stopPoints) {

                newGeofence = newGeofencefromStop(stop)
                geofencingClient.addGeofences(getGeofencingRequest(newGeofence), geofencePendingIntent)
                Log.d(stop.stopName, stop.toString())
            }
        }
        else{
            Log.e("LOCATIONPROBLEM", "STOPS NOT CREATED")
            Toast.makeText(this, "Stops not created", Toast.LENGTH_LONG).show()
        }

        if(isMapReady){
            mMap.isMyLocationEnabled = true;
        }
    }

    /**
     * Manipulates the map once available.
     * This callback is triggered when the map is ready to be used.
     * This is where we can add markers or lines, add listeners or move the camera.
     * If Google Play services is not installed on the device, the user will be prompted to install
     * it inside the SupportMapFragment. This method will only be triggered once the user has
     * installed Google Play services and returned to the app.
     */
    override fun onMapReady(googleMap: GoogleMap) {
        mMap = googleMap
        isMapReady = true

        for(stop in stopPoints){
            mMap.addMarker(MarkerOptions().position(LatLng(stop.latitude, stop.longitude)).title(stop.stopName))

            mMap.addCircle(CircleOptions()
                .center(LatLng(stop.latitude, stop.longitude))
                .radius(GEOFENCE_RADIUS.toDouble())
                .fillColor(0x40ff0000)
                .strokeColor(Color.BLACK)
                .strokeWidth(2.0f)
            )
        }
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED) {
            mMap.isMyLocationEnabled = true
        }

        mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(LatLng(stopPoints[0].latitude, stopPoints[0].longitude), 13.0f))
    }

    //Geofencing
    private val geofencePendingIntent: PendingIntent by lazy {
        val intent = Intent(this, GeofenceTransitionsIntentService::class.java)
        PendingIntent.getService(this, 0, intent, PendingIntent.FLAG_UPDATE_CURRENT)
    }

    private fun newGeofencefromStop(stop: TrolleyStop): Geofence{
        return Geofence.Builder()
            .setRequestId(stop.stopNumber.toString())
            .setCircularRegion(
                stop.latitude,
                stop.longitude,
                GEOFENCE_RADIUS
            )

            .setExpirationDuration(Geofence.NEVER_EXPIRE)
            .setTransitionTypes(Geofence.GEOFENCE_TRANSITION_EXIT or Geofence.GEOFENCE_TRANSITION_ENTER)
            .build()
    }

    private fun getGeofencingRequest(geofence: Geofence): GeofencingRequest {
        return GeofencingRequest.Builder().apply {
            setInitialTrigger(GeofencingRequest.INITIAL_TRIGGER_ENTER)
            addGeofences(listOf(geofence))
        }.build()
    }
}