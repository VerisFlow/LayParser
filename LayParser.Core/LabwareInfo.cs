using System.Text;

namespace VerisFlow.VenusDeckParser.Core
{
    /// <summary>
    /// Represents a 3D vector for TForm data.
    /// </summary>
    public class TFormVector
    {
        public double X { get; set; } = 0.0;
        public double Y { get; set; } = 0.0;
        public double Z { get; set; } = 0.0;

        public override string ToString()
        {
            return $"X={X:F3}, Y={Y:F3}, Z={Z:F3}";
        }
    }

    /// <summary>
    /// Represents detailed information about a single piece of labware.
    /// </summary>
    public class LabwareInfo
    {
        public int Index { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string SiteId { get; set; } = string.Empty;
        public TFormVector TForm1 { get; set; } = new TFormVector();
        public TFormVector TForm2 { get; set; } = new TFormVector();
        public TFormVector TForm3 { get; set; } = new TFormVector();
        public double ZTransValue { get; set; } = 0.0;
        public double ZTrans { get; set; } = 0.0;
        public string Template { get; set; } = string.Empty;

        public string GetTFormAsMarkdown()
        {
            var sb = new StringBuilder();
            sb.Append($"**1:** {TForm1}<br>");
            sb.Append($"**2:** {TForm2}<br>");
            sb.Append($"**3:** {TForm3}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A data class to store properties extracted from a labware file.
    /// </summary>
    public class LabwareProperties
    {
        public double DimDx { get; set; }
        public double DimDy { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int IxIndex { get; set; }
        public double CntrBase { get; set; }
    }
}