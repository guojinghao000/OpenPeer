namespace OpenPeer.Application.DTOs.Common;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "操作成功")
        => new() { Code = 200, Message = message, Data = data };

    public static ApiResponse<T> Success(int code, string message, T data)
        => new() { Code = code, Message = message, Data = data };

    public static ApiResponse<T> Error(int code, string message, T? data = default)
        => new() { Code = code, Message = message, Data = data };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string message = "操作成功")
        => new() { Code = 200, Message = message, Data = null };

    public static ApiResponse Error(int code, string message)
        => new() { Code = code, Message = message, Data = null };

    public static ApiResponse Error(int code, string message, List<ValidationError> errors)
        => new() { Code = code, Message = message, Data = errors };
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
