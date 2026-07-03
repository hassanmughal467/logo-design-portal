import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  ClientAnalytics,
  OrdersPerClientItem,
  ClientRetentionTrendItem,
  ClientLifetimeValueItem
} from '@core/services/admin-analytics.service';
import {
  compactCurrencyLabel,
  formatCurrencyAmount,
  resolveSingleCurrencyCode
} from '@core/utils/currency-format';

@Component({
  selector: 'app-client-analytics',
  templateUrl: './client-analytics.component.html',
  styleUrls: ['./client-analytics.component.scss']
})
export class ClientAnalyticsComponent implements OnChanges {
  @Input() data: ClientAnalytics | null = null;

  newVsReturningChart: any;
  ordersPerClientChart: any;
  retentionTrendChart: any;
  topClientsByOrdersChart: any;
  clientLifetimeValueChart: any;

  readonly chartColors = ['#0d47a1', '#10b981', '#f59e0b', '#8b5cf6'];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.buildCharts();
    }
  }

  private buildCharts(): void {
    if (!this.data) return;

    this.newVsReturningChart = {
      series: [this.data.newClients, this.data.returningClients],
      chart: { type: 'donut', height: 260 },
      labels: ['New Clients', 'Returning Clients'],
      colors: ['#0d47a1', '#10b981'],
      legend: { position: 'bottom' },
      dataLabels: { enabled: true }
    };

    const opc = this.data.ordersPerClient || [];
    this.ordersPerClientChart = {
      series: [{ name: 'Orders', data: opc.map((i: OrdersPerClientItem) => i.orderCount) }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '60%' } },
      colors: ['#0d47a1'],
      xaxis: { categories: opc.map((i: OrdersPerClientItem) => i.clientName.substring(0, 25) + (i.clientName.length > 25 ? '...' : '')), labels: { rotate: -45 } },
      yaxis: { min: 0 },
      dataLabels: { enabled: true }
    };

    const rtt = this.data.clientRetentionTrend || [];
    this.retentionTrendChart = {
      series: [
        { name: 'New Clients', data: rtt.map((i: ClientRetentionTrendItem) => i.newClients) },
        { name: 'Returning Orders', data: rtt.map((i: ClientRetentionTrendItem) => i.returningOrders) }
      ],
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#0d47a1', '#10b981'],
      xaxis: { categories: rtt.map((i: ClientRetentionTrendItem) => i.month) },
      yaxis: { min: 0 },
      legend: { position: 'top' },
      dataLabels: { enabled: false }
    };

    const topOrders = this.data.topClientsByOrders || [];
    this.topClientsByOrdersChart = topOrders.length > 0 ? {
      series: [{ name: 'Orders', data: topOrders.map((i: any) => i.orderCount) }],
      chart: { type: 'bar', height: 260, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '75%' } },
      colors: ['#0d47a1'],
      xaxis: { categories: topOrders.map((i: any) => (i.clientName || '').substring(0, 25) + ((i.clientName || '').length > 25 ? '...' : '')) },
      yaxis: { labels: { maxWidth: 140 } },
      dataLabels: { enabled: true }
    } : null;

    const clv = this.data.clientLifetimeValue || [];
    const { code: clvCode, mixed: clvMixed } = resolveSingleCurrencyCode(clv.map(i => i.currencyCode));
    const clvAxis = (v: number) => compactCurrencyLabel(v, clvMixed ? undefined : clvCode, clvMixed);
    this.clientLifetimeValueChart = clv.length > 0 ? {
      series: [{ name: 'Revenue', data: clv.map((i: ClientLifetimeValueItem) => i.revenue) }],
      chart: { type: 'bar', height: 260, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '75%' } },
      colors: ['#10b981'],
      xaxis: { categories: clv.map((i: ClientLifetimeValueItem) => (i.clientName || '').substring(0, 25) + ((i.clientName || '').length > 25 ? '...' : '')) },
      yaxis: { labels: { maxWidth: 140, formatter: clvAxis } },
      dataLabels: { enabled: true, formatter: clvAxis },
      tooltip: {
        y: {
          formatter: (v: number, opts: any) => {
            const idx = opts?.dataPointIndex ?? 0;
            return formatCurrencyAmount(v, clv[idx]?.currencyCode);
          }
        }
      }
    } : null;
  }
}
