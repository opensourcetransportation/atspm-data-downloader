#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Logging/IDownloaderLogMessages.cs
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

namespace atspm_data_downloader.Logging;

/// <summary>
/// Defines standard log messages for high-efficiency dataset stream downloads.
/// </summary>
public interface IDownloaderLogMessages
{
    /// <summary>
    /// Logs when a stream download starts.
    /// </summary>
    void DownloadStarting(string locationIdentifier, DateTime start, DateTime end);

    /// <summary>
    /// Logs the API endpoint request URL for debugging.
    /// </summary>
    void RequestingUrl(string requestUri);

    /// <summary>
    /// Logs successful completion of the download.
    /// </summary>
    void DownloadCompleted(int recordCount);

    /// <summary>
    /// Logs an unexpected exception during download.
    /// </summary>
    void DownloadFailedException(Exception ex);

    /// <summary>
    /// Logs an unsuccessful HTTP status response error from the server.
    /// </summary>
    void ApiErrorResponse(int statusCode, string responseBody);

    /// <summary>
    /// Logs periodical stream download counts progress.
    /// </summary>
    void ProgressProgressed(int count);

    /// <summary>
    /// Logs when a download file starts writing to the file system.
    /// </summary>
    void SavingDownload(string label, string filePath);

    /// <summary>
    /// Logs the list of locations that failed to download.
    /// </summary>
    void DownloadsFailedSummary(string locations);
}
