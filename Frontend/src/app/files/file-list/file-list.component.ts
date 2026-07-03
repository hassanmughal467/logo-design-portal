import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { SharedListDataService } from '@core/services/shared-list-data.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { LogoGroup, ClientGroup, GroupedFilesResponse, FileCategory, FileStatus, LogoFile } from '@shared/models/file.model';
import { TagSeverity } from '@shared/types/primeng.types';

@Component({
  selector: 'app-file-list',
  templateUrl: './file-list.component.html',
  styleUrls: ['./file-list.component.scss']
})
export class FileListComponent implements OnInit, OnDestroy {
  // Data structures
  logoGroups: LogoGroup[] = []; // For non-SuperAdmin view
  clientGroups: ClientGroup[] = []; // For SuperAdmin hierarchical view (filtered)
  allFiles: LogoFile[] = []; // All files for filtering
  
  // Original unfiltered data (for filtering)
  originalClientGroups: ClientGroup[] = []; // Original unfiltered client groups
  originalLogoGroups: LogoGroup[] = []; // Original unfiltered logo groups
  
  viewMode: 'table' | 'grid' = 'table';
  
  // User role
  isSuperAdmin = false;
  isAdmin = false;
  
  // Search-first UI
  globalSearch = '';
  
  // Filters
  clientFilter: string | null = null;
  logoNameFilter = '';
  fileCategoryFilter: string | null = null;
  fileStatusFilter: string | null = null;
  uploadedByFilter: string | null = null;
  dateFrom: Date | null = null;
  dateTo: Date | null = null;
  
  // Pagination
  pageNumber = 1;
  pageSize = 10;
  totalLogos = 0;
  totalClients = 0;
  totalFiles = 0;
  totalPages = 0;
  
  // Expanded state
  expandedLogos = new Set<string>();
  expandedClients = new Set<string>();
  
  // Options for dropdowns
  clientOptions: { label: string; value: string }[] = [];
  uploadedByOptions: { label: string; value: string }[] = [];
  
  // User map for role lookup
  userMap = new Map<string, { name: string; role: string }>();
  
  // Map User.Id to ClientProfile.Id for filtering
  userIdToClientProfileIdMap = new Map<string, string>();
  
  // Store client users for matching
  clientUsersForMatching: any[] = [];
  
  fileCategoryOptions = [
    { label: 'All Categories', value: null },
    { label: 'Source', value: FileCategory.Source },
    { label: 'Print', value: FileCategory.Print },
    { label: 'Web', value: FileCategory.Web },
    { label: 'Embroidery', value: FileCategory.Embroidery }
  ];
  
  fileStatusOptions = [
    { label: 'All Statuses', value: null },
    { label: 'Draft', value: FileStatus.Draft },
    { label: 'Final', value: FileStatus.Final },
    { label: 'Approved', value: FileStatus.Approved }
  ];

  private destroy$ = new Subject<void>();
  private filterSubject = new Subject<void>();
  private searchSubject = new Subject<string>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private route: ActivatedRoute,
    private sharedListData: SharedListDataService
  ) {}

  ngOnInit(): void {
    
    // Check user role
    const user = this.authService.getCurrentUser();
    this.isSuperAdmin = user?.role === 'SuperAdmin' || user?.roleName === 'SuperAdmin';
    this.isAdmin = (user?.role === 'Admin' || user?.roleName === 'Admin') || this.isSuperAdmin;
    
    // Load clients and users for filters (Admin/SuperAdmin only)
    // Note: Clients will be loaded before grouping in loadOrdersAndGroup
    if (this.isSuperAdmin) {
      this.loadClientsForFilter();
    }
    // Only load users for Admin/SuperAdmin (clients don't have access to /users endpoint)
    if (this.isAdmin || this.isSuperAdmin) {
      this.loadUsersForFilter();
    }
    
    // Debounce filter changes
    this.filterSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.pageNumber = 1;
      this.applyFilters();
    });
    
    // Debounce search
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.pageNumber = 1;
      this.applyFilters();
    });

    this.loadFiles();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClientsForFilter(): Promise<void> {
    return new Promise((resolve) => {
      this.sharedListData
        .getAllUsers()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (usersRaw) => {
            const users = usersRaw as any[];
            // Build mapping of User.Id to ClientProfile.Id
            this.userIdToClientProfileIdMap.clear();
            const clientUsers = users.filter(u => u.role === 'Client' || u.roleName === 'Client');
            
            clientUsers.forEach(user => {
              // If user has clientProfile with id, map it (from users API response)
              if (user.clientProfile && user.clientProfile.id) {
                this.userIdToClientProfileIdMap.set(user.id, user.clientProfile.id);
              }
            });
            
            // Also store client users for later matching
            this.clientUsersForMatching = clientUsers;
            
            this.clientOptions = [
              { label: 'All Clients', value: null as any },
              ...clientUsers.map(user => {
                const fullName = [user.firstName, user.lastName].filter(Boolean).join(' ').trim();
                const companyName = user.clientProfile?.companyName || user.companyName;
                const baseLabel = fullName || companyName || user.email || 'Unknown Client';
                const label = fullName && companyName ? `${fullName} (${companyName})` : baseLabel;
                return { label, value: user.id };
              })
            ];
            resolve();
          },
          error: () => {
            this.clientOptions = [{ label: 'All Clients', value: null as any }];
            resolve();
          }
        });
    });
  }

  loadUsersForFilter(): void {
    this.sharedListData
      .getAllUsers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (usersRaw) => {
          const users = usersRaw as any[];
          // Build user map for role lookup
          users.forEach(user => {
            const userId = user.id;
            const userName = `${user.firstName} ${user.lastName}`;
            const userRole = user.role || user.roleName || 'Client';
            this.userMap.set(userId, { name: userName, role: userRole });
          });
          
          // Get unique uploadedBy names from files
          const uploadedBySet = new Set<string>();
          this.allFiles.forEach(file => {
            if (file.uploadedByName) {
              uploadedBySet.add(file.uploadedByName);
            }
          });
          
          this.uploadedByOptions = [
            { label: 'All Users', value: null as any },
            ...Array.from(uploadedBySet).map(name => ({
              label: name,
              value: name
            }))
          ];
          
          // Enrich files with user roles
          this.enrichFilesWithUserRoles();
        },
        error: () => {
          this.uploadedByOptions = [{ label: 'All Users', value: null as any }];
        }
      });
  }

  enrichFilesWithUserRoles(): void {
    // Enrich all files with user roles
    this.allFiles.forEach(file => {
      if (file.uploadedBy) {
        const userInfo = this.userMap.get(file.uploadedBy);
        if (userInfo) {
          file.uploadedByRole = userInfo.role;
        }
      }
    });
    
    // Also enrich files in groups
    this.originalClientGroups.forEach(clientGroup => {
      clientGroup.logos.forEach(logo => {
        logo.files.forEach(file => {
          if (file.uploadedBy && !file.uploadedByRole) {
            const userInfo = this.userMap.get(file.uploadedBy);
            if (userInfo) {
              file.uploadedByRole = userInfo.role;
            }
          }
        });
      });
    });
    
    this.originalLogoGroups.forEach(logo => {
      logo.files.forEach(file => {
        if (file.uploadedBy && !file.uploadedByRole) {
          const userInfo = this.userMap.get(file.uploadedBy);
          if (userInfo) {
            file.uploadedByRole = userInfo.role;
          }
        }
      });
    });
  }

  loadFiles(): void {
    this.apiService.get<any>(`files?page=${this.pageNumber}&pageSize=${this.pageSize}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const files = ApiService.extractItems<any>(response);
          const meta = ApiService.extractPagedMeta(response);
          this.totalFiles = meta.total;
          this.totalPages = this.pageSize > 0 ? Math.ceil(this.totalFiles / this.pageSize) : 0;
          if (!files || files.length === 0) {
            this.logoGroups = [];
            this.clientGroups = [];
            this.allFiles = [];
            this.totalLogos = 0;
            this.totalClients = 0;
            this.totalFiles = 0;
            this.totalPages = 0;
            this.messageService.add({
              severity: 'info',
              summary: 'No Files',
              detail: 'No files found. Files will appear here after logo approval.'
            });
            return;
          }

          // Map files and enrich with user roles if available
          this.allFiles = files.map(file => {
            const mappedFile: any = {
              ...file,
              fileCategory: file.fileCategory || null,
              fileStatus: file.fileStatus || (file.isFinalVersion && file.isAdminApproved ? 'Approved' : file.fileType === 'Final' ? 'Final' : 'Draft')
            };
            
            // Try to get user role from userMap if available
            if (file.uploadedBy && this.userMap.has(file.uploadedBy)) {
              mappedFile.uploadedByRole = this.userMap.get(file.uploadedBy)!.role;
            }
            
            return mappedFile;
          });

          // Load orders to get client and logo information
          this.loadOrdersAndGroup();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to load files. Please ensure the backend is running.'
          });
          this.logoGroups = [];
          this.clientGroups = [];
          this.allFiles = [];
        }
      });
  }

  loadOrdersAndGroup(): void {
    const user = this.authService.getCurrentUser();

    const finishError = () => {
      if (this.isSuperAdmin) {
        this.groupByClient(new Map());
      } else {
        this.groupByLogo(new Map());
      }
      this.applyFilters();
      
    };

    const handleOrdersResponse = (response: unknown) => {
          const orders = Array.isArray(response)
            ? response
            : (response as any)?.items ?? [];
          const orderMap = new Map<string, any>();
          orders.forEach((order: any) => {
            orderMap.set(order.id, order);
          });

          // Load clients first to populate clientOptions before grouping
          if (this.isSuperAdmin) {
            this.loadClientsForFilter().then(() => {
              // Build mapping from orders - order.clientId is ClientProfile.Id
              // Find matching User.Id from clientOptions by matching client info
              orders.forEach((order: any) => {
                if (order.clientId && order.client) {
                  // First, try to find by clientProfile.id if available in users
                  let matchingUserId: string | null = null;
                  
                  // Try to find user whose clientProfile.id matches order.clientId
                  const userWithMatchingProfile = this.clientUsersForMatching.find(user => 
                    user.clientProfile && user.clientProfile.id === order.clientId
                  );
                  
                  if (userWithMatchingProfile) {
                    matchingUserId = userWithMatchingProfile.id;
                  } else {
                    // Fallback: Try to find matching user by name/company
                    const orderClientFirstName = order.client.firstName || '';
                    const orderClientLastName = order.client.lastName || '';
                    const orderClientFullName = orderClientFirstName && orderClientLastName 
                      ? `${orderClientFirstName} ${orderClientLastName}`.trim()
                      : '';
                    const orderClientCompany = order.client.companyName || '';
                    
                    const matchingUser = this.clientOptions.find(opt => {
                      if (!opt.value || opt.value === null || opt.label === 'All Clients') return false;
                      
                      // Extract name from label (format: "FirstName LastName (CompanyName)")
                      const labelParts = opt.label.split(' (');
                      const optionName = labelParts[0].trim();
                      const optionCompany = labelParts.length > 1 ? labelParts[1].replace(')', '').trim() : '';
                      
                      // Match by full name (exact) or company name (exact)
                      const nameMatch = orderClientFullName && optionName === orderClientFullName;
                      const companyMatch = orderClientCompany && optionCompany && optionCompany === orderClientCompany;
                      
                      return nameMatch || companyMatch;
                    });
                    
                    if (matchingUser && matchingUser.value) {
                      matchingUserId = matchingUser.value;
                    }
                  }
                  
                  if (matchingUserId) {
                    this.userIdToClientProfileIdMap.set(matchingUserId, order.clientId);
                  }
                }
              });
              
              this.groupByClient(orderMap);
              // Only load users for Admin/SuperAdmin
              if (this.isAdmin || this.isSuperAdmin) {
                this.loadUsersForFilter();
              }
              this.applyFilters();
              
            });
          } else {
            this.groupByLogo(orderMap);
            // Only load users for Admin/SuperAdmin
            if (this.isAdmin || this.isSuperAdmin) {
              this.loadUsersForFilter();
            }
            this.applyFilters();
            
          }
    };

    if (user?.role === 'Client') {
      this.apiService.get<any>('orders/my-orders')
        .pipe(takeUntil(this.destroy$))
        .subscribe({ next: handleOrdersResponse, error: finishError });
    } else if (user?.role === 'Designer') {
      this.apiService.get<any>('orders/assigned-orders')
        .pipe(takeUntil(this.destroy$))
        .subscribe({ next: handleOrdersResponse, error: finishError });
    } else {
      this.sharedListData
        .fetchAllOrdersUncached()
        .pipe(takeUntil(this.destroy$))
        .subscribe({ next: handleOrdersResponse, error: finishError });
    }
  }

  groupByClient(orderMap: Map<string, any>): void {
    const clientMap = new Map<string, ClientGroup>();
    const logoMap = new Map<string, LogoGroup>();

    // First, group files by order (logo)
    this.allFiles.forEach(file => {
      if (!file.orderId) return;
      
      if (!logoMap.has(file.orderId)) {
        const order = orderMap.get(file.orderId);
        const clientId = order?.clientId || '';
        
        // Extract client name - try multiple possible structures
        let clientName = 'Unknown Client';
        let clientCompanyName = undefined;
        
        if (order?.client) {
          // Try order.client structure (from OrderResponseDto)
          if (order.client.firstName && order.client.lastName) {
            clientName = `${order.client.firstName} ${order.client.lastName}`;
          } else if (order.client.companyName) {
            clientName = order.client.companyName;
          }
          clientCompanyName = order.client.companyName;
        } else if (order?.clientName) {
          // Try direct clientName property
          clientName = order.clientName;
        }
        
        // If still unknown and we have clientId, try to get from clientOptions
        if (clientName === 'Unknown Client' && clientId) {
          const clientOption = this.clientOptions.find(opt => opt.value === clientId);
          if (clientOption && clientOption.label !== 'All Clients') {
            // Extract name from label (format: "FirstName LastName (CompanyName)")
            const labelParts = clientOption.label.split(' (');
            clientName = labelParts[0];
            if (labelParts.length > 1) {
              clientCompanyName = labelParts[1].replace(')', '');
            }
          }
        }
        
        logoMap.set(file.orderId, {
          orderId: file.orderId,
          logoName: order?.title || `Order ${file.orderId.substring(0, 8)}...`,
          clientId: clientId,
          clientName: clientName,
          clientCompanyName: clientCompanyName,
          createdAt: order?.createdAt ? new Date(order.createdAt) : (file.createdAt ? new Date(file.createdAt) : new Date()),
          fileCount: 0,
          files: []
        });
      }
      
      const logo = logoMap.get(file.orderId)!;
      logo.files.push(file);
      logo.fileCount = logo.files.length;
    });

    // Then, group logos by client
    logoMap.forEach((logo, orderId) => {
      // Only skip if clientId is truly empty (not just falsy)
      if (!logo.clientId || logo.clientId === '') {
        // Create an "Unknown Client" group for files without clientId
        const unknownClientId = 'unknown';
        if (!clientMap.has(unknownClientId)) {
          clientMap.set(unknownClientId, {
            clientId: unknownClientId,
            clientName: 'Unknown Client',
            clientCompanyName: undefined,
            clientEmail: undefined,
            logoCount: 0,
            totalFiles: 0,
            logos: []
          });
        }
        const client = clientMap.get(unknownClientId)!;
        client.logos.push(logo);
        client.logoCount = client.logos.length;
        client.totalFiles += logo.fileCount;
        return;
      }
      
      if (!clientMap.has(logo.clientId)) {
        const order = orderMap.get(orderId);
        
        // Use logo's client info, but try to get from order if missing
        let clientName = logo.clientName || 'Unknown Client';
        let clientCompanyName = logo.clientCompanyName;
        let clientEmail = order?.client?.email;
        
        // If client name is still unknown, try to get from order
        if (clientName === 'Unknown Client' && order?.client) {
          if (order.client.firstName && order.client.lastName) {
            clientName = `${order.client.firstName} ${order.client.lastName}`;
          } else if (order.client.companyName) {
            clientName = order.client.companyName;
          }
          clientCompanyName = order.client.companyName || clientCompanyName;
          clientEmail = order.client.email || clientEmail;
        }
        
        // If still unknown, try to get from clientOptions (loaded users)
        if (clientName === 'Unknown Client' && logo.clientId) {
          const clientOption = this.clientOptions.find(opt => opt.value === logo.clientId);
          if (clientOption && clientOption.label !== 'All Clients') {
            // Extract name from label (format: "FirstName LastName (CompanyName)")
            const labelParts = clientOption.label.split(' (');
            clientName = labelParts[0];
            if (labelParts.length > 1) {
              clientCompanyName = labelParts[1].replace(')', '');
            }
          }
        }
        
        clientMap.set(logo.clientId, {
          clientId: logo.clientId,
          clientName: clientName,
          clientCompanyName: clientCompanyName,
          clientEmail: clientEmail,
          logoCount: 0,
          totalFiles: 0,
          logos: []
        });
      }
      
      const client = clientMap.get(logo.clientId)!;
      client.logos.push(logo);
      client.logoCount = client.logos.length;
      client.totalFiles += logo.fileCount;
    });

    this.clientGroups = Array.from(clientMap.values());
    this.originalClientGroups = Array.from(clientMap.values()).map(cg => ({
      ...cg,
      logos: cg.logos.map(logo => ({
        ...logo,
        files: [...logo.files] // Deep copy
      }))
    })); // Store original with deep copy
    this.totalClients = this.clientGroups.length;
  }

  groupByLogo(orderMap: Map<string, any>): void {
    const grouped = new Map<string, LogoGroup>();
    
    this.allFiles.forEach(file => {
      if (!file.orderId) return;
      
      if (!grouped.has(file.orderId)) {
        const order = orderMap.get(file.orderId);
        grouped.set(file.orderId, {
          orderId: file.orderId,
          logoName: order?.title || `Order ${file.orderId.substring(0, 8)}...`,
          clientId: order?.clientId || '',
          clientName: order?.client?.firstName && order?.client?.lastName 
            ? `${order.client.firstName} ${order.client.lastName}` 
            : undefined,
          clientCompanyName: order?.client?.companyName || order?.clientName,
          createdAt: order?.createdAt ? new Date(order.createdAt) : (file.createdAt ? new Date(file.createdAt) : new Date()),
          fileCount: 0,
          files: []
        });
      }
      
      const group = grouped.get(file.orderId)!;
      group.files.push(file);
      group.fileCount = group.files.length;
    });

    this.logoGroups = Array.from(grouped.values());
    this.originalLogoGroups = Array.from(grouped.values()); // Store original
    this.totalLogos = this.logoGroups.length;
  }

  applyFilters(): void {
    if (this.isSuperAdmin) {
      this.applySuperAdminFilters();
    } else {
      this.applyRegularFilters();
    }
  }

  applySuperAdminFilters(): void {
    // Always start from original unfiltered data
    if (this.originalClientGroups.length === 0) {
      this.clientGroups = [];
      this.totalClients = 0;
      this.totalFiles = 0;
      return;
    }
    
    let filteredClientGroups = this.originalClientGroups.map(cg => ({
      ...cg,
      logos: cg.logos.map(logo => ({
        ...logo,
        files: [...logo.files] // Deep copy files
      }))
    }));

    // Client filter
    if (this.clientFilter) {
      // clientFilter contains User.Id, but clientGroups use ClientProfile.Id
      // Need to convert User.Id to ClientProfile.Id for comparison
      const clientProfileId = this.userIdToClientProfileIdMap.get(this.clientFilter);
      
      if (clientProfileId) {
        filteredClientGroups = filteredClientGroups.filter(cg => {
          // Compare ClientProfile.Id (include 'unknown' clientId if it matches)
          return String(cg.clientId) === String(clientProfileId);
        });
      } else {
        // If no mapping found, try direct comparison (in case clientId is already User.Id)
        filteredClientGroups = filteredClientGroups.filter(cg => {
          return String(cg.clientId) === String(this.clientFilter);
        });
      }
    }
    // When clientFilter is null (All Clients), show all including "Unknown Client"

    // Logo name filter
    if (this.logoNameFilter) {
      const searchLower = this.logoNameFilter.toLowerCase();
      filteredClientGroups = filteredClientGroups.map(cg => ({
        ...cg,
        logos: cg.logos.filter(logo => 
          logo.logoName.toLowerCase().includes(searchLower)
        )
      })).filter(cg => cg.logos.length > 0);
    }

    // Global search
    if (this.globalSearch) {
      const searchLower = this.globalSearch.toLowerCase();
      filteredClientGroups = filteredClientGroups.map(cg => ({
        ...cg,
        logos: cg.logos.map(logo => ({
          ...logo,
          files: logo.files.filter(file => 
            (file.originalFileName || file.fileName || '').toLowerCase().includes(searchLower) ||
            (file.uploadedByName || '').toLowerCase().includes(searchLower) ||
            logo.logoName.toLowerCase().includes(searchLower) ||
            (cg.clientName || '').toLowerCase().includes(searchLower) ||
            (cg.clientCompanyName || '').toLowerCase().includes(searchLower)
          )
        })).filter(logo => logo.files.length > 0)
      })).filter(cg => cg.logos.length > 0);
    }

    // File category filter
    if (this.fileCategoryFilter) {
      filteredClientGroups = filteredClientGroups.map(cg => ({
        ...cg,
        logos: cg.logos.map(logo => ({
          ...logo,
          files: logo.files.filter(file => file.fileCategory === this.fileCategoryFilter)
        })).filter(logo => logo.files.length > 0)
      })).filter(cg => cg.logos.length > 0);
    }

    // File status filter
    if (this.fileStatusFilter) {
      filteredClientGroups = filteredClientGroups.map(cg => ({
        ...cg,
        logos: cg.logos.map(logo => ({
          ...logo,
          files: logo.files.filter(file => file.fileStatus === this.fileStatusFilter)
        })).filter(logo => logo.files.length > 0)
      })).filter(cg => cg.logos.length > 0);
    }

    // Uploaded by filter
    if (this.uploadedByFilter) {
      filteredClientGroups = filteredClientGroups.map(cg => ({
        ...cg,
        logos: cg.logos.map(logo => ({
          ...logo,
          files: logo.files.filter(file => file.uploadedByName === this.uploadedByFilter)
        })).filter(logo => logo.files.length > 0)
      })).filter(cg => cg.logos.length > 0);
    }

    // Date range filter
    if (this.dateFrom || this.dateTo) {
      filteredClientGroups = filteredClientGroups.map(cg => ({
        ...cg,
        logos: cg.logos.map(logo => ({
          ...logo,
          files: logo.files.filter(file => {
            const fileDate = file.createdAt ? new Date(file.createdAt) : null;
            if (!fileDate) return false;
            if (this.dateFrom && fileDate < this.dateFrom) return false;
            if (this.dateTo) {
              const toDate = new Date(this.dateTo);
              toDate.setHours(23, 59, 59, 999);
              if (fileDate > toDate) return false;
            }
            return true;
          })
        })).filter(logo => logo.files.length > 0)
      })).filter(cg => cg.logos.length > 0);
    }

    // Recalculate totals
    filteredClientGroups.forEach(cg => {
      cg.logoCount = cg.logos.length;
      cg.totalFiles = cg.logos.reduce((sum, logo) => sum + logo.fileCount, 0);
    });

    this.clientGroups = filteredClientGroups;
    this.totalClients = filteredClientGroups.length;
    this.totalFiles = filteredClientGroups.reduce((sum, cg) => sum + cg.totalFiles, 0);
  }

  applyRegularFilters(): void {
    // Always start from original unfiltered data
    let filtered = this.originalLogoGroups.map(logo => ({
      ...logo,
      files: [...logo.files] // Deep copy files
    }));

    // Logo name filter
    if (this.logoNameFilter) {
      const searchLower = this.logoNameFilter.toLowerCase();
      filtered = filtered.filter(logo => 
        logo.logoName.toLowerCase().includes(searchLower)
      );
    }

    // Global search
    if (this.globalSearch) {
      const searchLower = this.globalSearch.toLowerCase();
      filtered = filtered.map(logo => ({
        ...logo,
        files: logo.files.filter(file => 
          (file.originalFileName || file.fileName || '').toLowerCase().includes(searchLower) ||
          logo.logoName.toLowerCase().includes(searchLower)
        )
      })).filter(logo => logo.files.length > 0);
    }

    // File category filter
    if (this.fileCategoryFilter) {
      filtered = filtered.map(logo => ({
        ...logo,
        files: logo.files.filter(file => file.fileCategory === this.fileCategoryFilter)
      })).filter(logo => logo.files.length > 0);
    }

    // File status filter
    if (this.fileStatusFilter) {
      filtered = filtered.map(logo => ({
        ...logo,
        files: logo.files.filter(file => file.fileStatus === this.fileStatusFilter)
      })).filter(logo => logo.files.length > 0);
    }

    // Date range filter
    if (this.dateFrom || this.dateTo) {
      filtered = filtered.map(logo => ({
        ...logo,
        files: logo.files.filter(file => {
          const fileDate = file.createdAt ? new Date(file.createdAt) : null;
          if (!fileDate) return false;
          if (this.dateFrom && fileDate < this.dateFrom) return false;
          if (this.dateTo) {
            const toDate = new Date(this.dateTo);
            toDate.setHours(23, 59, 59, 999);
            if (fileDate > toDate) return false;
          }
          return true;
        })
      })).filter(logo => logo.files.length > 0);
    }

    // Recalculate totals
    filtered.forEach(logo => {
      logo.fileCount = logo.files.length;
    });

    this.logoGroups = filtered;
    this.totalLogos = filtered.length;
    this.totalFiles = filtered.reduce((sum, logo) => sum + logo.fileCount, 0);
  }

  onFilterChange(): void {
    this.filterSubject.next();
  }

  onSearchChange(): void {
    this.searchSubject.next(this.globalSearch);
  }

  toggleClient(clientId: string): void {
    if (this.expandedClients.has(clientId)) {
      this.expandedClients.delete(clientId);
    } else {
      this.expandedClients.add(clientId);
    }
  }

  isClientExpanded(clientId: string): boolean {
    return this.expandedClients.has(clientId);
  }

  toggleLogo(logoId: string): void {
    if (this.expandedLogos.has(logoId)) {
      this.expandedLogos.delete(logoId);
    } else {
      this.expandedLogos.add(logoId);
    }
  }

  isLogoExpanded(logoId: string): boolean {
    return this.expandedLogos.has(logoId);
  }

  downloadFile(fileId: string, fileName: string): void {
    this.apiService.getBlob(`files/${fileId}/download`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName || `file-${fileId}`;
        link.click();
        URL.revokeObjectURL(url);
        this.messageService.add({ severity: 'success', summary: 'Download', detail: 'File downloaded successfully' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to download file' });
      }
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    try {
      return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return 'N/A';
    }
  }

  onPageChange(event: any): void {
    // PrimeNG pagination: event.page is 0-based, event.rows is page size
    // When rows (pageSize) changes, PrimeNG resets to page 0
    this.pageSize = event.rows;
    this.pageNumber = event.page + 1;
    
    // Ensure valid values
    if (this.pageNumber < 1) {
      this.pageNumber = 1;
    }
    if (this.pageSize < 1) {
      this.pageSize = 10;
    }
    
    // PrimeNG table handles pagination automatically on the [value] array
    // We just track the state for display purposes
    // No need to reload data - PrimeNG handles slicing automatically
  }

  clearFilters(): void {
    this.globalSearch = '';
    this.clientFilter = null;
    this.logoNameFilter = '';
    this.fileCategoryFilter = null;
    this.fileStatusFilter = null;
    this.uploadedByFilter = null;
    this.dateFrom = null;
    this.dateTo = null;
    this.onFilterChange();
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'table' ? 'grid' : 'table';
    // Reset to first page when switching views
    this.pageNumber = 1;
  }
  
  getPaginatedLogos(): LogoGroup[] {
    const start = (this.pageNumber - 1) * this.pageSize;
    const end = start + this.pageSize;
    return this.logoGroups.slice(start, end);
  }

  get Math() {
    return Math;
  }

  getFileStatusSeverity(fileStatus?: string): TagSeverity {
    if (!fileStatus) return 'warning';
    if (fileStatus === 'Approved') return 'success';
    if (fileStatus === 'Final') return 'info';
    return 'warning';
  }

  getFileStatusValue(fileStatus?: string): string {
    return fileStatus || 'Draft';
  }

  getUserRoleSeverity(role?: string): TagSeverity {
    if (!role) return 'secondary';
    const roleLower = role.toLowerCase();
    if (roleLower === 'superadmin' || roleLower === 'admin') return 'danger';
    if (roleLower === 'designer') return 'warning';
    if (roleLower === 'client') return 'info';
    return 'secondary';
  }
}
