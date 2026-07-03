import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { OrdersVsRevenueItem } from '@core/services/financial-analytics.service';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-orders-revenue-chart',
  templateUrl: './orders-revenue-chart.component.html',
  styleUrls: ['./orders-revenue-chart.component.scss']
})
export class OrdersRevenueChartComponent implements OnChanges {
  @Input() items: OrdersVsRevenueItem[] = [];
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
    const revAxis = (v: number) => compactCurrencyLabel(v, code, this.currencyMixed);
    this.chartOptions = {
      series: [
        { name: 'Orders Count', data: this.items.map(i => i.ordersCount), type: 'column' },
        { name: 'Revenue', data: this.items.map(i => i.revenue), type: 'line' }
      ],
      chart: { type: 'line', height: 320, toolbar: { show: false } },
      stroke: { width: [0, 3], curve: 'smooth' },
      plotOptions: { bar: { columnWidth: '50%', borderRadius: 4 } },
      colors: ['#3b82f6', '#10b981'],
      xaxis: { categories: this.items.map(i => i.month), labels: { rotate: -45 } },
      yaxis: [
        { min: 0, title: { text: 'Orders' }, labels: { formatter: (v: number) => Math.round(v) } },
        { opposite: true, min: 0, title: { text: 'Revenue' }, labels: { formatter: revAxis } }
      ],
      legend: { position: 'top' },
      dataLabels: { enabled: false },
      tooltip: {
        shared: true,
        y: [
          { formatter: (v: number) => String(v) },
          { formatter: (v: number) => formatCurrencyAmount(v, code) }
        ]
      }
    };
  }
}
