import { Component, ViewChild, OnInit, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {}

  embed = true;

  ngOnInit(): void {
    this.activatedRoute.queryParams.subscribe(params => {
      if (params['embed'] === 'true')
        this.embed = true;
      else
        this.embed = false;
    });
  }
}
