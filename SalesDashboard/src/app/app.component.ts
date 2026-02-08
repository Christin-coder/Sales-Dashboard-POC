import { Component, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { ApiService } from './services/api.service';
import { FormsModule } from '@angular/forms'; // Required for [(ngModel)]
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: '../styles.css',
  encapsulation: ViewEncapsulation.None
})
export class AppComponent implements OnInit {
  api = inject(ApiService);

  // Tracks which table we are looking at
  currentView = signal<'products' | 'customers' | 'sales'>('sales');
  //Tracks the status of the edit modal
  isEditModalOpen = signal(false);
  editingItem = signal<any>(null); // holds the item being edited.

  sortBy = signal<string | null>(null);
  isAscending = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalPages = this.api.totalPages;
  totalCount = this.api.totalCount;
  totalRevenue = this.api.totalRevenue;


  // Form Objects (Binded to the UI)
  newProduct = { productName: '', price: 0 };
  newCustomer = { fullName: '', email: '' };
  newSale = { customerID: 0, productID: 0, quantity: 1 };

  ngOnInit() {
    // Load initial data from SSMS
    this.api.getProducts(this.currentPage(), this.pageSize(), "ProductID", this.isAscending());
    this.api.getCustomers(this.currentPage(), this.pageSize(), "CustomerID", this.isAscending());
    this.api.getSales(this.currentPage(), this.pageSize(), "SaleID", this.isAscending());
  }

  onViewChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value as 'products' | 'customers' | 'sales';
    this.currentPage.set(1); // Reset to first page on view change
    this.currentView.set(value);
    if (value === 'sales') this.api.getSales(1, 10, 'SaleID', true);
    else if (value === 'products') this.api.getProducts(1, 10, 'ProductID', true);
    else this.api.getCustomers(1, 10, 'CustomerID', true);
  }

  toggleSort(column: string) {
    if (this.sortBy() === column) {
      this.isAscending.update(val => !val);
    } else {
      this.sortBy.set(column);
      this.isAscending.set(true);
    }
    this.currentView() === 'products' ?
      this.api.getProducts(this.currentPage(), this.pageSize(), this.sortBy()!, this.isAscending()) :
      this.currentView() === 'customers' ?
        this.api.getCustomers(this.currentPage(), this.pageSize(), this.sortBy()!, this.isAscending()) :
        this.api.getSales(this.currentPage(), this.pageSize(), this.sortBy()!, this.isAscending());
  }

  // Logic to save data based on what is selected in the dropdown
  saveData() {
    if (this.currentView() === 'products') {
      if (!this.newProduct.productName || this.newProduct.price <= 0) return;
      this.api.addProduct(this.newProduct).subscribe(() => {
        this.api.getProducts(1, 10, 'ProductID', true); // Refresh the list
        this.newProduct = { productName: '', price: 0 }; // Clear form
      });
    } else if (this.currentView() === 'customers') {
      if (!this.newCustomer.fullName || !this.newCustomer.email) return;
      this.api.addCustomer(this.newCustomer).subscribe(() => {
        this.api.getCustomers(1, 10, 'CustomerID', true);
        this.newCustomer = { fullName: '', email: '' };
      });
    } else {
      if (this.newSale.customerID === 0 || this.newSale.productID === 0) return;
      this.api.createSale(this.newSale).subscribe(() => {
        this.api.getSales(1, 10, 'SaleID', true);
        this.newSale = { customerID: 0, productID: 0, quantity: 1 };
      });
    }
  }

  //Logic to delete data safely or throw an error
  errorMessage = signal<string | null>(null);

  deleteItem(id: number) {
    const current = this.currentView();
    
    if (confirm(`Are you sure you want to delete this ${current}?`)) {
      // 1. Determine which API method to call
      let deleteObservable;
      if (current === 'sales') deleteObservable = this.api.deleteSale(id);
      else if (current === 'products') deleteObservable = this.api.deleteProduct(id);
      else deleteObservable = this.api.deleteCustomer(id);

      // 2. Execute and handle results
      deleteObservable.subscribe({
        next: () => {
          this.errorMessage.set(null);
          // Successful deletion, no further action needed as the service refreshes data
        },
        error: (err) => {
          // Display the "Cannot delete" message from C#
          alert(err.error || 'Delete failed');
        }
      });
    }
  }
  openEditModal(item: any) {
    this.isEditModalOpen.set(true);
    this.editingItem.set({ ...item }); // Create a copy to edit
  }

  closeEditModal() {
    this.isEditModalOpen.set(false);
    this.editingItem.set(null);
  }

  submitEdit() {
    const current = this.currentView();
    const item = this.editingItem();
    if (current === 'products') {
      this.api.editProduct(item.productID, item).subscribe(() => {
        this.closeEditModal();
      });
    }
    else this.api.editCustomer(item.customerID, item).subscribe(() => {
      this.closeEditModal();
    });
  }

  goToPage(page: number) {
    if (page >=1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      const view = this.currentView();
      const sort = this.sortBy();
      const isAsc = this.isAscending();
      const size = this.pageSize();

      if (view === 'products') {
        this.api.getProducts(page, size, sort || 'ProductID', isAsc);
      } else if (view === 'customers') {
        this.api.getCustomers(page, size, sort || 'CustomerID', isAsc);
      } else {
        this.api.getSales(page, size, sort || 'SaleID', isAsc);
      }
    }
  }
}