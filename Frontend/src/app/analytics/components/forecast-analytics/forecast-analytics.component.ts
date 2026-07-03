import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  ForecastAnalytics,
  OrderForecastItem,
  RevenueForecastItem
} from '@core/services/admin-analytics.service';
import {
  compactCurrencyLabel,
  currencyIconClass,
  formatCurrencyAmount
} from '@core/utils/currency-format';

@Component({
  selector: 'app-forecast-analytics',
  templateUrl: './forecast-analytics.component.html',
  styleUrls: ['./forecast-analytics.component.scss']
})
export class ForecastAnalyticsComponent implements OnChanges {
  @Input() data: ForecastAnalytics | null = null;

  orderForecastChart: any;
  revenueForecastChart: any;

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

  private buildCharts(): void {
    if (!this.data) return;

    const orderData = this.data.orderGrowthPrediction || [];
    if (orderData.length > 0) {
      const actuals = orderData.map((i: OrderForecastItem) => i.actual);
      const predicted = orderData.map((i: OrderForecastItem) => i.predicted ?? null);
      this.orderForecastChart = {
        series: [
          { name: 'Actual', data: actuals },
          { name: 'Predicted (MA)', data: predicted }
        ],
        chart: { type: 'line', height: 280, toolbar: { show: false } },
        stroke: { curve: 'smooth', width: 2 },
        colors: ['#0d47a1', '#10b981'],
        xaxis: { categories: orderData.map((i: OrderForecastItem) => i.period) },
        yaxis: { min: 0 },
        legend: { position: 'top' },
        dataLabels: { enabled: false }
      };
    } else {
      this.orderForecastChart = null;
    }

    const revenueData = this.data.revenueForecast || [];
    const code = this.revenueCurrencyMixed ? undefined : this.revenueCurrencyCode;
    const axis = (v: number) => compactCurrencyLabel(v, code, this.revenueCurrencyMixed);
    const fmt = (v: number) => (v != null ? formatCurrencyAmount(v, code) : '-');

    if (revenueData.length > 0) {
      const actuals = revenueData.map((i: RevenueForecastItem) => i.actual);
      const predicted = revenueData.map((i: RevenueForecastItem) => i.predicted ?? null);
      this.revenueForecastChart = {
        series: [
          { name: 'Actual', data: actuals },
          { name: 'Predicted (MA)', data: predicted }
        ],
        chart: { type: 'area', height: 280, toolbar: { show: false } },
        stroke: { curve: 'smooth', width: 2 },
        fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.1 } },
        colors: ['#10b981', '#0d47a1'],
        xaxis: { categories: revenueData.map((i: RevenueForecastItem) => i.period) },
        yaxis: { min: 0, labels: { formatter: axis } },
        legend: { position: 'top' },
        dataLabels: { enabled: false },
        tooltip: {
          shared: true,
          y: [{ formatter: fmt }, { formatter: fmt }]
        }
      };
    } else {
      this.revenueForecastChart = null;
    }
  }
}
