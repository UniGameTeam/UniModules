﻿namespace UniGreenModules.UniGame.SerializableContext.Runtime.Addressables
{
    using System;
    using AssetTypes;
    using UnityEngine.AddressableAssets;

    [Serializable]    
    public class ContextSourceAssetReference : AssetReferenceT<ContextAsset>
    {
        public ContextSourceAssetReference(string guid) : base(guid) {}
    }
}
