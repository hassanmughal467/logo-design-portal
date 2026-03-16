import { Component } from '@angular/core';

@Component({
  selector: 'app-client-intelligence',
  templateUrl: './client-intelligence.component.html',
  styleUrls: ['./client-intelligence.component.scss']
})
export class ClientIntelligenceComponent {
  activeTab: 'overview' | 'churn' = 'overview';
}
