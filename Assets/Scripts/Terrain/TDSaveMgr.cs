using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class TDSaveMgr : Singleton<TDSaveMgr>
{
    /// <summary>
    /// 数据保存路径
    /// </summary>
    private readonly string savePath;
    /// <summary>
    /// 有数据就创建地形
    /// </summary>
    private Queue<TDData> tDDatas;
    /// <summary>
    /// TerrainData的三种模板数据
    /// </summary>
    private DetailPrototype[] details;
    private TreePrototype[] trees;
    private TerrainLayer[] splats;

    /// <summary>
    /// 所有Terrain实体的父物体
    /// </summary>
    private Transform terrainParent;
    /// <summary>
    /// 地图信息数据头文件
    /// </summary>
    private TDHead head;

    /// <summary>
    /// 任务量
    /// </summary>
    private float taskCount;
    /// <summary>
    /// 已完成的任务量
    /// </summary>
    private float runedCount;

    /// <summary>
    /// 还未创建的Terrain数量
    /// </summary>
    private int terrainCountToCreate = -1;
    /// <summary>
    /// 地形创建完成后要执行的任务
    /// </summary>
    private Action RunAfterCreateTerrain;

    public TDSaveMgr()
    {

//#if UNITY_EDITOR
//        savePath = "D:/Test/TerrainData";
//#else
        savePath = Application.streamingAssetsPath + "/TerrainData";
//#endif

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        tDDatas = new Queue<TDData>();
    }

    /// <summary>
    /// 在Update中处理加载完的数据
    /// </summary>
    private void Update()
    {
        int index = 0;
        while (tDDatas.Count > 0 && index < 3)
        {
            lock (tDDatas)
            {
                // 取出一个数据并创建
                CreateTerrain(tDDatas.Dequeue());
            }
            index++;
            terrainCountToCreate--;
        }

        if (terrainCountToCreate == 0 && IsDown)
        {
            RunAfterCreateTerrain?.Invoke();
            MonoEvent.Instance.UPDATE -= Update;
            terrainCountToCreate = -1;
        }
        if (GetProgress() > 0.99)
            IsDown = true;
    }

    #region 自定义格式存储数据

    /// <summary>
    /// 保存和地图有关的全部数据
    /// </summary>
    public void SaveTerrainInfo(Terrain[] terrains)
    {
        string folderName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        if (!Directory.Exists(savePath + "/" + folderName))
        {
            Directory.CreateDirectory(savePath + "/" + folderName);
        }

        SaveTerrainsData(terrains, folderName);
    }

    /// <summary>
    /// 读取和地图有关的全部数据
    /// </summary>
    /// <param name="folder"></param>
    public void ReadTerrainInfo(string folder)
    {
        RunAfterCreateTerrain += () =>
        {
            TerrainUtility.ConfigActiveTerrains();
            TerrainUtility.ConfigTerrainData();
            RunAfterCreateTerrain = null;
        };
        ReadTerrainData(folder);
    }

    #region TD数据保存/读取

    /// <summary>
    /// 保存一组地图，外界可传入Terrain.activeTerrains
    /// </summary>
    /// <param name="terrains"></param>
    private void SaveTerrainsData(Terrain[] terrains, string folderName)
    {
        // 保存地形基础数据
        SaveTDHead(savePath + "/" + folderName);

        // 保存地形数据
        taskCount = terrains.Length;
        runedCount = 0;
        for (int i = 0; i < terrains.Length; i++)
        {
            SaveTerrainData(terrains[i], folderName, i.ToString());
        }
    }

    /// <summary>
    /// 将所有地形的相同基础数据保存为头文件
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveTDHead(string path)
    {
        Terrain terrain = Terrain.activeTerrain;
        TDHead tDHead = new TDHead
        {
            terrainSize = terrain.terrainData.size,
            ResolutionSize = terrain.terrainData.detailResolution,
            heightmapPixelError = terrain.heightmapPixelError,
            basemapDistance = terrain.basemapDistance,
            drawHeightmap = terrain.drawHeightmap,
        };
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddVector3(tDHead.terrainSize);
        protocol.AddInt32(tDHead.ResolutionSize);
        protocol.AddFloat(tDHead.heightmapPixelError);
        protocol.AddFloat(tDHead.basemapDistance);
        protocol.AddBoolen(tDHead.drawHeightmap);
        File.WriteAllBytes(path + "/TerrainData.tdHead", protocol.Encode());
    }

    /// <summary>
    /// 保存单个地形数据
    /// </summary>
    private void SaveTerrainData(Terrain terrain, string folderName, string fileName)
    {
        TerrainData terrainData = terrain.terrainData;

        // 新建一个TDData并赋值
        TDData tDData = new TDData();
        tDData.name = terrain.name;
        tDData.terrainPos = terrain.GetPosition();
        tDData.heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        int[][,] detailMap = new int[terrainData.detailPrototypes.Length][,];
        for (int i = 0; i < detailMap.Length; i++)
        {
            detailMap[i] = terrainData.GetDetailLayer(0, 0, terrainData.detailResolution, terrainData.detailResolution, i);
        }
        tDData.detailMap = detailMap;
        tDData.alphaMap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution);

        Vector3[][] treePoses = new Vector3[terrainData.treePrototypes.Length][];
        List<Vector3>[] temp = new List<Vector3>[treePoses.Length];
        for (int i = 0; i < treePoses.Length; i++)
        {
            temp[i] = new List<Vector3>();
        }
        foreach (var item in terrainData.treeInstances)
        {
            temp[item.prototypeIndex].Add(item.position);
        }
        for (int i = 0; i < treePoses.Length; i++)
        {
            treePoses[i] = temp[i].ToArray();
        }
        tDData.treePoses = treePoses;
        runedCount += 0.2f;

        //序列化TDDate并保存
        Task.Run(() =>
        {
            runedCount += 0.2f;
            byte[] data = null;
            data = TDDataSerialize(tDData);
         
            runedCount += 0.4f;
            data = IOUtility.Compress(data);
            File.WriteAllBytes(savePath + "/" + folderName + "/" + fileName + ".terrainData", data);
            runedCount += 0.2f;
    });
    }

    /// <summary>
    /// 读取文件夹下所有的Terrain数据
    /// </summary>
    /// <param name="folderName"></param>
    private void ReadTerrainData(string folderName)
    {
        MonoEvent.Instance.UPDATE += Update;
        InitPrototype();

        ReadTDHead(savePath + "/" + folderName, "TerrainData");

        terrainParent = new GameObject("Terrains").transform;
        string[] fileNames = Directory.GetFiles(savePath + "/" + folderName, "*.terrainData", SearchOption.TopDirectoryOnly);
        taskCount = fileNames.Length;
        terrainCountToCreate = fileNames.Length;
        runedCount = 0;

        for (int i = 0; i < fileNames.Length; i++)
        {
            int index = i;    // 不能省
            Task.Run(() =>
            {
                ReadTDData(fileNames[index]);
            });
        }
    }

    /// <summary>
    /// 读取地形基础数据
    /// </summary>
    private void ReadTDHead(string path, string fileName)
    {
        byte[] buffer = File.ReadAllBytes(path + "/" + fileName + ".tdHead");
        ProtocolBytes reader = new ProtocolBytes(buffer);
        head.terrainSize = reader.GetVector3();
        head.ResolutionSize = reader.GetInt32();
        head.heightmapPixelError = reader.GetFloat();
        head.basemapDistance = reader.GetFloat();
        head.drawHeightmap = reader.GetBoolen();
    }

    /// <summary>
    /// 读取一张地图的数据
    /// </summary>
    private void ReadTDData(string fileName)
    {
        byte[] data = File.ReadAllBytes(fileName);
        data = IOUtility.Decompress(data);
        runedCount += 0.2f;
        TDData tDData = TDDataDeserialize(data);
        lock (tDDatas)
        {
            tDDatas.Enqueue(tDData);
        }
        runedCount += 0.8f;
    }

    #endregion

    /// <summary>
    /// 创建单块地图
    /// </summary>
    /// <param name="tdData"></param>
    private void CreateTerrain(TDData tdData)
    {
        // 设置TerrainData的基础参数
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = head.ResolutionSize + 1;
        terrainData.SetDetailResolution(head.ResolutionSize, 8);
        terrainData.alphamapResolution = head.ResolutionSize * 2;
        terrainData.baseMapResolution = head.ResolutionSize;
        terrainData.size = head.terrainSize;

        terrainData.detailPrototypes = details;
        terrainData.treePrototypes = trees;
        terrainData.terrainLayers = splats;
        terrainData.RefreshPrototypes();

        // 高度，贴图，细节设置
        if (tdData.heightMap != null)
        {
            terrainData.SetHeights(0, 0, tdData.heightMap);
        }

        if (tdData.detailMap != null)
        {
            for (int i = 0; i < tdData.detailMap.Length; i++)
            {
                terrainData.SetDetailLayer(0, 0, i, tdData.detailMap[i]);
            }
        }

        if(tdData.alphaMap != null)
        {
            terrainData.SetAlphamaps(0, 0, tdData.alphaMap);
        }

        // 在场景中创建Terrain实体并设置实体的参数
        GameObject newTerrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
        newTerrainGameObject.name = terrainData.name;
        newTerrainGameObject.isStatic = false;
        newTerrainGameObject.transform.position = tdData.terrainPos;
        newTerrainGameObject.layer = LayerMask.NameToLayer("Terrain");

        // 设置Terrain类的参数
        Terrain terrain = newTerrainGameObject.GetComponent<Terrain>();
        terrain.heightmapPixelError = head.heightmapPixelError;
        terrain.basemapDistance = head.basemapDistance;
        terrain.drawHeightmap = head.drawHeightmap;
        terrain.name = tdData.name;

        if (tdData.treePoses != null)
        {
            // 为地形添加树木实体
            for (int index = 0; index < tdData.treePoses.Length; index++)
            {
                for (int i = 0; i < tdData.treePoses[index].Length; i++)
                {
                    TreeInstance instance = TerrainUtility.GetTreeInstance(index);
                    instance.position = tdData.treePoses[index][i];
                    terrain.AddTreeInstance(instance);
                }
            }
        }

        newTerrainGameObject.transform.SetParent(terrainParent);
    }

    /// <summary>
    /// 初始化原型模板
    /// </summary>
    private void InitPrototype()
    {
        details = TerrainUtility.CreateDetailPrototype();
        trees = TerrainUtility.CreatTreePrototype();
        splats = TerrainUtility.CreateSplatPrototype();
    }

    #endregion


    #region Serialize/Deserialize

    /// <summary>
    /// TDData序列化
    /// </summary>
    private byte[] TDDataSerialize(TDData tDData)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString(tDData.name);
        protocol.AddVector3(tDData.terrainPos);
        protocol.AddFloatArray2(tDData.heightMap);

        protocol.AddInt32(tDData.detailMap.Length);
        for (int i = 0; i < tDData.detailMap.Length; i++)
        {
            protocol.AddIntArray2(tDData.detailMap[i]);
        }
        protocol.AddFloatArray3(tDData.alphaMap);

        protocol.AddInt32(tDData.treePoses.Length);
        for (int i = 0; i < tDData.treePoses.Length; i++)
        {
            protocol.AddInt32(tDData.treePoses[i].Length);
            for (int j = 0; j < tDData.treePoses[i].Length; j++)
            {
                protocol.AddVector3(tDData.treePoses[i][j]);
            }
        }
        return protocol.Encode();
    }

    /// <summary>
    /// TDData反序列化
    /// </summary>
    private TDData TDDataDeserialize(byte[] buffer)
    {
        ProtocolBytes protocol = new ProtocolBytes(buffer);
        TDData tDData = new TDData
        {
            name = protocol.GetString(),
            terrainPos = protocol.GetVector3(),
            heightMap = protocol.GetFloatArray2()
        };

        int detailCount = protocol.GetInt32();
        for (int i = 0; i < detailCount; i++)
        {
            protocol.GetIntArray2();
        }
        tDData.alphaMap = protocol.GetFloatArray3();

        int treePrototypeCount = protocol.GetInt32();
        Vector3[][] treePoses = new Vector3[treePrototypeCount][];
        for (int i = 0; i < treePrototypeCount; i++)
        {
            int treeCount = protocol.GetInt32();
            Vector3[] poses = new Vector3[treeCount];
            for (int j = 0; j < treeCount; j++)
            {
                poses[j] = protocol.GetVector3();
            }
            treePoses[i] = poses;
        }
        tDData.treePoses = treePoses;

        return tDData;
    }

    #endregion

    // -------------- Tools ----------------- //

    /// <summary>
    /// 获取所有地图数据的文件夹名(不含拓展名)
    /// </summary>
    public string[] GetTDFolderName()
    {
        string[] fileNames = Directory.GetDirectories(savePath, "*", SearchOption.TopDirectoryOnly);
        for (int i = 0, length = fileNames.Length; i < length; i++)
        {
            fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
        }
        return fileNames;
    }

    // -------------- 接口实现 -----------------

    public float GetProgress()
    {
        return runedCount / taskCount;
    }
    public bool IsDown { get; set; }

    struct TDData
    {
        /// <summary>
        /// 地图名
        /// </summary>
        public string name;
        /// <summary>
        /// 地形位置
        /// </summary>
        public Vector3 terrainPos;
        /// <summary>
        /// 高度图
        /// </summary>
        public float[,] heightMap;
        /// <summary>
        /// 细节数据
        /// </summary>
        public int[][,] detailMap;
        /// <summary>
        /// 贴图数据
        /// </summary>
        public float[,,] alphaMap;
        /// <summary>
        /// 树的数据
        /// 第一个Vector3数组表示模板索引为1的所有树的位置
        /// </summary>
        public Vector3[][] treePoses;
    }

    struct TDHead
    {
        public Vector3 terrainSize;
        public int ResolutionSize;
        public float heightmapPixelError;
        public float basemapDistance;
        public bool drawHeightmap;
    }


    #region 利用图片存储数据  暂时放弃

    /// <summary>
    /// 资源贴图转二进制图片
    /// </summary>
    /// <param name="height"></param>
    /// <returns></returns>
    public byte[] Texture2DToPicture(float[,] height)
    {
        Color[] colors = new Color[height.GetLength(0) * height.GetLength(1)];
        int index = 0;
        for (int i = 0; i < height.GetLength(0); i++)
        {
            for (int j = 0; j < height.GetLength(1); j++)
            {
                colors[index] = Color.blue;
                colors[index].b = height[i, j];
                index++;
            }
        }
        Texture2D tex;
        Texture2D temp = Resources.Load("Terrain/Brushs/brush_0") as Texture2D;
        tex = new Texture2D(height.GetLength(0), height.GetLength(1), TextureFormat.ARGB32, false);
        tex.SetPixels(0, 0, height.GetLength(0), height.GetLength(1), colors);

        byte[] dataByte = tex.EncodeToJPG();

        File.WriteAllBytes(savePath + "/" + 1 + ".jpg", dataByte);
        Debug.Log("byte数组大小" + dataByte.Length);
        return dataByte;
    }

    /// <summary>
    /// 二进制图片转资源贴图
    /// </summary>
    /// <param name="pictureByte"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public float[,] PictureToTexture(byte[] pictureByte, int length)
    {
        byte[] array = File.ReadAllBytes(savePath + "/" + 1 + ".jpg");
        int index = 0;
        Texture2D tex = new Texture2D(length, length);
        tex.LoadImage(pictureByte, false);
        Color[] newColor = tex.GetPixels();
        float[,] newHeight = new float[length, length];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                newHeight[i, j] = newColor[index].b;
                index++;
            }
        }
        return newHeight;
    }

    #endregion
}