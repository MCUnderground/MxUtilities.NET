using System.Text;
using MxUtilities.AssetManagement.Enums;
using MxUtilities.AssetManagement.Models;
using MxUtilities.FileHandling.Constants;
using MxUtilities.FileHandling.Models;

namespace MxUtilities.AssetManagement
{
    internal static class StreamHandler
    {
        private static readonly Dictionary<string, AssetType> AssetTypeMap = Enum.GetValues<AssetType>()
            .ToDictionary(assetType => assetType.ToString(), assetType => assetType);

        public static List<Asset> ParseAssets(MaxStream stream, out bool success, bool ignoreUnexpectedEndOfStream = false)
        {
            var assets = new List<Asset>();
            var buffer = new byte[1024];
            success = true;

            bool isOldStream = stream.StreamName == MaxFileStreams.AssetStreamOld;

            while (stream.Position < stream.Size)
            {
                if (!TryReadBytes(stream, buffer, 16, out var assetIdBytes)) break;
                var assetId = new Guid(assetIdBytes);

                if (assetId == Guid.Empty)
                {
                    break;
                }

                int typeSize = ReadInt32(stream, buffer);
                string typeString = ReadUtf16String(stream, buffer, typeSize);
                if (!AssetTypeMap.TryGetValue(typeString, out var type))
                {
                    if (!ignoreUnexpectedEndOfStream)
                    {
                        throw new Exception($"Unsupported asset type: {typeString}");
                    }

                    success = false;
                    return assets;
                }

                string? resolvedFile = null;
                if (!isOldStream)
                {
                    int resolvedFileSize = ReadInt32(stream, buffer);
                    resolvedFile = ReadUtf16String(stream, buffer, resolvedFileSize);
                }

                int fileNameSize = ReadInt32(stream, buffer);
                string fileName = ReadUtf16String(stream, buffer, fileNameSize);

                var asset = new Asset(assetId, type, resolvedFile, fileName, isOldStream);
                assets.Add(asset);
            }

            return assets;
        }

        public static MaxStream BuildAssetStream(AssetCollection assetCollection)
        {
            var maxStream = new MaxStream(assetCollection.Assets.Count * 1106);

            foreach (var asset in assetCollection.Assets)
            {
                maxStream.Write(asset.AssetId.ToByteArray());
                WriteUtf16String(maxStream, asset.Type.ToString());

                if (!assetCollection.IsOldStream)
                {
                    WriteUtf16String(maxStream, asset.ResolvedFile);
                }

                WriteUtf16String(maxStream, asset.FileName);
            }

            maxStream.Trim();
            return maxStream;
        }

        private static int ReadInt32(MaxStream stream, byte[] buffer)
        {
            ReadBytes(stream, buffer, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        private static string ReadUtf16String(MaxStream stream, byte[] buffer, int length)
        {
            var byteCount = (length + 1) * 2; // +1 for null terminator
            ReadBytes(stream, buffer, byteCount);
            return Encoding.Unicode.GetString(buffer, 0, byteCount - 2); // exclude null terminator
        }

        private static void WriteUtf16String(MaxStream maxStream, string value)
        {
            var length = value.Length;
            var encoded = Encoding.Unicode.GetBytes(value + '\0');

            maxStream.Write(BitConverter.GetBytes(length));
            maxStream.Write(encoded);
        }

        private static void ReadBytes(MaxStream stream, byte[] buffer, int count)
        {
            if (stream.Read(buffer, count) != count)
            {
                throw new EndOfStreamException("Failed to read the required number of bytes.");
            }
        }

        private static bool TryReadBytes(MaxStream stream, byte[] buffer, int count, out byte[] result)
        {
            if (stream.Read(buffer, count) != count)
            {
                result = Array.Empty<byte>();
                return false;
            }

            result = buffer.Take(count).ToArray();
            return true;
        }
    }
}
