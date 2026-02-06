import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-project-detail',
  template: '<p>Project detail page - Coming soon</p>'
})
export class ProjectDetailComponent implements OnInit {
  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    const projectId = this.route.snapshot.paramMap.get('id');
    console.log('Project ID:', projectId);
  }
}
