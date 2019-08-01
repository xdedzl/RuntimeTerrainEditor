// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-10-31 12:07:09
// 版本： V 1.0
// ==========================================
using System.Collections.Generic;
using XFramework.Mathematics;
using UnityEngine;

namespace XFramework.Draw
{
    /// <summary>
    /// 所有的MeshPrefab 的管理类, 单例
    /// </summary>
    public class MeshManager : IGameModule
    {
        /// <summary>
        /// 用于画线的材质
        /// </summary>
        public Material LineMaterial
        {
            get
            {
                if (m_LineMaterial == null)
                    m_LineMaterial = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                return m_LineMaterial;
            }
        }
        private Material m_LineMaterial;

        /// <summary>
        /// 用于面片的材质
        /// </summary>
        public Material QuadeMaterial
        {
            get
            {
                if (m_QuadeMaterial == null)
                    m_QuadeMaterial = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                return m_QuadeMaterial;
            }
        }
        private Material m_QuadeMaterial;

        /// <summary>
        /// 用于画三维物体的材质
        /// </summary>
        public Material ShapeMaterial
        {
            get
            {
                if (m_ShapeMaterial == null)
                    m_ShapeMaterial = new Material(Shader.Find("RunTimeHandles/Shape"));
                return m_ShapeMaterial;
            }
        }
        private Material m_ShapeMaterial;

        /// <summary>
        /// 创建圆柱形区域
        /// </summary>
        /// <param name="id"> 命令ID,用作字典key </param>
        /// <param name="point"> 圆心点 </param>
        /// <param name="radius"> 半径 </param>
        /// <param name="height"> 高度 </param>
        public void CreateCylinder(Vector3 point, float radius, float height, Color color)
        {
            Mesh mesh = GLDraw.CreateCylinder(point, radius, height, color.Int32());    // 画出图形   
            Debug.Log(color.Int32().Color());

            Game.GraphicsModule.AddGraphics(Camera.main, () =>
            {
                ShapeMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });
        }

        /// <summary>
        /// 创建两个圆柱，近距火力支援等待空域
        /// </summary>
        /// <param name="id"> 命令ID,用作字典key </param>
        /// <param name="point">低点圆柱的底面圆心点</param>
        /// <param name="radius">半径</param>
        /// <param name="height">圆柱高度</param>
        /// <param name="heightDifference">两个圆柱的高度差</param>
        public void DoubleCylinder(Vector3 point, float radius, float height, float heightDifference, Color color)
        {
            // 下面圆柱
            Mesh meshUp = GLDraw.CreateCylinder(point, radius, height, color.Int32());

            // 上面圆柱
            Vector3 highPoint = point + Vector3.up * (height + heightDifference);
            Mesh meshDown = GLDraw.CreateCylinder(highPoint, radius, height);

            Game.GraphicsModule.AddGraphics(Camera.main, () =>
            {
                ShapeMaterial.SetPass(0);
                Graphics.DrawMeshNow(meshUp, Matrix4x4.identity);
                Graphics.DrawMeshNow(meshDown, Matrix4x4.identity);
            });
        }

        /// <summary>
        /// 创建通用多边形区域
        /// </summary>
        /// <param name="id"> 命令ID,用作字典key </param>
        /// <param name="list">下底面点链表</param>
        /// <param name="height">高度</param>
        public void CreatePolygon(List<Vector3> list, float height, Color color)
        {
            PhysicsMath.CheckVector(list);
            Vector3[] vector3s = list.ToArray();                                        // 使数组逆时针排序
            Mesh mesh = GLDraw.CreatePolygon(vector3s, height, color.Int32());          // 画出图形

            Game.GraphicsModule.AddGraphics(Camera.main, () =>
            {
                ShapeMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });
        }

        /// <summary>
        /// 创建空中走廊
        /// </summary>
        /// <param name="id"> 命令ID,用作字典key </param>
        /// <param name="list">中心点链表</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void CreateAirCorridorSpace(List<Vector3> list, float width, float height, Color color)
        {
            //Vector3[] vertices = PhysicsMath.GetAirCorridorSpace(list, width, height);       // 获取点集
            //DrawTriangles.DrawAirCorridorSpace(vertices, meshFilter, lineRenderers);        // 画出图形

            Mesh mesh = GLDraw.CreateLineMesh(list, width, height, color.Int32());

            Game.GraphicsModule.AddGraphics(Camera.main, () =>
            {
                ShapeMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });
        }

        /// <summary>
        /// 创建扇形防空区
        /// </summary>
        /// <param name="id"> 命令ID,用作字典key </param>
        /// <param name="origin">起始点</param>
        /// <param name="tarPoint">水平最远距离点</param>
        /// <param name="alpha">横向张角</param>
        /// <param name="theta">纵向张角</param>
        public void CreateSector(Vector3 origin, Vector3 tarPoint, float alpha, float theta, Color color)
        {
            Vector3[] vertices = PhysicsMath.GetSectorPoints_2(origin, tarPoint, alpha, theta);
            Mesh mesh = GLDraw.CreatePolygon(vertices, color.Int32());

            Game.GraphicsModule.AddGraphics(Camera.main, () =>
            {
                ShapeMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });
        }

        /// <summary>
        /// 创建紫色杀伤盒
        /// </summary>
        /// <param name="id"> 命令ID,用作字典key </param>
        /// <param name="list">底面四点链表</param>
        /// <param name="lower">下限高度</param>
        /// <param name="Ceiling">上限高度</param>
        public void CreateKillBox(List<Vector3> list, float lower, float Ceiling, Color color)
        {
            // 第一个杀伤盒
            PhysicsMath.CheckVector(list);
            Vector3[] vector3s1 = list.ToArray();                                     // 使数组逆时针排序
            Mesh mesh0 = GLDraw.CreatePolygon(vector3s1, lower, color.Int32());               // 画出图形

            // 第二个杀伤盒
            List<Vector3> CeilingList = new List<Vector3>();   // 中层顶点集合
            foreach (var item in list)
            {
                CeilingList.Add(item + Vector3.up * lower);
            }

            PhysicsMath.CheckVector(CeilingList);
            Vector3[] vector3s2 = CeilingList.ToArray();                               // 使数组逆时针排序
            Mesh mesh1 = GLDraw.CreatePolygon(vector3s2, Ceiling - lower, color.Int32());      // 画出图形

            Game.GraphicsModule.AddGraphics(Camera.main, () =>
            {
                ShapeMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh0, Matrix4x4.identity);
                Graphics.DrawMeshNow(mesh1, Matrix4x4.identity);
            });
        }

        /// <summary>
        /// 创建蓝色杀伤盒
        /// </summary>
        /// <param name="id"> 命令ID,用作字典key </param>
        /// <param name="list">底面四点链表</param>
        /// <param name="ceiling">上限高度</param>
        public void CreateKillBox(List<Vector3> list, float ceiling, Color color)
        {
            CreatePolygon(list, ceiling, color);
        }

        /// <summary>
        /// 创建半圆形防空区域
        /// </summary>
        /// <param name="origin">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="angle">张角（默认90度）</param>
        public void CreateHemisphere(Vector3 origin, float radius, Color color, int angle = 90)
        {
            Mesh mesh = GLDraw.CreateHemisphere(origin, radius, 90, color.Int32());

            Game.GraphicsModule.AddGraphics(Camera.main, () =>
            {
                ShapeMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });
        }

        public int Priority => 200;

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
            m_LineMaterial = null;
            m_QuadeMaterial = null;
            m_ShapeMaterial = null;
        }
    }
}