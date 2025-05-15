using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
using Npgsql;


var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=4224;Database=HoneyRaes";


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
    List<Employee> employees = new List<Employee>();

    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();

    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM Employee";

    using NpgsqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        employees.Add(new Employee
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Specialty = reader.GetString(reader.GetOrdinal("Specialty"))
        });
    }

    return employees;
});


app.MapGet("/employees/{id}", (int id) =>
{
    EmployeeDTO? employeeDto = null;

    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();

    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        SELECT 
            e.Id,
            e.Name, 
            e.Specialty, 
            st.Id AS serviceTicketId, 
            st.CustomerId,
            st.EmployeeId,
            st.Description,
            st.Emergency,
            st.DateCompleted 
        FROM Employee e
        LEFT JOIN ServiceTicket st ON st.EmployeeId = e.Id
        WHERE e.Id = @id";
    command.Parameters.AddWithValue("@id", id);

    using NpgsqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        if (employeeDto == null)
        {
            employeeDto = new EmployeeDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                ServiceTickets = new List<ServiceTicketDTO>()
            };
        }

        if (!reader.IsDBNull(reader.GetOrdinal("serviceTicketId")))
        {
            employeeDto.ServiceTickets.Add(new ServiceTicketDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("serviceTicketId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                EmployeeId = id,
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Emergency = reader.GetBoolean(reader.GetOrdinal("Emergency")),
                DateCompleted = reader.IsDBNull(reader.GetOrdinal("DateCompleted"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("DateCompleted"))
            });
        }
    }

    return employeeDto == null ? Results.NotFound() : Results.Ok(employeeDto);
});

app.MapPost("/employees", (Employee employee) =>
{
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();

    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Employee (Name, Specialty)
        VALUES (@name, @specialty)
        RETURNING Id;";

    command.Parameters.AddWithValue("@name", employee.Name);
    command.Parameters.AddWithValue("@specialty", employee.Specialty);

    // get the new ID and return it
    var result = command.ExecuteScalar();
    if (result is int newId)
    {
        employee.Id = newId;
        return Results.Created($"/employees/{newId}", employee);
    }
    else
    {
        return Results.Problem("Failed to insert employee.");
    }

});

app.MapPut("/employees/{id}", (int id, Employee employee) =>
{
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();

    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        UPDATE Employee
        SET Name = @name,
            Specialty = @specialty
        WHERE Id = @id;";

    command.Parameters.AddWithValue("@name", employee.Name);
    command.Parameters.AddWithValue("@specialty", employee.Specialty);
    command.Parameters.AddWithValue("@id", id);

    int rowsAffected = command.ExecuteNonQuery();
    return rowsAffected == 0 ? Results.NotFound() : Results.NoContent();
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

