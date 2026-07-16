using Revision.DTOs;

namespace Revision.Repositories;

public interface IRentalRepository
{
    public Task<CustomerRentalResponseDTO?> GetCustomerWithRentalsAsync(int customerId);
    public Task<bool> DoesCustomerExistAsync(int customerId);
    public Task<int?> GetMovieByTitleAsync(string title);
    public Task AddRentalWithItemsAsync(int customerId, CreateRentalDTO rentalData, List<int> movieIds);
    public Task<List<string>> GetTitlesAlreadyRentedByCustomerAsync(int customerId, List<string> requestedTitles);
}