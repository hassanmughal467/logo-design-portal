import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-client-detail',
  template: '<p>Client detail page - Coming soon</p>'
})
export class ClientDetailComponent implements OnInit {
  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    const clientId = this.route.snapshot.paramMap.get('id');
    console.log('Client ID:', clientId);
  }
}
