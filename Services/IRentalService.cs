using Revision.Common;
using Revision.DTOs;

namespace Revision.Services;

public interface IRentalService
{
    public Task<CustomerRentalResponseDTO?> GetCustomerRentalsAsync(int customerId);
    public Task<RentalOperationResult> CreateRentalAsync(int customerId, CreateRentalDTO rentalData);
}