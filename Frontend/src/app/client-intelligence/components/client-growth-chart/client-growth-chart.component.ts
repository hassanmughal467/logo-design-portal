import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-client-growth-chart',
  templateUrl: './client-growth-chart.component.html',
  styleUrls: ['./client-growth-chart.component.scss']
})
export class ClientGrowthChartComponent implements OnChanges {
  @Input() items: any[] = [];
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const displayItems = this.items.slice(0, 10);
    const labels = displayItems.map(i => (i.clientName?.length > 25 ? i.clientName.substring(0, 25) + '...' : i.clientName) || 'Unknown');
    const changeData = displayItems.map(i => i.revenueChangePercent);
    const colors = displayItems.map(i =>
      i.growthStatus === 'Increase' ? '#10b981' : i.growthStatus === 'Decrease' ? '#ef4444' : '#6b7280'
    );
    this.chartOptions = {
      series: [{ name: 'Revenue Change %', data: changeData }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 4, columnWidth: '70%', distributed: true } },
      colors,
      xaxis: { categories: labels },
      yaxis: { labels: { formatter: (v: number) => v + '%' } },
      tooltip: {
        y: { formatter: (v: number) => v + '%' }
      },
      dataLabels: { enabled: true, formatter: (v: number) => v + '%' }
    };
  }
}
