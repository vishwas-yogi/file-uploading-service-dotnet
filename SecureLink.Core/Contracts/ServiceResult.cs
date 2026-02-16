namespace SecureLink.Core.Contracts;

// Base type
public class ServiceResult<TError>
{
    private static readonly HashSet<ResponseStatus> _successStatuses =
    [
        ResponseStatus.Success,
        ResponseStatus.Created,
        ResponseStatus.Deleted,
    ];

    public ResponseStatus Status { get; protected init; }
    public TError? Error { get; protected init; }

    public bool IsSuccess => _successStatuses.Contains(Status);

    public static ServiceResult<TError> Success()
    {
        return new ServiceResult<TError> { Status = ResponseStatus.Success };
    }

    public static ServiceResult<TError> Deleted()
    {
        return new ServiceResult<TError> { Status = ResponseStatus.Deleted };
    }

    public static ServiceResult<TError> ValidationError(TError error)
    {
        return new ServiceResult<TError> { Status = ResponseStatus.ValidationError, Error = error };
    }

    public static ServiceResult<TError> NotFound(TError error)
    {
        return new ServiceResult<TError> { Status = ResponseStatus.NotFound, Error = error };
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

    public static ServiceResult<TData, TError> Created(TData data)
    {
        return new ServiceResult<TData, TError> { Status = ResponseStatus.Created, Data = data };
    }

    public static ServiceResult<TData, TError> Deleted(TData? data, TError? error)
    {
        return new ServiceResult<TData, TError>
        {
            Status = ResponseStatus.Deleted,
            Data = data,
            Error = error,
        };
    }

    public static new ServiceResult<TData, TError> ValidationError(TError error)
    {
        return new ServiceResult<TData, TError>
        {
            Status = ResponseStatus.ValidationError,
            Error = error,
        };
    }

    public static new ServiceResult<TData, TError> NotFound(TError error)
    {
        return new ServiceResult<TData, TError> { Status = ResponseStatus.NotFound, Error = error };
    }
}
