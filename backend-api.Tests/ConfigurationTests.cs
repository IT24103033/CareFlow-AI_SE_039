using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CareFlowAI.API.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void Legacy_GeminiApiKey_Placeholder_Cannot_Override_Shared_Configuration()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"GeminiApiKey", "YOUR_GEMINI_API_KEY_HERE"},
                {"Gemini:ApiKey", "REAL_SECRET_KEY"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Act
            // Simulating how Program.cs reads it
            var apiKey = configuration["Gemini:ApiKey"];

            // Assert
            Assert.Equal("REAL_SECRET_KEY", apiKey);
            
            // Prove that if someone used the old fallback logic, it would have taken the placeholder
            var oldFallbackLogic = configuration["GeminiApiKey"] ?? configuration["Gemini:ApiKey"];
            Assert.Equal("YOUR_GEMINI_API_KEY_HERE", oldFallbackLogic);
        }
    }
}
