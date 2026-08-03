#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Commands/EventsCommand.cs
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
using System.CommandLine;
using atspm_data_downloader.Commands.Options;
using atspm_data_downloader.Configuration;
using atspm_data_downloader.HostedServices;

namespace atspm_data_downloader.Commands;

/// <summary>
/// Subcommand for downloading ATSPM event log data streams.
/// </summary>
public class EventsCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventsCommand"/> class.
    /// </summary>
    public EventsCommand() : base("events", "Download ATSPM event log data streams")
    {
        var startOption = new StartOption();
        var endOption = new EndOption();
        var locationOption = new LocationOption();
        var dataTypeOption = new DataTypeOption();
        var apiKeyOption = new ApiKeyOption();
        var apiUrlOption = new ApiUrlOption();
        var formatOption = new FormatOption();
        var outputOption = new OutputOption();

        AddOption(startOption);
        AddOption(endOption);
        AddOption(locationOption);
        AddOption(dataTypeOption);
        AddOption(apiKeyOption);
        AddOption(apiUrlOption);
        AddOption(formatOption);
        AddOption(outputOption);

        this.SetHandler(async (context) =>
        {
            var options = new EventLogDownloaderOptions
            {
                Start = context.ParseResult.GetValueForOption(startOption),
                End = context.ParseResult.GetValueForOption(endOption),
                LocationId = context.ParseResult.GetValueForOption(locationOption)!,
                DataType = context.ParseResult.GetValueForOption(dataTypeOption),
                ApiKey = context.ParseResult.GetValueForOption(apiKeyOption),
                ApiUrl = context.ParseResult.GetValueForOption(apiUrlOption),
                Format = context.ParseResult.GetValueForOption(formatOption)!,
                OutputPath = context.ParseResult.GetValueForOption(outputOption)
            };

            await HostBootstrapper.RunHostAsync<EventLogDownloaderService, EventLogDownloaderOptions>(options);
        });
    }
}
