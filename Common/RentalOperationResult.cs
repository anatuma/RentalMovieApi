namespace Revision.Common;

public class RentalOperationResult
{
    public bool IsSuccess { get; set; }
    public RentalError Error { get; set; }

    public static RentalOperationResult Success() 
        => new() {IsSuccess = true, Error = RentalError.None};
    
    public static RentalOperationResult Failure(RentalError error)
        => new() {IsSuccess = false, Error = error};
}