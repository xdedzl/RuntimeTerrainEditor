using System.Collections.Generic;
using UnityEngine;

namespace XFramework
{
    public partial class ResourceManager
    {
        public interface ITask
        {
            /// <summary>
            /// 执行任务
            /// </summary>
            /// <returns>是否成功执行</returns>
            bool Excute();
        }

        /// <summary>
        /// AB包加载资源任务
        /// </summary>
        public class ABLoadTask<T> : ITask where T : Object
        {
            private AssetBundleRequest request;
            private System.Action<T> callBack;

            public ABLoadTask(AssetBundleRequest asyncOperation, System.Action<T> callBack)
            {
                request = asyncOperation;
                this.callBack = callBack;
            }

            public bool Excute()
            {
                if (!request.isDone)
                    return false;
                else
                {
                    callBack(request.asset as T);
                    return true;
                }
            }
        }

        /// <summary>
        /// Resources加载资源任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ResLoadTask<T> : ITask where T : Object
        {
            private ResourceRequest request;
            private System.Action<T> callBack;

            public ResLoadTask(ResourceRequest asyncOperation, System.Action<T> callBack)
            {
                request = asyncOperation;
                this.callBack = callBack;
            }

            public bool Excute()
            {
                if (!request.isDone)
                    return false;
                else
                {
                    callBack(request.asset as T);
                    return true;
                }
            }
        }

        /// <summary>
        /// 加载AB包任务
        /// </summary>
        public class AssetBundleCreateTask : ITask
        {
            private AssetBundleCreateRequest request;

            private System.Action<AssetBundle> callBack;

            public AssetBundleCreateTask(AssetBundleCreateRequest request, System.Action<AssetBundle> callBack)
            {
                this.request = request;
                this.callBack = callBack;
            }

            public bool Excute()
            {
                if (!request.isDone)
                    return false;
                else
                {
                    callBack(request.assetBundle);
                    return true;
                }
            }
        }

        /// <summary>
        /// 加载依赖包任务
        /// </summary>
        public class LoadDependenciesTask : ITask
        {
            private AssetBundleCreateRequest mainRequest;
            private List<AssetBundleCreateRequest> requests;
            private System.Action<AssetBundle> allEndCallBack;
            private System.Action<AssetBundle> callBack;

            public LoadDependenciesTask(List<AssetBundleCreateRequest> requests, System.Action<AssetBundle> callBack, System.Action<AssetBundle> allEndCallBack)
            {
                this.mainRequest = requests[0];
                this.requests = requests;
                this.callBack = callBack;
                this.allEndCallBack = allEndCallBack;
            }

            public bool Excute()
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    if (requests[i].isDone)
                    {
                        callBack(requests[i].assetBundle);
                        requests.RemoveAt(i);
                        i--;
                    }
                }
                if (requests.Count <= 0)
                {
                    allEndCallBack(mainRequest.assetBundle);
                    return true;
                }
                else
                    return false;
            }
        }
    }
}