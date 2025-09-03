namespace VerisFlow.VenusDeckParser.Core
{
    /// <summary>
    /// Enum to define the type of labware.
    /// </summary>
    public enum LabwareType
    {
        Carrier,
        RackCarrier,
        Rack,
        Container,
        Unknown
    }

    /// <summary>
    /// Represents the final, processed data for a single piece of labware,
    /// including the calculated final coordinates and type information.
    /// </summary>
    public class ProcessedLabwareInfo
    {
        public int Index { get; set; }
        public string Id { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public double FinalX { get; set; }
        public double FinalY { get; set; }
        public double FinalZ { get; set; }
        public string Template { get; set; } = string.Empty;
        public LabwareType LabwareType { get; set; }
        public bool Loadable { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public bool AlphaIndex { get; set; }
        public bool TipRack { get; set; }
    }
}