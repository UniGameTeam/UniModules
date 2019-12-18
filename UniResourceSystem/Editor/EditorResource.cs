﻿namespace UniGreenModules.UniResourceSystem.Editor
{
    using Runtime;
    using UniCore.Runtime.ProfilerTools;
    using UnityEditor;
    using UnityEngine;

    public class EditorResource : ResourceItem
    {
        public string AssetPath => assetPath;

        public bool IsInstance => string.IsNullOrEmpty(AssetPath);

        public Object Target => asset;

        public PrefabInstanceStatus InstanceStatus { get; protected set; } = PrefabInstanceStatus.NotAPrefab;

        public PrefabAssetType PrefabAssetType { get; protected set; } = PrefabAssetType.NotAPrefab;

        protected override void OnUpdateAsset(Object targetAsset)
        {
            var resultAsset = targetAsset;
            var resultPath = string.Empty;

            if (targetAsset is Component componentAsset)
                resultAsset = componentAsset.gameObject;
            if (resultAsset is GameObject targetGameObject) {
                
                InstanceStatus = PrefabUtility.GetPrefabInstanceStatus(targetGameObject);
                PrefabAssetType = PrefabUtility.GetPrefabAssetType(targetGameObject);
                
                GameLog.Log($"PREFAB {targetGameObject.name} : InstanceStatus {InstanceStatus} PrefabAssetType {PrefabAssetType}");
                
                resultAsset = PrefabUtility.GetOutermostPrefabInstanceRoot(targetGameObject);
                
                if (resultAsset!=null) {
                    resultPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(resultAsset);
                }
            }

            if (resultAsset == null) {
                resultAsset = targetAsset;
                resultPath = AssetDatabase.GetAssetPath(targetAsset);
            }
    
            assetPath = resultPath;
            asset = resultAsset;
        }

        protected override TResult LoadAsset<TResult>()
        {
            if (string.IsNullOrEmpty(AssetPath))
                return null;
            
            var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetPath);
            
            return GetTargetFromSource<TResult>(asset);
        }
    }
}
