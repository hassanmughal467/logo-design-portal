import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-skeleton-card',
  templateUrl: './skeleton-card.component.html',
  styleUrls: ['./skeleton-card.component.scss']
})
export class SkeletonCardComponent {
  /** Number of text lines below optional title bar */
  @Input() lines = 3;

  get lineIndices(): number[] {
    return Array.from({ length: this.lines }, (_, i) => i);
  }

  /** Compact height for KPI-style placeholders */
  @Input() compact = false;

  /** Show a title bar shimmer */
  @Input() showTitle = true;
}
