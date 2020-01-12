using System;
using System.Collections.Generic;
using UnityEngine;

namespace XFramework.TerrainMoudule
{
    /// <summary>
    /// 地形管理器
    /// </summary>
    public partial class RuntimeTerrainEditor
    {
        #region  工具

        /// <summary>
        /// 左下角地图相对00点的偏移
        /// </summary>
        private Vector3 excursionPos = Vector2.zero;
        private Terrain[,] terrains;

        /// <summary>
        /// 配置场景中的Terrain索引
        /// </summary>
        private void ConfigActiveTerrains()
        {
            Terrain[] activeTerrains = Terrain.activeTerrains;
            if (activeTerrains.Length == 0)
            {
                return;
            }
            terrainSize = activeTerrains[0].terrainData.size;
            float halfX = terrainSize.x / 2;
            float halfZ = terrainSize.z / 2;
            float x = 0, z = 0;
            excursionPos = activeTerrains[0].GetPosition();
            for (int i = 0; i < activeTerrains.Length; i++)
            {
                if (x < activeTerrains[i].GetPosition().x)
                    x = activeTerrains[i].GetPosition().x;
                if (z < activeTerrains[i].GetPosition().z)
                    z = activeTerrains[i].GetPosition().z;

                if (excursionPos.x > activeTerrains[i].GetPosition().x)
                    excursionPos.x = activeTerrains[i].GetPosition().x;
                if (excursionPos.z > activeTerrains[i].GetPosition().z)
                    excursionPos.z = activeTerrains[i].GetPosition().z;
            }
            terrains = new Terrain[(int)((x - excursionPos.x + halfX) / terrainSize.x) + 1, (int)((z - excursionPos.z + halfZ) / terrainSize.z) + 1];

            int u, v;

            for (int i = 0; i < activeTerrains.Length; i++)
            {
                u = (int)((activeTerrains[i].GetPosition().x - excursionPos.x + halfX) / terrainSize.x);
                v = (int)((activeTerrains[i].GetPosition().z - excursionPos.z + halfZ) / terrainSize.z);
                terrains[u, v] = activeTerrains[i];
            }
        }

        /// <summary>
        /// 获取当前位置所对应的地图块
        /// </summary>
        public Terrain GetTerrain(Vector3 pos)
        {
            //RaycastHit hitInfo;
            //Physics.Raycast(pos + Vector3.up * 10000, Vector3.down, out hitInfo, float.MaxValue, LayerMask.GetMask("Terrain"));
            //return hitInfo.collider?.GetComponent<Terrain>();


            int i, j;
            i = (int)((pos.x - excursionPos.x) / terrainSize.x);
            j = (int)((pos.z - excursionPos.z) / terrainSize.z);
            if (i + 1 > terrains.GetLength(0) || j + 1 > terrains.GetLength(1))
            {
                return null;
            }

            return terrains[i, j];
        }

        /// <summary>
        /// 获取当前位置在地图上的对应位置
        /// </summary>
        /// <returns></returns>
        public bool GetPositionOnTerrain(Vector3 pos, out Vector3 posOnTerrain)
        {
            Terrain terrain = GetTerrain(pos);
            return TerrainUtility.GetPositionOnTerrain(terrain, pos, out posOnTerrain);
        }

        /// <summary>
        /// 获取当前Index对应世界空间的坐标
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetPositionInWorild(Terrain terrain, int x, int y)
        {
            Vector3 pos = terrain.GetPosition();
            pos.x += x * pieceWidth;
            pos.z += y * pieceHeight;
            pos.y = terrain.SampleHeight(pos);
            return pos;
        }

        /// <summary>
        /// 获取地形区域范围
        /// </summary>
        /// <returns>返回一个长度为4的数组，分别表示地图左下角x值，地图左下角z值，地图在x轴上的宽度，地图在z轴上的宽度</returns>
        public float[] GetTerrainArea()
        {
            float[] f = new float[4];
            f[0] = terrains[0, 0].GetPosition().x;
            f[1] = terrains[0, 0].GetPosition().z;
            f[2] = terrains.GetLength(0) * terrainSize.x;
            f[3] = terrains.GetLength(1) * terrainSize.z;
            return f;
        }

        /// <summary>
        /// 判断是否在地图范围内
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool OnTerrainWide(Vector3 point)
        {
            float min_X = terrains[0, 0].GetPosition().x;
            float min_Z = terrains[0, 0].GetPosition().z;
            float max_X = min_X + terrains.GetLength(0) * terrainSize.x;
            float max_Z = min_Z + terrains.GetLength(1) * terrainSize.z;
            if (point.x < max_X && point.x > min_X && point.z > min_Z && point.z < max_Z)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取多边形内部 位于地图map上的点集
        /// </summary>
        /// <param name="_Points">世界坐标下的多边形点集</param>
        /// <returns>世界坐标下多边形的内部点集</returns>
        public List<Vector3> GetPointsInMaps(List<Vector3> _Points)
        {
            List<Vector3> outList = new List<Vector3>();

            //List<Vector3[]> triangles = PhysicsMath.PolygonToTriangles(_Points);    // 拿到所有三角形
            //for (int i = 0; i < triangles.Count; i++)
            //{
            //    // 遍历所有三角形取得内部点集
            //    outList.AddRange(PhysicsMath.GetPointsInTriangle(triangles[i], alphaPieceWidth, alphaPieceHeight));
            //}

            return outList;
        }

        #endregion


        #region 由原始Dem数据推算高程
        /// <summary>
        /// 原始数据地图的宽度间隙
        /// </summary>
        public float prototypeDetaWidth;
        /// <summary>
        /// 原始数据地图的长度间隙
        /// </summary>
        public float prototypeDetaLength;
        /// <summary>
        /// 原始数据地图
        /// </summary>
        public float[,] PrototypeMap = null;
        /// <summary>
        /// 原始数据地图东西方向采样点数
        /// </summary>
        public int PrototypeMapColumnCount;
        /// <summary>
        /// 原始数据地图南北方向采样点数
        /// </summary>
        public int PrototypeMapLineCount;
        /// <summary>
        /// 从原始高程数据级中获取高度
        /// </summary>
        /// <param name="point">待计算的位置</param>
        /// <returns></returns>
        public float GetHeightByPrototypeMap(Vector3 point)
        {
            if (PrototypeMap == null)
            {
                return -1;
            }

            float width = point.z / prototypeDetaWidth;
            float length = point.x / prototypeDetaLength;
            int widthIndex = Convert.ToInt32(width);
            int lengthIndex = Convert.ToInt32(length);
            float detaX = length - lengthIndex;
            float detaY = width - widthIndex;

            float tempHeight = -1;

            if (lengthIndex < (PrototypeMapLineCount - 1) && widthIndex < (PrototypeMapColumnCount - 1))
            {
                tempHeight = (1 - detaX) * (1 - detaY) * PrototypeMap[lengthIndex, widthIndex]
                    + detaX * (1 - detaY) * PrototypeMap[lengthIndex + 1, widthIndex]
                    + (1 - detaX) * detaY * PrototypeMap[lengthIndex, widthIndex + 1]
                    + detaX * detaY * PrototypeMap[lengthIndex + 1, widthIndex + 1];
            }
            else if (lengthIndex == (PrototypeMapLineCount - 1) && widthIndex < (PrototypeMapColumnCount - 1))
            {
                tempHeight = (1 - detaY) * PrototypeMap[lengthIndex, widthIndex] + detaY * PrototypeMap[lengthIndex, widthIndex + 1];
            }
            else if (widthIndex == (PrototypeMapColumnCount - 1) && lengthIndex < (PrototypeMapLineCount - 1))
            {
                tempHeight = (1 - detaX) * PrototypeMap[lengthIndex, widthIndex] + detaX * PrototypeMap[lengthIndex + 1, widthIndex];
            }
            else if (lengthIndex == (PrototypeMapLineCount - 1) && widthIndex == (PrototypeMapColumnCount - 1))
            {
                tempHeight = PrototypeMap[lengthIndex, widthIndex];
            }
            else
            {
                Debug.Log("Data error");
            }

            return tempHeight;
        }
        #endregion
    }
}