import { Component, Input } from '@angular/core';

export type SkeletonDashboardVariant = 'admin' | 'client' | 'analytics' | 'table';

@Component({
  selector: 'app-skeleton-dashboard',
  templateUrl: './skeleton-dashboard.component.html',
  styleUrls: ['./skeleton-dashboard.component.scss']
})
export class SkeletonDashboardComponent {
  /** Layout preset: admin dashboard, client stats, analytics sections, or KPI + table focus. */
  @Input() variant: SkeletonDashboardVariant = 'admin';

  @Input() kpiCount = 5;

  get kpiIndices(): number[] {
    return Array.from({ length: this.kpiCount }, (_, i) => i);
  }

  get sectionIndices(): number[] {
    const n = this.variant === 'analytics' ? 4 : 2;
    return Array.from({ length: n }, (_, i) => i);
  }
}
