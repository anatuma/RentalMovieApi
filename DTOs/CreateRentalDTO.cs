namespace Revision.DTOs;

public class CreateRentalDTO
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public List<CreateRentalMovieDTO> Movies { get; set; } = new();
}