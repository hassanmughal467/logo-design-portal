import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-inactive-clients-table',
  templateUrl: './inactive-clients-table.component.html',
  styleUrls: ['./inactive-clients-table.component.scss']
})
export class InactiveClientsTableComponent {
  @Input() items: any[] = [];

  formatDate(value: string | undefined): string {
    if (!value) return '-';
    return new Date(value).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
