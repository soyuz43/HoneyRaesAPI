namespace HoneyRaesAPI.Models;

public class ServiceTicket
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Emergency { get; set; }
    public DateTime? DateCompleted { get; set; }
}
