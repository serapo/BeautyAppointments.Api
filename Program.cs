using Microsoft.EntityFrameworkCore;
using BeautyAppointments.Api.Data;
using BeautyAppointments.Api.Domain;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Encoder =
            System.Text.Encodings.Web.JavaScriptEncoder.Create(
                System.Text.Unicode.UnicodeRanges.All);
    });



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { status = "healthy" }));


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Services.Any())
    {
        db.Services.AddRange(
            new Service { Name = "Cilt Bakýmý", Price = 750, DurationMinutes = 60 },
            new Service { Name = "Lazer Epilasyon", Price = 1200, DurationMinutes = 45 },
            new Service { Name = "Masaj", Price = 900, DurationMinutes = 50 }
        );
    }

    if (!db.Customers.Any())
    {
        db.Customers.AddRange(
            new Customer { FullName = "Ayþe Yýlmaz", Phone = "5551112233", Email = "ayse@example.com" },
            new Customer { FullName = "Mehmet Demir", Phone = "5554445566", Email = "mehmet@example.com" }
        );
    }

    db.SaveChanges();

    if (!db.Appointments.Any())
    {
        var c1 = db.Customers.FirstOrDefault();
        var s1 = db.Services.FirstOrDefault();
        if (c1 != null && s1 != null)
        {
            db.Appointments.Add(new Appointment
            {
                CustomerId = c1.Id,
                ServiceId = s1.Id,
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = "Confirmed"
            });
            db.SaveChanges();
        }
    }

}
    app.Run();
