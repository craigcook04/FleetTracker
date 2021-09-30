import {Component } from '@angular/core';
import { TrolleyStop } from '../trolley-stop';
import { TrolleyService } from '../trolley.service';
import { FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field'

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, flatMap } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import Chart from 'chart.js';

@Component({
  selector: './app-analytics-component',
  templateUrl: './analytics-component.component.html',
  styleUrls: ['./analytics-component.component.css'],
})
export class AnalyticsComponent {

  constructor(private trolleyService: TrolleyService, private http: HttpClient, private datePipe: DatePipe){}

  startDate = new FormControl(new Date(new Date().getTime() - 6.048e+8));
  endDate = new FormControl(new Date());

  private stopInfoUrl = 'api/admin/stopInfo';

  stopList = new FormControl('All');
  stops: TrolleyStop[] = undefined;

  chart;
  labels: any = [];
  data: any = [];

  ngOnInit(): void {
    this.trolleyService.getStops(1)
    .subscribe(trolleyStops => {
      this.stops = trolleyStops as TrolleyStop []
    })

    this.chart = new Chart('canvas', {
      type: 'line',
      data: {
        labels: this.labels,
        datasets: [
          {
            data: this.data,
            borderColor: '#3cba9f',
            fill: false
          }
        ]
      },
      options: {
        maintainAspectRatio : false,
        responsive: true,
        legend: {
          display: false
        },
        scales: {
          xAxes: [{
            display: true,
            ticks: {
              autoSkip: true,
              maxTicksLimit: 20,
            }
          }],
          yAxes: [{
            display: true,
            scaleLabel: {
              display: true,
              labelString: 'Passengers'
            }
          }]
        }
      }
    })
  }

  getStopInfo(stopName)
  {
    const url = `${this.stopInfoUrl}/${stopName.value}`;
    //console.log(url);

    const whatever = new HttpHeaders ({
      'startDate' : this.transformDate(this.startDate.value),
      'endDate' : this.transformDate(this.endDate.value)
    });

    this.http.get(url, {headers:whatever}).subscribe((res)=>{
      var parsed = JSON.parse(res.toString());
      //console.log(parsed);

      this.removeData(this.chart);

      parsed.forEach(element => {
        this.addData(this.chart, this.datePipe.transform(new Date(element.Timestamp), 'short', "-0600"), element.Passengers);
      });
      
      this.chart.update();
    });
  }

  transformDate(date){
    return this.datePipe.transform(date, 'shortDate');
  }

  addData(chart, label, data) {
    chart.data.labels.push(label);
    chart.data.datasets.forEach((dataset) => {
        dataset.data.push(data);
    });
    chart.update();
  }

  removeData(chart) {  
    ///chart.data.labels.pop();

    chart.data.labels = [];

    chart.data.datasets.data = [];

    chart.update();
  }
}
