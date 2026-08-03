#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Commands.Options/FormatOption.cs
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

namespace atspm_data_downloader.Commands.Options;

/// <summary>
/// Command-line option for specifying the output file format (json, csv, ndjson).
/// </summary>
public class FormatOption : Option<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormatOption"/> class.
    /// </summary>
    public FormatOption() : base(
        aliases: new[] { "--format", "-f" },
        getDefaultValue: () => "ndjson",
        description: "Output format (json, csv, ndjson)")
    {
        this.FromAmong("json", "csv", "ndjson");
    }
}
