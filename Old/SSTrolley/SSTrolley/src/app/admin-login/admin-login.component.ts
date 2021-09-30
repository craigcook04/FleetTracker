import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AdminService } from '../admin.service';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButton } from '@angular/material';
import {ViewEncapsulation} from '@angular/core';

@Component({
  selector: 'app-admin-login',
  templateUrl: './admin-login.component.html',
  styleUrls: ['./admin-login.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class AdminLoginComponent implements OnInit {
  @ViewChild('username') username: ElementRef;
  @ViewChild('password') password: ElementRef;
  @ViewChild('submitButton') submitButton: MatButton;

  constructor(private adminService: AdminService, private router: Router) { }

  ngOnInit() {
  }

  login() {
    this.submitButton.disabled = true;
    this.adminService.login(this.username.nativeElement.value, this.password.nativeElement.value).subscribe(auth => {
      if (auth) {
        setTimeout(() => {
          this.submitButton.disabled = false;
          this.router.navigate(['admin']);
        }, 100);
      }
      else {
        this.submitButton.disabled = false;
        alert('Login Failed');
      }
    });
  }
}
