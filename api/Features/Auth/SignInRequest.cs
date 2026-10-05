using System.ComponentModel.DataAnnotations;

namespace BikeShop.Api.Features.Auth;

public sealed record SignInRequest([Required] string Username, [Required] string Password);
