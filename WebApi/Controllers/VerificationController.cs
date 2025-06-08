using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

// This controller handles sending and verifying email verification codes.
[Route("api/[controller]")]
[ApiController]
public class VerificationController(IVerificationService verificationService) : ControllerBase
{
    // Service responsible for verification logic
    private readonly IVerificationService _verificationService = verificationService;

    // Endpoint for sending a verification code to the user's email
    [HttpPost("send")]
    public async Task<IActionResult> Send(SendVerificationCodeRequest request)
    {
        // Validate the input model
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Recipient email address is required." });

        // Send the verification code via the service
        var result = await _verificationService.SendVerificationCodeAsync(request);

        // Return 200 OK if successful, otherwise return 500 Internal Server Error
        return result.Succeeded
            ? Ok(result)
            : StatusCode(500, result);
    }

    // Endpoint for verifying the code submitted by the user
    [HttpPost("verify")]
    public IActionResult Verify(VerifyVerificationCodeRequest request)
    {
        // Validate the input model
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Invalid or expired verification code." });

        // Check if the code is valid using the service
        var result = _verificationService.VerifyVerificationCode(request);

        // Return 200 OK if the code is valid, otherwise return 500 Internal Server Error
        return result.Succeeded
            ? Ok(result)
            : StatusCode(500, result);
    }
}
