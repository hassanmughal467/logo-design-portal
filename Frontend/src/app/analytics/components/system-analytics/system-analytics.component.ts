import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  SystemAnalytics,
  DailyActivityItem,
  NotificationActivityItem,
  FileUploadAnalytics
} from '@core/services/admin-analytics.service';

@Component({
  selector: 'app-system-analytics',
  templateUrl: './system-analytics.component.html',
  styleUrls: ['./system-analytics.component.scss']
})
export class SystemAnalyticsComponent implements OnChanges {
  @Input() data: SystemAnalytics | null = null;

  dailyActivityChart: any;
  notificationActivityChart: any;
  fileUploadByDayChart: any;
  fileUploadByTypeChart: any;

  readonly chartColors = ['#0d47a1', '#10b981', '#f59e0b', '#8b5cf6', '#ef4444'];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.buildCharts();
    }
  }

  private buildCharts(): void {
    if (!this.data) return;

    const da = this.data.dailyActivity || [];
    this.dailyActivityChart = da.length > 0 ? {
      series: [
        { name: 'Orders Created', data: da.map((i: DailyActivityItem) => i.ordersCreated) },
        { name: 'Files Uploaded', data: da.map((i: DailyActivityItem) => i.filesUploaded) },
        { name: 'Revisions Requested', data: da.map((i: DailyActivityItem) => i.revisionsRequested) },
        { name: 'Approvals Completed', data: da.map((i: DailyActivityItem) => i.approvalsCompleted) }
      ],
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: this.chartColors,
      xaxis: { categories: da.map((i: DailyActivityItem) => i.date) },
      yaxis: { min: 0 },
      legend: { position: 'top' },
      dataLabels: { enabled: false }
    } : null;

    const na = this.data.notificationActivity || [];
    this.notificationActivityChart = na.length > 0 ? {
      series: na.map((i: NotificationActivityItem) => i.count),
      chart: { type: 'donut', height: 260 },
      labels: na.map((i: NotificationActivityItem) => i.type),
      colors: this.chartColors,
      legend: { position: 'bottom' },
      dataLabels: { enabled: true }
    } : null;

    const fa = this.data.fileUploadAnalytics;
    if (fa?.uploadsByDay?.length) {
      this.fileUploadByDayChart = {
        series: [{ name: 'Uploads', data: fa.uploadsByDay.map((i: any) => i.count) }],
        chart: { type: 'area', height: 260, toolbar: { show: false } },
        stroke: { curve: 'smooth', width: 2 },
        fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
        colors: ['#0d47a1'],
        xaxis: { categories: fa.uploadsByDay.map((i: any) => i.date) },
        yaxis: { min: 0 },
        dataLabels: { enabled: false }
      };
    } else {
      this.fileUploadByDayChart = null;
    }

    if (fa?.uploadsByType?.length) {
      this.fileUploadByTypeChart = {
        series: [{ name: 'Uploads', data: fa.uploadsByType.map((i: any) => i.count) }],
        chart: { type: 'bar', height: 260, toolbar: { show: false } },
        plotOptions: { bar: { borderRadius: 6, columnWidth: '70%' } },
        colors: ['#10b981'],
        xaxis: { categories: fa.uploadsByType.map((i: any) => i.fileType) },
        yaxis: { min: 0 },
        dataLabels: { enabled: true }
      };
    } else {
      this.fileUploadByTypeChart = null;
    }
  }

  get fileUploadAnalytics(): FileUploadAnalytics | null {
    return this.data?.fileUploadAnalytics ?? null;
  }
}
