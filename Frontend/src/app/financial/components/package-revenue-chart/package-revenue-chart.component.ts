import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { PackageRevenueItem } from '@core/services/financial-analytics.service';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-package-revenue-chart',
  templateUrl: './package-revenue-chart.component.html',
  styleUrls: ['./package-revenue-chart.component.scss']
})
export class PackageRevenueChartComponent implements OnChanges {
  @Input() items: PackageRevenueItem[] = [];
  @Input() currencyCode = 'USD';
  @Input() currencyMixed = false;

  chartOptions: any = null;
  readonly colors = ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6'];

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
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '60%', distributed: true } },
      colors: this.colors,
      xaxis: { categories: this.items.map(i => i.package) },
      yaxis: { min: 0, labels: { formatter: axis } },
      dataLabels: { enabled: true, formatter: axis },
      tooltip: { y: { formatter: (v: number) => formatCurrencyAmount(v, code) } }
    };
  }
}
