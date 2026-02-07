namespace SecureLink.Core.Contracts;

public interface IContextProvider<T>
    where T : class
{
    T? Context { get; set; }
}
