using Nine.AssetReferences.Editor.Utilities;
using UnityEngine.AddressableAssets;

namespace Nine.AssetReferences.Editor.Validation
{
    public class AssetNotAddressableFixRequest : IFixRequest
    {
        private readonly AssetReference reference;

        public AssetNotAddressableFixRequest(AssetReference reference)
        {
            this.reference = reference;
        }

        public bool Fix()
        {
            if (reference.editorAsset == null)
            {
                return false;
            }

            AddressableUtility.MarkAsAddressable(reference.editorAsset);

            return true;
        }
    }
}