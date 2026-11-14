using MxUtilities.AssetManagement.Enums;

namespace MxUtilities.AssetManagement.Models
{
    public class Asset(Guid assetId, AssetType type, string? resolvedFile, string fileName, bool isOldStream)
    {
        public Guid AssetId { get; set; } = assetId;
        public AssetType Type { get; private set; } = type;
        public string? ResolvedFile { get; set; } = resolvedFile;
        public string FileName { get; set; } = fileName;

        public bool IsOldStream { get; set; } = isOldStream;

        public override string ToString()
        {
            return $"AssetID: '{AssetId}'\nType: '{Type}'\nResolved File: '{ResolvedFile}'\nFile Name: '{FileName}'\n";
        }
    }
}
