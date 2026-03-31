import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-skeleton-table',
  templateUrl: './skeleton-table.component.html',
  styleUrls: ['./skeleton-table.component.scss']
})
export class SkeletonTableComponent {
  @Input() rows = 8;

  @Input() columns = 6;

  @Input() showToolbar = true;

  get rowIndices(): number[] {
    return Array.from({ length: this.rows }, (_, i) => i);
  }

  get colIndices(): number[] {
    return Array.from({ length: this.columns }, (_, i) => i);
  }
}
