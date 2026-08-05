#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Commands/DownloadCommand.cs
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

using atspm_data_downloader.Commands.Options;
using atspm_data_downloader.Configuration;
using System.CommandLine;

namespace atspm_data_downloader.Commands;

/// <summary>
/// Root parent command for ATSPM dataset downloads.
/// </summary>
public class DownloadCommand : Command
{
    /// <summary>
    /// Gets the shared start date command-line option.
    /// </summary>
    public static readonly StartOption StartOption = new();

    /// <summary>
    /// Gets the shared end date command-line option.
    /// </summary>
    public static readonly EndOption EndOption = new();

    /// <summary>
    /// Gets the shared location identifier command-line option.
    /// </summary>
    public static readonly LocationIdentifierOption LocationOption = new();

    /// <summary>
    /// Gets the shared data type command-line option.
    /// </summary>
    public static readonly DataTypeOption DataTypeOption = new();

    /// <summary>
    /// Gets the shared API key command-line option.
    /// </summary>
    public static readonly ApiKeyOption ApiKeyOption = new();

    /// <summary>
    /// Gets the shared API base URL command-line option.
    /// </summary>
    public static readonly ApiUrlOption ApiUrlOption = new();

    /// <summary>
    /// Gets the shared output format command-line option.
    /// </summary>
    public static readonly FormatOption FormatOption = new();

    /// <summary>
    /// Gets the shared output save path command-line option.
    /// </summary>
    public static readonly OutputOption OutputOption = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadCommand"/> class with a custom name and description.
    /// Used by subcommands inheriting from DownloadCommand.
    /// </summary>
    protected DownloadCommand(string name, string description) : base(name, description)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadCommand"/> class.
    /// </summary>
    public DownloadCommand() : base("download", "Download ATSPM datasets")
    {
        AddGlobalOption(StartOption);
        AddGlobalOption(EndOption);
        AddGlobalOption(LocationOption);
        AddGlobalOption(DataTypeOption);
        AddGlobalOption(ApiKeyOption);
        AddGlobalOption(ApiUrlOption);
        AddGlobalOption(FormatOption);
        AddGlobalOption(OutputOption);

        AddCommand(new AggregationsCommand());
        AddCommand(new EventsCommand());
    }

    /// <summary>
    /// Utility method to parse all shared options into a unified configuration and start the host.
    /// </summary>
    protected static async Task RunDownloaderAsync<TService>(System.CommandLine.Invocation.InvocationContext context)
        where TService : class, Microsoft.Extensions.Hosting.IHostedService
    {
        var options = new DownloaderConfiguration();

        if (IsSpecified(context.ParseResult, StartOption))
            options.Start = context.ParseResult.GetValueForOption(StartOption);

        if (IsSpecified(context.ParseResult, EndOption))
            options.End = context.ParseResult.GetValueForOption(EndOption);

        if (IsSpecified(context.ParseResult, LocationOption))
            options.LocationIdentifiers = context.ParseResult.GetValueForOption(LocationOption) ?? new();

        if (IsSpecified(context.ParseResult, DataTypeOption))
            options.DataType = context.ParseResult.GetValueForOption(DataTypeOption);

        if (IsSpecified(context.ParseResult, ApiKeyOption))
            options.ApiKey = context.ParseResult.GetValueForOption(ApiKeyOption);

        if (IsSpecified(context.ParseResult, ApiUrlOption))
            options.ApiUrl = context.ParseResult.GetValueForOption(ApiUrlOption);

        if (IsSpecified(context.ParseResult, FormatOption))
            options.Format = context.ParseResult.GetValueForOption(FormatOption)!;

        if (IsSpecified(context.ParseResult, OutputOption))
            options.OutputPath = context.ParseResult.GetValueForOption(OutputOption);

        await HostBootstrapper.RunHostAsync<TService>(options);
    }

    private static bool IsSpecified(System.CommandLine.Parsing.ParseResult parseResult, Option option)
    {
        return parseResult.FindResultFor(option) is { } res && !res.IsImplicit;
    }
}
