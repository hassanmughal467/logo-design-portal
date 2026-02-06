export interface Order {
  id: string;
  clientId: string;
  designerId?: string;
  title: string;
  description: string;
  status: OrderStatus;
  priority: OrderPriority;
  createdAt: Date;
  updatedAt?: Date;
  dueDate?: Date;
  client?: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    companyName: string;
  };
  designer?: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
  };
}

export enum OrderStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Review = 'Review',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum OrderPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Urgent = 'Urgent'
}

export interface CreateOrderRequest {
  title: string;
  description: string;
  priority: OrderPriority;
  dueDate?: Date;
}

export interface AssignOrderRequest {
  designerId: string;
  notes?: string;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}
