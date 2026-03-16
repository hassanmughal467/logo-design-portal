import { Component, Input } from '@angular/core';
import { Order } from '@shared/models/order.model';

export type TimelineStepState = 'completed' | 'current' | 'upcoming' | 'revision';

export interface TimelineStep {
  id: string;
  label: string;
  state: TimelineStepState;
  timestamp?: Date;
}

@Component({
  selector: 'app-order-progress-timeline',
  templateUrl: './order-progress-timeline.component.html',
  styleUrls: ['./order-progress-timeline.component.scss']
})
export class OrderProgressTimelineComponent {
  @Input() order: Order | null = null;

  get steps(): TimelineStep[] {
    if (!this.order) return [];

    const status = (this.order.status as string) || '';
    const revisionCount = this.order.revisionCount ?? 0;
    const hasRevisionLoop = revisionCount > 0;

    // Build step definitions - insert revision loop after first Preview Delivered when applicable
    const stepDefs: Array<{ id: string; label: string }> = [
      { id: 'order_submitted', label: 'Order Submitted' },
      { id: 'admin_review', label: 'Admin Review' },
      { id: 'design_in_progress', label: 'Design In Progress' },
      { id: 'preview_delivered', label: 'Preview Delivered' }
    ];

    if (hasRevisionLoop) {
      stepDefs.push(
        { id: 'revision_requested', label: 'Revision Requested' },
        { id: 'design_updated', label: 'Design Updated' },
        { id: 'preview_delivered_2', label: 'Preview Delivered' }
      );
    }

    stepDefs.push(
      { id: 'client_approval', label: 'Client Approval' },
      { id: 'completed', label: 'Completed' }
    );

    // Determine current step index based on status
    const getCurrentIndex = (): number => {
      switch (status) {
        case 'WaitingForAdminApproval':
        case 'PriceApprovalPending':
          return 1;
        case 'InProgress':
          return hasRevisionLoop ? 5 : 2; // design_updated : design_in_progress
        case 'PreviewDelivered':
          return hasRevisionLoop ? 6 : 3; // preview_delivered_2 : preview_delivered
        case 'RevisionRequested':
          return 4; // revision_requested (only exists when hasRevisionLoop)
        case 'ClientApproved':
          return hasRevisionLoop ? 7 : 4;
        case 'Completed':
          return hasRevisionLoop ? 8 : 5;
        case 'Cancelled':
        case 'CancelledByUser':
        case 'CancelledByAdmin':
        case 'Refunded':
          // For terminal states, show Order Submitted as completed, Admin Review as current
          return 1;
        default:
          return 0;
      }
    };

    const currentIndex = getCurrentIndex();

    return stepDefs.map((step, index) => {
      let state: TimelineStepState = 'upcoming';
      if (index < currentIndex) {
        state = 'completed';
      } else if (index === currentIndex) {
        state = (step.id === 'revision_requested' && status === 'RevisionRequested') ? 'revision' : 'current';
      }

      let timestamp: Date | undefined;
      if (step.id === 'order_submitted' && this.order?.createdAt) {
        timestamp = new Date(this.order.createdAt);
      } else if (step.id === 'completed' && this.order?.updatedAt && status === 'Completed') {
        timestamp = new Date(this.order.updatedAt);
      }

      return {
        id: step.id,
        label: step.label,
        state,
        timestamp
      };
    });
  }
}
