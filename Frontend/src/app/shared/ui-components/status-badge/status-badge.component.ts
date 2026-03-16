import { Component, Input } from '@angular/core';

export type StatusVariant = 'completed' | 'pending' | 'inProgress' | 'cancelled' | 'warning' | 'info' | 'success' | 'danger';

@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.component.html',
  styleUrls: ['./status-badge.component.scss']
})
export class StatusBadgeComponent {
  @Input() value = '';
  @Input() variant: StatusVariant = 'pending';
}
