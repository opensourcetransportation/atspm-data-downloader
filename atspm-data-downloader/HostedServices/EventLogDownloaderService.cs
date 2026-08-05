#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.HostedServices/EventLogDownloaderService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using atspm_data_downloader.Configuration;
using atspm_data_downloader.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace atspm_data_downloader.HostedServices;

/// <summary>
/// Hosted service designed for managing the high-efficiency stream downloading of ATSPM event log data.
/// </summary>
public class EventLogDownloaderService : HostedServiceBase
{
    private readonly HttpClient _httpClient;
    private readonly DownloaderConfiguration _options;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<EventLogDownloaderService> _logger;
    private readonly EventLogDownloaderLogMessages _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogDownloaderService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The active service provider scope factory.</param>
    /// <param name="httpClient">The HTTP client instance for requesting the endpoint.</param>
    /// <param name="options">The configured downloader configuration options.</param>
    /// <param name="lifetime">The application hosting lifetime controller.</param>
    /// <param name="logger">The system logger provider.</param>
    public EventLogDownloaderService(
        IServiceScopeFactory serviceProvider,
        HttpClient httpClient,
        IOptions<DownloaderConfiguration> options,
        IHostApplicationLifetime lifetime,
        ILogger<EventLogDownloaderService> logger) : base(logger, serviceProvider)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _lifetime = lifetime;
        _logger = logger;
        _log = new EventLogDownloaderLogMessages(logger);
    }

    /// <summary>
    /// Core execution logic for executing the stream event log data downloads.
    /// </summary>
    /// <param name="scope">The execution dependency service scope.</param>
    /// <param name="stopwatch">The tracking diagnostics stopwatch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns a task tracking execution completion.</returns>
    public override async Task Process(IServiceScope scope, Stopwatch? stopwatch = null, CancellationToken cancellationToken = default)
    {
        var format = _options.Format;
        var ext = format.ToString().ToLowerInvariant();
        var failedLocations = new System.Collections.Generic.List<string>();

        foreach (var locationIdentifier in _options.LocationIdentifiers)
        {
            try
            {
                _log.DownloadStarting(locationIdentifier, _options.Start, _options.End);

                var startStr = Uri.EscapeDataString(_options.Start.ToString("yyyy-MM-ddTHH:mm:ss"));
                var endStr = Uri.EscapeDataString(_options.End.ToString("yyyy-MM-ddTHH:mm:ss"));

                string relativeUrl;
                if (string.IsNullOrEmpty(_options.DataType))
                {
                    relativeUrl = $"api/v1/EventLog/StreamData/{locationIdentifier}?start={startStr}&end={endStr}";
                }
                else
                {
                    relativeUrl = $"api/v1/EventLog/StreamData/{locationIdentifier}/{Uri.EscapeDataString(_options.DataType)}?start={startStr}&end={endStr}";
                }

                var requestUri = new Uri(new Uri(_options.ApiUrl!.TrimEnd('/') + "/"), relativeUrl);
                _log.RequestingUrl(requestUri.ToString());

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                if (!string.IsNullOrEmpty(_options.ApiKey))
                {
                    request.Headers.Add("X-API-KEY", _options.ApiKey);
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    _log.ApiErrorResponse((int)response.StatusCode, body);
                    throw new Exception($"API returned error code {response.StatusCode} for location {locationIdentifier}.");
                }

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                var outputFilePath = GetOutputFilePath(locationIdentifier, "events", ext);
                _logger.LogInformation("Saving event logs download to {filePath}", outputFilePath);

                using TextWriter writer = new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read));

                int recordCount = 0;

                if (format == DownloadFormat.Csv)
                {
                    recordCount = await ProcessCsvStreamAsync(reader, writer);
                }
                else if (format == DownloadFormat.Json)
                {
                    recordCount = await ProcessJsonStreamAsync(reader, writer);
                }
                else
                {
                    recordCount = await ProcessNdJsonStreamAsync(reader, writer);
                }

                _log.DownloadCompleted(recordCount);
            }
            catch (Exception ex)
            {
                _log.DownloadFailedException(ex);
                failedLocations.Add(locationIdentifier);
            }
        }

        if (failedLocations.Count > 0)
        {
            _logger.LogError("Downloads failed for the following locations: {locations}", string.Join(", ", failedLocations));
        }

        _lifetime.StopApplication();
    }

    private string GetOutputFilePath(string locationIdentifier, string dataType, string format)
    {
        var dir = _options.OutputPath;
        if (string.IsNullOrEmpty(dir))
        {
            dir = Directory.GetCurrentDirectory();
        }

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var startStr = _options.Start.ToString("yyyyMMddHHmmss");
        var endStr = _options.End.ToString("yyyyMMddHHmmss");
        var ext = format.ToLowerInvariant();

        var typeStr = string.IsNullOrEmpty(_options.DataType)
            ? dataType
            : $"{dataType}-{_options.DataType.Replace('/', '_').Replace('\\', '_')}";

        var filename = $"{locationIdentifier}-{typeStr}-{startStr}-{endStr}.{ext}";

        return Path.Combine(dir, filename);
    }

    private async Task<int> ProcessNdJsonStreamAsync(StreamReader reader, TextWriter writer)
    {
        int count = 0;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            await writer.WriteLineAsync(line);
            count++;

            if (count % 100 == 0)
            {
                _log.ProgressProgressed(count);
            }
        }
        return count;
    }

    private async Task<int> ProcessJsonStreamAsync(StreamReader reader, TextWriter writer)
    {
        await writer.WriteAsync("[");
        bool isFirst = true;
        int count = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (!isFirst)
            {
                await writer.WriteAsync(",");
            }
            isFirst = false;

            await writer.WriteAsync(line);
            count++;

            if (count % 100 == 0)
            {
                _log.ProgressProgressed(count);
            }
        }

        await writer.WriteAsync("]");
        return count;
    }

    private async Task<int> ProcessCsvStreamAsync(StreamReader reader, TextWriter writer)
    {
        bool headerWritten = false;
        string[]? headers = null;
        int count = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object) continue;

            if (!headerWritten)
            {
                var properties = root.EnumerateObject();
                var headerList = new System.Collections.Generic.List<string>();
                foreach (var prop in properties)
                {
                    headerList.Add(prop.Name);
                }
                headers = headerList.ToArray();
                await writer.WriteLineAsync(string.Join(",", System.Array.ConvertAll(headers, EscapeCsvValue)));
                headerWritten = true;
            }

            if (headers != null)
            {
                var values = new string[headers.Length];
                for (int i = 0; i < headers.Length; i++)
                {
                    if (root.TryGetProperty(headers[i], out var propValue))
                    {
                        values[i] = propValue.ValueKind switch
                        {
                            JsonValueKind.Null => string.Empty,
                            JsonValueKind.String => propValue.GetString() ?? string.Empty,
                            _ => propValue.GetRawText()
                        };
                    }
                    else
                    {
                        values[i] = string.Empty;
                    }
                }
                await writer.WriteLineAsync(string.Join(",", System.Array.ConvertAll(values, EscapeCsvValue)));
            }

            count++;
            if (count % 100 == 0)
            {
                _log.ProgressProgressed(count);
            }
        }
        return count;
    }

    private static string EscapeCsvValue(string? value)
    {
        if (value == null) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
