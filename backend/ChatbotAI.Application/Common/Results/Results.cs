using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatbotAI.Application.Common.Errors;

namespace ChatbotAI.Application.Common.Results;
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
}