import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-client-monthly-revenue-chart',
  templateUrl: './client-monthly-revenue-chart.component.html',
  styleUrls: ['./client-monthly-revenue-chart.component.scss']
})
export class ClientMonthlyRevenueChartComponent implements OnChanges {
  @Input() items: any[] = [];
  @Input() loading = false;
  /** ISO 4217 code of the selected client. */
  @Input() currencyCode = 'USD';
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['items'] || changes['currencyCode']) && this.items?.length) {
      this.buildChart();
    } else if (changes['items'] && !this.items?.length) {
      this.chartOptions = null;
    }
  }

  private buildChart(): void {
    const categories = this.items.map(i => i.month);
    const revenue = this.items.map(i => i.revenue);
    const code = this.currencyCode;
    const compact = (v: number) =>
      Math.abs(v) >= 1000
        ? formatCurrencyAmount(0, code).replace(/[\d.,\s]/g, '') + (v / 1000).toFixed(v >= 10000 ? 0 : 1) + 'k'
        : formatCurrencyAmount(v, code);

    this.chartOptions = {
      series: [{ name: 'Revenue', data: revenue }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 4, columnWidth: '70%' } },
      colors: ['#8b5cf6'],
      xaxis: { categories },
      yaxis: { labels: { formatter: compact } },
      tooltip: { y: { formatter: (v: number) => formatCurrencyAmount(v, code) } },
      dataLabels: { enabled: false }
    };
  }
}
