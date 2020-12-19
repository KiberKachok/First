#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AlmostEngine
{
    public class AssetUtils
    {
        public static List<string> GetAllGuids<AssetType, ClassType>(List<string> paths = null, bool includeSubAssets = true)
        {
            string filter = "t:";
            if (typeof(AssetType) == typeof(GameObject))
            {
                filter += "prefab";
            }
            else
            {
                filter += typeof(AssetType).ToString();
            }
            List<string> guids = AssetDatabase.FindAssets(filter).ToList<string>();

            List<string> filteredGuids = new List<string>();
            foreach (var id in guids)
            {
                if (!AssetPathUtils.InPaths(AssetDatabase.GUIDToAssetPath(id), paths))
                    continue;

                // Debug.Log("Found id " + id);
                if (includeSubAssets)
                {
                    var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(id));
                    foreach (var obj in assetsAtPath)
                    {
                        if (obj != null && obj is ClassType)
                        {
                            if (!filteredGuids.Contains(id))
                            {
                                filteredGuids.Add(id);
                            }
                        }

                    }
                }
                else
                {
                    var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(id), typeof(AssetType));
                    if (obj != null && obj is ClassType)
                    {
                        if (!filteredGuids.Contains(id))
                        {
                            filteredGuids.Add(id);
                        }
                    }
                }

            }

            // Debug.Log("T : " + typeof(AssetType).ToString() + " Found guids " + filteredGuids.Count);

            return filteredGuids;
        }
        public static List<T> LoadAll<T>(List<string> paths = null, bool includeSubAssets = true) where T : Object
        {
            return LoadAll<T, T>(paths, includeSubAssets);
        }

        public static List<AssetType> LoadAll<AssetType, ClassType>(List<string> paths = null, bool includeSubAssets = true) where AssetType : Object
        {
            List<string> guids = GetAllGuids<AssetType, ClassType>(paths);

            List<AssetType> assets = new List<AssetType>();
            foreach (var id in guids)
            {
                if (includeSubAssets)
                {
                    var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(id));
                    foreach (var obj in assetsAtPath)
                    {
                        if (obj != null && obj is ClassType)
                        {
                            assets.Add((AssetType)obj);
                        }
                    }
                }
                else
                {
                    var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(id), typeof(AssetType));
                    if (obj != null && obj is ClassType)
                    {
                        assets.Add((AssetType)obj);
                    }
                }
            }
            return assets;
        }

        public static T GetFirst<T>() where T : ScriptableObject
        {
            List<T> objs = LoadAll<T>();
            if (objs.Count == 0)
            {
                return null;
            }
            else
            {
                return objs[0];
            }
        }


        public static T Create<T>(string name, string path = "Assets/") where T : ScriptableObject
        {
            Debug.Log("Asset created at " + path);
            string fullpath = path + name + ".asset";
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            AssetDatabase.CreateAsset(asset, fullpath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        public static T GetFirstOrCreate<T>(string name, string path = "Assets/") where T : ScriptableObject
        {
            T asset = GetFirst<T>();
            if (asset == null)
            {
                asset = Create<T>(name, path);
            }
            return asset;
        }

    }
}



#endif