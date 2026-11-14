namespace MxUtilities.FileHandling.Models
{
    public class RenderData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float Aspect {  get; set; }
        public string Renderer { get; set; } = string.Empty;

        public string RenderOutput { get; set; } = string.Empty;
        public float RenderOutputGamma { get; set; }

        public uint RenderFlags { get; set; }

        public uint SceneFlags { get; set; }
        public int RenderElements { get; set; }

        public ICollection<string> Cameras { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"RenderData: [Width={Width}, Height={Height}, Aspect={Aspect}, Renderer={Renderer}, " +
                   $"RenderOutput={RenderOutput}, RenderOutputGamma={RenderOutputGamma}, " +
                   $"RenderFlags={RenderFlags}, SceneFlags={SceneFlags}, RenderElements={RenderElements}, " +
                   $"Cameras={string.Join(", ", Cameras)}]";
        }
    }
}
