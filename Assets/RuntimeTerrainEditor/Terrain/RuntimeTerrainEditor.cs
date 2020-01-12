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
    public partial class RuntimeTerrainEditor
    {
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
        /// 构造函数
        /// </summary>
        public RuntimeTerrainEditor(Texture2D[] brushs = null)
        {
            ConfigTerrainData();
            InitBrushs(brushs);
            ConfigActiveTerrains();
        }

        /// <summary>
        /// 使用前的配置
        /// </summary>
        public void Config()
        {
            ConfigTerrainData();
            ConfigActiveTerrains();
        }

        /// <summary>
        /// 配置地形的基础参数
        /// </summary>
        private void ConfigTerrainData()
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
            else
            {
                Debug.LogWarning("当前还没有任何地形,地形加载完成之后请手动调用ConfigTerrainData()");
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

        #region OtherClass

        /// <summary>
        /// 高度图修改的初始化参数
        /// </summary>
        public struct HeightCmdData
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

            public HeightCmdData(HeightCmdData source)
            {
                terrain = source.terrain;
                startMapIndex = source.startMapIndex;
                mapRadiusX = source.mapRadiusX;
                mapRadiusZ = source.mapRadiusZ;
                heightMap = new float[source.heightMap.GetLength(0), source.heightMap.GetLength(0)];
                Buffer.BlockCopy(source.heightMap, 0, heightMap, 0, heightMap.Length * 4);
            }
        }

        public struct DetialCmdData
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
            /// 层级
            /// </summary>
            public int layer;
            /// <summary>
            /// 细节图
            /// </summary>
            public int[,] detailMap;

            public DetialCmdData(Terrain terrain, Vector2Int startMapIndex, int layer, int[,] detailMap)
            {
                this.terrain = terrain;
                this.startMapIndex = startMapIndex;
                this.layer = layer;
                this.detailMap = new int[detailMap.GetLength(0), detailMap.GetLength(1)];
                Buffer.BlockCopy(detailMap, 0, this.detailMap, 0, detailMap.Length * 4);
            }
        }

        public struct TextureCmdData
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
            /// 纹理图
            /// </summary>
            public float[,,] alphaMap;

            public TextureCmdData(Terrain terrain, Vector2Int startMapIndex, float[,,] alphaMap)
            {
                this.terrain = terrain;
                this.startMapIndex = startMapIndex;
                this.alphaMap = new float[alphaMap.GetLength(0), alphaMap.GetLength(1), alphaMap.GetLength(2)];
                Buffer.BlockCopy(alphaMap, 0, this.alphaMap, 0, alphaMap.Length * 4);
            }
        }

        public interface ICmd
        {
            void Undo(RuntimeTerrainEditor t);
        }

        public struct HeightCmd : ICmd
        {
            private HeightCmdData cmd;

            public HeightCmd(HeightCmdData cmd)
            {
                this.cmd = cmd;
            }

            public void Undo(RuntimeTerrainEditor t)
            {
                t.SetHeightMap(cmd.terrain, cmd.heightMap, cmd.startMapIndex.x, cmd.startMapIndex.y, true);
            }
        }

        public struct DetialCmd : ICmd
        {
            private DetialCmdData cmd;

            public DetialCmd(DetialCmdData cmd)
            {
                this.cmd = cmd;
            }

            public void Undo(RuntimeTerrainEditor t)
            {
                TerrainUtility.SetDetailLayer(cmd.terrain, cmd.detailMap, cmd.startMapIndex.x, cmd.startMapIndex.y, cmd.layer);
            }
        }

        public struct TextureCmd : ICmd
        {
            private TextureCmdData cmd;

            public TextureCmd(TextureCmdData cmd)
            {
                this.cmd = cmd;
            }

            public void Undo(RuntimeTerrainEditor t)
            {
                TerrainUtility.SetAlphaMap(cmd.terrain, cmd.alphaMap, cmd.startMapIndex.x, cmd.startMapIndex.y);
            }
        }

        #endregion
    }
}