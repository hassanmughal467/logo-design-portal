import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  AdminAnalyticsService,
  OrderAnalytics,
  OrdersTrendItem,
  OrdersByStatusItem,
  OrdersByPackageItem,
  DailyActivityItem
} from '@core/services/admin-analytics.service';

@Component({
  selector: 'app-order-analytics',
  templateUrl: './order-analytics.component.html',
  styleUrls: ['./order-analytics.component.scss']
})
export class OrderAnalyticsComponent implements OnChanges {
  @Input() data: OrderAnalytics | null = null;
  @Input() loading = false;

  ordersTrendChart: any;
  ordersTrendDailyChart: any;
  ordersByStatusChart: any;
  ordersByPackageChart: any;
  ordersByDayOfWeekChart: any;
  ordersByHourChart: any;
  ordersByClientChart: any;
  ordersByDesignerChart: any;
  dailyActivityChart: any;

  readonly chartColors = ['#0d47a1', '#1976d2', '#42a5f5', '#64b5f6', '#90caf9', '#bbdefb', '#10b981', '#f59e0b'];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.buildCharts();
    }
  }

  private buildCharts(): void {
    if (!this.data) return;

    this.buildOrdersTrendChart();
    this.buildOrdersTrendDailyChart();
    this.buildOrdersByStatusChart();
    this.buildOrdersByPackageChart();
    this.buildOrdersByDayOfWeekChart();
    this.buildOrdersByHourChart();
    this.buildOrdersByClientChart();
    this.buildOrdersByDesignerChart();
    this.buildDailyActivityChart();
  }

  private buildOrdersTrendChart(): void {
    const items = this.data!.ordersTrend || [];
    this.ordersTrendChart = {
      series: [{ name: 'Orders', data: items.map((i: OrdersTrendItem) => i.count) }],
      chart: { type: 'line', height: 280, toolbar: { show: false }, zoom: { enabled: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.1 } },
      colors: ['#0d47a1'],
      xaxis: { categories: items.map((i: OrdersTrendItem) => i.month), labels: { rotate: -45 } },
      yaxis: { min: 0, forceNiceScale: true },
      tooltip: { y: { formatter: (v: number) => v + ' orders' } },
      dataLabels: { enabled: false }
    };
  }

  private buildOrdersTrendDailyChart(): void {
    const items = this.data!.ordersTrendDaily || [];
    this.ordersTrendDailyChart = items.length > 0 ? {
      series: [{ name: 'Orders', data: items.map((i: any) => i.count) }],
      chart: { type: 'area', height: 260, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
      colors: ['#10b981'],
      xaxis: { categories: items.map((i: any) => i.date) },
      yaxis: { min: 0 },
      tooltip: { y: { formatter: (v: number) => v + ' orders' } },
      dataLabels: { enabled: false }
    } : null;
  }

  private buildOrdersByDayOfWeekChart(): void {
    const items = this.data!.ordersByDayOfWeek || [];
    this.ordersByDayOfWeekChart = items.length > 0 ? {
      series: [{ name: 'Orders', data: items.map((i: any) => i.count) }],
      chart: { type: 'bar', height: 260, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '70%' } },
      colors: ['#0d47a1'],
      xaxis: { categories: items.map((i: any) => i.dayOfWeek) },
      yaxis: { min: 0 },
      dataLabels: { enabled: true }
    } : null;
  }

  private buildOrdersByHourChart(): void {
    const items = this.data!.ordersByHour || [];
    this.ordersByHourChart = items.length > 0 ? {
      series: [{ name: 'Orders', data: items.map((i: any) => i.count) }],
      chart: { type: 'bar', height: 260, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '90%' } },
      colors: ['#8b5cf6'],
      xaxis: { categories: items.map((i: any) => i.hour + ':00') },
      yaxis: { min: 0 },
      dataLabels: { enabled: false }
    } : null;
  }

  private buildOrdersByClientChart(): void {
    const items = this.data!.ordersByClient || [];
    this.ordersByClientChart = items.length > 0 ? {
      series: [{ name: 'Orders', data: items.map((i: any) => i.orderCount) }],
      chart: { type: 'bar', height: Math.max(260, items.length * 36), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '75%' } },
      colors: ['#0d47a1'],
      xaxis: { categories: items.map((i: any) => (i.clientName || '').substring(0, 28) + ((i.clientName || '').length > 28 ? '...' : '')) },
      yaxis: { labels: { maxWidth: 140 } },
      dataLabels: { enabled: true }
    } : null;
  }

  private buildOrdersByDesignerChart(): void {
    const items = this.data!.ordersByDesigner || [];
    this.ordersByDesignerChart = items.length > 0 ? {
      series: [{ name: 'Orders', data: items.map((i: any) => i.orderCount) }],
      chart: { type: 'bar', height: Math.max(260, items.length * 36), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '75%' } },
      colors: ['#10b981'],
      xaxis: { categories: items.map((i: any) => i.designerName || 'Unknown') },
      yaxis: { labels: { maxWidth: 120 } },
      dataLabels: { enabled: true }
    } : null;
  }

  private buildOrdersByStatusChart(): void {
    const items = this.data!.ordersByStatus || [];
    this.ordersByStatusChart = {
      series: items.map((i: OrdersByStatusItem) => i.count),
      chart: { type: 'donut', height: 280 },
      labels: items.map((i: OrdersByStatusItem) => this.formatStatus(i.status)),
      colors: this.chartColors.slice(0, items.length),
      legend: { position: 'bottom' },
      plotOptions: { pie: { donut: { size: '65%' } } },
      dataLabels: { enabled: true }
    };
  }

  private buildOrdersByPackageChart(): void {
    const items = this.data!.ordersByPackage || [];
    this.ordersByPackageChart = {
      series: [{ name: 'Orders', data: items.map((i: OrdersByPackageItem) => i.count) }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: false, columnWidth: '60%' } },
      colors: ['#0d47a1'],
      xaxis: { categories: items.map((i: OrdersByPackageItem) => i.package) },
      yaxis: { min: 0, forceNiceScale: true },
      dataLabels: { enabled: false }
    };
  }

  private buildDailyActivityChart(): void {
    const items = this.data!.dailyActivity || [];
    this.dailyActivityChart = {
      series: [
        { name: 'Orders Created', data: items.map((i: DailyActivityItem) => i.ordersCreated) },
        { name: 'Files Uploaded', data: items.map((i: DailyActivityItem) => i.filesUploaded) },
        { name: 'Revisions Requested', data: items.map((i: DailyActivityItem) => i.revisionsRequested) },
        { name: 'Approvals Completed', data: items.map((i: DailyActivityItem) => i.approvalsCompleted) }
      ],
      chart: { type: 'line', height: 280, toolbar: { show: false }, zoom: { enabled: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#0d47a1', '#10b981', '#f59e0b', '#8b5cf6'],
      xaxis: { categories: items.map((i: DailyActivityItem) => i.date) },
      yaxis: { min: 0, forceNiceScale: true },
      legend: { position: 'top' },
      dataLabels: { enabled: false }
    };
  }

  formatStatus(status: string): string {
    if (status === 'ClientApproved') return 'Approved';
    return status.replace(/([A-Z])/g, ' $1').trim();
  }
}
