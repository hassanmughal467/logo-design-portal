import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  RevenueAnalytics,
  RevenueTrendItem,
  RevenueByPackageItem,
  AverageOrderValueTrendItem,
  TopClientByRevenue
} from '@core/services/admin-analytics.service';
import {
  compactCurrencyLabel,
  currencyIconClass,
  formatCurrencyAmount,
  resolveSingleCurrencyCode
} from '@core/utils/currency-format';

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

  get revenueCurrencyCode(): string {
    return this.data?.revenueCurrencyCode ?? 'USD';
  }

  get revenueCurrencyMixed(): boolean {
    return !!this.data?.revenueCurrencyMixed;
  }

  get revenueIconClass(): string {
    return currencyIconClass(this.revenueCurrencyCode);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.buildCharts();
    }
  }

  private chartCode(): string | undefined {
    return this.revenueCurrencyMixed ? undefined : this.revenueCurrencyCode;
  }

  private buildCharts(): void {
    if (!this.data) return;

    const code = this.chartCode();
    const axis = (v: number) => compactCurrencyLabel(v, code, this.revenueCurrencyMixed);
    const tooltip = (v: number) => formatCurrencyAmount(v, code);

    const rt = this.data.revenueTrend || [];
    this.revenueTrendChart = {
      series: [{ name: 'Revenue', data: rt.map((i: RevenueTrendItem) => i.revenue) }],
      chart: { type: 'area', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.6, opacityTo: 0.2 } },
      colors: ['#10b981'],
      xaxis: { categories: rt.map((i: RevenueTrendItem) => i.month) },
      yaxis: { min: 0, labels: { formatter: axis } },
      dataLabels: { enabled: false },
      tooltip: { y: { formatter: tooltip } }
    };

    const rtd = this.data.revenueTrendDaily || [];
    this.revenueTrendDailyChart = rtd.length > 0 ? {
      series: [{ name: 'Revenue', data: rtd.map((i: any) => i.revenue) }],
      chart: { type: 'area', height: 260, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
      colors: ['#10b981'],
      xaxis: { categories: rtd.map((i: any) => i.date) },
      yaxis: { min: 0, labels: { formatter: axis } },
      dataLabels: { enabled: false },
      tooltip: { y: { formatter: tooltip } }
    } : null;

    const rbp = this.data.revenueByPackage || [];
    this.revenueByPackageChart = {
      series: [{ name: 'Revenue', data: rbp.map((i: RevenueByPackageItem) => i.revenue) }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '60%' } },
      colors: this.chartColors,
      xaxis: { categories: rbp.map((i: RevenueByPackageItem) => i.package) },
      yaxis: { min: 0, labels: { formatter: axis } },
      dataLabels: { enabled: true, formatter: axis }
    };

    const aov = this.data.averageOrderValueTrend || [];
    this.aovTrendChart = {
      series: [{ name: 'Avg Order Value', data: aov.map((i: AverageOrderValueTrendItem) => i.averageOrderValue) }],
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#0d47a1'],
      xaxis: { categories: aov.map((i: AverageOrderValueTrendItem) => i.month) },
      yaxis: { min: 0, labels: { formatter: axis } },
      dataLabels: { enabled: false },
      tooltip: { y: { formatter: tooltip } }
    };

    const top = this.data.topClientsByRevenue || [];
    const { code: topCode, mixed: topMixed } = resolveSingleCurrencyCode(top.map((i: TopClientByRevenue) => i.currencyCode));
    const topAxis = (v: number) => compactCurrencyLabel(v, topMixed ? undefined : topCode, topMixed);
    this.topClientsChart = {
      series: [{ name: 'Revenue', data: top.map((i: TopClientByRevenue) => i.revenue) }],
      chart: { type: 'bar', height: Math.max(280, top.length * 50), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '70%' } },
      colors: ['#10b981'],
      xaxis: { categories: top.map((i: TopClientByRevenue) => i.clientName.substring(0, 30) + (i.clientName.length > 30 ? '...' : '')) },
      yaxis: { labels: { maxWidth: 150 } },
      dataLabels: { enabled: true, formatter: topAxis },
      tooltip: {
        y: {
          formatter: (v: number, opts: any) => {
            const idx = opts?.dataPointIndex ?? 0;
            return formatCurrencyAmount(v, top[idx]?.currencyCode);
          }
        }
      }
    };
  }

  formatCurrency(v: number): string {
    if (this.revenueCurrencyMixed) {
      return 'Mixed';
    }
    return formatCurrencyAmount(v, this.revenueCurrencyCode);
  }
}
