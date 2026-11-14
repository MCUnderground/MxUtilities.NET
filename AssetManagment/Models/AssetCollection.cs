namespace MxUtilities.AssetManagement.Models
{
    public class AssetCollection
    {
        public List<Asset> Assets { get; private set; }
        public bool ReachedEnd { get; private set; }
        public bool IsOldStream { get; private set; }

        public AssetCollection(List<Asset> assets, bool reachedEnd, bool isOldStream)
        {
            Assets = assets;
            ReachedEnd = reachedEnd;
            IsOldStream = isOldStream;
        }
    }
}
