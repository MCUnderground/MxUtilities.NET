using MxUtilities.AssetManagement;
using MxUtilities.FileHandling;
using MxUtilities.Scripting;

namespace Tests
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  Tests <file.max>                     print render data and assets");
                Console.WriteLine("  Tests <file.max> <origin> <target>   repath assets and save the file");
                return;
            }

            string filename = args[0];

            using (var maxFile = new MaxFile(filename))
            {
                Console.WriteLine(maxFile.GetRenderData());
            }

            foreach (var instance in InstanceManager.GetActiveInstances())
            {
                Console.WriteLine($"3ds Max {instance.Version} (PID {instance.ProcessId}): {instance.Title}");
            }

            if (AssetManager.TryReadAssets(filename, out var assets))
            {
                foreach (var asset in assets.Assets)
                {
                    Console.WriteLine(asset);
                }
            }

            if (args.Length == 3)
            {
                await OfflineRepath(filename, args[1], args[2]);
            }
        }

        private static async Task<bool> OfflineRepath(string filename, string originPath, string targetPath)
        {
            if (!AssetManager.TryReadAssets(filename, out var assets))
            {
                return false;
            }

            foreach (var asset in assets.Assets)
            {
                string oldFileName = asset.FileName;
                asset.FileName = asset.FileName.Replace(originPath, targetPath, StringComparison.OrdinalIgnoreCase);

                if (oldFileName != asset.FileName)
                {
                    Console.WriteLine($"{oldFileName} => {asset.FileName}");
                }

                if (!string.IsNullOrEmpty(asset.ResolvedFile))
                {
                    asset.ResolvedFile = asset.ResolvedFile.Replace(originPath, targetPath, StringComparison.OrdinalIgnoreCase);
                }
            }

            await AssetManager.WriteAssets(filename, assets);
            return true;
        }
    }
}
