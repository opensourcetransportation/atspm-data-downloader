#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm-data-downloader.Tests/Configuration/PropertyCopierTests.cs
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


using atspm_data_downloader.Configuration;

namespace atspm_data_downloader.Tests.Configuration;

public class PropertyCopierTests
{
    [Fact]
    public void Copy_ShouldCopyNonDefaultValues_WhenPropertiesAreSet()
    {
        // Arrange
        var source = new DownloaderConfiguration
        {
            Start = new DateTime(2026, 1, 1, 0, 0, 0),
            End = new DateTime(2026, 1, 2, 0, 0, 0),
            ApiUrl = "https://api.atspm.com",
            ApiKey = "secret-key",
            Format = DownloadFormat.Csv,
            OutputPath = "C:\\Outputs",
            LocationIdentifiers = new List<string> { "LOC1", "LOC2" }
        };

        var target = new DownloaderConfiguration();

        // Act
        PropertyCopier.Copy(source, target);

        // Assert
        Assert.Equal(source.Start, target.Start);
        Assert.Equal(source.End, target.End);
        Assert.Equal(source.ApiUrl, target.ApiUrl);
        Assert.Equal(source.ApiKey, target.ApiKey);
        Assert.Equal(source.Format, target.Format);
        Assert.Equal(source.OutputPath, target.OutputPath);
        Assert.Equal(source.LocationIdentifiers, target.LocationIdentifiers);
    }

    [Fact]
    public void Copy_ShouldNotOverwriteWithDefaultValues_WhenSourceHasDefaultValues()
    {
        // Arrange
        var expectedStart = new DateTime(2026, 1, 1, 0, 0, 0);
        var expectedEnd = new DateTime(2026, 1, 2, 0, 0, 0);
        var expectedApiUrl = "https://existing-api.com";
        var expectedApiKey = "existing-key";
        var expectedFormat = DownloadFormat.Json;
        var expectedOutputPath = "C:\\ExistingOutputs";
        var expectedLocations = new List<string> { "EXISTING1" };

        var source = new DownloaderConfiguration
        {
            Start = default, // Default value
            End = default,   // Default value
            ApiUrl = null,   // null
            ApiKey = "",     // Empty string should be considered default
            Format = default, // Default Format (e.g. NdJson if 0, but let's check what default is)
            OutputPath = null,
            LocationIdentifiers = new List<string>() // Empty list
        };

        var target = new DownloaderConfiguration
        {
            Start = expectedStart,
            End = expectedEnd,
            ApiUrl = expectedApiUrl,
            ApiKey = expectedApiKey,
            Format = expectedFormat,
            OutputPath = expectedOutputPath,
            LocationIdentifiers = new List<string>(expectedLocations)
        };

        // Act
        PropertyCopier.Copy(source, target);

        // Assert
        Assert.Equal(expectedStart, target.Start);
        Assert.Equal(expectedEnd, target.End);
        Assert.Equal(expectedApiUrl, target.ApiUrl);
        Assert.Equal(expectedApiKey, target.ApiKey);
        Assert.Equal(expectedFormat, target.Format);
        Assert.Equal(expectedOutputPath, target.OutputPath);
        Assert.Equal(expectedLocations, target.LocationIdentifiers);
    }
}
