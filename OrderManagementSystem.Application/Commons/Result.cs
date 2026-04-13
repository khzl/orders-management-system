using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Commons
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string? Error { get; protected set; }

        public static Result Success() => new Result { IsSuccess = true };
        public static Result Failure(string error) => new Result { IsSuccess = false, Error = error };

    }

    public class Result<T> : Result
    {
        public T? Data { get; private set; }

        public static Result<T> Sucess(T data) => new Result<T> { IsSuccess = true, Data = data };
        public new static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error };
    }

}
