using UnityEngine;
using System.Collections.Generic;
/** 
* Terrain的HeightMap坐标原点在左下角
*   z
*   ↑
*   0 → x
*/

/// <summary>
/// Terrain工具
/// terrainData.GetHeights和SetHeights的参数都是 值域为[0,1]的比例值
/// </summary>
public static partial class TerrainUtility
{
    #region TerrainData基础数据及初始化

    /// <summary>
    /// 用于修改高度的单位高度
    /// </summary>
    private static float deltaHeight;
    /// <summary>
    /// 地形大小
    /// </summary>
    private static Vector3 terrainSize;
    /// <summary>
    /// 高度图分辨率
    /// </summary>
    private static int heightMapRes;
    /// <summary>
    /// alphaMap分辨率
    /// </summary>
    private static int alphaMapRes;
    /// <summary>
    /// 笔刷数据
    /// </summary>
    private static Dictionary<int, float[,]> brushDic = new Dictionary<int, float[,]>();
    /// <summary>
    /// 用于记录要修改的Terrain目标数据，修改后统一刷新
    /// </summary>
    private static List<Terrain> terrainList = new List<Terrain>();

    /// <summary>
    /// 高度图每一小格的宽高
    /// </summary>
    private static float pieceWidth;
    private static float pieceHeight;

    /// <summary>
    /// AlphaMap每一小格的宽高
    /// </summary>
    public static float alphaPieceWidth { get; private set; }
    public static float alphaPieceHeight { get; private set; }

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static TerrainUtility()
    {
        InitBrushs();
    }

    /// <summary>
    /// 配置地形的基础参数
    /// </summary>
    public static void ConfigTerrainData()
    {
        if (Terrain.activeTerrain != null)
        {
            deltaHeight = 1 / Terrain.activeTerrain.terrainData.size.y;
            terrainSize = Terrain.activeTerrain.terrainData.size;
            heightMapRes = Terrain.activeTerrain.terrainData.heightmapResolution;
            alphaMapRes = Terrain.activeTerrain.terrainData.alphamapResolution;

            InitPrototype(true, true, true);

            pieceWidth = terrainSize.x / (heightMapRes - 1);
            pieceHeight = terrainSize.z / (heightMapRes - 1);

            alphaPieceWidth = terrainSize.x / (alphaMapRes);
            alphaPieceHeight = terrainSize.z / (alphaMapRes);
        }
    }

    /// <summary>
    /// 初始化原型模板
    /// TODO 从Utility中去除，交由场景自行调用
    /// </summary>
    public static void InitPrototype(bool tree = false, bool detail = false, bool texture = false)
    {
        if (tree)
            InitTreePrototype();
        if (detail)
            InitDetailPrototype();
        if (texture)
            InitTextures();
    }

    #endregion

    #region 道路系统

    ///// <summary>
    ///// 压平路面
    ///// </summary>
    ///// <param name="point_0">线段起点</param>
    ///// <param name="point_1">线段末点</param>
    ///// <param name="halfLength"></param>
    //public static void FlattenRoad(Vector3 pos_0, Vector3 pos_1, Vector3 pos_2, Vector3 pos_3, RoadsOrBridges road)
    //{
    //    Vector3 point_0 = (pos_0 + pos_1) / 2;
    //    Vector3 point_1 = (pos_2 + pos_3) / 2;

    //    float maxX = Mathf.Max(pos_0.x, pos_1.x, pos_2.x, pos_3.x);
    //    float minX = Mathf.Min(pos_0.x, pos_1.x, pos_2.x, pos_3.x);
    //    float maxZ = Mathf.Max(pos_0.z, pos_1.z, pos_2.z, pos_3.z);
    //    float minZ = Mathf.Min(pos_0.z, pos_1.z, pos_2.z, pos_3.z);
    //    Vector3 startPos = new Vector3(minX, 0, minZ);
    //    Terrain terrain = GetTerrain(startPos);

    //    int mapWidth = (int)((maxX - minX) / pieceWidth);
    //    int mapHeight = (int)((maxZ - minZ) / pieceHeight);

    //    if (terrain != null)
    //    {
    //        // 构造坐标系
    //        GameCoordinate localCor = new GameCoordinate(pos_0, pos_1 - pos_0);
    //        Vector3 localPos_0 = localCor.World2Loacal3(pos_0);
    //        Vector3 localPos_1 = localCor.World2Loacal3(pos_1);
    //        Vector3 localPos_2 = localCor.World2Loacal3(pos_2);
    //        Vector3 localPos_3 = localCor.World2Loacal3(pos_3);
    //        float maxLX = Mathf.Max(localPos_0.x, localPos_1.x, localPos_2.x, localPos_3.x);
    //        float minLX = Mathf.Min(localPos_0.x, localPos_1.x, localPos_2.x, localPos_3.x);
    //        float maxLZ = Mathf.Max(localPos_0.z, localPos_1.z, localPos_2.z, localPos_3.z);
    //        float minLZ = Mathf.Min(localPos_0.z, localPos_1.z, localPos_2.z, localPos_3.z)/* - localPos_3.y / 2*/;

    //        Vector2Int mapIndex = GetHeightmapIndex(terrain, startPos);
    //        startPos = GetPositionInWorild(terrain, mapIndex.x, mapIndex.y);
    //        startPos.y = 0;

    //        float[,] heightMap = GetHeightMap(terrain, mapIndex.x, mapIndex.y, mapWidth + 1, mapHeight + 1);

    //        for (int i = 0; i < heightMap.GetLength(0); i++)
    //        {
    //            for (int j = 0; j < heightMap.GetLength(1); j++)
    //            {
    //                Vector2Int newMapIndex = new Vector2Int(mapIndex.x + j, mapIndex.y + i);
    //                if (newMapIndex.x > (heightMapRes - 1))
    //                {
    //                    terrain = terrain.Right() ?? terrain;
    //                    newMapIndex.x -= heightMapRes;
    //                }
    //                if (newMapIndex.y > (heightMapRes - 1))
    //                {
    //                    terrain = terrain.Top() ?? terrain;
    //                    newMapIndex.y -= heightMapRes;
    //                }
    //                Vector3 worldPos = GetPositionInWorild(terrain, newMapIndex.x, newMapIndex.y);
    //                Vector3 loacalPos = localCor.World2Loacal3(worldPos);
    //                if(loacalPos.x < minLX || loacalPos.x > maxLX || loacalPos.z < minLZ || loacalPos.z > maxLZ)
    //                {
    //                    //continue;
    //                }

    //                Vector3 pos = startPos + new Vector3(j * pieceWidth, 0, i * pieceHeight);
    //                Vector3 intersection;
    //                // 判断相交
    //                if (Math3d.LineLineIntersection(out intersection, point_0, point_1 - point_0, pos, pos_3 - pos_2))
    //                {
    //                    // 存储路面撤回相关的数据
    //                    if (!road.terrains.Contains(terrain))
    //                    {
    //                        float[,] oldHM = GetHeightMap(terrain);
    //                        road.terrains.Add(terrain);
    //                        road.oldHeigtMaps.Add(oldHM);
    //                        road.isChanges.Add(new bool[heightMapRes, heightMapRes]);
    //                    }

    //                    road.isChanges[road.terrains.IndexOf(terrain)][newMapIndex.y, newMapIndex.x] = true;

    //                    // 修改HeightMap
    //                    heightMap[i, j] = GetPointHeight(GetTerrain(intersection), intersection) * deltaHeight;
    //                }
    //            }
    //        }

    //        SetHeightMap(terrain, heightMap, mapIndex.x, mapIndex.y, false);
    //    }
    //}

    ///// <summary>
    ///// 删除路的时候调用
    ///// </summary>
    ///// <param name="roadId"></param>
    //public static void RecoverRoadTerrainData(RoadsOrBridges road)
    //{
    //    if (road != null)
    //    {
    //        for (int i = 0, lenghth = road.terrains.Count; i < lenghth; i++)
    //        {
    //            bool[,] isChange = road.isChanges[i];
    //            float[,] HM = GetHeightMap(road.terrains[i]);
    //            float[,] oldHM = road.oldHeigtMaps[i];
    //            for (int j = 0; j < HM.GetLength(0); j++)
    //            {
    //                for (int k = 0; k < HM.GetLength(1); k++)
    //                {
    //                    if (isChange[j, k])
    //                        HM[j, k] = oldHM[j, k];
    //                }
    //            }
    //            SetSingleHeightMapDelayLOD(road.terrains[i], 0, 0, HM);
    //        }
    //        Refresh();
    //    }
    //}

    #endregion

    #region 修改与恢复

    /// <summary>
    /// 用于地形操作的撤回
    /// </summary>
    private static Stack<TerrainCmdData> terrainDataStack = new Stack<TerrainCmdData>();
    private static Dictionary<Terrain, float[,]> terrainDataDic = new Dictionary<Terrain, float[,]>();

    /// <summary>
    /// 添加一个记录点
    /// </summary>
    public static void AddOldData()
    {
        terrainDataStack.Push(new TerrainCmdData(terrainDataDic));
        terrainDataDic.Clear();
    }

    /// <summary>
    /// 恢复一次数据
    /// </summary>
    public static void Recover()
    {
        if (!(terrainDataStack.Count > 0))
            return;
        TerrainCmdData terrainCmdData = terrainDataStack.Pop();
        terrainCmdData.Recover();
    }

    #endregion

    #region Other Class

    /// <summary>
    /// 用于地形恢复的临时数据
    /// </summary>
    struct TerrainCmdData
    {
        public Dictionary<Terrain, float[,]> terrainDataDic;

        public TerrainCmdData(Dictionary<Terrain,float[,]> dic)
        {
            terrainDataDic = new Dictionary<Terrain, float[,]>();
            foreach (var item in dic)
            {
                terrainDataDic.Add(item.Key, item.Value);
            }
        }

        public void Recover()
        {
            foreach (var item in terrainDataDic)
            {
                item.Key.terrainData.SetHeights(0, 0, item.Value);
            }
        }
    }

    /// <summary>
    /// 高度图修改的初始化参数
    /// </summary>
    struct HMArg
    {
        public int mapRadiusX;
        public int mapRadiusZ;
        public Vector2Int centerMapIndex;
        public Vector2Int startMapIndex;
        public float[,] heightMap;
        public float limit;
    }

    #endregion
}