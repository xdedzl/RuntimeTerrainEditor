#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XFramework.Resource
{
    public class AssetDataBaseLoadHelper : IResourceLoadHelper
    {
        private readonly string m_AssetPath;

        public AssetDataBaseLoadHelper()
        {
            m_AssetPath = Application.dataPath.Replace("/Assets", "");
        }

        public string AssetPath => m_AssetPath;

        public T Load<T>(string assetName) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetName);
        }

        public T[] LoadAll<T>(string path) where T : UnityEngine.Object
        {
            return LoadAllWithADB<T>(path);
        }

        public IProgress LoadAsync<T>(string assetName, System.Action<T> callback) where T : UnityEngine.Object
        {
            T obj = AssetDatabase.LoadAssetAtPath<T>(assetName);
            callback(obj);
            return new DefaultProgress();
        }

        /// <summary>
        /// 加载一个文件夹下的所有资源
        /// </summary>
        private T[] LoadAllWithADB<T>(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly) where T : Object
        {
            List<T> objs = new List<T>();
            DirectoryInfo info = new DirectoryInfo(Application.dataPath.Replace("Assets", "") + path);
            foreach (var item in info.GetFiles("*", searchOption))
            {
                if (item.Name.EndsWith(".meta"))
                    continue;
                string assetName = path + "/" + item.Name;
                objs.Add(AssetDatabase.LoadAssetAtPath<T>(assetName));
            }
            return objs.ToArray();
        }
    }
}

#endif