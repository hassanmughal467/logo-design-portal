import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { DesignerRevenueItem } from '@core/services/financial-analytics.service';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-designer-revenue-chart',
  templateUrl: './designer-revenue-chart.component.html',
  styleUrls: ['./designer-revenue-chart.component.scss']
})
export class DesignerRevenueChartComponent implements OnChanges {
  @Input() items: DesignerRevenueItem[] = [];
  /** Order revenue currency for designer-attributed totals (designer payouts may use PKR separately). */
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
    const labels = this.items.map(i => (i.designerName.length > 25 ? i.designerName.substring(0, 25) + '...' : i.designerName));
    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.totalRevenue) }],
      chart: { type: 'bar', height: Math.max(280, this.items.length * 36), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '70%' } },
      colors: ['#10b981'],
      xaxis: { categories: labels, labels: { formatter: axis } },
      yaxis: { labels: { maxWidth: 150 } },
      dataLabels: { enabled: true, formatter: axis },
      tooltip: { y: { formatter: (v: number) => formatCurrencyAmount(v, code) } }
    };
  }
}
