#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Configuration/DownloadFormat.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace atspm_data_downloader.Configuration;

/// <summary>
/// Supported output stream download formats.
/// </summary>
public enum DownloadFormat
{
    /// <summary>
    /// Newline-delimited JSON format (default).
    /// </summary>
    NdJson,

    /// <summary>
    /// Comma-separated values format.
    /// </summary>
    Csv,

    /// <summary>
    /// Standard JSON array format.
    /// </summary>
    Json
}
