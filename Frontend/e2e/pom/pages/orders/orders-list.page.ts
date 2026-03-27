import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';
import { BasePage } from '../../base.page';
import { OrderCreateModalPage } from './order-create.modal';
import { OrderDetailModalPage } from './order-detail.modal';

const OrdersListSelectors = {
  openCreate: 'orders-open-create',
} as const;

/**
 * Orders route (`/orders`): KPIs, table, and embedded create/detail modals.
 */
export class OrdersListPage extends BasePage {
  readonly createOrder: OrderCreateModalPage;
  readonly orderDetail: OrderDetailModalPage;

  constructor(page: Page) {
    super(page);
    this.createOrder = new OrderCreateModalPage(page);
    this.orderDetail = new OrderDetailModalPage(page);
  }

  async goto(): Promise<void> {
    await this.page.goto('/orders');
    await expect(
      this.page.getByRole('heading', { name: /My Orders|Orders Management|Orders/i })
    ).toBeVisible();
  }

  async openCreateOrderModal(): Promise<void> {
    await this.page.getByTestId(OrdersListSelectors.openCreate).click();
    await this.createOrder.expectVisible();
  }

  async openOrderInTableByTitle(title: string): Promise<void> {
    await this.page.getByText(title, { exact: true }).first().click();
    await this.orderDetail.expectApproveVisible();
  }
}
