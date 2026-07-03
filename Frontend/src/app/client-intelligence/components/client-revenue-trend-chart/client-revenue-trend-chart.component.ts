import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-client-revenue-trend-chart',
  templateUrl: './client-revenue-trend-chart.component.html',
  styleUrls: ['./client-revenue-trend-chart.component.scss']
})
export class ClientRevenueTrendChartComponent implements OnChanges {
  @Input() items: any[] = [];
  /** Currency for the trend window. When mixed, parent passes 'USD'. */
  @Input() currencyCode = 'USD';
  /** True when the trend window contains more than one currency. */
  @Input() currencyMixed = false;
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['items'] || changes['currencyCode'] || changes['currencyMixed']) && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const categories = this.items.map(i => i.month);
    const revenue = this.items.map(i => i.revenue);
    const code = this.currencyMixed ? undefined : this.currencyCode;
    const compact = (v: number) =>
      Math.abs(v) >= 1000
        ? formatCurrencyAmount(0, code).replace(/[\d.,\s]/g, '') + (v / 1000).toFixed(v >= 10000 ? 0 : 1) + 'k'
        : formatCurrencyAmount(v, code);

    this.chartOptions = {
      series: [{ name: 'Revenue', data: revenue }],
      chart: { type: 'area', height: 320, toolbar: { show: false }, zoom: { enabled: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.1 } },
      colors: ['#10b981'],
      xaxis: { categories },
      yaxis: { labels: { formatter: compact } },
      tooltip: {
        y: { formatter: (v: number) => formatCurrencyAmount(v, code) }
      },
      dataLabels: { enabled: false }
    };
  }
}
