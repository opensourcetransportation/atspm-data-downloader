#region license
// Copyright 2026 Utah Departement of Transportation
// for atspm-data-downloader - atspm_data_downloader.Configuration/PropertyCopier.cs
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
using System.Reflection;

namespace atspm_data_downloader.Configuration;

/// <summary>
/// Reflection helper class to safely duplicate property values across identical options models.
/// </summary>
public static class PropertyCopier
{
    /// <summary>
    /// Copies public, instance, read-write property values from a source object to a destination target.
    /// </summary>
    /// <typeparam name="T">The type of the source and target options structures.</typeparam>
    /// <param name="source">The source options model containing populated CLI inputs.</param>
    /// <param name="target">The destination target options model to be initialized.</param>
    public static void Copy<T>(T source, T target) where T : class
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (prop.CanRead && prop.CanWrite)
            {
                var val = prop.GetValue(source);
                prop.SetValue(target, val);
            }
        }
    }
}
