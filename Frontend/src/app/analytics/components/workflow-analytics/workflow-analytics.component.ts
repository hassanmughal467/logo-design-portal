import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  WorkflowAnalytics,
  OrderFunnel,
  RevisionDistributionItem
} from '@core/services/admin-analytics.service';

@Component({
  selector: 'app-workflow-analytics',
  templateUrl: './workflow-analytics.component.html',
  styleUrls: ['./workflow-analytics.component.scss']
})
export class WorkflowAnalyticsComponent implements OnChanges {
  @Input() data: WorkflowAnalytics | null = null;
  @Input() loading = false;

  funnelChart: any;
  revisionChart: any;
  revisionQualityChart: any;
  ordersStuckChart: any;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.buildCharts();
    }
  }

  private buildCharts(): void {
    if (!this.data) return;

    const f = this.data.orderFunnel;
    this.funnelChart = {
      series: [{
        name: 'Orders',
        data: [
          f.ordersCreated,
          f.assignedToDesigner,
          f.designSubmitted,
          f.revisionRequested,
          f.clientApproval,
          f.completed
        ]
      }],
      chart: { type: 'bar', height: 300, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '70%', distributed: false } },
      colors: ['#0d47a1'],
      xaxis: {
        categories: [
          'Orders Created',
          'Assigned to Designer',
          'Design Submitted',
          'Revision Requested',
          'Client Approval',
          'Completed'
        ],
        labels: { rotate: -25 }
      },
      yaxis: { min: 0 },
      dataLabels: { enabled: true }
    };

    const rd = this.data.revisionDistribution || [];
    this.revisionChart = rd.length > 0 ? {
      series: rd.map((i: RevisionDistributionItem) => i.orderCount),
      chart: { type: 'donut', height: 260 },
      labels: rd.map((i: RevisionDistributionItem) => i.revisionCount + ' revision(s)'),
      colors: ['#0d47a1', '#1976d2', '#42a5f5', '#64b5f6', '#90caf9'],
      legend: { position: 'bottom' },
      dataLabels: { enabled: true }
    } : null;

    const rq = this.data.revisionQuality;
    this.revisionQualityChart = rq ? {
      series: [rq.ordersWithNoRevisions, rq.ordersWith1Revision, rq.ordersWith2PlusRevisions],
      chart: { type: 'donut', height: 260 },
      labels: ['No Revisions', '1 Revision', '2+ Revisions'],
      colors: ['#10b981', '#f59e0b', '#ef4444'],
      legend: { position: 'bottom' },
      dataLabels: { enabled: true }
    } : null;

    const stuck = this.data.ordersStuckInStage || [];
    this.ordersStuckChart = stuck.length > 0 ? {
      series: [{ name: 'Orders', data: stuck.map((i: any) => i.count) }],
      chart: { type: 'bar', height: 260, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '70%' } },
      colors: ['#ef4444'],
      xaxis: { categories: stuck.map((i: any) => i.stage) },
      yaxis: { min: 0 },
      dataLabels: { enabled: true }
    } : null;
  }

  get funnel(): OrderFunnel | null {
    return this.data?.orderFunnel || null;
  }
}
