using System.Linq;
using UnityEngine;
using XFramework.TerrainMoudule;

public class Game : MonoBehaviour
{
    public static RuntimeTerrainEditor TerrainModule;

    private void Awake()
    {
        var brushs = Resources.LoadAll<Texture2D>("Terrain/Brushs");

        TerrainModule = new RuntimeTerrainEditor(brushs);

        var trees = Resources.LoadAll<GameObject>("Terrain/Trees");
        InitTreePrototype(trees);

        var detailes = Resources.LoadAll<Texture2D>("Terrain/Details");
        InitDetailPrototype(detailes);

        var terrainLayers = Resources.LoadAll<TerrainLayer>("Terrain/TerrainLayers");
        InitTerrainLayers(terrainLayers);
    }

    /// <summary>
    /// 初始化树木原型组
    /// </summary>
    public static void InitTreePrototype(GameObject[] treeObjs)
    {
        TreePrototype[] trees = TerrainUtility.CreatTreePrototype(treeObjs);
        Terrain[] terrains = Terrain.activeTerrains;
        for (int i = 0, length = terrains.Length; i < length; i++)
        {
            terrains[i].terrainData.treePrototypes = trees;
        }
    }

    /// <summary>
    /// 初始化细节原型组
    /// </summary>
    public static void InitDetailPrototype(Texture2D[] textures)
    {
        DetailPrototype[] details = TerrainUtility.CreateDetailPrototype(textures);

        Terrain[] terrains = Terrain.activeTerrains;
        for (int i = 0, length = terrains.Length; i < length; i++)
        {
            terrains[i].terrainData.detailPrototypes = details;
            terrains[i].detailObjectDistance = 250;  // 设置草的消失距离 
        }
    }

    /// <summary>
    /// 初始化贴图原型
    /// </summary>
    public static void InitTerrainLayers(TerrainLayer[] terrainLayers)
    {
        Terrain[] terrains = Terrain.activeTerrains;
        for (int i = 0, length = terrains.Length; i < length; i++)
        {
            terrains[i].terrainData.terrainLayers = terrainLayers;
        }
    }
}