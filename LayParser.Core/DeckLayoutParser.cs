using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace VerisFlow.VenusDeckParser.Core
{
    public static class DeckLayoutParser
    {
        private const string HamiltonLabwareBasePath = @"C:\Program Files (x86)\HAMILTON\LabWare\";

        /// <summary>
        /// Parses a deck layout file to extract detailed labware information based on specific rules.
        /// </summary>
        /// <param name="deckLayoutFilePath">The full path to the .lay file.</param>
        /// <returns>A list of LabwareInfo objects.</returns>
        public static List<LabwareInfo> GetLabwareInfo(string deckLayoutFilePath)
        {
            var labwareList = new List<LabwareInfo>();
            string content;
            try
            {
                content = File.ReadAllText(deckLayoutFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading deck layout file: {ex.Message}");
                return labwareList; // Return empty list on read error
            }

            // 1. Get the total number of labware instances
            var countMatch = Regex.Match(content, @"Labware\.Cnt[\s\x00-\x1F\x7F]+(\d+)");
            if (!countMatch.Success || !int.TryParse(countMatch.Groups[1].Value, out int labwareCount))
            {
                return labwareList; // Return empty if count is not found
            }

            // 2. Iterate through each labware instance
            for (int i = 1; i <= labwareCount; i++)
            {
                var labware = new LabwareInfo { Index = i };

                // 3. Extract each required field using helper methods
                labware.FilePath = ExtractFilePath(content, i);
                labware.Id = ExtractStringValue(content, i, "Id");
                labware.SiteId = ExtractStringValue(content, i, "SiteId");
                labware.Template = ExtractStringValue(content, i, "Template");
                labware.ZTrans = ExtractNumericValue(content, i, "ZTrans");
                labware.ZTransValue = ExtractNumericValue(content, i, "ZTransValue");
                labware.TForm1 = ExtractTFormVector(content, i, 1);
                labware.TForm2 = ExtractTFormVector(content, i, 2);
                labware.TForm3 = ExtractTFormVector(content, i, 3);

                labwareList.Add(labware);
            }

            return labwareList;
        }

        /// <summary>
        /// Extracts a string value for a given labware index and property name.
        /// </summary>
        private static string ExtractStringValue(string content, int index, string property)
        {
            var match = Regex.Match(content, $@"Labware\.{index}\.{property}[\s\x00-\x1F\x7F]+([^\s\x00-\x1F\x7F]+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        /// <summary>
        /// Extracts and processes the File path for a given labware index.
        /// </summary>
        private static string ExtractFilePath(string content, int index)
        {
            // This regex captures the character after "File" and the path itself.
            var match = Regex.Match(content, $@"Labware\.{index}\.File.([^\t\r\n\v\f\x00-\x1F\x7F]+)");
            if (!match.Success)
            {
                return string.Empty;
            }

            string rawPath = match.Groups[1].Value;

            // Check if the path is relative (doesn't contain a colon like C:)
            if (!Path.IsPathRooted(rawPath))
            {
                // Prepend the base path for relative paths
                return Path.Combine(HamiltonLabwareBasePath, rawPath);
            }

            return rawPath;
        }

        /// <summary>
        /// Extracts a numeric value, floors it to 3 decimal places.
        /// </summary>
        private static double ExtractNumericValue(string content, int index, string property)
        {
            var match = Regex.Match(content, $@"Labware\.{index}\.{property}[\s\x00-\x1F\x7F]+([-\d\.]+)");
            if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                // Floor the value to 3 decimal places
                return Math.Floor(value * 1000) / 1000;
            }
            return 0.0;
        }

        /// <summary>
        /// Extracts a TForm vector (X, Y, Z) for a given labware and TForm index.
        /// </summary>
        private static TFormVector ExtractTFormVector(string content, int labwareIndex, int tformIndex)
        {
            var vector = new TFormVector();

            // This pattern handles both simple numeric values and values surrounded by control characters.
            string patternTemplate = $@"Labware\.{labwareIndex}\.TForm\.{tformIndex}\.{{0}}[\s\x00-\x1F\x7F]*([-\d\.]+)";

            var xMatch = Regex.Match(content, string.Format(patternTemplate, "X"));
            if (xMatch.Success && double.TryParse(xMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double x))
            {
                vector.X = Math.Floor(x * 1000) / 1000;
            }

            var yMatch = Regex.Match(content, string.Format(patternTemplate, "Y"));
            if (yMatch.Success && double.TryParse(yMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
            {
                vector.Y = Math.Floor(y * 1000) / 1000;
            }

            var zMatch = Regex.Match(content, string.Format(patternTemplate, "Z"));
            if (zMatch.Success && double.TryParse(zMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double z))
            {
                vector.Z = Math.Floor(z * 1000) / 1000;
            }

            return vector;
        }
    }
}