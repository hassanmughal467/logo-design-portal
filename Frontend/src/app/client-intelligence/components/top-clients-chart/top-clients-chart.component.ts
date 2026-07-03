import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  formatCurrencyAmount,
  resolveSingleCurrencyCode
} from '@core/utils/currency-format';

@Component({
  selector: 'app-top-clients-chart',
  templateUrl: './top-clients-chart.component.html',
  styleUrls: ['./top-clients-chart.component.scss']
})
export class TopClientsChartComponent implements OnChanges {
  @Input() items: any[] = [];
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const labels = this.items.map(i => (i.clientName?.length > 35 ? i.clientName.substring(0, 35) + '...' : i.clientName) || 'Unknown');
    const { code: chartCurrency, mixed } = resolveSingleCurrencyCode(
      this.items.map(i => i.currencyCode)
    );

    const compactFormat = (v: number) =>
      mixed
        ? this.compact(v)
        : this.compactWithCurrency(v, chartCurrency);

    const fullFormat = (v: number, perItemCode?: string) =>
      formatCurrencyAmount(v, mixed ? perItemCode : chartCurrency);

    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.totalRevenue) }],
      chart: { type: 'bar', height: Math.max(320, this.items.length * 40), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '75%' } },
      colors: ['#3b82f6'],
      xaxis: { categories: labels, labels: { formatter: compactFormat } },
      yaxis: { labels: { maxWidth: 200 } },
      dataLabels: { enabled: true, formatter: compactFormat },
      tooltip: {
        y: {
          formatter: (v: number, opts: any) => {
            const idx = opts?.dataPointIndex ?? 0;
            const code = this.items[idx]?.currencyCode;
            return fullFormat(v, code);
          }
        }
      }
    };
  }

  private compactWithCurrency(value: number, code: string): string {
    if (Math.abs(value) >= 1000) {
      const compact = (value / 1000).toFixed(value >= 10000 ? 0 : 1) + 'k';
      return formatCurrencyAmount(0, code).replace(/[\d.,\s]/g, '') + compact;
    }
    return formatCurrencyAmount(value, code);
  }

  private compact(value: number): string {
    return Math.abs(value) >= 1000 ? (value / 1000).toFixed(1) + 'k' : value.toString();
  }

  formatCurrency(value: number, code?: string): string {
    return formatCurrencyAmount(value, code);
  }
}
