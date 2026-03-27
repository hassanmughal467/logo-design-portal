import { TestBed } from '@angular/core/testing';
import { LoggerService } from './logger.service';

describe('LoggerService', () => {
  let service: LoggerService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [LoggerService] });
    service = TestBed.inject(LoggerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('warn forwards to console.warn', () => {
    const spy = spyOn(console, 'warn');
    service.warn('a', 'b');
    expect(spy).toHaveBeenCalledWith('a', 'b');
  });

  it('error forwards to console.error', () => {
    const spy = spyOn(console, 'error');
    service.error('e');
    expect(spy).toHaveBeenCalledWith('e');
  });
});
