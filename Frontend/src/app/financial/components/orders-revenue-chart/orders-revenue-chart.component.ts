import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { OrdersVsRevenueItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-orders-revenue-chart',
  templateUrl: './orders-revenue-chart.component.html',
  styleUrls: ['./orders-revenue-chart.component.scss']
})
export class OrdersRevenueChartComponent implements OnChanges {
  @Input() items: OrdersVsRevenueItem[] = [];

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
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
        { opposite: true, min: 0, title: { text: 'Revenue' }, labels: { formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) } }
      ],
      legend: { position: 'top' },
      dataLabels: { enabled: false },
      tooltip: {
        shared: true,
        y: [
          { formatter: (v: number) => v },
          { formatter: (v: number) => '$' + v.toLocaleString() }
        ]
      }
    };
  }
}
