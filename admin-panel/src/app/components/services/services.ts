import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Service } from '../../services/api.service';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './services.html',
  styleUrl: './services.css',
})
export class Services implements OnInit {
  services: Service[] = [];
  loading = true;

  constructor(private apiService: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadServices();
  }

  loadServices() {
    this.loading = true;
    console.log('Loading services...');
    this.apiService.getServices(false).subscribe({
      next: (data) => {
        console.log('Services loaded:', data);
        this.services = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading services:', err);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }
}
