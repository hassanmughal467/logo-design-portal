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
    // Show revision segment whenever a revision was filed or is in progress
    const hasRevisionLoop = revisionCount > 0 || status === 'RevisionRequested';

    const stepDefs: Array<{ id: string; label: string }> = [
      { id: 'order_submitted', label: 'Order Submitted' },
      { id: 'admin_review', label: 'Admin Review' },
      { id: 'design_in_progress', label: 'Design In Progress' },
      { id: 'preview_delivered', label: 'Preview Delivered' }
    ];

    if (hasRevisionLoop) {
      const revisionLabel =
        revisionCount > 0
          ? `Revision requested (${revisionCount})`
          : 'Revision requested';
      stepDefs.push(
        { id: 'revision_requested', label: revisionLabel },
        { id: 'design_updated', label: 'Design updated' },
        { id: 'preview_delivered_2', label: 'Preview delivered' }
      );
    }

    stepDefs.push(
      { id: 'client_approval', label: 'Client approval' },
      { id: 'completed', label: 'Completed' }
    );

    const lastIndex = stepDefs.length - 1;

    const getCurrentIndex = (): number => {
      switch (status) {
        case 'Completed':
          // Past the last step so every milestone shows completed (checkmarks)
          return stepDefs.length;
        case 'ClientApproved':
          // Client has approved; order is pending final admin completion
          return lastIndex;
        case 'WaitingForAdminApproval':
        case 'PriceApprovalPending':
          return 1;
        case 'InProgress':
          return hasRevisionLoop ? 5 : 2;
        case 'PreviewDelivered':
          return hasRevisionLoop ? 6 : 3;
        case 'RevisionRequested':
          return 4;
        case 'Cancelled':
        case 'CancelledByUser':
        case 'CancelledByAdmin':
        case 'Refunded':
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
        state =
          step.id === 'revision_requested' && status === 'RevisionRequested'
            ? 'revision'
            : 'current';
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
