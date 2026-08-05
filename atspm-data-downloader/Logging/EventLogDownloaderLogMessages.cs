#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Logging/EventLogDownloaderLogMessages.cs
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

using Microsoft.Extensions.Logging;

namespace atspm_data_downloader.Logging;

/// <summary>
/// Log messages for EventLogDownloaderService using high-performance source-generated loggers.
/// </summary>
public partial class EventLogDownloaderLogMessages
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogDownloaderLogMessages"/> class.
    /// </summary>
    /// <param name="logger">The active log provider instance.</param>
    public EventLogDownloaderLogMessages(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs when an event log stream download starts.
    /// </summary>
    /// <param name="locationIdentifier">The target location identifier.</param>
    /// <param name="start">The start date/time constraint.</param>
    /// <param name="end">The end date/time constraint.</param>
    [LoggerMessage(EventId = 2000, EventName = "Download Starting", Level = LogLevel.Information, Message = "Starting Event Logs Stream Download for location {locationIdentifier} from {start} to {end}")]
    public partial void DownloadStarting(string locationIdentifier, DateTime start, DateTime end);

    /// <summary>
    /// Logs the API endpoint request URL for debugging.
    /// </summary>
    /// <param name="requestUri">The full requested API URL.</param>
    [LoggerMessage(EventId = 2001, EventName = "Requesting URL", Level = LogLevel.Debug, Message = "Requesting API URL: {requestUri}")]
    public partial void RequestingUrl(string requestUri);

    /// <summary>
    /// Logs successful completion of the event logs download.
    /// </summary>
    /// <param name="recordCount">The final count of retrieved records.</param>
    [LoggerMessage(EventId = 2002, EventName = "Download Completed", Level = LogLevel.Information, Message = "Event logs download completed successfully. Processed {recordCount} records.")]
    public partial void DownloadCompleted(int recordCount);

    /// <summary>
    /// Logs an unexpected exception during download.
    /// </summary>
    /// <param name="ex">The underlying exception thrown.</param>
    [LoggerMessage(EventId = 2003, EventName = "Download Failed Exception", Level = LogLevel.Error, Message = "Critical failure during event logs download process.")]
    public partial void DownloadFailedException(Exception ex);

    /// <summary>
    /// Logs an unsuccessful HTTP status response error from the server.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned.</param>
    /// <param name="responseBody">The error response payload details.</param>
    [LoggerMessage(EventId = 2004, EventName = "API Error Response", Level = LogLevel.Error, Message = "API returned error code {statusCode}. Response details: {responseBody}")]
    public partial void ApiErrorResponse(int statusCode, string responseBody);

    /// <summary>
    /// Logs periodical stream download counts progress.
    /// </summary>
    /// <param name="count">The current total of processed records.</param>
    [LoggerMessage(EventId = 2005, EventName = "Progress Progressed", Level = LogLevel.Information, Message = "Processed {count} event log records so far...")]
    public partial void ProgressProgressed(int count);
}
