using System;
using System.Collections.Generic;
using System.Reflection;

namespace XFramework
{
    public static partial class Extend
    {
        /// <summary>
        /// 通过反射和函数名调用非公有方法
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="methodName">函数名</param>
        /// <param name="objs">参数数组</param>
        public static void Invoke(this object obj, string methodName, params object[] objs)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            Type type = obj.GetType();
            MethodInfo m = type.GetMethod(methodName, flags);
            m.Invoke(obj, objs);
        }

        public static string[] GetSonNames(this Type typeBase, string assemblyName = "Assembly-CSharp")
        {
            List<string> typeNames = new List<string>();
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch
            {
                return new string[0];
            }

            if (assembly == null)
            {
                return new string[0];
            }

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeBase))
                {
                    typeNames.Add(type.FullName);
                }
            }
            typeNames.Sort();
            return typeNames.ToArray();
        }
    }
}