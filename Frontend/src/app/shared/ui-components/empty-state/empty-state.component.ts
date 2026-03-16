import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  templateUrl: './empty-state.component.html',
  styleUrls: ['./empty-state.component.scss']
})
export class EmptyStateComponent {
  @Input() icon = 'pi pi-inbox';
  @Input() title = 'No data found';
  @Input() subtitle = '';
}
