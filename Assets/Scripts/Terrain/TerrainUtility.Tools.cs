using UnityEngine;

public static partial class TerrainUtility
{
    #region  工具

    /// <summary>
    /// 左下角地图相对00点的偏移
    /// </summary>
    private static Vector3 excursionPos = Vector2.zero;
    private static Terrain[,] terrains;

    /// <summary>
    /// 配置场景中的Terrain索引
    /// </summary>
    public static void ConfigActiveTerrains()
    {
        Terrain[] activeTerrains = Terrain.activeTerrains;
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
    public static Terrain GetTerrain(Vector3 pos)
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
    public static bool GetPositionOnTerrain(Vector3 pos, out Vector3 posOnTerrain)
    {
        //RaycastHit hitInfo;
        //Physics.Raycast(pos + Vector3.up * 10000, Vector3.down, out hitInfo, float.MaxValue, LayerMask.GetMask("Terrain"));
        //if (hitInfo.collider)
        //{
        //    posOnTerrain = hitInfo.point;
        //    return true;
        //}
        //else
        //{
        //    posOnTerrain = Vector3.zero;
        //    return false;
        //}

        Terrain terrain = GetTerrain(pos);
        if (terrain != null)
        {
            posOnTerrain = pos.WithY(terrain.SampleHeight(pos));
            return true;
        }
        else
        {
            posOnTerrain = Vector3.zero;
            return false;
        }
    }

    /// <summary>
    /// 获取当前Index对应世界空间的坐标
    /// </summary>
    /// <param name="terrain"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector3 GetPositionInWorild(Terrain terrain, int x, int y)
    {
        Vector3 pos = terrain.GetPosition();
        pos.x += x * pieceWidth;
        pos.z += y * pieceHeight;
        pos.y = terrain.SampleHeight(pos);
        return pos;
    }

    /// <summary>
    /// 返回GameObject在Terrain上的相对（于Terrain的）位置。
    /// </summary>
    /// <param name="terrain">Terrain</param>
    /// <param name="go">GameObject</param>
    /// <returns>相对位置</returns>
    public static Vector3 GetRelativePosition(Terrain terrain, GameObject go)
    {
        return go.transform.position - terrain.GetPosition();
    }

    /// <summary>
    /// 返回Terrain上指定点在世界坐标系下的高度。
    /// </summary>
    /// <param name="terrain">Terrain</param>
    /// <param name="point">Terrain上的某点</param>
    /// <param name="vertex">true: 获取最近顶点高度  false: 获取实际高度</param>
    /// <returns>点在世界坐标系下的高度</returns>
    public static float GetPointHeight(Terrain terrain, Vector3 point, bool vertex = false)
    {
        // 对于水平面上的点来说，vertex参数没有影响
        if (vertex)
        {
            // GetHeight得到的是离点最近的顶点的高度
            Vector2Int index = GetHeightmapIndex(terrain, point);
            return terrain.terrainData.GetHeight(index.x, index.y);
        }
        else
        {
            // SampleHeight得到的是点在斜面上的实际高度
            return terrain.SampleHeight(point);
        }
    }

    /// <summary>
    /// 获取地形区域范围
    /// 返回一个长度为4的数组，分别表示地图左下角x值，地图左下角z值，地图在x轴上的宽度，地图在z轴上的宽度
    /// </summary>
    /// <returns></returns>
    public static float[] GetTerrainArea()
    {
        float[] f = new float[4];
        f[0] = terrains[0, 0].GetPosition().x;
        f[1] = terrains[0, 0].GetPosition().z;
        f[2] = terrains.GetLength(0) * terrainSize.x;
        f[3] = terrains.GetLength(1) * terrainSize.z;
        return f;
    }

    /// <summary>
    /// 获取地形上某一位置的坡度值
    /// </summary>
    /// <param name="pos"></param>
    public static float GetSeepness(Vector3 pos)
    {
        Terrain terrain = GetTerrain(pos);
        Vector3 differ = pos - terrain.GetPosition();

        if (terrain)
            return terrain.terrainData.GetSteepness(differ.x / terrainSize.x, differ.y / terrainSize.z);
        else
            return 0;
    }

    #endregion
}