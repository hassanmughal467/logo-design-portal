import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-client-activity-table',
  templateUrl: './client-activity-table.component.html',
  styleUrls: ['./client-activity-table.component.scss']
})
export class ClientActivityTableComponent {
  @Input() items: any[] = [];

  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Active: 'Active',
      LowActivity: 'Low Activity',
      Inactive: 'Inactive'
    };
    return map[status] || status;
  }

  getStatusSeverity(status: string): string {
    const map: Record<string, string> = {
      Active: 'success',
      LowActivity: 'warn',
      Inactive: 'danger'
    };
    return map[status] || 'info';
  }

  formatDate(value: string | undefined): string {
    if (!value) return '-';
    return new Date(value).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
