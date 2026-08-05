#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.HostedServices/AggregationDownloaderService.cs
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

namespace atspm_data_downloader.HostedServices;

/// <summary>
/// Hosted service designed for managing the high-efficiency stream downloading of ATSPM aggregation data.
/// </summary>
public class AggregationDownloaderService : DownloaderServiceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregationDownloaderService"/> class.
    /// </summary>
    public AggregationDownloaderService(
        IServiceScopeFactory serviceProvider,
        HttpClient httpClient,
        IOptions<DownloaderConfiguration> options,
        IHostApplicationLifetime lifetime,
        ILogger<AggregationDownloaderService> logger)
        : base(serviceProvider, httpClient, options, lifetime, logger, new AggregationDownloaderLogMessages(logger))
    {
    }

    /// <inheritdoc/>
    protected override string BaseRelativePath => "api/v1/Aggregation/StreamData";

    /// <inheritdoc/>
    protected override string DatasetLabel => "aggregations";
}
