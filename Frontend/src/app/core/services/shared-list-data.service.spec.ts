import { of } from 'rxjs';
import { ApiService } from './api.service';
import { SharedListDataService, SHARED_LIST_PAGE_SIZE } from './shared-list-data.service';

describe('SharedListDataService', () => {
  let service: SharedListDataService;
  let api: jasmine.SpyObj<ApiService>;

  const page = (items: unknown[], totalPages = 1): unknown => ({
    data: {
      items,
      total: items.length,
      page: 1,
      pageSize: SHARED_LIST_PAGE_SIZE
    },
    meta: { totalPages }
  });

  beforeEach(() => {
    api = jasmine.createSpyObj<ApiService>('ApiService', ['get']);
    service = new SharedListDataService(api);
  });

  it('searchUsers encodes query, optional role, and clamps limit', (done) => {
    api.get.and.returnValue(of([]));

    service.searchUsers(' jane + client ', 'Client Manager', 999).subscribe((result) => {
      expect(result).toEqual([]);
      expect(api.get).toHaveBeenCalledWith(
        'users/search?query=jane%20%2B%20client&limit=20&role=Client%20Manager'
      );
      done();
    });
  });

  it('searchUsers clamps low limits and omits blank roles', (done) => {
    api.get.and.returnValue(of([]));

    service.searchUsers('a/b', null, -3).subscribe(() => {
      expect(api.get).toHaveBeenCalledWith('users/search?query=a%2Fb&limit=1');
      done();
    });
  });

  it('getAllUsers fetches the first page and caches the shared result', (done) => {
    api.get.and.returnValue(of(page([{ id: 'u1' }])));

    service.getAllUsers().subscribe((first) => {
      service.getAllUsers().subscribe((second) => {
        expect(first).toEqual([{ id: 'u1' }]);
        expect(second).toEqual([{ id: 'u1' }]);
        expect(api.get).toHaveBeenCalledTimes(1);
        expect(api.get).toHaveBeenCalledWith(`users?page=1&pageSize=${SHARED_LIST_PAGE_SIZE}`);
        done();
      });
    });
  });

  it('clearUsersCache causes a fresh users fetch', (done) => {
    api.get.and.returnValues(
      of(page([{ id: 'u1' }])),
      of(page([{ id: 'u2' }]))
    );

    service.getAllUsers().subscribe((first) => {
      service.clearUsersCache();
      service.getAllUsers().subscribe((second) => {
        expect(first).toEqual([{ id: 'u1' }]);
        expect(second).toEqual([{ id: 'u2' }]);
        expect(api.get).toHaveBeenCalledTimes(2);
        done();
      });
    });
  });

  it('getAllOrdersAdmin fetches multiple pages and combines results', (done) => {
    api.get.and.callFake((endpoint: string): any => {
      if (endpoint.includes('page=1')) {
        return of(page([{ id: 'o1' }], 3));
      }
      if (endpoint.includes('page=2')) {
        return of(page([{ id: 'o2' }]));
      }
      return of(page([{ id: 'o3' }]));
    });

    service.getAllOrdersAdmin().subscribe((orders) => {
      expect(orders).toEqual([{ id: 'o1' }, { id: 'o2' }, { id: 'o3' }]);
      expect(api.get.calls.allArgs().map(args => args[0])).toEqual([
        `orders?page=1&pageSize=${SHARED_LIST_PAGE_SIZE}`,
        `orders?page=2&pageSize=${SHARED_LIST_PAGE_SIZE}`,
        `orders?page=3&pageSize=${SHARED_LIST_PAGE_SIZE}`
      ]);
      done();
    });
  });

  it('uncached users and orders methods bypass cached streams', (done) => {
    api.get.and.returnValues(
      of(page([{ id: 'cached-user' }])),
      of(page([{ id: 'fresh-user' }])),
      of(page([{ id: 'cached-order' }])),
      of(page([{ id: 'fresh-order' }]))
    );

    service.getAllUsers().subscribe(() => {
      service.fetchAllUsersUncached().subscribe((users) => {
        service.getAllOrdersAdmin().subscribe(() => {
          service.fetchAllOrdersUncached().subscribe((orders) => {
            expect(users).toEqual([{ id: 'fresh-user' }]);
            expect(orders).toEqual([{ id: 'fresh-order' }]);
            expect(api.get).toHaveBeenCalledTimes(4);
            done();
          });
        });
      });
    });
  });

  it('single-page pagination returns immediately without requesting additional pages', (done) => {
    api.get.and.returnValue(of(page([{ id: 'only' }], 1)));

    service.fetchAllUsersUncached().subscribe((users) => {
      expect(users).toEqual([{ id: 'only' }]);
      expect(api.get).toHaveBeenCalledTimes(1);
      done();
    });
  });

  it('malformed totalPages is normalized to one page', (done) => {
    api.get.and.returnValue(of({
      data: { items: [{ id: 'safe' }], total: 10, pageSize: SHARED_LIST_PAGE_SIZE },
      meta: { totalPages: Number.POSITIVE_INFINITY }
    }));

    service.fetchAllUsersUncached().subscribe((users) => {
      expect(users).toEqual([{ id: 'safe' }]);
      expect(api.get).toHaveBeenCalledTimes(1);
      done();
    });
  });

  it('excessive totalPages is capped before requesting additional pages', (done) => {
    api.get.and.callFake((endpoint: string): any => {
      if (endpoint.includes('page=1')) {
        return of(page([{ id: 1 }], 501));
      }
      const match = endpoint.match(/page=(\d+)/);
      const id = match ? Number(match[1]) : 0;
      return of(page([{ id }]));
    });

    service.fetchAllOrdersUncached().subscribe((orders) => {
      expect(orders.length).toBe(500);
      expect(orders[0]).toEqual({ id: 1 });
      expect(orders[499]).toEqual({ id: 500 });
      expect(api.get).toHaveBeenCalledTimes(500);
      expect(api.get).toHaveBeenCalledWith(`orders?page=500&pageSize=${SHARED_LIST_PAGE_SIZE}`);
      expect(api.get).not.toHaveBeenCalledWith(`orders?page=501&pageSize=${SHARED_LIST_PAGE_SIZE}`);
      done();
    });
  });
});
