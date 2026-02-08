using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAPI.Data;
using SalesAPI.Models;
using SalesAPI.DTOs;


namespace SalesAPI.Controllers;

[Route("api/[Controller]")] // This makes the URL: api/sales
[ApiController]
public class SalesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<SaleDTO>>> GetSales(
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "SaleDate",
        bool isAscending = true)
    {
        var query = context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Product)
            .AsQueryable();
        query = sortBy switch
        {
            "SaleID" => isAscending ? query.OrderBy(s => s.SaleID) : query.OrderByDescending(s => s.SaleID),
            "SaleDate" => isAscending ? query.OrderBy(s => s.SaleDate) : query.OrderByDescending(s => s.SaleDate),
            "CustomerName" => isAscending ? query.OrderBy(s => s.Customer.FullName) : query.OrderByDescending(s => s.Customer.FullName),
            "ProductName" => isAscending ? query.OrderBy(s => s.Product.ProductName) : query.OrderByDescending(s => s.Product.ProductName),
            "Quantity" => isAscending ? query.OrderBy(s => s.Quantity) : query.OrderByDescending(s => s.Quantity),
            "TotalPrice" => isAscending ? query.OrderBy(s => s.Quantity * s.Product.Price) : query.OrderByDescending(s => s.Quantity * s.Product.Price),
                _ => query.OrderBy(s => s.SaleDate)
        };
        var totalRevenue = await (
            from s in context.Sales
            join p in context.Products on s.ProductID equals p.ProductID
            select s.Quantity * p.Price
        ).SumAsync();

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var sales = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToDTO(p))
                .ToListAsync();

        return Ok(new PagedResponse<SaleDTO>
        {
            Data = sales,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalRevenue = totalRevenue
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDTO>> GetSale(int id)
    {
        var sale = await context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.SaleID == id);

        if (sale == null)
        {
            return NotFound();
        }

        return MapToDTO(sale);
    }

    [HttpPost]
    public async Task<ActionResult<SaleDTO>> CreateSale(SaleCreateDTO createSaleDTO)
    {
        // 1. Map DTO to Entity
        var sale = new Sale
        {
            Quantity = createSaleDTO.Quantity,
            CustomerID = createSaleDTO.CustomerID,
            ProductID = createSaleDTO.ProductID
        };

        // 2. Save to Database
        context.Sales.Add(sale);
        await context.SaveChangesAsync();
        
        // 3. Fetch the fully populated sale to return names to the frontend
        var savedSale = await context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Product)
            .SingleOrDefaultAsync(s => s.SaleID == sale.SaleID);

        return savedSale == null ? NotFound() : CreatedAtAction(nameof(GetSale), new { id = sale.SaleID }, MapToDTO(savedSale));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<SaleDTO>> DeleteSale(int id)
    {
        var sale = await context.Sales.FindAsync(id);
        if (sale == null) return NotFound();

        context.Sales.Remove(sale);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private static SaleDTO MapToDTO(Sale sale)
    {
        return new SaleDTO
        {
            SaleID = sale.SaleID,
            CustomerName = sale.Customer?.FullName ?? "Unknown Customer",
            ProductName = sale.Product?.ProductName ?? "Unknown Product",
            Quantity = sale.Quantity,
            SaleDate = sale.SaleDate,
            Price = sale.Product?.Price ?? 0,
            Total = sale.Quantity * (sale.Product?.Price ?? 0)
        };
    }
}