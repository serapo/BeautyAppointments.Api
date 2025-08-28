using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautyAppointments.Api.Data;
using BeautyAppointments.Api.Domain;

namespace BeautyAppointments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AppointmentsController(AppDbContext db) => _db = db;

    // GET: api/appointments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAll()
    {
        return await _db.Appointments
                        .Include(a => a.Customer)
                        .Include(a => a.Service)
                        .ToListAsync();
    }

    // GET: api/appointments/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetById(int id)
    {
        var appointment = await _db.Appointments
                                   .Include(a => a.Customer)
                                   .Include(a => a.Service)
                                   .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return NotFound();
        return appointment;
    }

    // POST: api/appointments
    [HttpPost]
    public async Task<ActionResult<Appointment>> Create(Appointment appointment)
    {
        // validasyon: müşteri/hizmet var mı?
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == appointment.CustomerId);
        var serviceExists = await _db.Services.AnyAsync(s => s.Id == appointment.ServiceId);

        if (!customerExists || !serviceExists)
            return BadRequest(new { message = "CustomerId veya ServiceId geçersiz" });

        appointment.Status = "Pending";
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    // PUT: api/appointments/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Appointment updated)
    {
        if (id != updated.Id) return BadRequest();

        var exists = await _db.Appointments.AnyAsync(a => a.Id == id);
        if (!exists) return NotFound();

        _db.Entry(updated).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/appointments/5/cancel
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/appointments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        _db.Appointments.Remove(appointment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
