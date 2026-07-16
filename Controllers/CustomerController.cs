using Microsoft.AspNetCore.Mvc;
using Revision.Common;
using Revision.DTOs;
using Revision.Services;

namespace Revision.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly IRentalService _rentalService;
    
    public CustomersController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    [HttpGet("{id}/rentals")]
    public async Task<IActionResult> GetCustomerRentals(int id)
    {
        var result = await _rentalService.GetCustomerRentalsAsync(id);
        if (result == null)
        {
            return NotFound("Customer not found.");
        }
        return Ok(result);
    }

    [HttpPost("{id}/rentals")]
    public async Task<IActionResult> CreateCustomerRentalAsync(int id, [FromBody] CreateRentalDTO rentalData)
    {
        var result = await _rentalService.CreateRentalAsync(id, rentalData);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetCustomerRentals), new { id }, null);
        }

        return result.Error switch
        {
            RentalError.CustomerNotFound => NotFound("Customer not found."),
            RentalError.MovieNotFound => BadRequest("One or more movies in the request do not exist."),
            RentalError.AlreadyRented => Conflict("Customer has already rented one or more of these movies."),
            _ => StatusCode(500)
        };
    }
}