using Microsoft.Data.SqlClient;
using Revision.DTOs;

namespace Revision.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly string _connectionString;
    
    public RentalRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException(
                                "Missing 'DefaultConnection' in appsettings.json.");
    }

    public async Task<CustomerRentalResponseDTO?> GetCustomerWithRentalsAsync(int customerId)
    {
        var flatResults = new List<FlatRentalRow>();
        string sql = """
                        SELECT c.first_name, c.last_name,
                        r.rental_id, r.rental_Date, r.return_date, s.name AS status_name,
                        m.title AS movie_title, ri.price_at_rental
                        FROM customer AS c
                        LEFT JOIN rental AS r ON r.customer_id = c.customer_id
                        LEFT JOIN status AS s ON r.status_id = s.status_id
                        LEFT JOIN rental_item AS ri ON ri.rental_id = r.rental_id
                        LEFT JOIN movie AS m ON m.movie_id = ri.movie_id
                        WHERE c.customer_id = @customerId
                     """;
        
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            flatResults.Add(new FlatRentalRow
            {
                FirstName = reader["first_name"].ToString()!,
                LastName = reader["last_name"].ToString()!,
                RentalId = reader["rental_id"] != DBNull.Value ? (int)reader["rental_id"] : null,
                RentalDate = reader["rental_date"] != DBNull.Value ? (DateTime)reader["rental_date"] : null,
                ReturnDate = reader["return_date"] != DBNull.Value ? (DateTime)reader["return_date"] : null,
                StatusName = reader["status_name"] != DBNull.Value ? (string)reader["status_name"] : null,
                MovieTitle = reader["movie_title"] != DBNull.Value ? (string)reader["movie_title"] : null,
                PriceAtRental = reader["price_at_rental"] != DBNull.Value ? (decimal)reader["price_at_rental"] : null
            });
        }
        if (flatResults.Count == 0) return null;

        return flatResults
            .GroupBy(row => new { row.FirstName, row.LastName })
            .Select(cg => new CustomerRentalResponseDTO
            {
                FirstName = cg.Key.FirstName,
                LastName = cg.Key.LastName,
                RentalResponses = cg
                    .Where(row => row.RentalId.HasValue)
                    .GroupBy(row => new { row.RentalId, row.RentalDate, row.ReturnDate, row.StatusName })
                    .Select(rg => new RentalResponseDTO
                    {
                        Id = rg.Key.RentalId!.Value,
                        RentalDate = rg.Key.RentalDate!.Value,
                        ReturnDate = rg.Key.ReturnDate,
                        Status = rg.Key.StatusName!,
                        RentalMovies = rg.Select(rm => new RentalMovieResponseDTO
                        {
                            Title = rm.MovieTitle!,
                            PriceAtRental = rm.PriceAtRental??0
                        }).ToList()
                    }).ToList()
            }).FirstOrDefault();
    }

    public async Task<bool> DoesCustomerExistAsync(int customerId)
    {
        string sql = "SELECT 1 FROM Customer WHERE customer_id = @customerId";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);
        await connection.OpenAsync();
        return await command.ExecuteScalarAsync() != null;
    }

    public async Task<int?> GetMovieByTitleAsync(string title)
    {
        string sql = "SELECT movie_id FROM Movie WHERE title = @title";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@title", title);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return (int?)result;
    }

    public async Task AddRentalWithItemsAsync(int customerId, CreateRentalDTO rentalData, List<int> movieIds)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = connection.BeginTransaction();

        try
        {
            var rentalCommand = new SqlCommand("""
                                               INSERT INTO Rental (rental_date, customer_id, status_id)
                                               OUTPUT INSERTED.rental_id
                                               VALUES (@rDate, @cId, @sId)
                                               """, connection, transaction
            );

            rentalCommand.Parameters.AddWithValue("@rDate", rentalData.RentalDate);
            rentalCommand.Parameters.AddWithValue("@cId", customerId);
            rentalCommand.Parameters.AddWithValue("@sId", 1);
            
            var newlyGeneratedId = await rentalCommand.ExecuteScalarAsync();

            for (int i = 0; i < movieIds.Count; i++)
            {
                var itemCommand = new SqlCommand("""
                                                 INSERT INTO Rental_Item (rental_id, movie_id, price_at_rental)                             
                                                 VALUES (@rId, @mId, @price)
                                                 """, connection, transaction
                );

                itemCommand.Parameters.AddWithValue("@rId", newlyGeneratedId);
                itemCommand.Parameters.AddWithValue("@mId", movieIds[i]);
                itemCommand.Parameters.AddWithValue("@price", rentalData.Movies[i].PriceAtRental);
                await itemCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<string>> GetTitlesAlreadyRentedByCustomerAsync(int customerId, List<string> requestedTitles)
    {
        var rentedTitles = new List<string>();
    
        string sql = """
                     SELECT m.title 
                     FROM Rental r
                     JOIN Rental_Item ri ON r.rental_id = ri.rental_id
                     JOIN Movie m ON ri.movie_id = m.movie_id
                     WHERE r.customer_id = @customerId 
                     AND m.title IN ({0})
                     """;

        var parameters = requestedTitles.Select((t, i) => $"@t{i}").ToList();
        var formattedSql = string.Format(sql, string.Join(",", parameters));

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(formattedSql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);
    
        for (int i = 0; i < requestedTitles.Count; i++)
            command.Parameters.AddWithValue($"@t{i}", requestedTitles[i]);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rentedTitles.Add(reader["title"].ToString()!);
        }
        return rentedTitles;
    }

    private class FlatRentalRow
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int? RentalId { get; set; }
        public DateTime? RentalDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? StatusName { get; set; }
        public string? MovieTitle { get; set; }
        public decimal? PriceAtRental { get; set; }
    }
}