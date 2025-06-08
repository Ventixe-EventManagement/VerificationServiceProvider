using Azure.Communication.Email;
using Azure.Identity;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Load secrets from Azure Key Vault
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());

// Register controllers and endpoints
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Azure Communication Services Email Client
builder.Services.AddMemoryCache();
builder.Services.AddSingleton(x =>
    new EmailClient(builder.Configuration["ACS:ConnectionString"])
);
builder.Services.AddTransient<IVerificationService, VerificationService>();

var app = builder.Build();

// Middleware pipeline
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
