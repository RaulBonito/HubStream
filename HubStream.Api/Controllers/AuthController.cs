using HubStream.Application.Features.Authentication.Commands.ChangePassword;
using HubStream.Application.Features.Authentication.Commands.ExternalLogin;
using HubStream.Application.Features.Authentication.Commands.ForgotPassword;
using HubStream.Application.Features.Authentication.Commands.Login;
using HubStream.Application.Features.Authentication.Commands.Logout;
using HubStream.Application.Features.Authentication.Commands.RefreshToken;
using HubStream.Application.Features.Authentication.Commands.ResendVerificationEmail;
using HubStream.Application.Features.Authentication.Commands.ResetPassword;
using HubStream.Application.Features.Authentication.Commands.SignUp;
using HubStream.Application.Features.Authentication.Commands.VerifyEmail;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HubStream.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _mediator;

        public AuthController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return Unauthorized(ApiResponse<object>.CreateFailure(apiError));
            }

            return Ok(ApiResponse<LoginResult>.CreateSuccess(result.Value));
        }

        [HttpPost("signup")]
        [ProducesResponseType(typeof(ApiResponse<SignUpResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return BadRequest(ApiResponse<object>.CreateFailure(apiError));
            }

            return Ok(ApiResponse<SignUpResult>.CreateSuccess(result.Value));
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // CAMBIO AQUÍ: de ApiResponse<Result> a ApiResponse<object>
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command); // 'result' es de tipo HubStream.Shared.Kernel.Common.Result

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return BadRequest(ApiResponse<object>.CreateFailure(apiError));
            }

            // CAMBIO AQUÍ: en lugar de ApiResponse<Result>.CreateSuccess(result)
            // ahora devuelve null en el campo Data, ya que el Success del ApiResponse es suficiente
            return Ok(ApiResponse<object>.CreateSuccess(null));
            // Opcional, si deseas un mensaje de éxito en el Data:
            // return Ok(ApiResponse<string>.CreateSuccess("Correo de restablecimiento de contraseña enviado."));
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // CAMBIO AQUÍ
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return BadRequest(ApiResponse<object>.CreateFailure(apiError));
            }

            return Ok(ApiResponse<object>.CreateSuccess(null)); // CAMBIO AQUÍ
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return Unauthorized(ApiResponse<object>.CreateFailure(apiError));
            }

            return Ok(ApiResponse<LoginResult>.CreateSuccess(result.Value));
        }

        [HttpPost("logout")]
        [Authorize] // Este también debería tenerlo.
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return BadRequest(ApiResponse<object>.CreateFailure(apiError));
            }
            return Ok(ApiResponse<object>.CreateSuccess(null));
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // CAMBIO AQUÍ
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return BadRequest(ApiResponse<object>.CreateFailure(apiError));
            }

            return Ok(ApiResponse<object>.CreateSuccess(null)); // CAMBIO AQUÍ
        }

        [HttpPost("resend-verification-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // CAMBIO AQUÍ
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return BadRequest(ApiResponse<object>.CreateFailure(apiError));
            }

            return Ok(ApiResponse<object>.CreateSuccess(null)); // CAMBIO AQUÍ
        }


        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return BadRequest(ApiResponse<object>.CreateFailure(apiError));
            }
            return Ok(ApiResponse<object>.CreateSuccess(null));
        }

        [HttpPost("external-login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                var apiError = new ApiError { Code = result.Error.Code, Message = result.Error.Message };
                return Unauthorized(ApiResponse<object>.CreateFailure(apiError));
            }

            return Ok(ApiResponse<LoginResult>.CreateSuccess(result.Value));
        }
    }
}
