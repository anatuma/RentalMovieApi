namespace Revision.DTOs;

public class RentalResponseDTO
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = null!;

    public List<RentalMovieResponseDTO> RentalMovies { get; set; } = new List<RentalMovieResponseDTO>();

}