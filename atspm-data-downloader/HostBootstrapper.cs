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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

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
    /// <typeparam name="TOptions">The configuration options mapping class.</typeparam>
    /// <param name="options">The initialized command-line options object.</param>
    /// <returns>Returns a task representing the asynchronous execution.</returns>
    public static async Task RunHostAsync<TService, TOptions>(TOptions options)
        where TService : class, IHostedService
        where TOptions : class, IDownloaderOptions, new()
    {
        var builder = Host.CreateDefaultBuilder();

        builder.ConfigureServices((hostContext, services) =>
        {
            services.Configure<TOptions>(opt =>
            {
                PropertyCopier.Copy(options, opt);
                opt.ApplyConfiguration(hostContext.Configuration);
            });

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<TOptions>>().Value);

            services.AddHttpClient<TService>();
            services.AddHostedService<TService>();
        });

        using var host = builder.Build();
        await host.RunAsync();
    }
}
