using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VerisFlow.VenusDeckParser.Core
{
    /// <summary>
    /// A service class responsible for processing the raw labware data
    /// into its final, calculated form.
    /// </summary>
    public static class LabwareDataProcessor
    {
        /// <summary>
        /// Processes a list of raw LabwareInfo objects to calculate final coordinates.
        /// TForm vectors are summed, and ZTrans/ZTransValue are added to the Z coordinate.
        /// It also determines the LabwareType and if the labware is Loadable.
        /// </summary>
        /// <param name="rawData">The list of raw LabwareInfo objects parsed from the file.</param>
        /// <returns>A list of ProcessedLabwareInfo objects with the final calculated data.</returns>
        public static List<ProcessedLabwareInfo> Process(List<LabwareInfo> rawData)
        {
            return rawData.Select(rawLabware =>
            {
                // Sum the X, Y, and Z components of the three TForm vectors
                double sumX = rawLabware.TForm3.X;
                double sumY = rawLabware.TForm3.Y;
                //double sumZ = rawLabware.TForm1.Z + rawLabware.TForm2.Z + rawLabware.TForm3.Z;

                // Add ZTrans and ZTransValue to the summed Z value to get the final Z coordinate
                double finalZ = rawLabware.ZTrans;

                var labwareType = LabwareType.Unknown; // Default value
                string extension = Path.GetExtension(rawLabware.FilePath)?.ToLowerInvariant();

                switch (extension)
                {
                    case ".tml":
                        labwareType = LabwareType.Carrier;
                        break;
                    case ".rck":
                        labwareType = LabwareType.Rack;
                        break;
                    case ".ctr":
                        labwareType = LabwareType.Container;
                        break;
                }

                if (labwareType == LabwareType.Rack && rawLabware.Template == "default")
                {
                    labwareType = LabwareType.RackCarrier;
                }

                // Determine if the labware is loadable based on its type and SiteId
                var isLoadable = labwareType == LabwareType.Carrier && !string.IsNullOrEmpty(rawLabware.SiteId);

                var properties = ReadLabwareProperties(rawLabware.FilePath);

                var isTipRack = properties.CntrBase < -10;
                var alphaIndex = properties.IxIndex == 1;

                var row = properties.Rows;
                var column = (row > 0 && properties.Columns == 0) ? 1 : properties.Columns;

                var template = rawLabware.Template == "default" ? "" : rawLabware.Template;

                return new ProcessedLabwareInfo
                {
                    Index = rawLabware.Index,
                    Id = rawLabware.Id,
                    FilePath = rawLabware.FilePath,
                    FinalX = sumX,
                    FinalY = sumY,
                    FinalZ = finalZ,
                    Template = template,
                    LabwareType = labwareType,
                    Loadable = isLoadable,
                    Dx = properties.DimDx,
                    Dy = properties.DimDy,
                    Column = column,
                    Row = row,
                    AlphaIndex = alphaIndex,
                    TipRack = isTipRack
                };
            }).ToList();
        }

        /// <summary>
        /// Reads the content of a labware file to extract key properties.
        /// </summary>
        /// <param name="filePath">The full path to the labware file (.rck, .tml, etc.).</param>
        /// <returns>A LabwareProperties object containing the extracted values. Returns an object with default values (0) if the file cannot be read or properties are not found.</returns>
        private static LabwareProperties ReadLabwareProperties(string filePath)
        {
            // Initialize with default values (0 for numeric types)
            var properties = new LabwareProperties();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return properties;
            }

            try
            {
                string content = File.ReadAllText(filePath);

                // --- Extract Double Values ---
                // \b is a word boundary to ensure we match "Dim.Dx" and not "OtherDim.Dx"
                // \s+ matches one or more whitespace characters
                // ([-\d\.]+) captures a group of digits, a hyphen, or a dot (for the value)
                var dxMatch = Regex.Match(content, @"\bDim\.Dx[\s\x00-\x1F\x7F]+([-\d\.]+)");
                if (dxMatch.Success)
                {
                    double.TryParse(dxMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double dx);
                    properties.DimDx = dx;
                }

                var dyMatch = Regex.Match(content, @"\bDim\.Dy[\s\x00-\x1F\x7F]+([-\d\.]+)");
                if (dyMatch.Success)
                {
                    double.TryParse(dyMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double dy);
                    properties.DimDy = dy;
                }

                // Note the escaped dots in the property name
                var cntrBaseMatch = Regex.Match(content, @"\bCntr\.1\.base[\s\x00-\x1F\x7F]+([-\d\.]+)");
                if (cntrBaseMatch.Success)
                {
                    double.TryParse(cntrBaseMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double cntrBase);
                    properties.CntrBase = cntrBase;
                }

                // --- Extract Integer Values ---
                var ixIndexMatch = Regex.Match(content, @"\bIX\.Index[\s\x00-\x1F\x7F]+([-\d\.]+)");
                if (ixIndexMatch.Success)
                {
                    int.TryParse(ixIndexMatch.Groups[1].Value, out int ixIndex);
                    properties.IxIndex = ixIndex;
                }

                // --- Handle Special Logic for Rows and Columns ---
                bool rowsFound = false;
                var rowsMatch = Regex.Match(content, @"\bRows[\s\x00-\x1F\x7F]+([-\d\.]+)");
                // Check for match and then parse. The 'rowsValue' variable is now safely scoped inside the 'if'.
                if (rowsMatch.Success && int.TryParse(rowsMatch.Groups[1].Value, out int rowsValue))
                {
                    properties.Rows = rowsValue;
                    rowsFound = true;
                }

                bool colsFound = false;
                var colsMatch = Regex.Match(content, @"\bColumns[\s\x00-\x1F\x7F]+([-\d\.]+)");
                if (colsMatch.Success && int.TryParse(colsMatch.Groups[1].Value, out int colsValue))
                {
                    properties.Columns = colsValue;
                    colsFound = true;
                }

                // If BOTH Rows and Columns were not found in the file, check for HoleCnt
                if (!rowsFound && !colsFound)
                {
                    var holeCntMatch = Regex.Match(content, @"\bHoleCnt[\s\x00-\x1F\x7F]+([-\d\.]+)");
                    if (holeCntMatch.Success && int.TryParse(holeCntMatch.Groups[1].Value, out int holeCnt))
                    {
                        // As per the requirement, assign the HoleCnt value to Rows
                        properties.Rows = holeCnt;
                    }
                }

                return properties;
            }
            catch (Exception ex)
            {
                // If there's an error reading the file (e.g., access denied), return default values
                Console.WriteLine($"Could not read properties from {filePath}: {ex.Message}");
                return properties;
            }
        }
    }
}