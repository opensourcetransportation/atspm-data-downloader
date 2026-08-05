#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Commands.Options/LocationIdentifierOption.cs
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

using System.CommandLine;

namespace atspm_data_downloader.Commands.Options;

/// <summary>
/// Command-line option for specifying the location friendly identifier(s).
/// </summary>
public class LocationIdentifierOption : Option<List<string>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationIdentifierOption"/> class.
    /// </summary>
    public LocationIdentifierOption() : base(
        aliases: new[] { "--location", "-l" },
        description: "Location Identifier(s) (e.g. 1001)")
    {
        Arity = ArgumentArity.OneOrMore;
        AllowMultipleArgumentsPerToken = true;
    }
}
