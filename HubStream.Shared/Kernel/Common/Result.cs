using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Shared.Kernel.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public Error Error { get; }

        protected Result(T value, bool isSuccess, Error error)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        // Factory method para éxito
        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, null);
        }

        // Factory method para fallo
        public static Result<T> Failure(Error error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            return new Result<T>(default, false, error);
        }
    }

    // Versión no genérica para operaciones que no devuelven valor
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, null);

        public static Result Failure(Error error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            return new Result(false, error);
        }
    }
}
