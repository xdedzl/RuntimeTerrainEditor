using UnityEngine;
using System.Collections.Generic;
using System;
/** 
* Terrain的HeightMap坐标原点在左下角
*   z
*   ↑
*   0 → x
*/


namespace XFramework.TerrainMoudule
{
    /// <summary>
    /// Terrain工具
    /// terrainData.GetHeights和SetHeights的参数都是 值域为[0,1]的比例值
    /// </summary>
    public partial class TerrainManager
    {
        private TDInfo tdInfo;

        #region TerrainData基础数据及初始化

        /// <summary>
        /// 用于修改高度的单位高度
        /// </summary>
        private float deltaHeight;
        /// <summary>
        /// 地形大小
        /// </summary>
        public Vector3 terrainSize { get; private set; }
        /// <summary>
        /// 高度图分辨率
        /// </summary>
        private int heightMapRes;
        /// <summary>
        /// alphaMap分辨率
        /// </summary>
        public int alphaMapRes { get; private set; }
        /// <summary>
        /// 笔刷数据
        /// </summary>
        private Dictionary<int, float[,]> brushDic = new Dictionary<int, float[,]>();
        /// <summary>
        /// 用于记录要修改的Terrain目标数据，修改后统一刷新
        /// </summary>
        private List<Terrain> terrainList = new List<Terrain>();

        /// <summary>
        /// 高度图每一小格的宽高
        /// </summary>
        public float pieceWidth { get; private set; }
        public float pieceHeight { get; private set; }

        /// <summary>
        /// AlphaMap每一小格的宽高
        /// </summary>
        public float alphaPieceWidth { get; private set; }
        public float alphaPieceHeight { get; private set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        public TerrainManager(Texture2D[] brushs)
        {
            ConfigTerrainData();
            tdInfo = new TDInfo(Terrain.activeTerrain);
            InitBrushs(brushs);
            ConfigActiveTerrains();
        }

        /// <summary>
        /// 配置地形的基础参数
        /// </summary>
        public void ConfigTerrainData()
        {
            if (Terrain.activeTerrain != null)
            {
                deltaHeight = 1 / Terrain.activeTerrain.terrainData.size.y;
                terrainSize = Terrain.activeTerrain.terrainData.size;
                heightMapRes = Terrain.activeTerrain.terrainData.heightmapResolution;
                alphaMapRes = Terrain.activeTerrain.terrainData.alphamapResolution;

                pieceWidth = terrainSize.x / (heightMapRes - 1);
                pieceHeight = terrainSize.z / (heightMapRes - 1);

                alphaPieceWidth = terrainSize.x / (alphaMapRes);
                alphaPieceHeight = terrainSize.z / (alphaMapRes);
            }
        }

        #endregion

        #region 修改与恢复

        private Stack<ICmd> m_CmdStack = new Stack<ICmd>();

        /// <summary>
        /// 注册撤销命令                                                                                      
        /// </summary>
        /// <param name="cmd"></param>
        public void RegisterUndo(ICmd cmd)
        {
            m_CmdStack.Push(cmd);
        }

        public void Undo()
        {
            if(m_CmdStack.Count > 0)
                m_CmdStack.Pop().Undo(this);
        }

        #endregion
    }

    /// <summary>
    /// 高度图修改的初始化参数
    /// </summary>
    public struct HeightMapCmd
    {
        /// <summary>
        /// 要执行的地形
        /// </summary>
        public Terrain terrain;
        /// <summary>
        /// 修改地形的起始索引
        /// </summary>
        public Vector2Int startMapIndex;
        /// <summary>
        /// 准备修改的宽度
        /// </summary>
        public int mapRadiusX;
        /// <summary>
        /// 准备修改的宽度
        /// </summary>
        public int mapRadiusZ;
        /// <summary>
        /// 修改的目标（当修改的时地图最外圈时，其一二维的length和mapRadius不一定相等）
        /// </summary>
        public float[,] heightMap;

        public HeightMapCmd(HeightMapCmd source)
        {
            terrain = source.terrain;
            startMapIndex = source.startMapIndex;
            mapRadiusX = source.mapRadiusX;
            mapRadiusZ = source.mapRadiusZ;
            heightMap = new float[source.heightMap.GetLength(0), source.heightMap.GetLength(0)];
            Buffer.BlockCopy(source.heightMap, 0, heightMap, 0, heightMap.Length * 4);
        }
    }

    /// <summary>
    /// 地形数据
    /// </summary>
    public struct TDInfo
    {
        /// <summary>
        /// 用于修改高度的单位高度
        /// </summary>
        public float deltaHeight;
        /// <summary>
        /// 地形大小
        /// </summary>
        public Vector3 terrainSize;
        /// <summary>
        /// 高度图分辨率
        /// </summary>
        public int heightMapRes;
        /// <summary>
        /// alphaMap分辨率
        /// </summary>
        public int alphaMapRes;
        /// <summary>
        /// 高度图每一小格的宽度
        /// </summary>
        public float pieceWidth;
        /// <summary>
        /// 高度图每一小格的高度
        /// </summary>
        public float pieceHeight;

        /// <summary>
        /// AlphaMap每一小格的宽度
        /// </summary>
        public float alphaPieceWidth;
        /// <summary>
        /// AlphaMap每一小格的高度
        /// </summary>
        public float alphaPieceHeight;

        public TDInfo(Terrain terrain)
        {
            if (terrain != null)
            {
                deltaHeight = 1 / terrain.terrainData.size.y;
                terrainSize = terrain.terrainData.size;
                heightMapRes = terrain.terrainData.heightmapResolution;
                alphaMapRes = terrain.terrainData.alphamapResolution;

                pieceWidth = terrainSize.x / (heightMapRes - 1);
                pieceHeight = terrainSize.z / (heightMapRes - 1);

                alphaPieceWidth = terrainSize.x / (alphaMapRes);
                alphaPieceHeight = terrainSize.z / (alphaMapRes);
            }
            else
            {
                throw new System.Exception("terrain 不能为空");
            }
        }
    }

    public interface ICmd 
    {
        void Undo(TerrainManager t);
    }

    public class TerrainCmd : ICmd
    {
        private HeightMapCmd cmd;

        public TerrainCmd(HeightMapCmd cmd)
        {
            this.cmd = cmd;
        }

        public void Undo(TerrainManager t)
        {
            t.SetHeightMap(cmd.terrain, cmd.heightMap, cmd.startMapIndex.x, cmd.startMapIndex.y, true);
        }
    }

    public struct TerrainCmdData
    {
        public int x;
        public int y;
        public Terrain terrain;
        public float[,] heights;
    }
}