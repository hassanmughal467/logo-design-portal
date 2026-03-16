import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-kpi-card',
  templateUrl: './kpi-card.component.html',
  styleUrls: ['./kpi-card.component.scss']
})
export class KpiCardComponent {
  @Input() label = '';
  @Input() value: string | number = '';
  @Input() icon = 'pi pi-chart-bar';
  @Input() variant: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral' | string = 'primary';
  @Input() clickable = false;
  @Output() cardClick = new EventEmitter<void>();
}
