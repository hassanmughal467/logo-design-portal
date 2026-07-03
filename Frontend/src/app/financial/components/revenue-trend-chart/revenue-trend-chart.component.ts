import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { RevenueTrendItem } from '@core/services/financial-analytics.service';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-revenue-trend-chart',
  templateUrl: './revenue-trend-chart.component.html',
  styleUrls: ['./revenue-trend-chart.component.scss']
})
export class RevenueTrendChartComponent implements OnChanges {
  @Input() items: RevenueTrendItem[] = [];
  @Input() currencyCode = 'USD';
  @Input() currencyMixed = false;

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['items'] || changes['currencyCode'] || changes['currencyMixed']) && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const code = this.currencyMixed ? undefined : this.currencyCode;
    const axis = (v: number) => compactCurrencyLabel(v, code, this.currencyMixed);
    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.revenue) }],
      chart: { type: 'area', height: 320, toolbar: { show: false }, zoom: { enabled: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.6, opacityTo: 0.15 } },
      colors: ['#10b981'],
      xaxis: { categories: this.items.map(i => i.month), labels: { rotate: -45 } },
      yaxis: { min: 0, labels: { formatter: axis } },
      dataLabels: { enabled: false },
      tooltip: { y: { formatter: (v: number) => formatCurrencyAmount(v, code) } }
    };
  }
}
