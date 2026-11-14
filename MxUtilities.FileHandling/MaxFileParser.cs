
using MxUtilities.FileHandling.Helpers;
using MxUtilities.FileHandling.Models;
using System.Text.RegularExpressions;

namespace MxUtilities.FileHandling
{
    internal class MaxFileParser
    {
        internal static RenderData GetRenderData(MaxStream stream)
        {
            var content = MaxFileParserHelper.GetStreamAsCleanString(stream);

            var renderData = new RenderData();

            var patterns = new Dictionary<string, string>
            {
                { "Width", @"Render Width=(\d+)" },
                { "Height", @"Render Height=(\d+)" },
                { "Aspect", @"Render Aspect=([\d\.]+)" },
                { "Renderer", @"Renderer Name=([^$]+)(?=Render Output|T|Animation Start)" },

                { "RenderOutput", @"Render Output=(.+?)(?=Render Output Gamma=|Animation Start)" },
                { "RenderOutputGamma", @"Render Output Gamma=([\d\.]+)" },

                { "RenderFlags", @"Render Flags=(\d+)" },
                { "SceneFlags", @"Scene Flags=(\d+)" },
                { "RenderElements", @"Render Elements=(\d+)" },
                { "Cameras", @"Render Camera \d+=([^\$]+?)(?=\$|Render Width)" }
            };

            foreach (var entry in patterns)
            {
                if (entry.Key == "Cameras")
                {
                    var matches = Regex.Matches(content, entry.Value);

                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            renderData.Cameras.Add(match.Groups[1].Value.Trim());
                        }
                    }
                }
                else
                {
                    var match = Regex.Match(content, entry.Value);
                    if (match.Success)
                    {
                        switch (entry.Key)
                        {
                            case "Width":
                                renderData.Width = int.Parse(match.Groups[1].Value);
                                break;
                            case "Height":
                                renderData.Height = int.Parse(match.Groups[1].Value);
                                break;
                            case "Aspect":
                                renderData.Aspect = float.Parse(match.Groups[1].Value);
                                break;
                            case "Renderer":
                                renderData.Renderer = match.Groups[1].Value.Trim();
                                break;
                            case "RenderOutput":
                                renderData.RenderOutput = match.Groups[1].Value.Trim();
                                break;
                            case "RenderOutputGamma":
                                renderData.RenderOutputGamma = float.Parse(match.Groups[1].Value);
                                break;
                            case "RenderFlags":
                                renderData.RenderFlags = uint.Parse(match.Groups[1].Value);
                                break;
                            case "SceneFlags":
                                renderData.SceneFlags = uint.Parse(match.Groups[1].Value);
                                break;
                            case "RenderElements":
                                renderData.RenderElements = int.Parse(match.Groups[1].Value);
                                break;
                        }
                    }
                }
            }

            return renderData;
        }
    }
}
