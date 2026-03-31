import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  DesignerAnalytics,
  DesignerPerformanceItem
} from '@core/services/admin-analytics.service';

@Component({
  selector: 'app-designer-analytics',
  templateUrl: './designer-analytics.component.html',
  styleUrls: ['./designer-analytics.component.scss']
})
export class DesignerAnalyticsComponent implements OnChanges {
  @Input() data: DesignerAnalytics | null = null;

  designerChart: any;
  designerActivityChart: any;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const items = this.data!.designerPerformance || [];
    this.designerChart = {
      series: [{ name: 'Orders Completed', data: items.map((i: DesignerPerformanceItem) => i.ordersCompleted) }],
      chart: { type: 'bar', height: Math.max(280, items.length * 40), toolbar: { show: false }, horizontal: true },
      plotOptions: { bar: { borderRadius: 6, barHeight: '70%', horizontal: true } },
      colors: ['#0d47a1'],
      xaxis: { categories: items.map((i: DesignerPerformanceItem) => i.designerName) },
      yaxis: { labels: { maxWidth: 120 } },
      dataLabels: { enabled: true },
      legend: { show: false }
    };

    const timeline = this.data!.designerActivityTimeline || [];
    if (timeline.length > 0) {
      const byDateKey = timeline.reduce((acc: Record<string, { date: string; sum: number }>, t: any) => {
        if (!acc[t.dateKey]) acc[t.dateKey] = { date: t.date, sum: 0 };
        acc[t.dateKey].sum += t.ordersCompleted || 0;
        return acc;
      }, {});
      const sorted = Object.values(byDateKey).sort((a, b) => a.date.localeCompare(b.date));
      this.designerActivityChart = {
        series: [{ name: 'Orders Completed', data: sorted.map(s => s.sum) }],
        chart: { type: 'area', height: 260, toolbar: { show: false } },
        stroke: { curve: 'smooth', width: 2 },
        fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
        colors: ['#0d47a1'],
        xaxis: { categories: sorted.map(s => s.date) },
        yaxis: { min: 0 },
        dataLabels: { enabled: false }
      };
    } else {
      this.designerActivityChart = null;
    }
  }

  get designers(): DesignerPerformanceItem[] {
    return this.data?.designerPerformance || [];
  }
}
