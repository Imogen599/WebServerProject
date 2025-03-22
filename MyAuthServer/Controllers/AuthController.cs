using Microsoft.AspNetCore.Mvc;
using MyAuthServer.RequestStructures;
using MyAuthServer.Services;
using Serilog;

namespace MyAuthServer.Controllers
{
	[Route("api/auth")]
	[ApiController]
	[RequireHttps]
	public class AuthController(IUserService userService, ISessionTokenService sessionTokenService) : ControllerBase
	{
		private readonly IUserService _userService = userService;

		private readonly ISessionTokenService _sessionTokenService = sessionTokenService;

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] CustomLoginRequest request)
		{
			if (await _sessionTokenService.ValidateSession(request.SessionToken, request.DeviceId))
				return Unauthorized(new { message = "You are already logged in. You must log out first", message2 = string.Empty });

			var userLoginModel = await _userService.LoginUserAsync(request);
			if (userLoginModel.Successful)
			{
				Log.Information($"Successful login attempt for user {request.Username} from IP: {HttpContext.Connection.RemoteIpAddress}");
				await _sessionTokenService.CreateSession(userLoginModel.UserId, userLoginModel.GeneratedSessionToken, request.DeviceId);
				return Ok(new { message = "Login successful!", message2 = userLoginModel.GeneratedSessionToken });
			}

			Log.Information($"Failed login attempt for user {request.Username} from IP: {HttpContext.Connection.RemoteIpAddress}");
			return Unauthorized(new { message = "Invalid credentials", message2 = string.Empty });
		}

		[HttpPost("checkLogin")]
		public async Task<IActionResult> CheckIfLoggedIn([FromBody] BasicRequest request)
		{
			if (await _sessionTokenService.ValidateSession(request.SessionToken, request.DeviceId))
				return Ok(new { message = "You are currently logged in" });
			return Unauthorized(new { message = "You are not currently logged in" });
		}

		[HttpPost("changePassword")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			if (!await _sessionTokenService.ValidateSession(request.SessionToken, request.DeviceId))
				return Unauthorized(new { message = "You need to be logged in in order to change your password" });

			var fetchResult = await _sessionTokenService.FetchUserIdForSession(request.SessionToken, request.DeviceId);

			if (!fetchResult.Successful)
				return Unauthorized(new { message = "You need to be logged in in order to change your password" });

			var changeUserPasswordResult = await _userService.ChangeUserPassword(fetchResult.UserId, request, HttpContext.Connection.RemoteIpAddress.ToString());
			if (changeUserPasswordResult.Successful)
				return Ok(new { message = "Password successfully changed!" });

			return Unauthorized(new { message = changeUserPasswordResult.ErrorMessage.ToString() });
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout([FromBody] BasicRequest request)
		{
			if (await _sessionTokenService.ValidateSession(request.SessionToken, request.DeviceId))
			{
				await _userService.LogoutAsync(request);
				Log.Information($"Session Token: {request.SessionToken} assigned to Device Id: {request.DeviceId} has been canceled due to the user logging out, from IP: {HttpContext.Connection.RemoteIpAddress}.");
				return Ok(new { message = "You have been logged out" });
			}

			return Unauthorized(new { message = "You need to be logged in in order to log out" });
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] CustomLoginRequest request)
		{
			if (await _userService.IsUsernameTakenAsync(request.Username))
				return Unauthorized(new { message = "Username already belongs to an existing account!" });

			await _userService.RegisterUserAsync(request);
			Log.Information($"User {request.Username} has been created from Device Id: {request.DeviceId}, IP: {HttpContext.Connection.RemoteIpAddress}.");
			return Ok(new { message = "Register successful!" });
		}
	}
}
