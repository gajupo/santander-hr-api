using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using santander_hr_api.Controllers;
using santander_hr_api.Extensions;
using santander_hr_api.Interfaces;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace santander_hr_api.tests
{
    [TestFixture]
    public class WebApiTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private string[] _testArgs;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["HackerRankBaseUrl"]).Returns("https://test-api.com");
            _testArgs = Array.Empty<string>();

            // Reset logger before each test
            Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
        }

        [Test]
        public void CreateHostBuilder_ShouldReturnValidBuilder()
        {
            // Act
            var builder = Program.CreateHostBuilder(_testArgs);

            // Assert
            builder.Should().NotBeNull();
            builder.Services.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_ShouldConfigureRequiredServices()
        {
            // Act
            var builder = Program.CreateHostBuilder(_testArgs);

            // Assert
            var services = builder.Services;
            services.Should().Contain(x => x.ServiceType == typeof(IMemoryCache));
            services.Should().Contain(x => x.ServiceType == typeof(IHttpClientFactory));
            services.Should().Contain(x => x.ServiceType == typeof(IMapper));
        }

        [Test]
        public void ConfigureHttpClient_ShouldUseCorrectBaseUrl()
        {
            // Arrange
            var builder = Program.CreateHostBuilder(_testArgs);

            // Act
            var serviceProvider = builder.Services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("hackerNews");

            // Assert
            client.BaseAddress.Should().Be(new Uri("https://hacker-news.firebaseio.com/v0/"));
            client.DefaultRequestHeaders.Accept.First().ToString().Should().Be("application/json");
        }

        [Test]
        public void ConfigureHttpClient_WithoutBaseUrl_ShouldUseDefaultUrl()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["HackerRankBaseUrl"]).Returns((string)null);
            var builder = Program.CreateHostBuilder(_testArgs);

            // Act
            var serviceProvider = builder.Services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("hackerNews");

            // Assert
            client.BaseAddress.Should().Be(new Uri("https://hacker-news.firebaseio.com/v0/"));
        }

        [Test]
        public async Task Main_SuccessfulStartup_LogsCorrectly()
        {
            // Arrange
            var testLogger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
            using var context = TestCorrelator.CreateContext();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var builder = Program.CreateHostBuilder(_testArgs);

            // Act
            try
            {
                await Program.RunApplication(builder, cts.Token, testLogger);
            }
            catch (OperationCanceledException)
            {
                // Expected - application was cancelled
            }

            // Assert
            var logs = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
            logs.Should().Contain(log => log.MessageTemplate.Text == "Starting up");
        }

        [TearDown]
        public void Cleanup()
        {
            Log.CloseAndFlush();
        }
    }
}
