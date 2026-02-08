using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAPI.Data;
using SalesAPI.Models;
using SalesAPI.DTOs;

namespace SalesAPI.Controllers;

[Route("api/[controller]")] // This makes the URL: api/products
[ApiController]
public class ProductsController(AppDbContext context) : ControllerBase
{

    // 1. GET: api/products (Fetch all products)
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductDTO>>> GetProducts(
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "ProductID",
        bool isAscending = true)
    {
        var query = context.Products.AsQueryable();
        query = sortBy switch
        {
            "ProductID" => isAscending ? query.OrderBy(s => s.ProductID) : query.OrderByDescending(s => s.ProductID),
            "ProductName" => isAscending ? query.OrderBy(s => s.ProductName) : query.OrderByDescending(s => s.ProductName),
            "Price" => isAscending ? query.OrderBy(s => s.Price) : query.OrderByDescending(s => s.Price),
                _ => query.OrderBy(s => s.ProductID)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToDTO(p))
                .ToListAsync();

        return Ok(new PagedResponse<ProductDTO>
        {
            Data = products,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    // 1.5 GET: api/products/5 (Fetch a single product)
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetProduct(int id)
    {
        var product = await context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return MapToDTO(product);
    }

    // 2. POST: api/products (Add a new product)
    [HttpPost]
    public async Task<ActionResult<ProductDTO>> CreateProduct(ProductDTO productDto)
    {
        // Convert DTO back to a Model for the database
        var product = new Product
        {
            ProductName = productDto.ProductName,
            Price = productDto.Price
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, MapToDTO(product));
    }
    // 3. PUT: api/products/5 (Update an existing product)
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDTO>> UpdateProduct(int id, ProductDTO productDto)
    {
        if (id != productDto.ProductID) return BadRequest("ID mismatch");
        var product = await context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.ProductName = productDto.ProductName;
        product.Price = productDto.Price;

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ProductDTO>> DeleteProduct(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null) return NotFound();

        try {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException) {
            // This catches the Foreign Key error from SSMS
            return BadRequest("This product has existing sales and cannot be deleted.");
        }
    }
    private static ProductDTO MapToDTO(Product product)
    {
        return new ProductDTO
        {
            ProductID = product.ProductID,
            ProductName = product.ProductName,
            Price = product.Price
        };
    }
}