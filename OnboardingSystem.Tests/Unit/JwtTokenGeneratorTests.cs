using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OnboardingSystem.Services;
using Xunit;

namespace OnboardingSystem.Tests.Unit;

public class JwtTokenGeneratorTests
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenGenerator> _logger;
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretSigningKey1234567890_ABCDEF0123456789",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:ExpiresInHours"] = "24"
            })
            .Build();

        _logger = NullLogger<JwtTokenGenerator>.Instance;
        _generator = new JwtTokenGenerator(_configuration, _logger);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyJwt()
    {
        var token = _generator.GenerateToken(123, "user@example.com", "Иван Иванов", new List<string> { "employee", "admin" });

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Contains('.', token);
    }

    [Fact]
    public void ValidateToken_ReturnsTrue_ForValidToken()
    {
        var token = _generator.GenerateToken(123, "user@example.com", "Иван Иванов");

        var result = _generator.ValidateToken(token);

        Assert.True(result);
    }

    [Fact]
    public void ValidateToken_ReturnsFalse_ForInvalidToken()
    {
        var result = _generator.ValidateToken("invalid.token.value");

        Assert.False(result);
    }

    [Fact]
    public void GetClaimsFromToken_ReturnsExpectedClaims()
    {
        var token = _generator.GenerateToken(77, "test@company.local", "Тестовый Пользователь", new List<string> { "employee" });

        var claims = _generator.GetClaimsFromToken(token);

        Assert.NotNull(claims);
        Assert.Equal("77", claims!["sub"]);
        Assert.Equal("test@company.local", claims["email"]);
        Assert.Equal("Тестовый Пользователь", claims["name"]);
        Assert.Equal("Local", claims["provider"]);
    }
}
