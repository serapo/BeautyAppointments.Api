namespace BeautyAppointments.Api.Domain
{
    public class Appointment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Confirmed/Cancelled

        public Customer? Customer { get; set; }
        public Service? Service { get; set; }
    }
}
