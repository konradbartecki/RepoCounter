using FluentAssertions;
using GraphQL.Client.Http;
using Me.Bartecki.RepoCounter.Domain.Model;
using Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores;
using Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores.GitHub;
using Me.Bartecki.RepoCounter.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Me.Bartecki.RepoCounter.Api.IntegrationTests
{
    [TestClass]
    public class IntegrationTests
    {
        private HttpClient _cilent;

        private HttpClient GetMockedHttpClinet(string filename)
        {
            var projectDir = Directory.GetCurrentDirectory();
            var responsePath = Path.Combine(projectDir, filename);
            var responseText = File.ReadAllText(responsePath);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseText),
                })
                .Verifiable();
            return new HttpClient(handlerMock.Object);
        }

        private IHostBuilder GetHostBuilder()
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.json");

            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    // Add TestServer
                    webHost.UseTestServer();
                    webHost.ConfigureAppConfiguration(c => c.AddJsonFile(configPath));
                    webHost.UseStartup<Startup>();
                });
            return hostBuilder;
        }

        [TestMethod]
        public void CanCalculateStatistics_Real()
        {
            var client = GetHostBuilder().Start().GetTestClient();
            var response = client.SendAsync((new HttpRequestMessage(HttpMethod.Get, "/repositories/konradbartecki"))).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var stats = JsonConvert.DeserializeObject<UserStatistics>(json);

            stats.Owner.Should().Be("konradbartecki");
            stats.AverageForks.Should().BeGreaterThan(0);
            stats.AverageSize.Should().BeGreaterThan(0);
            stats.AverageWatchers.Should().BeGreaterThan(0);
            stats.AverageStargazers.Should().BeGreaterThan(0);
            stats.Letters.Should().NotBeEmpty();
        }

        [TestMethod]
        public void CanReturn_NotFound_OnUserNotFound()
        {
            var client = GetHostBuilder().Start().GetTestClient();
            var response = client.SendAsync((new HttpRequestMessage(HttpMethod.Get, $"/repositories/user-that-does-not-exist-{Guid.NewGuid():N}"))).Result;
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void CanReturn_NotFound_OnNoRepositories()
        {
            var mockedHttpClient = GetMockedHttpClinet("example_response_norepos.json");
            var embeddedService = new EmbeddedResourceService();
            var githubService = new GitHubIntegrationService(mockedHttpClient, embeddedService);

            var client = GetHostBuilder().ConfigureServices(x =>
                    x.AddScoped<IRepositoryStoreService, GitHubIntegrationService>(provider => githubService))
                .Start()
                .GetTestClient();
            var response = client.SendAsync((new HttpRequestMessage(HttpMethod.Get, "/repositories/ignored"))).Result;
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task CanThrowException_OnIncorrectGitHubToken()
        {
            var fakeConfig = new Dictionary<string, string>()
            {
                {"Integrations.Github.Token", "incorrect-token"}
            };
            var client = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureAppConfiguration(config => config.AddInMemoryCollection(fakeConfig));
                    webHost.UseStartup<Startup>();
                })
                .Start()
                .GetTestClient();

            //We want this exception rethrowed
            await Assert.ThrowsExceptionAsync<GraphQLHttpException>(() =>
                client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/repositories/ignored")));
        }

        [TestMethod]
        public void CanReturn_DependencyFailed_OnGitHubTimeout()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.RequestTimeout,
                })
                .Verifiable();
            var mockedClient = new HttpClient(handlerMock.Object);
            var embeddedService = new EmbeddedResourceService();
            var githubService = new GitHubIntegrationService(mockedClient, embeddedService);

            var client = GetHostBuilder().ConfigureServices(x =>
                    x.AddScoped<IRepositoryStoreService, GitHubIntegrationService>(provider => githubService))
                .Start()
                .GetTestClient();

            var response = client.SendAsync((new HttpRequestMessage(HttpMethod.Get, "/repositories/ignored"))).Result;
            Assert.AreEqual(HttpStatusCode.FailedDependency, response.StatusCode);
        }


        [TestMethod]
        public void CanCalculateStatistics_Mocked()
        {
            var mockedHttpClient = GetMockedHttpClinet("example_response.json");
            var embeddedService = new EmbeddedResourceService();
            var githubService = new GitHubIntegrationService(mockedHttpClient, embeddedService);

            var client = GetHostBuilder().ConfigureServices(x =>
                x.AddScoped<IRepositoryStoreService, GitHubIntegrationService>(provider => githubService))
                .Start()
                .GetTestClient();
            var response = client.SendAsync((new HttpRequestMessage(HttpMethod.Get, "/repositories/ignored"))).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var stats = JsonConvert.DeserializeObject<UserStatistics>(json);

            stats.AverageForks.Should().Be(2);
            stats.AverageStargazers.Should().Be(3);
            stats.AverageWatchers.Should().Be(7);
            stats.AverageSize.Should().Be(500);
            
            stats.Letters.Should().BeEquivalentTo(new Dictionary<char, int>()
            {
                ['r'] = 4,
                ['e'] = 4,
                ['p'] = 2,
                ['o'] = 4,
                ['c'] = 2,
                ['u'] = 2,
                ['n'] = 2,
                ['t'] = 2,
                ['a'] = 1,
                ['l'] = 2
            });
            stats.Letters.First().Key.Should().Be('a');
            stats.Letters.Last().Key.Should().Be('u');
        }
    }
}
