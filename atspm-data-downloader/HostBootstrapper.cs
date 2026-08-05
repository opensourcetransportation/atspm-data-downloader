#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader/HostBootstrapper.cs
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net;

namespace atspm_data_downloader;

/// <summary>
/// Static bootstrapper to initialize, configure, and execute the generic host for downloader services.
/// </summary>
public static class HostBootstrapper
{
    /// <summary>
    /// Executes the generic host for a specific downloader service and configuration option.
    /// </summary>
    /// <typeparam name="TService">The target IHostedService class to execute.</typeparam>
    /// <param name="configureAction">The action callback to configure downloader options.</param>
    /// <returns>Returns a task representing the asynchronous execution.</returns>
    public static async Task RunHostAsync<TService>(Action<DownloaderConfiguration> configureAction)
        where TService : class, IHostedService
    {
        var builder = Host.CreateDefaultBuilder();

        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddOptions<DownloaderConfiguration>()
                .Bind(hostContext.Configuration.GetSection("DownloaderConfiguration"))
                .Configure(configureAction)
                .Validate(opt => 
                {
                    return opt.Start != default && 
                           opt.End != default && 
                           opt.Start <= opt.End &&
                           opt.LocationIdentifiers != null && 
                           opt.LocationIdentifiers.Count > 0 &&
                           !string.IsNullOrWhiteSpace(opt.ApiUrl) &&
                           Uri.TryCreate(opt.ApiUrl, UriKind.Absolute, out _);
                }, "Required downloader configuration options are missing or invalid. Please provide '--start', '--end', '--location', and '--api-url' (a valid absolute URI) options via command line, environment variables (e.g. DownloaderConfiguration__Start), or appsettings.json, and ensure start date is before or equal to end date.")
                .ValidateOnStart();

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<DownloaderConfiguration>>().Value);

            services.AddHttpClient<TService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            });
            services.AddHostedService<TService>();
        });

        using var host = builder.Build();
        await host.RunAsync();
    }
}
