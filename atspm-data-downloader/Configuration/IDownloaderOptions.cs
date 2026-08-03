#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Configuration/IDownloaderOptions.cs
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

using Microsoft.Extensions.Configuration;

namespace atspm_data_downloader.Configuration;

/// <summary>
/// Defines contract for mapping and applying environment-specific configuration values to downloader options.
/// </summary>
public interface IDownloaderOptions
{
    /// <summary>
    /// Integrates environmental settings and appsettings key configurations into the existing options structure.
    /// </summary>
    /// <param name="config">The underlying application configuration provider.</param>
    void ApplyConfiguration(IConfiguration config);
}
