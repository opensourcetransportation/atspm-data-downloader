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
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace atspm_data_downloader.HostedServices;

/// <summary>
/// Hosted service designed for managing the high-efficiency stream downloading of ATSPM event log data.
/// </summary>
public class EventLogDownloaderService : HostedServiceBase
{
    private readonly HttpClient _httpClient;
    private readonly EventLogDownloaderOptions _options;
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
        IOptions<EventLogDownloaderOptions> options,
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
        _log.DownloadStarting(_options.LocationId, _options.Start, _options.End);

        var startStr = Uri.EscapeDataString(_options.Start.ToString("yyyy-MM-ddTHH:mm:ss"));
        var endStr = Uri.EscapeDataString(_options.End.ToString("yyyy-MM-ddTHH:mm:ss"));

        string relativeUrl;
        if (string.IsNullOrEmpty(_options.DataType))
        {
            relativeUrl = $"api/v1/EventLog/StreamData/{_options.LocationId}?start={startStr}&end={endStr}";
        }
        else
        {
            relativeUrl = $"api/v1/EventLog/StreamData/{_options.LocationId}/{Uri.EscapeDataString(_options.DataType)}?start={startStr}&end={endStr}";
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
            throw new Exception($"API returned error code {response.StatusCode}.");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        using TextWriter writer = !string.IsNullOrEmpty(_options.OutputPath)
            ? new StreamWriter(new FileStream(_options.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            : Console.Out;

        var format = _options.Format.ToLowerInvariant();
        int recordCount = 0;

        if (format == "csv")
        {
            recordCount = await ProcessCsvStreamAsync(reader, writer);
        }
        else if (format == "json")
        {
            recordCount = await ProcessJsonStreamAsync(reader, writer);
        }
        else
        {
            recordCount = await ProcessNdJsonStreamAsync(reader, writer);
        }

        _log.DownloadCompleted(recordCount);
        _lifetime.StopApplication();
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
