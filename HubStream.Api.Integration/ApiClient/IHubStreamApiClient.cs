using HubStream.Api.Integration.Features.Authentication.ChangePassword;
using HubStream.Api.Integration.Features.Authentication.Login;
using HubStream.Api.Integration.Features.Authentication.SignUp;
using HubStream.Api.Integration.Features.Authentication.VerifyEmail;
using HubStream.Shared.Kernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Api.Integration.ApiClient
{
    public interface IHubStreamApiClient
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<SignUpResponse>> SignUpAsync(SignUpRequest request);
        Task<ApiResponse<object>> ChangePasswordAsync(ChangePasswordRequest request);
        Task<ApiResponse<object>> VerifyEmailAsync(VerifyEmailRequest request);
    }
}
