using HoneyRaesAPI.Models;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

// List<HoneyRaesAPI.Models.Customer> customers = new List<HoneyRaesAPI.Models.Customer> { };
// List<HoneyRaesAPI.Models.Employee> employees = new List<HoneyRaesAPI.Models.Employee> { };
// List<HoneyRaesAPI.Models.ServiceTicket> serviceTickets = new List<HoneyRaesAPI.Models.ServiceTicket> { };

List<Customer> customers = new List<Customer>
{
new Customer()
{
    Id = 1,
    Name = "John Smith",
    Address = "123 Fake St."
},
new Customer()
{
    Id = 2,
    Name = "Jane Smith",
    Address = "130 Fake St."
},
new Customer()
{
    Id = 3,
    Name = "Jimmy Smith",
    Address = "250 Fake St."
}
    };
List<Employee> employees = new List<Employee>
    {
    new Employee()
    {
        Id = 1,
        Name = "Bob Self",
        Specialty = "Hard Drives"
    },
        new Employee()
    {
        Id = 2,
        Name = "Bill Self",
        Specialty = "Mac computers"
    },
    };
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Failed HDD",
        Emergency = true,
        DateCompleted = new DateTime(2023, 8, 15)
    },
        new ServiceTicket()
    {
        Id = 2,
        CustomerId = 2,
        Description = "Failed HDD",
        Emergency = false,

    },
        new ServiceTicket()
    {
        Id = 3,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "Failed HDD",
        Emergency = true,
        DateCompleted = new DateTime(2023, 8, 15)
    },
        new ServiceTicket()
    {
        Id = 4,
        CustomerId = 3,
        Description = "Failed HDD",
        Emergency = false,
    },
        new ServiceTicket()
    {
        Id = 5,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "Failed HDD",
        Emergency = true,
        DateCompleted = new DateTime(2023, 8, 15)
    },
};


var builder = WebApplication.CreateBuilder(args);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

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

app.UseHttpsRedirection();

app.MapGet("/employees", () =>
{
    return employees;
});
// For reference to the following look at Book 2 chapter 5
app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("/customers", () =>
{
    return customers;
});
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});


app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    // Adding employee data in the serviveTicket object
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Count > 0 ? serviceTickets.Max(st => st.Id) + 1 : 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/serviceTickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

}
);
/*Using the MapPut method creates the endpoint with a route pattern that takes the URL and a placeholder for the id(serviceTicketsId) and will be triggered when the PUT request is made to the API.
The handler takes id(ServiceTicketId) and ServiceTicket(object)  as parameters and using an anonymous/ lambda function handles the incoming request
*/
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    // ServiceTicket ticketToUpdate gets the serviceTicket that corresponds to the matching id in the URL
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    // serviceTicket.InexOf(ticketToUpdate) gets the index of the serviceTicket object that was assigned to ticketToUpdate and assigns it's value to ticketIndex
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    // If the ticket doesn't exist return NotFound
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //If the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    // This accesses the object in the serviceTickets list that is specified by the ticketIndex and updates the object in the list
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
    if (ticketToComplete == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    return Results.Ok();

});

app.Run();

