using System.Collections.Generic;

namespace XFramework
{
    public partial class ResourceManager
    {

        /// <summary>
        /// 任务池
        /// </summary>
        public class TaskPool
        {
            /// <summary>
            /// 任务池
            /// </summary>
            private List<ITask> m_TaskPool;
            /// <summary>
            /// 正在加载中的AB包
            /// </summary>
            private Dictionary<string, LoadDependenciesTask> m_LoadingAB;

            public TaskPool()
            {
                m_TaskPool = new List<ITask>();
                m_LoadingAB = new Dictionary<string, LoadDependenciesTask>();
            }

            public void Update()
            {
                for (int i = 0; i < m_TaskPool.Count; i++)
                {
                    if (m_TaskPool[i].Excute())
                    {
                        m_TaskPool.RemoveAt(i);
                        i--;
                    }
                }
            }

            /// <summary>
            /// 添加一个任务
            /// </summary>
            /// <param name="baseTask"></param>
            public void Add(ITask baseTask)
            {
                m_TaskPool.Add(baseTask);
            }

            /// <summary>
            /// 是否正在加载
            /// </summary>
            /// <returns></returns>
            public bool IsLoading(string abKey)
            {
                return m_LoadingAB.ContainsKey(abKey);
            }

            /// <summary>
            /// 添加一个正在加载的AB包
            /// </summary>
            /// <param name="abkey"></param>
            /// <param name="task"></param>
            public void AddLoadingAB(string abkey, LoadDependenciesTask task)
            {
                m_LoadingAB.Add(abkey, task);
            }

            public bool AddLoadDependenciesTask(string abKey, System.Action callBack)
            {
                if (m_LoadingAB.ContainsKey(abKey))
                {
                    return true;
                }
                return false;
            }
        }
    }
}