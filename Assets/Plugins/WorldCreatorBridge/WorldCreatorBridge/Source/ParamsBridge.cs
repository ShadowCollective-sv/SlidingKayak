// Project: WorldCreatorBridge
// Filename: SettingsBridge.cs
// Copyright (c) 2023 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace BtB.WC.Bridge
{
    [Serializable]
    public class ParamsBridge
    {
        public int userSplit = 3;
        public int internalSplit = 4096;

        public string bridgeFilePath = "";
        public string terrainsFolderName = "WorldCreatorTerrains";
        public string assetName = "WC_Terrain";
        public bool deleteUnusedAssets = true;

        public bool isImportLayers = true;
        public bool layerWarning = false;
        
        public float worldScale = 1;
        public string worldScaleString = "1.00";
        
        public MaterialType materialType = MaterialType.Standard;
        public Material customMaterial;

        public DefaultAsset materialsFolder;  // По умолчанию null — fallback на дефолтный путь
        
        public int InternalSplit => Math.Min(internalSplit, 128 << userSplit);
        public int UserSplit => 128 << userSplit; //возможно это как раз параметр, который определяет делить ли большой террейн на много мелких.

        public ParamsBridge()
        {
            bridgeFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/World Creator/Sync/bridge.xml";
        }

        public bool IsBridgeFileValid()
        {
            return !string.IsNullOrEmpty(bridgeFilePath);
        }
    }

    public enum MaterialType
    {
        Standard,
        HDRP,
        URP,
        Custom
    }
}

#endif