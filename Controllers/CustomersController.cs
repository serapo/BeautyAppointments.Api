using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautyAppointments.Api.Data;
using BeautyAppointments.Api.Domain;

namespace BeautyAppointments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    static string Fold(string s) => string.IsNullOrWhiteSpace(s) ? string.Empty :
    s.Replace('ç', 'c').Replace('Ç', 'c')
     .Replace('ğ', 'g').Replace('Ğ', 'g')
     .Replace('ı', 'i').Replace('I', 'i').Replace('İ', 'i')
     .Replace('ö', 'o').Replace('Ö', 'o')
     .Replace('ş', 's').Replace('Ş', 's')
     .Replace('ü', 'u').Replace('Ü', 'u')
     .ToLowerInvariant();

    // GET: api/customers
    [HttpGet]
    public async Task<ActionResult<PagedResult<Customer>>> GetAll(
        [FromQuery] string? name= null,
        [FromQuery] string? phone= null,
        [FromQuery] string? email= null,
        [FromQuery] int page=1,
        [FromQuery] int pageSize= 10
        )
    {
        var q =_db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var folded = Fold(name);
            // NOT: Aşağıdaki Replace/ToLower zinciri EF Core tarafından SQLite'a translate edilir (REPLACE/LOWER)
            q = q.Where(c =>
                c.FullName
                 .Replace("ç", "c").Replace("Ç", "c")
                 .Replace("ğ", "g").Replace("Ğ", "g")
                 .Replace("ı", "i").Replace("I", "i").Replace("İ", "i")
                 .Replace("ö", "o").Replace("Ö", "o")
                 .Replace("ş", "s").Replace("Ş", "s")
                 .Replace("ü", "u").Replace("Ü", "u")
                 .ToLower()
                 .Contains(folded)
            );
        }

        if (!string.IsNullOrWhiteSpace(phone))
            q = q.Where(c => c.Phone.Contains(phone));

        if (!string.IsNullOrWhiteSpace(email))
            q = q.Where(c => c.Email.Contains(email));

        q = q.OrderBy(c => c.Id);

        return await q.ToPagedResultAsync(page, pageSize);
    }

    // GET: api/customers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return customer;
    }

    // POST: api/customers
    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    // PUT: api/customers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Customer updated)
    {
        if (id != updated.Id) return BadRequest();

        var exists = await _db.Customers.AnyAsync(c => c.Id == id);
        if (!exists) return NotFound();

        _db.Entry(updated).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/customers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
