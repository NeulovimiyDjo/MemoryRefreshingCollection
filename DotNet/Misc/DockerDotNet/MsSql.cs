using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Dangl.MyProject.TestUtilities
{
    public static class DockerSqlDatabaseUtilities
    {
        public const string SQLSERVER_SA_PASSWORD = "yourStrong(!)Password";
        public const string SQLSERVER_IMAGE = "mcr.microsoft.com/mssql/server";
        public const string SQLSERVER_IMAGE_TAG = "2017-latest";
        public const string SQLSERVER_CONTAINER_NAME_PREFIX = "MyProjectIntegrationTestsSql-";

        public static async Task<(string containerId, string port)> EnsureDockerStartedAndGetContainerIdAndPortAsync()
        {
            await CleanupRunningContainers();
            var dockerClient = GetDockerClient();
            var freePort = GetFreePort();

            // This call ensures that the latest SQL Server Docker image is pulled
            await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = $"{SQLSERVER_IMAGE}:{SQLSERVER_IMAGE_TAG}"
            }, null, new Progress<JSONMessage>());

            var sqlContainer = await dockerClient
                .Containers
                .CreateContainerAsync(new CreateContainerParameters
                {
                    Name = SQLSERVER_CONTAINER_NAME_PREFIX + Guid.NewGuid(),
                    Image = $"{SQLSERVER_IMAGE}:{SQLSERVER_IMAGE_TAG}",
                    Env = new List<string>
                    {
                        "ACCEPT_EULA=Y",
                        $"SA_PASSWORD={SQLSERVER_SA_PASSWORD}"
                    },
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                "1433/tcp",
                                new PortBinding[]
                                {
                                    new PortBinding
                                    {
                                        HostPort = freePort
                                    }
                                }
                            }
                        }
                    }
                });

            await dockerClient
                .Containers
                .StartContainerAsync(sqlContainer.ID, new ContainerStartParameters());

            await WaitUntilDatabaseAvailableAsync(freePort);
            return (sqlContainer.ID, freePort);
        }

        public static async Task EnsureDockerStoppedAndRemovedAsync(string dockerContainerId)
        {
            var dockerClient = GetDockerClient();
            await dockerClient.Containers
                .StopContainerAsync(dockerContainerId, new ContainerStopParameters());
            await dockerClient.Containers
                .RemoveContainerAsync(dockerContainerId, new ContainerRemoveParameters());
        }

        private static DockerClient GetDockerClient()
        {
            var dockerUri = IsRunningOnWindows()
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";
            return new DockerClientConfiguration(new Uri(dockerUri))
                .CreateClient();
        }

        private static async Task CleanupRunningContainers()
        {
            var dockerClient = GetDockerClient();

            var runningContainers = await dockerClient.Containers
                .ListContainersAsync(new ContainersListParameters());

            foreach (var runningContainer in runningContainers.Where(cont => cont.Names.Any(n => n.Contains(SQLSERVER_CONTAINER_NAME_PREFIX))))
            {
                // Stopping all test containers that are older than one hour, they likely failed to cleanup
                if (runningContainer.Created < DateTime.UtcNow.AddHours(-1))
                {
                    try
                    {
                        await EnsureDockerStoppedAndRemovedAsync(runningContainer.ID);
                    }
                    catch
                    {
                        // Ignoring failures to stop running containers
                    }
                }
            }
        }

        private static async Task WaitUntilDatabaseAvailableAsync(string databasePort)
        {
            var start = DateTime.UtcNow;
            const int maxWaitTimeSeconds = 60;
            var connectionEstablised = false;
            while (!connectionEstablised && start.AddSeconds(maxWaitTimeSeconds) > DateTime.UtcNow)
            {
                try
                {
                    var sqlConnectionString = $"Data Source=localhost,{databasePort};Integrated Security=False;User ID=SA;Password={SQLSERVER_SA_PASSWORD}";
                    using var sqlConnection = new SqlConnection(sqlConnectionString);
                    await sqlConnection.OpenAsync();
                    connectionEstablised = true;
                }
                catch
                {
                    // If opening the SQL connection fails, SQL Server is not ready yet
                    await Task.Delay(500);
                }
            }

            if (!connectionEstablised)
            {
                throw new Exception("Connection to the SQL docker database could not be established within 60 seconds.");
            }

            return;
        }

        private static string GetFreePort()
        {
            // Taken from https://stackoverflow.com/a/150974/4190785
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port.ToString();
        }

        private static bool IsRunningOnWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
    }
}


using System.Threading.Tasks;
using Xunit;

namespace Dangl.MyProject.TestUtilities
{
    public class SqlServerDockerCollectionFixture : IAsyncLifetime
    {
        public const string DATABASE_NAME_PLACEHOLDER = "@@databaseName@@";
        private string _dockerContainerId;
        private string _dockerSqlPort;

        public string GetSqlConnectionString()
        {
            return $"Data Source=localhost,{_dockerSqlPort};" +
                $"Initial Catalog={DATABASE_NAME_PLACEHOLDER};" +
                "Integrated Security=False;" +
                "User ID=SA;" +
                $"Password={DockerSqlDatabaseUtilities.SQLSERVER_SA_PASSWORD}";
        }

        public async Task InitializeAsync()
        {
            (_dockerContainerId, _dockerSqlPort) = await DockerSqlDatabaseUtilities.EnsureDockerStartedAndGetContainerIdAndPortAsync();
        }

        public Task DisposeAsync()
        {
            return DockerSqlDatabaseUtilities.EnsureDockerStoppedAndRemovedAsync(_dockerContainerId);
        }
    }
}


using Xunit;

// This is required to have the IAssemblyFixture from the Xunit.Extensions.Ordering Package available
[assembly: TestFramework("Xunit.Extensions.Ordering.TestFramework", "Xunit.Extensions.Ordering")]



using Dangl.AspNetCore.FileHandling;
using Dangl.Identity.Client.Models;
using Dangl.Identity.Client.Mvc.Services;
using Dangl.Identity.TestHost;
using Dangl.Identity.TestHost.SetupData;
using Dangl.MyProject.Data;
using Dangl.MyProject.TestUtilities.TestData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dangl.MyProject.TestUtilities
{
    public class TestHelper
    {
        public static DanglIdentityTestServerManager DanglIdentityTestServerManager { get; }
        private TestServer _testServer;
        private readonly string _databaseConnectionString;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        static TestHelper()
        {
            DanglIdentityTestServerManager = new DanglIdentityTestServerManager(Users.Values,
                Clients.Values,
                new List<string> { IntegrationTestConstants.REQUIRED_SCOPE });
        }

        public TestHelper(string databaseConnectionString)
        {
            _databaseConnectionString = databaseConnectionString
                .Replace(SqlServerDockerCollectionFixture.DATABASE_NAME_PLACEHOLDER, _databaseName);
        }

        public async Task InitializeTestServer()
        {
            if (_testServer != null)
            {
                return;
            }

            var webHostBuilder = new WebHostBuilder()
                .ConfigureLogging(c => c.AddDebug())
                .ConfigureServices(services => services.ConfigureIntegrationTestServices(_databaseConnectionString))
                .Configure((ctx, app) => app.ConfigureIntegrationTestApp(ctx.HostingEnvironment));
            var testServer = new TestServer(webHostBuilder);
            await SeedDatabase(testServer);
            testServer.BaseAddress = new Uri("https://example.com");
            _testServer = testServer;
        }

        public async Task SeedDatabase(TestServer testServer)
        {
            await DatabaseInitializer.InitializeDatabase(testServer.Host.Services);
        }

        public TestServer GetTestServer()
        {
            return _testServer;
        }

        public HttpMessageHandler GetHttpMessageHandler()
        {
            return GetTestServer().CreateHandler();
        }

        public IServiceScope GetScope()
        {
            return GetTestServer().Host.Services.CreateScope();
        }

        public MyDbContext GetNewMyDbContext()
        {
            return GetScope().ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public HttpClient GetAnonymousClient()
        {
            return GetTestServer().CreateClient();
        }

        public async Task<HttpClient> GetJwtAuthenticatedClientAsync(UserSetupDto user = null)
        {
            user ??= Users.User;
            var jwtToken = await GetJwtTokenAsync(user.Email, user.Password);
            var client = GetAnonymousClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken.AccessToken);
            return client;
        }

        public async Task<HttpClient> GetJwtAuthenticatedClientCredentialsClientAsync(ClientSetupDto client)
        {
            var jwtToken = await DanglIdentityTestServerManager.GetJwtClientCredentialsGrantToken(client.ClientId, client.ClientSecret, IntegrationTestConstants.REQUIRED_SCOPE);
            var httpClient = GetAnonymousClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken.AccessToken);
            return httpClient;
        }

        public async Task CleanupTestsAndDropDatabaseAsync()
        {
            using var scope = GetScope();
            var serviceProvider = scope.ServiceProvider;
            var fileManager = serviceProvider.GetRequiredService<IFileManager>() as InstanceInMemoryFileManager;
            fileManager.ClearFiles();

            var integrationTestMemoryCache = scope.ServiceProvider.GetRequiredService<IUserInfoUpdaterCache>() as MockUserInfoUpdaterCache;
            integrationTestMemoryCache.Clear();

            if (Environment.GetEnvironmentVariable("DANGL_MYPROJECT_SKIP_DATABASE_DROP_TEST_CLEAN") != "true")
            {
                using var context = serviceProvider.GetRequiredService<MyDbContext>();
                await context.Database.EnsureDeletedAsync();
            }
        }

        public async Task<TokenResponseGet> GetJwtTokenAsync(string userIdentifier, string password)
        {
            var client = GetAnonymousClient();
            var loginModel = new TokenLoginPost
            {
                Identifier = userIdentifier,
                Password = password
            };

            const string tokenUrl = "/identity/token-login";
            var tokenResponse = await client.PostAsJsonAsync(loginModel, tokenUrl);
            var responseString = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<TokenResponseGet>(responseString);
            return token;
        }
    }
}



using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Dangl.MyProject.TestUtilities
{
    public abstract class IntegrationTestBase : IAsyncLifetime, IAssemblyFixture<SqlServerDockerCollectionFixture>
    {
        protected readonly SqlServerDockerCollectionFixture _fixture;
        protected TestHelper _testHelper;

        protected IntegrationTestBase(SqlServerDockerCollectionFixture fixture)
        {
            _fixture = fixture;
        }

        public virtual async Task InitializeAsync()
        {
            var sqlConnectionString = _fixture.GetSqlConnectionString();
            _testHelper = new TestHelper(sqlConnectionString);
            await _testHelper.InitializeTestServer();
        }

        public virtual async Task DisposeAsync()
        {
            await _testHelper.CleanupTestsAndDropDatabaseAsync();
        }
    }
}