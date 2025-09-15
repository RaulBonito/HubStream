using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HubStream.Shared.Kernel.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T Data { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ApiError Error { get; private set; }

        // Constructor para respuesta exitosa
        private ApiResponse(T data)
        {
            Success = true;
            Data = data;
            Error = null;
        }

        // Constructor para respuesta fallida
        private ApiResponse(ApiError error)
        {
            Success = false;
            Data = default;
            Error = error;
        }

        // Métodos estáticos para facilitar la creación (Factory Pattern)
        public static ApiResponse<T> CreateSuccess(T data)
        {
            return new ApiResponse<T>(data);
        }

        public static ApiResponse<T> CreateFailure(string errorCode, string errorMessage)
        {
            var error = new ApiError { Code = errorCode, Message = errorMessage };
            return new ApiResponse<T>(error);
        }

        public static ApiResponse<T> CreateFailure(ApiError error)
        {
            return new ApiResponse<T>(error);
        }
    }
}
