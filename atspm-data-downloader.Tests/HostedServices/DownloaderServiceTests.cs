#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm-data-downloader.Tests/HostedServices/DownloaderServiceTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using atspm_data_downloader.Configuration;
using atspm_data_downloader.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace atspm_data_downloader.Tests.HostedServices;

public class DownloaderServiceTests : IDisposable
{
    private readonly string _tempOutputDir;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IHostApplicationLifetime> _mockLifetime;
    private readonly Mock<ILogger<AggregationDownloaderService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public DownloaderServiceTests()
    {
        // Setup temporary output directory for test isolation
        _tempOutputDir = Path.Combine(Path.GetTempPath(), "atspm_tests_" + Guid.NewGuid().ToString("N"));
        if (!Directory.Exists(_tempOutputDir))
        {
            Directory.CreateDirectory(_tempOutputDir);
        }

        // Mock generic host & service provider structures
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLifetime = new Mock<IHostApplicationLifetime>();
        _mockLogger = new Mock<ILogger<AggregationDownloaderService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(_mockScopeFactory.Object);
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
    }

    public void Dispose()
    {
        // Clean up temporary output directory
        if (Directory.Exists(_tempOutputDir))
        {
            try
            {
                Directory.Delete(_tempOutputDir, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    private AggregationDownloaderService CreateService(DownloaderConfiguration config, HttpClient httpClient)
    {
        var options = Microsoft.Extensions.Options.Options.Create(config);
        return new AggregationDownloaderService(
            _mockScopeFactory.Object,
            httpClient,
            options,
            _mockLifetime.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Process_ShouldDownloadAndWriteNdJson_WhenFormatIsNdJson()
    {
        // Arrange
        var config = new DownloaderConfiguration
        {
            Start = new DateTime(2026, 8, 1, 0, 0, 0),
            End = new DateTime(2026, 8, 1, 1, 0, 0),
            LocationIdentifiers = new() { "LOC100" },
            ApiUrl = "https://test-atspm.com",
            Format = DownloadFormat.NdJson,
            OutputPath = _tempOutputDir
        };

        var responseContent = "{\"Id\":1,\"Val\":10}\n{\"Id\":2,\"Val\":20}";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                // Verify URL is formatted correctly using UrlDecode for casing robustness
                var decodedUri = WebUtility.UrlDecode(request.RequestUri!.ToString());
                Assert.Contains("/api/v1/Aggregation/StreamData/LOC100", decodedUri);
                Assert.Contains("start=2026-08-01T00:00:00", decodedUri);
                Assert.Contains("end=2026-08-01T01:00:00", decodedUri);

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                };
            });

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        var service = CreateService(config, httpClient);

        // Act
        await service.Process(_mockScope.Object);

        // Assert
        var expectedFilename = "LOC100-aggregations-20260801000000-20260801010000.ndjson";
        var expectedFilePath = Path.Combine(_tempOutputDir, expectedFilename);

        Assert.True(File.Exists(expectedFilePath));
        var fileContent = await File.ReadAllTextAsync(expectedFilePath);
        var expectedNormalized = responseContent.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim() + Environment.NewLine;
        Assert.Equal(expectedNormalized, fileContent);

        _mockLifetime.Verify(l => l.StopApplication(), Times.Once);
    }

    [Fact]
    public async Task Process_ShouldDownloadAndWriteJsonArray_WhenFormatIsJson()
    {
        // Arrange
        var config = new DownloaderConfiguration
        {
            Start = new DateTime(2026, 8, 1, 0, 0, 0),
            End = new DateTime(2026, 8, 1, 1, 0, 0),
            LocationIdentifiers = new() { "LOC200" },
            ApiUrl = "https://test-atspm.com",
            Format = DownloadFormat.Json,
            OutputPath = _tempOutputDir
        };

        var responseContent = "{\"Id\":1,\"Val\":10}\n{\"Id\":2,\"Val\":20}";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        var service = CreateService(config, httpClient);

        // Act
        await service.Process(_mockScope.Object);

        // Assert
        var expectedFilename = "LOC200-aggregations-20260801000000-20260801010000.json";
        var expectedFilePath = Path.Combine(_tempOutputDir, expectedFilename);

        Assert.True(File.Exists(expectedFilePath));
        var fileContent = await File.ReadAllTextAsync(expectedFilePath);
        // JSON format converts lines of objects into a JSON array: [{"Id":1,"Val":10},{"Id":2,"Val":20}]
        Assert.Equal("[{\"Id\":1,\"Val\":10},{\"Id\":2,\"Val\":20}]", fileContent);

        _mockLifetime.Verify(l => l.StopApplication(), Times.Once);
    }

    [Fact]
    public async Task Process_ShouldDownloadAndWriteCsv_WhenFormatIsCsv()
    {
        // Arrange
        var config = new DownloaderConfiguration
        {
            Start = new DateTime(2026, 8, 1, 0, 0, 0),
            End = new DateTime(2026, 8, 1, 1, 0, 0),
            LocationIdentifiers = new() { "LOC300" },
            ApiUrl = "https://test-atspm.com",
            Format = DownloadFormat.Csv,
            OutputPath = _tempOutputDir
        };

        // Stream JSON lines to be converted into CSV rows
        var responseContent = "{\"Id\":1,\"Name\":\"John, Doe\",\"Val\":10.5}\n{\"Id\":2,\"Name\":\"Jane \\\"Smith\\\"\",\"Val\":null}";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        var service = CreateService(config, httpClient);

        // Act
        await service.Process(_mockScope.Object);

        // Assert
        var expectedFilename = "LOC300-aggregations-20260801000000-20260801010000.csv";
        var expectedFilePath = Path.Combine(_tempOutputDir, expectedFilename);

        Assert.True(File.Exists(expectedFilePath));
        var fileLines = await File.ReadAllLinesAsync(expectedFilePath);

        Assert.Equal(3, fileLines.Length);
        Assert.Equal("Id,Name,Val", fileLines[0]);
        Assert.Equal("1,\"John, Doe\",10.5", fileLines[1]);
        Assert.Equal("2,\"Jane \"\"Smith\"\"\",", fileLines[2]); // null outputs empty string, quotes escaped

        _mockLifetime.Verify(l => l.StopApplication(), Times.Once);
    }

    [Fact]
    public async Task Process_ShouldAddApiKeyHeader_WhenApiKeyIsProvided()
    {
        // Arrange
        var config = new DownloaderConfiguration
        {
            Start = new DateTime(2026, 8, 1, 0, 0, 0),
            End = new DateTime(2026, 8, 1, 1, 0, 0),
            LocationIdentifiers = new() { "LOC400" },
            ApiUrl = "https://test-atspm.com",
            ApiKey = "my-secret-token-123",
            Format = DownloadFormat.NdJson,
            OutputPath = _tempOutputDir
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                // Assert the custom header is present and holds the API Key
                Assert.True(request.Headers.Contains("X-API-KEY"));
                var apiKeyHeader = request.Headers.GetValues("X-API-KEY");
                Assert.Contains("my-secret-token-123", apiKeyHeader);

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"Id\":1}")
                };
            });

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        var service = CreateService(config, httpClient);

        // Act
        await service.Process(_mockScope.Object);

        // Assert
        var expectedFilename = "LOC400-aggregations-20260801000000-20260801010000.ndjson";
        var expectedFilePath = Path.Combine(_tempOutputDir, expectedFilename);
        Assert.True(File.Exists(expectedFilePath));

        _mockLifetime.Verify(l => l.StopApplication(), Times.Once);
    }

    [Fact]
    public async Task Process_ShouldHandleApiErrorAndContinue_WhenOneLocationFails()
    {
        // Arrange
        var config = new DownloaderConfiguration
        {
            Start = new DateTime(2026, 8, 1, 0, 0, 0),
            End = new DateTime(2026, 8, 1, 1, 0, 0),
            LocationIdentifiers = new() { "FAIL_LOC", "SUCCESS_LOC" },
            ApiUrl = "https://test-atspm.com",
            Format = DownloadFormat.NdJson,
            OutputPath = _tempOutputDir
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                var uri = request.RequestUri!.ToString();
                if (uri.Contains("FAIL_LOC"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Content = new StringContent("Internal database crash!")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"Id\":100}")
                };
            });

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        var service = CreateService(config, httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.Process(_mockScope.Object));
        Assert.Contains("FAIL_LOC", exception.Message);

        // The failed location shouldn't produce a file
        var failedFilename = "FAIL_LOC-aggregations-20260801000000-20260801010000.ndjson";
        Assert.False(File.Exists(Path.Combine(_tempOutputDir, failedFilename)));

        // The successful location should produce its file
        var successFilename = "SUCCESS_LOC-aggregations-20260801000000-20260801010000.ndjson";
        Assert.True(File.Exists(Path.Combine(_tempOutputDir, successFilename)));

        _mockLifetime.Verify(l => l.StopApplication(), Times.Once);
    }
}
