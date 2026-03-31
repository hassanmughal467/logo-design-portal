import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  RevenueAnalytics,
  RevenueTrendItem,
  RevenueByPackageItem,
  AverageOrderValueTrendItem,
  TopClientByRevenue
} from '@core/services/admin-analytics.service';

@Component({
  selector: 'app-revenue-analytics',
  templateUrl: './revenue-analytics.component.html',
  styleUrls: ['./revenue-analytics.component.scss']
})
export class RevenueAnalyticsComponent implements OnChanges {
  @Input() data: RevenueAnalytics | null = null;

  revenueTrendChart: any;
  revenueTrendDailyChart: any;
  revenueByPackageChart: any;
  aovTrendChart: any;
  topClientsChart: any;

  readonly chartColors = ['#0d47a1', '#10b981', '#f59e0b', '#8b5cf6', '#ef4444'];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.buildCharts();
    }
  }

  private buildCharts(): void {
    if (!this.data) return;

    const rt = this.data.revenueTrend || [];
    const rtd = this.data.revenueTrendDaily || [];
    this.revenueTrendChart = {
      series: [{ name: 'Revenue', data: rt.map((i: RevenueTrendItem) => i.revenue) }],
      chart: { type: 'area', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.6, opacityTo: 0.2 } },
      colors: ['#10b981'],
      xaxis: { categories: rt.map((i: RevenueTrendItem) => i.month) },
      yaxis: { min: 0, labels: { formatter: (v: number) => '$' + v } },
      dataLabels: { enabled: false }
    };

    this.revenueTrendDailyChart = rtd.length > 0 ? {
      series: [{ name: 'Revenue', data: rtd.map((i: any) => i.revenue) }],
      chart: { type: 'area', height: 260, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
      colors: ['#10b981'],
      xaxis: { categories: rtd.map((i: any) => i.date) },
      yaxis: { min: 0, labels: { formatter: (v: number) => '$' + v } },
      dataLabels: { enabled: false }
    } : null;

    const rbp = this.data.revenueByPackage || [];
    this.revenueByPackageChart = {
      series: [{ name: 'Revenue', data: rbp.map((i: RevenueByPackageItem) => i.revenue) }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '60%' } },
      colors: this.chartColors,
      xaxis: { categories: rbp.map((i: RevenueByPackageItem) => i.package) },
      yaxis: { min: 0, labels: { formatter: (v: number) => '$' + v } },
      dataLabels: { enabled: true }
    };

    const aov = this.data.averageOrderValueTrend || [];
    this.aovTrendChart = {
      series: [{ name: 'Avg Order Value', data: aov.map((i: AverageOrderValueTrendItem) => i.averageOrderValue) }],
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#0d47a1'],
      xaxis: { categories: aov.map((i: AverageOrderValueTrendItem) => i.month) },
      yaxis: { min: 0, labels: { formatter: (v: number) => '$' + v } },
      dataLabels: { enabled: false }
    };

    const top = this.data.topClientsByRevenue || [];
    this.topClientsChart = {
      series: [{ name: 'Revenue', data: top.map((i: TopClientByRevenue) => i.revenue) }],
      chart: { type: 'bar', height: Math.max(280, top.length * 50), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '70%' } },
      colors: ['#10b981'],
      xaxis: { categories: top.map((i: TopClientByRevenue) => i.clientName.substring(0, 30) + (i.clientName.length > 30 ? '...' : '')) },
      yaxis: { labels: { maxWidth: 150 } },
      dataLabels: { enabled: true, formatter: (v: number) => '$' + v }
    };
  }

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0 }).format(v);
  }
}
