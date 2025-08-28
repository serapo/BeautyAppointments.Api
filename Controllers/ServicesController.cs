using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautyAppointments.Api.Data;
using BeautyAppointments.Api.Domain;

namespace BeautyAppointments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServicesController(AppDbContext db) => _db = db;

    // GET: api/services
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Service>>> GetAll()
    {
        return await _db.Services.ToListAsync();
    }

    // GET: api/services/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Service>> GetById(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service == null) return NotFound();
        return service;
    }

    // POST: api/services
    [HttpPost]
    public async Task<ActionResult<Service>> Create(Service service)
    {
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    // PUT: api/services/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Service updated)
    {
        if (id != updated.Id) return BadRequest();

        var exists = await _db.Services.AnyAsync(s => s.Id == id);
        if (!exists) return NotFound();

        _db.Entry(updated).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/services/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service == null) return NotFound();

        _db.Services.Remove(service);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
