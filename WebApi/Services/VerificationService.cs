using System.Diagnostics;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Caching.Memory;
using WebApi.Models;

namespace WebApi.Services;

// Interface defining the contract for verification operations
public interface IVerificationService
{
    Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request);
    void SaveVerificationCode(SaveVerificationCodeRequest request);
    VerificationServiceResult VerifyVerificationCode(VerifyVerificationCodeRequest request);
}

public class VerificationService(IConfiguration configuration, EmailClient emailClient, IMemoryCache cache) : IVerificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly EmailClient _emailClient = emailClient;
    private readonly IMemoryCache _cache = cache;
    private static readonly Random _random = new();

    // Sends a verification code via email
    public async Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request)
    {
        try
        {
            // Validate the input email address
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return new VerificationServiceResult { Succeeded = false, Error = "Recipient email address is required." };

            // Generate a random 6-digit verification code
            var verificationCode = _random.Next(100000, 999999).ToString();

            // Compose subject and plain text message
            var subject = $"Your code is {verificationCode}";
            var plainTextContent = @$"
            Verify Your Email Address

            Hello,

            To complete your verification, please enter the following code:

            {verificationCode}

            Alternatively, you can open the verification page using the following link:
            https://agreeable-stone-0b26cb203.6.azurestaticapps.net/verify?email={Uri.EscapeDataString(request.Email)}&token={verificationCode}

            If you did not initiate this request, you can safely disregard this email.
            ";

            // Compose a styled HTML email body
            var htmlContent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <title>Your verification code</title>
            </head>
            <body style=""max-width:600px; margin:32px auto; background:#FFFFFF; border-radius:16px; padding:32px; font-family: Inter, sans-serif;"">
                <h1 style=""text-align:center;"">Verify Your Email Address</h1>
                <p>Hello,</p>
                <p>Please enter the code below or click the button:</p>
                <div style=""text-align:center; font-size:24px;"">{verificationCode}</div>
                <div style=""text-align:center; margin-top:32px;"">
                    <a href=""https://agreeable-stone-0b26cb203.6.azurestaticapps.net/verify?email={Uri.EscapeDataString(request.Email)}&token={verificationCode}"" style=""background:#F26CF9; padding:12px 24px; color:#fff; text-decoration:none; border-radius:8px;"">
                       Open Verification Page
                    </a>
                </div>
            </body>
            </html>";

            // Create email message and send
            var emailMessage = new EmailMessage(
                senderAddress: _configuration["ACS:SenderAddress"],
                recipients: new EmailRecipients([new(request.Email)]),
                content: new EmailContent(subject)
                {
                    PlainText = plainTextContent,
                    Html = htmlContent
                });

            var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage);

            // Save the verification code in memory cache
            SaveVerificationCode(new SaveVerificationCodeRequest
            {
                Email = request.Email,
                Code = verificationCode,
                ValidFor = TimeSpan.FromMinutes(5)
            });

            return new VerificationServiceResult { Succeeded = true, Message = "Verification email sent successfully." };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return new VerificationServiceResult { Succeeded = false, Error = "Failed to send verification email." };
        }
    }

    // Saves the generated verification code in memory cache
    public void SaveVerificationCode(SaveVerificationCodeRequest request)
    {
        _cache.Set(request.Email.ToLowerInvariant(), request.Code, request.ValidFor);
    }

    // Verifies the submitted code against the cached one
    public VerificationServiceResult VerifyVerificationCode(VerifyVerificationCodeRequest request)
    {
        var key = request.Email.ToLowerInvariant();

        if (_cache.TryGetValue(key, out string? storedCode))
        {
            if (storedCode == request.Code)
            {
                // Remove the code after successful verification
                _cache.Remove(key);
                return new VerificationServiceResult
                {
                    Succeeded = true,
                    Message = "Verification successful."
                };
            }
        }

        return new VerificationServiceResult
        {
            Succeeded = false,
            Error = "Invalid or expired verification code."
        };
    }
}
