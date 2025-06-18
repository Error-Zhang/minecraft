using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MineCraftService.Web;

public class ApiResponse
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "OK";
    public object? Data { get; set; }

    

    // 成功响应
    public static IResult Success(object data, string message = "OK") => Results.Ok(new ApiResponse
        { Message = message, Data = data });

    public static IResult Success(string message = "OK") =>
        Results.Ok(new ApiResponse { Message = message });

    // 失败响应
    public static IResult Fail(int code, string message, object data) =>
        Results.Ok(new ApiResponse { Code = code, Message = message, Data = data });

    // 失败响应
    public static IResult Fail(int code, string message) =>
        Results.Ok(new ApiResponse { Code = code, Message = message });

    public static IResult NotFound(string message) =>
        Results.Ok(new ApiResponse { Code = 404, Message = $"{message} not found"  });
    
    // --- SignalR 用（直接返回对象） ---
    public static ApiResponse RawSuccess(object data, string message = "OK") =>
        new() { Code = 200, Message = message, Data = data };

    public static ApiResponse RawSuccess(string message = "OK") =>
        new() { Code = 200, Message = message };

    public static ApiResponse RawFail(int code, string message, object data) =>
        new() { Code = code, Message = message, Data = data };

    public static ApiResponse RawFail(int code, string message) =>
        new() { Code = code, Message = message };

    public static ApiResponse RawNotFound(string message) =>
        new() { Code = 404, Message = $"{message} not found" };
}