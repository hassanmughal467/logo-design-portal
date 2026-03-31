import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { OrderValueTrendItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-order-value-chart',
  templateUrl: './order-value-chart.component.html',
  styleUrls: ['./order-value-chart.component.scss']
})
export class OrderValueChartComponent implements OnChanges {
  @Input() items: OrderValueTrendItem[] = [];

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    this.chartOptions = {
      series: [{ name: 'Avg Order Value', data: this.items.map(i => i.averageOrderValue) }],
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#0d47a1'],
      xaxis: { categories: this.items.map(i => i.month), labels: { rotate: -45 } },
      yaxis: { min: 0, labels: { formatter: (v: number) => '$' + v } },
      dataLabels: { enabled: false },
      tooltip: { y: { formatter: (v: number) => '$' + v.toLocaleString() } }
    };
  }
}
