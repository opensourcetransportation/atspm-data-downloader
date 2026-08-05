#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Configuration/DownloaderConfiguration.cs
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

using System.ComponentModel.DataAnnotations;
using Utah.Udot.NetStandardToolkit.Configuration;

namespace atspm_data_downloader.Configuration;

/// <summary>
/// Configuration options model for the ATSPM downloader services.
/// </summary>
[ConfigurationSection(nameof(DownloaderConfiguration), null)]
public class DownloaderConfiguration
{
    /// <summary>
    /// Gets or sets the inclusive stream start date/time.
    /// </summary>
    [Required]
    public DateTime Start { get; set; }

    /// <summary>
    /// Gets or sets the inclusive stream end date/time.
    /// </summary>
    [Required]
    public DateTime End { get; set; }

    /// <summary>
    /// Gets or sets the target location friendly identifiers.
    /// </summary>
    [Required]
    public List<string> LocationIdentifiers { get; set; } = new();

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
    public DownloadFormat Format { get; set; } = DownloadFormat.NdJson;

    /// <summary>
    /// Gets or sets the destination save directory path.
    /// </summary>
    public string? OutputPath { get; set; }
}
