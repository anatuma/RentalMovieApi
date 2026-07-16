namespace Revision.DTOs;

public class CustomerRentalResponseDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<RentalResponseDTO> RentalResponses { get; set; } = new List<RentalResponseDTO>();
}