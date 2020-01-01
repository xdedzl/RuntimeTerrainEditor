// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-10-24 16:26:10
// 版本： V 1.0
// ==========================================
using System.Collections.Generic;
using UnityEngine;
using System;

namespace XFramework
{
    /// <summary>
    /// 使用工具类
    /// </summary>
    public static partial class Utility
    {
        /// <summary>
        /// 发射射线并返回RaycastInfo
        /// </summary>
        public static RaycastHit SendRay(int layer = -1)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, float.MaxValue, layer))
            {
                return hitInfo;
            }
            else
            {
                return default;
            }
        }
        public static RaycastHit SendRayDown(Vector3 start, int layer = -1)
        {
            start.y += 10000;
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hitInfo, float.MaxValue, layer))
            {
                return hitInfo;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// 创建立方体
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static GameObject CreatPrimitiveType(PrimitiveType type, Vector3 pos = default, float size = 1)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.transform.position = pos;
            obj.transform.localScale = Vector3.one * size;
            return obj;
        }
        public static GameObject CreatPrimitiveType(PrimitiveType type, Color color, Vector3 pos = default, float size = 1)
        {
            GameObject obj = CreatPrimitiveType(type, pos, size);
            obj.GetComponent<MeshRenderer>().material.color = color;
            return obj;
        }

        /// <summary>
        /// 获取一组位置
        /// </summary>
        public static Vector3[] GetPositions(Transform[] trans)
        {
            Vector3[] poses = new Vector3[trans.Length];
            for (int i = 0, length = trans.Length; i < length; i++)
            {
                poses[i] = trans[i].position;
            }
            return poses;
        }
        public static Vector3[] GetPositions(List<Transform> trans)
        {
            Vector3[] poses = new Vector3[trans.Count];
            for (int i = 0, length = trans.Count; i < length; i++)
            {
                poses[i] = trans[i].position;
            }
            return poses;
        }

        /// <summary>
        /// 获取一组欧拉角
        /// </summary>
        public static Vector3[] GetAngles(Transform[] trans)
        {
            Vector3[] angles = new Vector3[trans.Length];
            for (int i = 0, length = trans.Length; i < length; i++)
            {
                angles[i] = trans[i].localEulerAngles;
            }
            return angles;
        }
        public static Vector3[] GetAngles(List<Transform> trans)
        {
            Vector3[] angles = new Vector3[trans.Count];
            for (int i = 0, length = trans.Count; i < length; i++)
            {
                angles[i] = trans[i].localEulerAngles;
            }
            return angles;
        }

        /// <summary>
        /// 执行一个方法并返回它的执行时间
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static float DebugActionRunTime(Action action)
        {
            float time = DateTime.Now.Millisecond + DateTime.Now.Second * 1000;
            action();
            return DateTime.Now.Millisecond + DateTime.Now.Second * 1000 - time;
        }
    }
}