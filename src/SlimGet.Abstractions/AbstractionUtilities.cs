// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Humanizer;
using Humanizer.Localisation;
using SlimGet.Data;

namespace SlimGet
{
    /// <summary>
    /// Various static helpers.
    /// </summary>
    public static class AbstractionUtilities
    {
        /// <summary>
        /// Gets the common, properly-configured instance of the UTF-8 encoder.
        /// </summary>
        public static Encoding UTF8 { get; } = new UTF8Encoding(false);

        /// <summary>
        /// Contains an enumerable of currently-loaded assemblies.
        /// </summary>
        internal static IEnumerable<Assembly> AssemblyCache { get; }

        /// <summary>
        /// Gets the directory the current assembly resides in.
        /// </summary>
        private static DirectoryInfo CurrentDirectory { get; }

        /// <summary>
        /// Gets loadable assemblies from assembly's directory.
        /// </summary>
        private static IEnumerable<FileInfo> LoadableAssemblies { get; }

        /// <summary>
        /// Gets default JSON serializer options.
        /// </summary>
        public static JsonSerializerOptions DefaultJsonOptions { get; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            IgnoreReadOnlyProperties = false
        };

        /// <summary>
        /// Gets the PascalCase JSON naming serializer options.
        /// </summary>
        public static JsonSerializerOptions PascalCaseJsonOptions { get; } = new JsonSerializerOptions
        {
            IgnoreNullValues = false,
            IgnoreReadOnlyProperties = false
        };

        /// <summary>
        /// Gets the snake_case JSON naming serializer options.
        /// </summary>
        public static JsonSerializerOptions SnakeCaseJsonOptions { get; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            IgnoreNullValues = false,
            IgnoreReadOnlyProperties = false
        };

        static AbstractionUtilities()
        {
            ForceLoadAssemblies();
            AssemblyCache = AppDomain.CurrentDomain.GetAssemblies();
            CurrentDirectory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            LoadableAssemblies = CurrentDirectory.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Parses a numeric string as long.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <returns>Parsed long.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ParseAsLong(this string str)
            => long.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts a long to a string.
        /// </summary>
        /// <param name="l">Long to convert.</param>
        /// <returns>Converted long.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AsString(this long l)
            => l.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Parses a numeric string as ulong.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <returns>Parsed string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ParseAsUlong(this string str)
            => ulong.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts a ulong to a string.
        /// </summary>
        /// <param name="l">Ulong to convert.</param>
        /// <returns>Converted ulong.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AsString(this ulong l)
            => l.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Parses a numeric string as int.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <returns>Parsed string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseAsInt(this ReadOnlySpan<char> str)
            => int.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts a timespan to a human-readable string.
        /// </summary>
        /// <param name="timeSpan">Timespan to humanize.</param>
        /// <returns>Humanized string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHumanString(this TimeSpan timeSpan)
            => timeSpan.Humanize(3, CultureInfo.InvariantCulture, maxUnit: TimeUnit.Month, minUnit: TimeUnit.Second);

        private static void ForceLoadAssemblies()
        {
            var asns = new HashSet<string>(AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(x => x.GetName().Name));

            var a = Assembly.GetEntryAssembly();
            var loc = Path.GetFullPath(a.Location);
            var dir = Path.GetDirectoryName(loc.AsSpan());
            var assemblies = Directory.GetFiles(new string(dir), "SlimGet.*.dll");

            foreach (var assembly in a.GetReferencedAssemblies())
                Assembly.Load(assembly);

            foreach (var assembly in assemblies)
            {
                var asn = Path.GetFileNameWithoutExtension(assembly.AsSpan());
                var asnst = new string(asn);
                if (!asns.Contains(asnst))
                    Assembly.Load(asnst);
            }

            // So here's a great question. Why do I need this? I don't know. Without it, .NET just gives up on 
            // assembly loading altogether. Amazing software.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs ea)
        {
            var an = new AssemblyName(ea.Name);
            var dll = string.Create(an.Name.Length + 4, an.Name, CreateDllString);
            return LoadableAssemblies.Any(x => x.Name == dll)
                ? Assembly.LoadFrom(Path.Combine(CurrentDirectory.FullName, dll))
                : null;

            static void CreateDllString(Span<char> buffer, string state)
            {
                buffer[^1] = 'l';
                buffer[^2] = 'l';
                buffer[^3] = 'd';
                buffer[^4] = '.';
                state.AsSpan().CopyTo(buffer);
            }
        }
    }
}
