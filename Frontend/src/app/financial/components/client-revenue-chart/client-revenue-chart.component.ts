import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ClientRevenueItem } from '@core/services/financial-analytics.service';
import {
  formatCurrencyAmount,
  resolveSingleCurrencyCode
} from '@core/utils/currency-format';

@Component({
  selector: 'app-client-revenue-chart',
  templateUrl: './client-revenue-chart.component.html',
  styleUrls: ['./client-revenue-chart.component.scss']
})
export class ClientRevenueChartComponent implements OnChanges {
  @Input() items: ClientRevenueItem[] = [];

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const labels = this.items.map(i => (i.clientName.length > 30 ? i.clientName.substring(0, 30) + '...' : i.clientName));
    const { code: chartCurrency, mixed } = resolveSingleCurrencyCode(
      this.items.map(i => i.currencyCode)
    );
    const compact = (v: number) => {
      if (mixed) {
        return Math.abs(v) >= 1000 ? (v / 1000).toFixed(1) + 'k' : v.toString();
      }
      if (Math.abs(v) >= 1000) {
        const sym = formatCurrencyAmount(0, chartCurrency).replace(/[\d.,\s\u00a0]/g, '');
        return sym + (v / 1000).toFixed(v >= 10000 ? 0 : 1) + 'k';
      }
      return formatCurrencyAmount(v, chartCurrency);
    };

    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.totalRevenue) }],
      chart: { type: 'bar', height: Math.max(280, this.items.length * 36), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '70%' } },
      colors: ['#3b82f6'],
      xaxis: { categories: labels, labels: { formatter: compact } },
      yaxis: { labels: { maxWidth: 180 } },
      dataLabels: { enabled: true, formatter: compact },
      tooltip: {
        y: {
          formatter: (v: number, opts: any) => {
            const idx = opts?.dataPointIndex ?? 0;
            return formatCurrencyAmount(v, this.items[idx]?.currencyCode);
          }
        }
      }
    };
  }
}
