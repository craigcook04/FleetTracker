import { Component, OnInit, HostListener, ViewChild, ElementRef, AfterViewInit } from '@angular/core';

import Map from 'ol/map';
import View from 'ol/view';
import Overlay from 'ol/overlay';
import TileLayer from 'ol/layer/tile';
import OSM from 'ol/source/osm';
import VectorLayer from 'ol/layer/vector';
import Vector from 'ol/source/vector';
import Feature from 'ol/feature';
import Point from 'ol/geom/point';
import Style from 'ol/style/style';
import Icon from 'ol/style/icon';
import Stroke from 'ol/style/stroke';
import Fill from 'ol/style/fill';
import Circle from 'ol/style/circle';
import RegularShape from 'ol/style/regularshape';
import ObjectEvent from 'ol/object';
import proj from 'ol/proj';
import interaction from 'ol/interaction';
import control from 'ol/control';
import MouseWheelZoom from 'ol/interaction/mousewheelzoom';
import DragPan from 'ol/interaction/dragpan';
import GPX from 'ol/format/gpx';
import MapBrowserEvent from 'ol/mapbrowserevent';
import Geometry from 'ol/geom/geometry';
import MultiLineString from 'ol/geom/multilinestring';
import condition from 'ol/events/condition';
import { Trolley } from 'src/app/trolley';
import { TrolleyStop } from 'src/app/trolley-stop';
import { Observable } from 'rxjs';
import { Point as SSPoint } from '../point';
import { TrolleyProcessingService as TPS } from '../trolley-processing.service';
import { TrolleyService } from '../trolley.service';
import dragpan from 'ol/interaction/dragpan';
import ZoomToExtent from 'ol/control/zoomToExtent';
import Attribution from 'ol/control/attribution';
import Kinetic from 'ol/kinetic';
import { ControlContainer } from '@angular/forms';
import Control from 'ol/control/control';
import { Place } from '../place';
import { ActivatedRoute } from '@angular/router';

enum PopupContent {
  None,
  Stop,
  Place
}

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements OnInit, AfterViewInit {
  @ViewChild('popupElement') popupElement: ElementRef;
  @ViewChild('mapImage') mapImage: ElementRef;
  @ViewChild('resetView') resetView: ElementRef;
  @ViewChild('resetButton') resetButton: ElementRef;

  PopupContent = PopupContent;

  map: Map;
  vectorSource: Vector = new Vector();
  vectorLayer = new VectorLayer({
    source: this.vectorSource,
    updateWhileAnimating: true,
    updateWhileInteracting: true
  });
  placeSource: Vector = new Vector();
  placeLayer = new VectorLayer({
    source: this.placeSource,
    updateWhileAnimating: true,
    updateWhileInteracting: true
  });
  mouseWheelZoom = new MouseWheelZoom({ constrainResolution: true });
  zoomedIn: boolean;
  panMap = false;
  showPopup = PopupContent.None;

  mapCursor = '';

  places: Place[];

  gpxZoomedInStyle = new Style({
    stroke: new Stroke({
      color: 'rgba(0,182,255, 0.6)',
      width: 7
    })
  });

  gpxZoomedOutStyle = new Style({
    stroke: new Stroke({
      color: 'rgba(0,182,255, 0.9)',
      width: 4
    })
  });

  gpxLayer: VectorLayer;

  stopNormalStyle = new Style({
    image: new Icon({
      anchor: [0.5, 1],
      anchorXUnits: 'fraction',
      anchorYUnits: 'fraction',
      src: 'assets/images/marker.png',
      snapToPixel: false
    })
  });

  stopCloseStyle = new Style({
    image: new Icon({
      anchor: [0.5, 1],
      anchorXUnits: 'fraction',
      anchorYUnits: 'fraction',
      src: 'assets/images/marker2.png',
      snapToPixel: false
    })
  });

  userStyle = new Style({
    image: new Circle({
      fill: new Fill({
        color: 'rgba(66, 134, 244,5)'
      }),
      stroke: new Stroke({
        color: 'rgb(255, 255, 255)',
        width: 2
      }),
      radius: 7,
      snapToPixel: false
    })
  });

  trolleyStyle = new Style({
      image: new Icon({
        anchor: [0.5, 0.5],
        anchorXUnits: 'fraction',
        anchorYUnits: 'fraction',
        scale: 0.4,
        rotateWithView: true,
        src: 'assets/images/tri.png',
        snapToPixel: false
      }),
  });

  popup: Overlay;
  selectedStop: TrolleyStop;
  selectedPlace: Place;
  width: number;
  height: number;
  embed = true;

  constructor(private trolleyService: TrolleyService, private elementRef: ElementRef, private activatedRoute: ActivatedRoute) { }

  ngOnInit() {
    this.mouseWheelZoom.setActive(false);
    this.activatedRoute.queryParams.subscribe(params => {
      if (params['embed'] === 'true')
        this.embed = true;
      else
        this.embed = false;
    });
  }

  getPointAtPixel(pixel: [number, number]): Point {
    return this.map.forEachFeatureAtPixel(pixel, t => {
      if (t.getGeometry().getType() as string === 'Point' && t.getGeometry().get('stop')) {
        return t.getGeometry() as Point;
      }
    }, { hitTolerance: 5 });
  }

  getPlaceAtPixel(pixel: [number, number]): Point {
    return this.map.forEachFeatureAtPixel(pixel, t => {
      if (t.getGeometry().getType() as string === 'Point' && t.getGeometry().get('place')) {
        return t.getGeometry() as Point;
      }
    }, { hitTolerance: 5 });
  }

  setPopupDisplay(val: PopupContent) {
    this.showPopup = val;
    setTimeout(() => {
      this.map.updateSize();
      this.map.render();
    }, 10);
  }

  ngAfterViewInit() {
    this.popup = new Overlay({
      element: this.popupElement.nativeElement,
      positioning: 'bottom-center',
      stopEvent: false,
      offset: [0, -20]
    });

    this.gpxLayer = new VectorLayer({
      source: new Vector({
        url: 'assets/ssroute.gpx',
        format: new GPX()
      }),
      style: this.gpxZoomedOutStyle,
      updateWhileAnimating: true,
      updateWhileInteracting: true
    });

    this.gpxLayer.getSource().once('addfeature', e => {
      const multiLineString = ((e as any).feature as Feature).getGeometry() as MultiLineString;

      this.map.getView().fit(multiLineString.getExtent(), { size: this.map.getSize(), padding: [30, 30, 30, 30], constrainResolution: false });

      this.resetButton.nativeElement.addEventListener('click', (pev: PointerEvent) => {
        this.map.getView().rotate(0);
        this.map.getView().fit(multiLineString.getExtent(), { size: this.map.getSize(), padding: [30, 30, 30, 30], constrainResolution: false });
      });
    });

    this.mapImage.nativeElement.addEventListener('touchstart', (e: TouchEvent) => {
      if (e.targetTouches.length > 0) this.panMap = true;
    });

    this.mapImage.nativeElement.addEventListener('touchend', (e: TouchEvent) => {
      if (e.targetTouches.length < 2) this.panMap = false;
    });

    this.mapImage.nativeElement.addEventListener('touchcancel', (e: TouchEvent) => {
      if (e.targetTouches.length < 2) this.panMap = false;
    });

    this.map = new Map({
      target: 'map-image',
      layers: [
        new TileLayer({
          source: new OSM()
        }),
        this.gpxLayer,
        this.placeLayer,
        this.vectorLayer
      ],

      view: new View({
        center: proj.fromLonLat([-81.379605, 44.461040]),
        zoom: 13
      }),
      interactions: interaction.defaults({
        doubleClickZoom: false,
        mouseWheelZoom: false,
        dragPan: false,
      }).extend([this.mouseWheelZoom, new DragPan({
        condition: (e: MapBrowserEvent) => {
          return ((window.innerWidth > 850 || this.panMap) && condition.noModifierKeys(e));
        },
        kinetic: new Kinetic(-0.005, 0.05, 100)
      })]),
      controls: control.defaults().extend([new Control({
        element: this.resetView.nativeElement
      })])
    });

    this.map.addOverlay(this.popup);

    this.map.on('click', (e: MapBrowserEvent) => {
      const geometry = this.getPointAtPixel(e.pixel);
      if (geometry) {
        this.popup.setPosition(geometry.getCoordinates());
        this.selectedStop = geometry.get('stop');
        this.setPopupDisplay(PopupContent.Stop);
        return;
      }

      const geometryPlace = this.getPlaceAtPixel(e.pixel);
      if (geometryPlace) {
        this.popup.setPosition(geometryPlace.getCoordinates());
        this.selectedPlace = geometryPlace.get('place');
        this.setPopupDisplay(PopupContent.Place);
        return;
      }

      this.setPopupDisplay(PopupContent.None);
    });

    this.map.on('pointermove', (e: MapBrowserEvent) => {
      this.mapCursor = (this.getPointAtPixel(e.pixel) || this.getPlaceAtPixel(e.pixel)) ? 'pointer' : '';
    });

    this.map.getView().on('change:resolution', e => {
      if (!this.zoomedIn && this.map.getView().getZoom() >= 16) {
        this.gpxLayer.setStyle(this.gpxZoomedInStyle);
        this.zoomedIn = true;
      }
      else if (this.zoomedIn && this.map.getView().getZoom() <= 15) {
        this.gpxLayer.setStyle(this.gpxZoomedOutStyle);
        this.zoomedIn = false;
      }
    });
  }

  @HostListener('window:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Shift' && !event.repeat) {
      this.mouseWheelZoom.setActive(true);
    }
  }

  @HostListener('window:keyup', ['$event'])
  onKeyUp(event: KeyboardEvent) {
    if (event.key === 'Shift') {
      this.mouseWheelZoom.setActive(false);
    }
  }

  handleData(trolley: Trolley, stops: TrolleyStop[], position: SSPoint) {
    this.vectorSource.clear();
    let i = 0;
    stops.forEach(s => {
      const point = new Point(proj.fromLonLat([s.longitude, s.latitude]));
      point.set('stop', s);
      const f = new Feature({
        geometry: point,
        name: s.name
      });
      f.setStyle((i < 3 && position) ? this.stopCloseStyle : this.stopNormalStyle);
      i++;
      this.vectorSource.addFeature(f);
    });

    if (position) {
      const posFeat = new Feature({
        geometry: new Point(proj.fromLonLat([position.longitude, position.latitude]))
      });

      posFeat.setStyle(this.userStyle);
      this.vectorSource.addFeature(posFeat);

      this.places.forEach(p => {
        if (position)
          p.distanceFromUser = TPS.calculateDistance(p, position);
        else
          p.distanceFromUser = undefined;
      });
    }

    const trolleyFeat = new Feature({
      geometry: new Point(proj.fromLonLat([trolley.longitude, trolley.latitude]))
    });
    this.trolleyStyle.getImage().setRotation(trolley.heading);
    trolleyFeat.setStyle(this.trolleyStyle);
    this.vectorSource.addFeature(trolleyFeat);
  }

  handlePlaces(places: Place[]) {
    this.places = places;
    this.placeSource.clear();
    places.forEach(place => {
      const point = new Point(proj.fromLonLat([place.longitude, place.latitude]));
      point.set('place', place);
      const f = new Feature({
        geometry: point,
        name: place.name
      });
      f.setStyle(new Style({
        image: new Icon({
          anchor: [0.5, 0.5],
          anchorXUnits: 'fraction',
          anchorYUnits: 'fraction',
          scale: 1,
          src: `assets/images/${place.icon}`,
          snapToPixel: false
        })
      }));
      this.placeSource.addFeature(f);
    });
  }
}
