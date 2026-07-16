import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ApiService } from './api.service';
import { environment } from '@environments/environment';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;
  const basePrefix = `${environment.apiUrl}/api${environment.apiVersion ? '/' + environment.apiVersion : ''}`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiService]
    });
    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('get should call correct URL', (done) => {
    service.get<{ id: string }>('orders/1').subscribe((res) => {
      expect(res.id).toBe('a');
      done();
    });
    const req = httpMock.expectOne(`${basePrefix}/orders/1`);
    expect(req.request.method).toBe('GET');
    req.flush({ id: 'a' });
  });

  it('post JSON should set Content-Type', () => {
    service.post('auth/login', { email: 'x' }).subscribe();
    const req = httpMock.expectOne(`${basePrefix}/auth/login`);
    expect(req.request.headers.get('Content-Type')).toContain('application/json');
    req.flush({});
  });

  it('post FormData should not force JSON headers', () => {
    const fd = new FormData();
    service.post('files', fd).subscribe();
    const req = httpMock.expectOne(`${basePrefix}/files`);
    expect(req.request.body).toBe(fd);
    req.flush({});
  });

  it('put should target built url', () => {
    service.put('users/1', { name: 'n' }).subscribe();
    const req = httpMock.expectOne(`${basePrefix}/users/1`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('delete with body sends JSON headers', () => {
    service.delete('orders/1', { reason: 'x' }).subscribe();
    const req = httpMock.expectOne(`${basePrefix}/orders/1`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.headers.get('Content-Type')).toContain('application/json');
    req.flush({});
  });

  it('delete without body omits Content-Type', () => {
    service.delete('orders/1').subscribe();
    const req = httpMock.expectOne(`${basePrefix}/orders/1`);
    expect(req.request.body).toBeNull();
    req.flush({} as object);
  });

  it('patch should target built url', () => {
    service.patch('x/1', { k: 1 }).subscribe();
    const req = httpMock.expectOne(`${basePrefix}/x/1`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('buildUrl strips leading api/ from endpoint', () => {
    service.get('api/orders').subscribe();
    httpMock.expectOne(`${basePrefix}/orders`).flush([]);
  });

  it('buildUrl strips leading slash', () => {
    service.get('/widgets').subscribe();
    httpMock.expectOne(`${basePrefix}/widgets`).flush([]);
  });

  it('getBlob should use blob responseType', () => {
    service.getBlob('invoices/1/pdf').subscribe();
    const req = httpMock.expectOne(`${basePrefix}/invoices/1/pdf`);
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob());
  });

  it('getBaseUrl exposes configured API root', () => {
    expect(service.getBaseUrl()).toBe(basePrefix);
  });

  describe('static extractItems', () => {
    it('returns empty array when response null', () => {
      expect(ApiService.extractItems(null)).toEqual([]);
    });

    it('returns array responses as-is', () => {
      expect(ApiService.extractItems<number>([1, 2])).toEqual([1, 2]);
    });

    it('reads data.items shape', () => {
      const res = { data: { items: [1, 2] } };
      expect(ApiService.extractItems<number>(res)).toEqual([1, 2]);
    });

    it('reads legacy items shape', () => {
      expect(ApiService.extractItems<string>({ items: ['a'] })).toEqual(['a']);
    });

    it('returns empty when items not an array', () => {
      expect(ApiService.extractItems({ data: { items: 1 } })).toEqual([]);
    });
  });

  describe('static extractPagedMeta', () => {
    it('derives meta from nested response', () => {
      const meta = ApiService.extractPagedMeta({
        data: { total: 40, page: 2, pageSize: 20 },
        meta: { totalPages: 2 }
      });
      expect(meta.total).toBe(40);
      expect(meta.page).toBe(2);
      expect(meta.pageSize).toBe(20);
      expect(meta.totalPages).toBe(2);
    });

    it('computes totalPages when meta omitted', () => {
      const meta = ApiService.extractPagedMeta({
        data: { total: 50, page: 1, pageSize: 10 },
        meta: {}
      });
      expect(meta.totalPages).toBe(5);
    });

    it('uses zero totalPages when pageSize missing', () => {
      const meta = ApiService.extractPagedMeta({ data: { total: 5 }, meta: {} });
      expect(meta.totalPages).toBe(0);
    });
  });
});
