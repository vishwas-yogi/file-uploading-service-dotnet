namespace FileUploader.Contracts;

public class ValidationResult<TError>
{
    public bool IsValid { get; set; }
    public TError? Error { get; set; }
}
