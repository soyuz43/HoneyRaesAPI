using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;

List<Customer> customers = new()
{
    new Customer { Id = 1, Name = "Alice Smith", Address = "123 Maple Ave" },
    new Customer { Id = 2, Name = "Bob Johnson", Address = "456 Oak St" },
    new Customer { Id = 3, Name = "Charlie Rose", Address = "789 Pine Rd" }
};

List<Employee> employees = new()
{
    new Employee { Id = 1, Name = "Dana Miles", Specialty = "HVAC Repair" },
    new Employee { Id = 2, Name = "Elijah Burke", Specialty = "Electrical Work" }
};

List<ServiceTicket> serviceTickets = new()
{
    new ServiceTicket { Id = 1, CustomerId = 1, EmployeeId = 1, Description = "AC not cooling", Emergency = true },
    new ServiceTicket { Id = 2, CustomerId = 2, EmployeeId = 2, Description = "Lights flickering", Emergency = false, DateCompleted = DateTime.Now.AddDays(-2) },
    new ServiceTicket { Id = 3, CustomerId = 3, Description = "Leaky pipe under sink", Emergency = false },
    new ServiceTicket { Id = 4, CustomerId = 1, EmployeeId = 2, Description = "Outlet not working", Emergency = true },
    new ServiceTicket { Id = 5, CustomerId = 2, Description = "Dryer not spinning", Emergency = false }
};



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Service Ticket Endpoints
app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    var ticket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (ticket is null) return Results.NotFound();

    var employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
    var customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);

    var dto = new ServiceTicketDTO
    {
        Id = ticket.Id,
        CustomerId = ticket.CustomerId,
        EmployeeId = ticket.EmployeeId,
        Description = ticket.Description,
        Emergency = ticket.Emergency,
        DateCompleted = ticket.DateCompleted,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        }
    };

    return Results.Ok(dto);
});

// Posts
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
Customer? customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId); // Mark the variable as nullable
    if (customer == null)
    {
        return Results.BadRequest();
    }

    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });
});
// Deletes
app.MapDelete("/servicetickets/{id}", (int id) =>
{
    var ticket = serviceTickets.FirstOrDefault(t => t.Id == id);
    if (ticket is null)
    {
        return Results.NotFound();
    }

    serviceTickets.Remove(ticket);
    return Results.NoContent(); // 204 response — standard for successful DELETE
});
// Put

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket? ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});
// Marking a Ticket as Complete (Custom POST)
app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket? ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToComplete == null)
    {
        return Results.NotFound();
    }

    ticketToComplete.DateCompleted = DateTime.Today;
    return Results.NoContent();
});


// Employees Endpoints

app.MapGet("/employees", () =>
{
    return employees.Select(e => new EmployeeDTO
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty,
        ServiceTickets = serviceTickets
            .Where(st => st.EmployeeId == e.Id)
            .Select(st => new ServiceTicketDTO
            {
                Id = st.Id,
                CustomerId = st.CustomerId,
                EmployeeId = st.EmployeeId,
                Description = st.Description,
                Emergency = st.Emergency,
                DateCompleted = st.DateCompleted
            }).ToList()
    });
});

app.MapGet("/employees/{id}", (int id) =>
{
    var employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee is null) return Results.NotFound();

    var tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();

    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

// Customers Endpoints

app.MapGet("/customers", () =>
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address,
        ServiceTickets = serviceTickets
            .Where(st => st.CustomerId == c.Id)
            .Select(st => new ServiceTicketDTO
            {
                Id = st.Id,
                CustomerId = st.CustomerId,
                EmployeeId = st.EmployeeId,
                Description = st.Description,
                Emergency = st.Emergency,
                DateCompleted = st.DateCompleted
            }).ToList()
    });
});

app.MapGet("/customers/{id}", (int id) =>
{
    var customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer is null) return Results.NotFound();

    var tickets = serviceTickets.Where(st => st.CustomerId == id).ToList();

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.Run();

