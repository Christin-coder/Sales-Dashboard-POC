import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of, tap } from 'rxjs';

// Interfaces matching your C# DTOs
export interface ProductDTO {
  productID: number;
  productName: string;
  price: number;
}

export interface CustomerDTO {
  customerID: number;
  fullName: string;
  email: string;
}

export interface SaleDTO {
  saleID: number;
  saleDate: string;
  quantity: number;
  customerName: string;
  productName: string;
  price: number;
  total: number;
}

export interface SaleCreateDTO {
  productID: number;
  customerID: number;
  quantity: number;
}

export interface PagedResponse<T> {
  data: T[];
  totalPages: number;
  totalCount: number;
  totalRevenue?: number;
}


@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5009/api'; // Matched to launchSettings.json

  // We use Signals to store the data globally
  products = signal<ProductDTO[]>([]);
  customers = signal<CustomerDTO[]>([]);
  sales = signal<SaleDTO[]>([]);
  totalPages = signal<number>(0);
  totalCount = signal<number>(0);
  totalRevenue = signal<number>(0);

  // Fetch Methods
  getProducts(pageNumber: number, pageSize: number, sortBy: string, isAscending: boolean) {
    this.http.get<PagedResponse<ProductDTO>>(`${this.baseUrl}/products?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=${sortBy}&isAscending=${isAscending}`)
      .pipe(catchError(err => { console.error('Error fetching products', err); return of({ data: [], totalPages: 0, totalCount: 0 }); }))
      .subscribe(res => {
        this.products.set(res.data);
        this.totalPages.set(res.totalPages);
        this.totalCount.set(res.totalCount);
      });
  }

  getCustomers(pageNumber: number, pageSize: number, sortBy: string, isAscending: boolean) {
    this.http.get<PagedResponse<CustomerDTO>>(`${this.baseUrl}/customers?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=${sortBy}&isAscending=${isAscending}`)
      .pipe(catchError(err => { console.error('Error fetching customers', err); return of({ data: [], totalPages: 0, totalCount: 0 }); }))
      .subscribe(res => {
        this.customers.set(res.data);
        this.totalPages.set(res.totalPages);
        this.totalCount.set(res.totalCount);
      });
  }

  getSales(pageNumber: number, pageSize: number, sortBy: string, isAscending: boolean) {
    this.http.get<PagedResponse<SaleDTO>>(`${this.baseUrl}/sales?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=${sortBy}&isAscending=${isAscending}`)
      .pipe(catchError(err => {
        console.error('Error fetching sales', err);
        return of({ data: [], totalPages: 0, totalCount: 0, totalRevenue: 0 });
      }))
      .subscribe(res => {
        this.sales.set(res.data);
        this.totalPages.set(res.totalPages);
        this.totalCount.set(res.totalCount);
        this.totalRevenue.set(res.totalRevenue ?? 0);
      });
  }

  // POST Methods
  addProduct(product: Omit<ProductDTO, 'productID'>) {
    return this.http.post<ProductDTO>(`${this.baseUrl}/products`, product).pipe(
      tap(() => this.getProducts(1, 10, 'ProductID', true))
    );
  }

  addCustomer(customer: Omit<CustomerDTO, 'customerID'>) {
    return this.http.post<CustomerDTO>(`${this.baseUrl}/customers`, customer).pipe(
      tap(() => this.getCustomers(1, 10, 'CustomerID', true))
    );
  }

  createSale(sale: SaleCreateDTO) {
    return this.http.post<SaleDTO>(`${this.baseUrl}/sales`, sale).pipe(
      tap(() => this.getSales(1, 10, 'SaleID', true))
    );
  }

  editCustomer(customerID: number, customer: CustomerDTO){
    return this.http.put<CustomerDTO>(`${this.baseUrl}/customers/${customerID}`, customer).pipe(
      tap(() => this.getCustomers(1, 10, 'CustomerID', true))
    );
  }
  editProduct(productID: number, product: ProductDTO){
    return this.http.put<ProductDTO>(`${this.baseUrl}/products/${productID}`, product).pipe(
      tap(() => this.getProducts(1, 10, 'ProductID', true))
    );
  }

  //DELETE Methods
  deleteCustomer(customerID: number){
    return this.http.delete(`${this.baseUrl}/customers/${customerID}`).pipe(
      tap(() => this.getCustomers(1, 10, 'CustomerID', true))
    );
  }
  deleteProduct(productID: number){
  return this.http.delete(`${this.baseUrl}/products/${productID}`).pipe(
    tap(() => this.getProducts(1, 10, 'ProductID', true))
    );
  }
  deleteSale(saleID: number){
  return this.http.delete(`${this.baseUrl}/sales/${saleID}`).pipe(
    tap(() => this.getSales(1, 10, 'SaleID', true))
    );
  }
}