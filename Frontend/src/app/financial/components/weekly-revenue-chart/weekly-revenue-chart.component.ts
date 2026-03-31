import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { WeeklyRevenueItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-weekly-revenue-chart',
  templateUrl: './weekly-revenue-chart.component.html',
  styleUrls: ['./weekly-revenue-chart.component.scss']
})
export class WeeklyRevenueChartComponent implements OnChanges {
  @Input() items: WeeklyRevenueItem[] = [];

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.revenue) }],
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#8b5cf6'],
      xaxis: { categories: this.items.map(i => i.dayOfWeek) },
      yaxis: { min: 0, labels: { formatter: (v: number) => '$' + v } },
      dataLabels: { enabled: false },
      tooltip: { y: { formatter: (v: number) => '$' + v.toLocaleString() } }
    };
  }
}
