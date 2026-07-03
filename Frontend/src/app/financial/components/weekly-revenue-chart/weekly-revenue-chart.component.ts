import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { WeeklyRevenueItem } from '@core/services/financial-analytics.service';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-weekly-revenue-chart',
  templateUrl: './weekly-revenue-chart.component.html',
  styleUrls: ['./weekly-revenue-chart.component.scss']
})
export class WeeklyRevenueChartComponent implements OnChanges {
  @Input() items: WeeklyRevenueItem[] = [];
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
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#8b5cf6'],
      xaxis: { categories: this.items.map(i => i.dayOfWeek) },
      yaxis: { min: 0, labels: { formatter: axis } },
      dataLabels: { enabled: false },
      tooltip: { y: { formatter: (v: number) => formatCurrencyAmount(v, code) } }
    };
  }
}
