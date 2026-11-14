namespace MxUtilities.FileHandling.Models
{
    public class MaxFileProperties
    {
        public GeneralProperties General { get; set; }
        public MeshTotals MeshTotals { get; set; }
        public SceneTotals SceneTotals { get; set; }

        public ICollection<string> ExternalDependencies { get; set; }
        public ICollection<string> Objects { get; set; }
        public ICollection<string> Materials { get; set; }
        public ICollection<string> UsedPlugins { get; set; }
    }

    public class GeneralProperties
    {
        public string Version { get; set; }
        public string Build { get; set; }
        public bool IsCompressed { get; set; }

        public GeneralProperties(List<string> values)
        {
            Version = values[0];
            Build = values[2];
            IsCompressed = values[1] != "Uncompressed";
        }
    }

    public class MeshTotals
    {
        public int Vertices { get; set; }
        public int Faces { get; set; }

        public MeshTotals(List<string> values)
        {
            Vertices = Convert.ToInt32(values[0]);
            Faces = Convert.ToInt32(values[1]);
        }
    }

    public class SceneTotals
    {
        public int Objects { get; set; }
        public int Shapes { get; set; }
        public int Lights { get; set; }
        public int Cameras { get; set; }
        public int Helpers { get; set; }
        public int SpaceWarps { get; set; }
        public int Total { get; set; }

        public SceneTotals(List<string> values)
        {
            Objects = Convert.ToInt32(values[0]);
            Shapes = Convert.ToInt32(values[1]);
            Lights = Convert.ToInt32(values[2]);
            Cameras = Convert.ToInt32(values[3]);
            Helpers = Convert.ToInt32(values[4]);
            SpaceWarps = Convert.ToInt32(values[5]);
            Total = Convert.ToInt32(values[6]);
        }
    }
}
