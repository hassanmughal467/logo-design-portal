import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { PackageRevenueItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-package-revenue-chart',
  templateUrl: './package-revenue-chart.component.html',
  styleUrls: ['./package-revenue-chart.component.scss']
})
export class PackageRevenueChartComponent implements OnChanges {
  @Input() items: PackageRevenueItem[] = [];
  @Input() loading = false;

  chartOptions: any = null;
  readonly colors = ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6'];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.revenue) }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, columnWidth: '60%', distributed: true } },
      colors: this.colors,
      xaxis: { categories: this.items.map(i => i.package) },
      yaxis: { min: 0, labels: { formatter: (v: number) => '$' + v } },
      dataLabels: { enabled: true, formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) },
      tooltip: { y: { formatter: (v: number) => '$' + v.toLocaleString() } }
    };
  }
}
