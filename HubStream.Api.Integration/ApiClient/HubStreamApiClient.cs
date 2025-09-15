using HubStream.Api.Integration.Features.Authentication.ChangePassword;
using HubStream.Api.Integration.Features.Authentication.Login;
using HubStream.Api.Integration.Features.Authentication.SignUp;
using HubStream.Api.Integration.Features.Authentication.VerifyEmail;
using HubStream.Shared.Kernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HubStream.Api.Integration.ApiClient
{
    public class HubStreamApiClient : IHubStreamApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public HubStreamApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // POST con cuerpo de solicitud y esperando una respuesta con contenido.
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            return await PostAsync<LoginRequest, LoginResponse>("api/auth/login", request);
        }

        // POST con cuerpo de solicitud y esperando una respuesta con contenido.
        public async Task<ApiResponse<SignUpResponse>> SignUpAsync(SignUpRequest request)
        {
            return await PostAsync<SignUpRequest, SignUpResponse>("api/auth/signup", request);
        }

        // POST con cuerpo de solicitud pero sin esperar contenido en la respuesta (solo éxito o fracaso).
        public async Task<ApiResponse<object>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            return await PostAsync<ChangePasswordRequest, object>("api/auth/change-password", request);
        }

        // POST con cuerpo de solicitud pero sin esperar contenido en la respuesta.
        public async Task<ApiResponse<object>> VerifyEmailAsync(VerifyEmailRequest request)
        {
            // Asumiendo que este es un endpoint POST. Podría ser GET también.
            return await PostAsync<VerifyEmailRequest, object>("api/auth/verify-email", request);
        }

        private async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string uri, TRequest requestData)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, requestData, _jsonOptions);

            // Intenta leer el cuerpo de la respuesta en cualquier caso
            var contentStream = await response.Content.ReadAsStreamAsync();
            if (contentStream.Length == 0)
            {
                return response.IsSuccessStatusCode
                    ? ApiResponse<TResponse>.CreateFailure(new ApiError("Api.Error", "La respuesta del servidor fue exitosa pero vacía."))
                    : ApiResponse<TResponse>.CreateFailure(new ApiError("Api.Error", $"Error de la API: {response.StatusCode}"));
            }

            var result = await JsonSerializer.DeserializeAsync<ApiResponse<TResponse>>(contentStream, _jsonOptions);

            if (result == null)
            {
                return ApiResponse<TResponse>.CreateFailure(new ApiError("Serialization.Error", "No se pudo deserializar la respuesta."));
            }

            return result;
        }
    }
}
