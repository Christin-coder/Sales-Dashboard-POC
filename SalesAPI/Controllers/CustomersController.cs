using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAPI.Data;
using SalesAPI.Models;
using SalesAPI.DTOs;

namespace SalesAPI.Controllers;
[Route("api/[controller]")] // This makes the URL: api/customers
[ApiController]

public class CustomersController(AppDbContext context) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers(
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "CustomerID",
        bool isAscending = true
    )
    {
        var query = context.Customers.AsQueryable();
        query = sortBy switch
        {
            "CustomerID" => isAscending ? query.OrderBy(s => s.CustomerID) : query.OrderByDescending(s => s.CustomerID),
            "FullName" => isAscending ? query.OrderBy(s => s.FullName) : query.OrderByDescending(s => s.FullName),
            "Email" => isAscending ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email),
            _ => query.OrderBy(s => s.CustomerID)
        };

        var totalCount = await query.CountAsync();
        var totaLPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var customers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDTO(c))
            .ToListAsync();

        return Ok(new PagedResponse<CustomerDTO>
        {
            Data = customers,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
    {
        var customer = await context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return MapToDTO(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> CreateCustomer(CustomerDTO customerDto)
    {
        var customer = new Customer
        {
            FullName = customerDto.FullName,
            Email = customerDto.Email
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerID }, MapToDTO(customer));
    }
    // PUT: api/customers/5 (Update an existing customer)
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDTO>> UpdateCustomer(int id, CustomerDTO customerDto)
    {
        if (id != customerDto.CustomerID) return BadRequest("ID mismatch");
        var customer = await context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        customer.FullName = customerDto.FullName;
        customer.Email = customerDto.Email;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<CustomerDTO>> DeleteCustomer(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        try {
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException) {
            // This catches the Foreign Key error from SSMS
            return BadRequest("This customer has existing sales and cannot be deleted.");
        }
    }

    private static CustomerDTO MapToDTO(Customer customer)
    {
        return new CustomerDTO
        {
            CustomerID = customer.CustomerID,
            FullName = customer.FullName,
            Email = customer.Email
        };
    }
}