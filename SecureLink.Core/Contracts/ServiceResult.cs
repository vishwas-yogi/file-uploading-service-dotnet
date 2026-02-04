namespace SecureLink.Core.Contracts;

// Base type
public class ServiceResult<TError>
{
    private static readonly HashSet<ResponseStatus> _sucessStatuses =
    [
        ResponseStatus.Success,
        ResponseStatus.Created,
        ResponseStatus.Deleted,
    ];

    public ResponseStatus Status { get; protected init; }
    public TError? Error { get; protected init; }

    public bool IsSuccess => _sucessStatuses.Contains(Status);

    public static ServiceResult<TError> Success()
    {
        return new ServiceResult<TError> { Status = ResponseStatus.Success };
    }

    public static ServiceResult<TError> ValidationError(TError error)
    {
        return new ServiceResult<TError> { Status = ResponseStatus.ValidationError, Error = error };
    }
}

// Type when we need to return data as well
public class ServiceResult<TData, TError> : ServiceResult<TError>
{
    public TData? Data { get; private init; }

    public static ServiceResult<TData, TError> Success(TData data)
    {
        return new ServiceResult<TData, TError> { Status = ResponseStatus.Success, Data = data };
    }

    public static new ServiceResult<TData, TError> ValidationError(TError error)
    {
        return new ServiceResult<TData, TError>
        {
            Status = ResponseStatus.ValidationError,
            Error = error,
        };
    }
}
