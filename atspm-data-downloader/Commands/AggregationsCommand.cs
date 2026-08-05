#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Commands/AggregationsCommand.cs
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

using atspm_data_downloader.HostedServices;
using System.CommandLine;

namespace atspm_data_downloader.Commands;

/// <summary>
/// Subcommand for downloading aggregated ATSPM data streams.
/// </summary>
public class AggregationsCommand : DownloadCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregationsCommand"/> class.
    /// </summary>
    public AggregationsCommand() : base("aggregations", "Download aggregated ATSPM data streams")
    {
        this.SetHandler(async (context) =>
        {
            await RunDownloaderAsync<AggregationDownloaderService>(context);
        });
    }
}
