using Revision.Common;
using Revision.DTOs;
using Revision.Repositories;

namespace Revision.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;

    public RentalService(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }
    public async Task<CustomerRentalResponseDTO?> GetCustomerRentalsAsync(int customerId)
    {
        return await _rentalRepository.GetCustomerWithRentalsAsync(customerId);
    }

    public async Task<RentalOperationResult> CreateRentalAsync(int customerId, CreateRentalDTO rentalData)
    {
        if (!await _rentalRepository.DoesCustomerExistAsync(customerId))
        {
            return RentalOperationResult.Failure(RentalError.CustomerNotFound);
        }
        
        var requestedTitles = rentalData.Movies.Select(m => m.Title).ToList();
        var alreadyRented = await _rentalRepository.GetTitlesAlreadyRentedByCustomerAsync(customerId, requestedTitles);

        if (alreadyRented.Any())
        {
            return RentalOperationResult.Failure(RentalError.AlreadyRented);
        }

        var movieIds = new List<int>();
        foreach (var movie in rentalData.Movies)
        {
            var id = await _rentalRepository.GetMovieByTitleAsync(movie.Title);
            if (id == null)
            {
                return RentalOperationResult.Failure(RentalError.MovieNotFound);
            }
            movieIds.Add(id.Value);
        }

        try
        {
            await _rentalRepository.AddRentalWithItemsAsync(customerId, rentalData, movieIds);
            return RentalOperationResult.Success();
        }
        catch (Exception ex)
        {
            return RentalOperationResult.Failure(RentalError.DatabaseError);
        }

    }
    
}