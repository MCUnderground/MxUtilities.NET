using MxUtilities.AssetManagement.Models;
using MxUtilities.FileHandling;
using MxUtilities.FileHandling.Constants;
using MxUtilities.FileHandling.Enums;
using MxUtilities.FileHandling.Models;
using System.Diagnostics;

namespace MxUtilities.AssetManagement
{
    public class AssetManager
    {
        public static AssetCollection? ReadAssets(string filename)
        {
            try
            {
                using (var maxFile = new MaxFile(filename))
                {
                    if (maxFile.TryGetStream(MaxFileStreams.AssetStream, out var assetStream))
                    {
                        var assets = StreamHandler.ParseAssets(assetStream, out _);
                        return new AssetCollection(assets, true, false);
                    }

                    if (maxFile.TryGetStream(MaxFileStreams.AssetStreamOld, out var oldAssetStream))
                    {
                        var assets = StreamHandler.ParseAssets(oldAssetStream, out _);
                        return new AssetCollection(assets, true, true);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read assets from the file.", ex);
            }
        }

        public static bool TryReadAssets(MaxFile maxFile, out AssetCollection collection)
        {
            collection = null;
            try
            {
                if (maxFile.TryGetStream(MaxFileStreams.AssetStream, out var assetStream))
                {
                    var assets = StreamHandler.ParseAssets(assetStream, out var wasSuccess, ignoreUnexpectedEndOfStream: true);
                    collection = new AssetCollection(assets, wasSuccess, false);
                }

                if (maxFile.TryGetStream(MaxFileStreams.AssetStreamOld, out var oldAssetStream))
                {
                    var assets = StreamHandler.ParseAssets(oldAssetStream, out var wasSuccess, ignoreUnexpectedEndOfStream: true);
                    collection = new AssetCollection(assets, wasSuccess, true);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return collection != null;
        }

        public static bool TryReadAssets(string filename, out AssetCollection collection)
        {
            collection = null;
            try
            {
                using (var maxFile = new MaxFile(filename))
                {
                    if (TryReadAssets(maxFile, out collection))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return false;
        }

        public static async Task WriteAssets(MaxFile maxFile, AssetCollection assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets), "Assets list cannot be null.");
            }

            try
            {
                var streamData = StreamHandler.BuildAssetStream(assets);
                var streamName = assets.IsOldStream ? MaxFileStreams.AssetStreamOld : MaxFileStreams.AssetStream;
                await maxFile.SetStream(streamName, streamData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to write assets to the file.", ex);
            }
        }

        public static async Task WriteAssets(string filename, AssetCollection assets)
        {
            try
            {
                using (var maxFile = new MaxFile(filename, MaxFileAccess.Write))
                {
                    await WriteAssets(maxFile, assets);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to write assets to the file.", ex);
            }
        }
    }
}
