#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Configuration/EventLogDownloaderOptions.cs
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

using System;
using Microsoft.Extensions.Configuration;

namespace atspm_data_downloader.Configuration;

/// <summary>
/// Configuration options structure model for the event log ATSPM downloader service.
/// </summary>
public class EventLogDownloaderOptions : IDownloaderOptions
{
    /// <summary>
    /// Gets or sets the inclusive stream start date/time.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Gets or sets the inclusive stream end date/time.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Gets or sets the target location identifier.
    /// </summary>
    public string LocationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the specialized data category stream type.
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    /// Gets or sets the authentication API Key.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the base endpoint host API URL.
    /// </summary>
    public string? ApiUrl { get; set; }

    /// <summary>
    /// Gets or sets the resulting file output format (e.g., csv, json, ndjson).
    /// </summary>
    public string Format { get; set; } = "ndjson";

    /// <summary>
    /// Gets or sets the destination save file path.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Implements priority hierarchy (CLI Override > Environment Variable > AppSettings) to map settings.
    /// </summary>
    /// <param name="config">The underlying application configuration provider.</param>
    public void ApplyConfiguration(IConfiguration config)
    {
        var apiKeyConfig = config["Atspm:ApiKey"] ?? config["ApiKey"];
        var apiUrlConfig = config["Atspm:ApiUrl"] ?? config["ApiUrl"];

        var apiKeyEnv = Environment.GetEnvironmentVariable("ATSPM_API_KEY");
        var apiUrlEnv = Environment.GetEnvironmentVariable("ATSPM_API_URL");
        var startEnvStr = Environment.GetEnvironmentVariable("ATSPM_START");
        var endEnvStr = Environment.GetEnvironmentVariable("ATSPM_END");
        var locationEnv = Environment.GetEnvironmentVariable("ATSPM_LOCATION");
        var dataTypeEnv = Environment.GetEnvironmentVariable("ATSPM_DATA_TYPE");
        var formatEnv = Environment.GetEnvironmentVariable("ATSPM_FORMAT");
        var outputEnv = Environment.GetEnvironmentVariable("ATSPM_OUTPUT");

        if (Start == default && DateTime.TryParse(startEnvStr, out var startEnv))
        {
            Start = startEnv;
        }

        if (End == default && DateTime.TryParse(endEnvStr, out var endEnv))
        {
            End = endEnv;
        }

        if (string.IsNullOrEmpty(LocationId) && !string.IsNullOrEmpty(locationEnv))
        {
            LocationId = locationEnv;
        }

        DataType = DataType ?? dataTypeEnv;
        Format = Format == "ndjson" && !string.IsNullOrEmpty(formatEnv) ? formatEnv : Format;
        OutputPath = OutputPath ?? outputEnv;

        ApiKey = ApiKey ?? apiKeyEnv ?? apiKeyConfig;
        ApiUrl = ApiUrl ?? apiUrlEnv ?? apiUrlConfig;
    }
}
