namespace SecureLink.Core.Contracts;

public record LoginRequest(string Username, string Password)
{
    public override string ToString()
    {
        return $"LoginRequest {{ Username = {Username}, Password = *** }}";
    }
}
