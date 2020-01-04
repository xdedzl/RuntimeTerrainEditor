using UnityEngine;

namespace XFramework.TerrainMoudule
{
    /// <summary>
    /// 地形工具类
    /// </summary>
    public static class TerrainUtility
    {

        #region Terrain Extern Fun

        /// <summary>
        /// 右边的地形块
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static Terrain Right(this Terrain terrain)
        {
#if UNITY_2018_3_OR_NEWER
            return terrain.rightNeighbor;
#else
            Vector3 rayStart = terrain.GetPosition() + new Vector3(terrain.terrainData.size.x * 1.5f, 10000, terrain.terrainData.size.z * 0.5f);
            RaycastHit hitInfo;
            Physics.Raycast(rayStart, Vector3.down, out hitInfo, float.MaxValue, LayerMask.GetMask("Terrain"));
            return hitInfo.collider?.GetComponent<Terrain>();
#endif
        }

        /// <summary>
        /// 上边的地形块
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static Terrain Top(this Terrain terrain)
        {
#if UNITY_2018_3_OR_NEWER
            return terrain.topNeighbor;
#else
            Vector3 rayStart = terrain.GetPosition() + new Vector3(terrain.terrainData.size.x * 0.5f, 10000, terrain.terrainData.size.z * 1.5f);
            RaycastHit hitInfo;
            Physics.Raycast(rayStart, Vector3.down, out hitInfo, float.MaxValue, LayerMask.GetMask("Terrain"));
            return hitInfo.collider?.GetComponent<Terrain>();
#endif
        }

        /// <summary>
        /// 左边的地形块
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static Terrain Left(this Terrain terrain)
        {
#if UNITY_2018_3_OR_NEWER
            return terrain.leftNeighbor;
#else
            Vector3 rayStart = terrain.GetPosition() + new Vector3(-terrain.terrainData.size.x * 0.5f, 10000, terrain.terrainData.size.z * 0.5f);
            RaycastHit hitInfo;
            Physics.Raycast(rayStart, Vector3.down, out hitInfo, float.MaxValue, LayerMask.GetMask("Terrain"));
            return hitInfo.collider?.GetComponent<Terrain>();
#endif
        }

        /// <summary>
        /// 下边的地形块
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static Terrain Bottom(this Terrain terrain)
        {
#if UNITY_2018_3_OR_NEWER
            return terrain.bottomNeighbor;
#else
            Vector3 rayStart = terrain.GetPosition() + new Vector3(terrain.terrainData.size.x * 0.5f, 10000, -terrain.terrainData.size.z * 0.5f);
            RaycastHit hitInfo;
            Physics.Raycast(rayStart, Vector3.down, out hitInfo, float.MaxValue, LayerMask.GetMask("Terrain"));
            return hitInfo.collider?.GetComponent<Terrain>();
#endif
        }

        #endregion

        #region HeightMap

        /// <summary>
        /// 返回Terrain上某一点的HeightMap索引。
        /// </summary>
        /// <param name="terrain">Terrain</param>
        /// <param name="point">Terrain上的某点</param>
        /// <returns>该点在HeightMap中的位置索引</returns>
        public static Vector2Int GetHeightmapIndex(Terrain terrain, Vector3 point)
        {
            TerrainData tData = terrain.terrainData;
            float width = tData.size.x;
            float length = tData.size.z;

            // 根据相对位置计算索引
            int x = (int)((point.x - terrain.GetPosition().x) / width * tData.heightmapWidth);
            int z = (int)((point.z - terrain.GetPosition().z) / length * tData.heightmapHeight);

            return new Vector2Int(x, z);
        }

        /// <summary>
        /// 返回地图Index对应的世界坐标系位置
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 GetIndexWorldPoint(Terrain terrain, int x, int z, float y)
        {
            TerrainData data = terrain.terrainData;
            float _x = data.size.x / (data.heightmapWidth - 1) * x;
            float _z = data.size.z / (data.heightmapHeight - 1) * z;
            return new Vector3(_x, terrain.terrainData.size.y * y, _z) + terrain.GetPosition();
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
        /// 获取当前位置在地图上的对应位置
        /// </summary>
        /// <returns></returns>
        public static bool GetPositionOnTerrain(Terrain terrain, Vector3 pos, out Vector3 posOnTerrain)
        {
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
        /// 获取当前位置在地图上的对应位置
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetPositionOnTerrain(Terrain terrain, Vector3 pos)
        {
            if (terrain != null)
            {
                return pos.WithY(terrain.SampleHeight(pos));
            }
            else
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// 获取当前位置在地图上的对应位置
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="posOnTerrain"></param>
        /// <returns></returns>
        public static bool GetPositionOnTerrain(Vector3 pos, out Vector3 posOnTerrain)
        {
            if (Physics.Raycast(pos + Vector3.up * 10000, Vector3.down, out RaycastHit hitInfo, float.MaxValue, LayerMask.GetMask("Terrain")))
            {
                posOnTerrain = hitInfo.point;
                return true;
            }
            else
            {
                posOnTerrain = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// 获取当前位置在地图上的对应位置
        /// </summary>
        public static Vector3 GetPositionOnTerrain(Vector3 pos)
        {
            Physics.Raycast(pos + Vector3.up * 10000, Vector3.down, out RaycastHit hitInfo, float.MaxValue, LayerMask.GetMask("Terrain"));
            if (hitInfo.collider)
            {
                return hitInfo.point;
            }
            else
            {
                return Vector3.zero;
            }
        }

        #endregion

        #region AlphaMap

        /// <summary>
        /// 返回Terrain上某一点的AlphalMap索引。
        /// </summary>
        /// <param name="terrain">Terrain</param>
        /// <param name="point">Terrain上的某点</param>
        /// <returns>该点在DetialMap中的位置索引</returns>
        public static Vector2Int GetAlphaMapIndex(Terrain terrain, Vector3 point)
        {
            TerrainData tData = terrain.terrainData;
            float width = tData.size.x;
            float length = tData.size.z;

            // 根据相对位置计算索引
            int x = (int)((point.x - terrain.GetPosition().x) / width * tData.alphamapWidth);
            int z = (int)((point.z - terrain.GetPosition().z) / length * tData.alphamapHeight);

            return new Vector2Int(x, z);
        }

        /// <summary>
        /// 获取地形的AlphaMap
        /// </summary>
        /// <param name="terrain">地形</param>
        /// <param name="xBase">x轴起始索引</param>
        /// <param name="yBase">y轴起始索引</param>
        /// <param name="width">x轴长度</param>
        /// <param name="height">y轴长度</param>
        /// <returns></returns>
        public static float[,,] GetAlphaMap(Terrain terrain, int xBase = 0, int yBase = 0, int width = 0, int height = 0)
        {
            if (xBase + yBase + width + height == 0)
            {
                width = height = terrain.terrainData.detailResolution;
                return terrain.terrainData.GetAlphamaps(xBase, yBase, width, height);
            }

            TerrainData terrainData = terrain.terrainData;
            int differX = xBase + width - terrainData.alphamapResolution;
            int differY = yBase + height - terrainData.alphamapResolution;
            float[,,] ret;
            if (differX <= 0 && differY <= 0)  // 无溢出
            {
                ret = terrain.terrainData.GetAlphamaps(xBase, yBase, width, height);
            }
            else if (differX > 0 && differY <= 0) // 右边溢出
            {
                ret = terrain.terrainData.GetAlphamaps(xBase, yBase, width - differX, height);
                float[,,] right = terrain.Right()?.terrainData.GetAlphamaps(0, yBase, differX, height);
                if (right != null)
                    ret = ret.Concat1(right);
            }
            else if (differX <= 0 && differY > 0)  // 上边溢出
            {
                ret = terrain.terrainData.GetAlphamaps(xBase, yBase, width, height - differY);
                float[,,] up = terrain.Top()?.terrainData.GetAlphamaps(xBase, 0, width, differY);
                if (up != null)
                    ret = ret.Concat0(up);
            }
            else // 上右均溢出
            {
                ret = terrain.terrainData.GetAlphamaps(xBase, yBase, width - differX, height - differY);
                float[,,] right = terrain.Right()?.terrainData.GetAlphamaps(0, yBase, differX, height - differY);
                float[,,] up = terrain.Top()?.terrainData.GetAlphamaps(xBase, 0, width - differX, differY);
                float[,,] upRight = terrain.Right()?.Top()?.terrainData.GetAlphamaps(0, 0, differX, differY);

                if (right != null)
                    ret = ret.Concat1(right);
                if (upRight != null)
                    ret = ret.Concat0(up.Concat0(upRight));
            }

            return ret;
        }

        /// <summary>
        /// 设置细节数据
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="alphaMap"></param>
        /// <param name="xBase"></param>
        /// <param name="yBase"></param>
        /// <param name="layer"></param>
        public static void SetAlphaMap(Terrain terrain, float[,,] alphaMap, int xBase, int yBase)
        {
            TerrainData terrainData = terrain.terrainData;
            int length_1 = alphaMap.GetLength(1);
            int length_0 = alphaMap.GetLength(0);

            int length_2 = alphaMap.GetLength(2);

            int differX = xBase + length_1 - (terrainData.alphamapResolution);
            int differY = yBase + length_0 - (terrainData.alphamapResolution);

            if (differX <= 0 && differY <= 0) // 无溢出
            {
                terrain.terrainData.SetAlphamaps(xBase, yBase, alphaMap);
            }
            else if (differX > 0 && differY <= 0) // 右溢出
            {
                terrain.terrainData.SetAlphamaps(xBase, yBase, alphaMap.GetPart(0, 0, 0, length_0, length_1 - differX, length_2));
                terrain.Right()?.terrainData.SetAlphamaps(0, yBase, alphaMap.GetPart(0, length_1 - differX, 0, length_0, differX, length_2));
            }
            else if (differX <= 0 && differY > 0) // 上溢出
            {
                terrain.terrainData.SetAlphamaps(xBase, yBase, alphaMap.GetPart(0, 0, 0, length_0 - differY, length_1, length_2));
                terrain.Top()?.terrainData.SetAlphamaps(xBase, 0, alphaMap.GetPart(length_0 - differY, 0, 0, differY, length_1, length_2));
            }
            else  // 右上均溢出
            {
                terrain.terrainData.SetAlphamaps(xBase, yBase, alphaMap.GetPart(0, 0, 0, length_0 - differY, length_1 - differX, length_2));
                terrain.Right()?.terrainData.SetAlphamaps(0, yBase, alphaMap.GetPart(0, length_1 - differX, 0, length_0 - differY, differX, length_2));
                terrain.Top()?.terrainData.SetAlphamaps(xBase, 0, alphaMap.GetPart(length_0 - differY, 0, 0, differY, length_1 - differX, length_2));
                terrain.Top()?.Right().terrainData.SetAlphamaps(0, 0, alphaMap.GetPart(length_0 - differY, length_1 - differX, 0, differY, differX, length_2));
            }
        }

        #endregion

        #region DetailMap

        /// <summary>
        /// 返回Terrain上某一点的DetialMap索引
        /// </summary>
        /// <param name="terrain">Terrain</param>
        /// <param name="point">Terrain上的某点</param>
        /// <returns>该点在DetialMap中的位置索引</returns>
        public static Vector2Int GetDetialMapIndex(Terrain terrain, Vector3 point)
        {
            TerrainData tData = terrain.terrainData;
            float width = tData.size.x;
            float length = tData.size.z;

            // 根据相对位置计算索引
            int x = (int)((point.x - terrain.GetPosition().x) / width * tData.detailWidth);
            int z = (int)((point.z - terrain.GetPosition().z) / length * tData.detailHeight);

            return new Vector2Int(x, z);
        }

        /// <summary>
        /// 获取细节数据
        /// </summary>
        public static int[,] GetDetailLayer(Terrain terrain, int xBase = 0, int yBase = 0, int width = 0, int height = 0, int layer = 0)
        {
            if (xBase + yBase + width + height == 0)
            {
                width = height = terrain.terrainData.detailResolution;
                return terrain.terrainData.GetDetailLayer(xBase, yBase, width, height, layer);
            }

            TerrainData terrainData = terrain.terrainData;
            int differX = xBase + width - terrainData.detailResolution;
            int differY = yBase + height - terrainData.detailResolution;
            int[,] ret;
            if (differX <= 0 && differY <= 0)  // 无溢出
            {
                ret = terrain.terrainData.GetDetailLayer(xBase, yBase, width, height, layer);
            }
            else if (differX > 0 && differY <= 0) // 右边溢出
            {
                ret = terrain.terrainData.GetDetailLayer(xBase, yBase, width - differX, height, layer);
                int[,] right = terrain.Right()?.terrainData.GetDetailLayer(0, yBase, differX, height, layer);
                if (right != null)
                    ret = ret.Concat1(right);
            }
            else if (differX <= 0 && differY > 0)  // 上边溢出
            {
                ret = terrain.terrainData.GetDetailLayer(xBase, yBase, width, height - differY, layer);
                int[,] up = terrain.Top()?.terrainData.GetDetailLayer(xBase, 0, width, differY, layer);
                if (up != null)
                    ret = ret.Concat0(up);
            }
            else // 上右均溢出
            {
                ret = terrain.terrainData.GetDetailLayer(xBase, yBase, width - differX, height - differY, layer);
                int[,] right = terrain.Right()?.terrainData.GetDetailLayer(0, yBase, differX, height - differY, layer);
                int[,] up = terrain.Top()?.terrainData.GetDetailLayer(xBase, 0, width - differX, differY, layer);
                int[,] upRight = terrain.Right()?.Top()?.terrainData.GetDetailLayer(0, 0, differX, differY, layer);

                if (right != null)
                    ret = ret.Concat1(right);
                if (upRight != null)
                    ret = ret.Concat0(up.Concat1(upRight));
            }

            return ret;
        }

        /// <summary>
        /// 设置细节数据
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="detailMap"></param>
        /// <param name="xBase"></param>
        /// <param name="yBase"></param>
        /// <param name="layer"></param>
        public static void SetDetailLayer(Terrain terrain, int[,] detailMap, int xBase, int yBase, int layer)
        {
            TerrainData terrainData = terrain.terrainData;
            int length_1 = detailMap.GetLength(1);
            int length_0 = detailMap.GetLength(0);

            int differX = xBase + length_1 - (terrainData.detailResolution);
            int differY = yBase + length_0 - (terrainData.detailResolution);

            if (differX <= 0 && differY <= 0) // 无溢出
            {
                terrain.terrainData.SetDetailLayer(xBase, yBase, layer, detailMap);
            }
            else if (differX > 0 && differY <= 0) // 右溢出
            {
                terrain.terrainData.SetDetailLayer(xBase, yBase, layer, detailMap.GetPart(0, 0, length_0, length_1 - differX));
                terrain.Right()?.terrainData.SetDetailLayer(0, yBase, layer, detailMap.GetPart(0, length_1 - differX, length_0, differX));
            }
            else if (differX <= 0 && differY > 0) // 上溢出
            {
                terrain.terrainData.SetDetailLayer(xBase, yBase, layer, detailMap.GetPart(0, 0, length_0 - differY, length_1));
                terrain.Top()?.terrainData.SetDetailLayer(xBase, 0, layer, detailMap.GetPart(length_0 - differY, 0, differY, length_1));
            }
            else  // 右上均溢出
            {
                terrain.terrainData.SetDetailLayer(xBase, yBase, layer, detailMap.GetPart(0, 0, length_0 - differY, length_1 - differX));
                terrain.Right()?.terrainData.SetDetailLayer(0, yBase, layer, detailMap.GetPart(0, length_1 - differX, length_0 - differY, differX));
                terrain.Top()?.terrainData.SetDetailLayer(xBase, 0, layer, detailMap.GetPart(length_0 - differY, 0, differY, length_1 - differX));
                terrain.Top()?.Right().terrainData.SetDetailLayer(0, 0, layer, detailMap.GetPart(length_0 - differY, length_1 - differX, differY, differX));
            }
        }

        #endregion

        #region Tools

        /// <summary>
        /// 通过树的预制体创建树模板
        /// </summary>
        /// <param name="treeObjs">树预制体</param>
        /// <returns>模板</returns>
        public static TreePrototype[] CreatTreePrototype(GameObject[] treeObjs)
        {
            TreePrototype[] trees = new TreePrototype[treeObjs.Length];
            for (int i = 0, length = treeObjs.Length; i < length; i++)
            {
                trees[i] = new TreePrototype();
                trees[i].prefab = treeObjs[i];
            }
            return trees;
        }

        /// <summary>
        /// 通过贴图创建细节模板
        /// </summary>
        /// <param name="textures">贴图</param>
        /// <returns>模板</returns>
        public static DetailPrototype[] CreateDetailPrototype(Texture2D[] textures)
        {
            DetailPrototype[] details = new DetailPrototype[textures.Length];

            for (int i = 0, length = details.Length; i < length; i++)
            {
                details[i] = new DetailPrototype();
                details[i].prototypeTexture = textures[i];
                details[i].minWidth = 1f;
                details[i].maxWidth = 2f;
                details[i].maxHeight = 0.2f;
                details[i].maxHeight = 0.8f;
                details[i].noiseSpread = 1f;
                details[i].healthyColor = Color.green;
                details[i].dryColor = Color.green;
                details[i].renderMode = DetailRenderMode.GrassBillboard;
            }

            return details;
        }

        /// <summary>
        /// 获取一个树的实例
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static TreeInstance GetTreeInstance(int index)
        {
            // 设置新添加的树的参数
            TreeInstance instance = new TreeInstance
            {
                prototypeIndex = index,
                color = Color.white,
                lightmapColor = Color.white,
                widthScale = 1,
                heightScale = 1
            };
            return instance;
        }

        /// <summary>
        /// 获取地形上某一位置的坡度(返回值在0~70, 可操作)
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static float GetSeepness(Vector3 pos)
        {
            Terrain terrain = GetTerrain(pos);
            Vector3 differ = pos - terrain.GetPosition();

            if (terrain)
                return terrain.terrainData.GetSteepness(differ.x / terrain.terrainData.size.x, differ.z / terrain.terrainData.size.z);
            else
                return 0;
        }

        /// <summary>
        /// 获取对应位置的Terrain
        /// </summary>
        public static Terrain GetTerrain(Vector3 pos)
        {
            pos.y += 10000;
            RaycastHit hit = Utility.SendRayDown(pos, LayerMask.GetMask("Terrain"));
            if (!hit.Equals(default(RaycastHit)))
            {
                Terrain terrain = hit.collider.GetComponent<Terrain>();
                return terrain;
            }
            return null;
        }

        /// <summary>
        /// 配置场景中的Terrain索引
        /// </summary>
        public static Terrain[,] GetActiveTerrains()
        {
            Terrain[] activeTerrains = Terrain.activeTerrains;
            if (activeTerrains.Length == 0)
            {
                return new Terrain[0, 0];
            }
            Vector3 terrainSize = activeTerrains[0].terrainData.size;
            float halfX = terrainSize.x / 2;
            float halfZ = terrainSize.z / 2;
            float x = 0, z = 0;
            Vector3 excursionPos = activeTerrains[0].GetPosition();
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
            Terrain[,] terrains = new Terrain[(int)((x - excursionPos.x + halfX) / terrainSize.x) + 1, (int)((z - excursionPos.z + halfZ) / terrainSize.z) + 1];

            int u, v;

            for (int i = 0; i < activeTerrains.Length; i++)
            {
                u = (int)((activeTerrains[i].GetPosition().x - excursionPos.x + halfX) / terrainSize.x);
                v = (int)((activeTerrains[i].GetPosition().z - excursionPos.z + halfZ) / terrainSize.z);
                terrains[u, v] = activeTerrains[i];
            }

            return terrains;
        }

        /// <summary>
        /// 处理地形接缝
        /// </summary>
        public static void HandleConnect()
        {
            if (Terrain.activeTerrain == null)
            {
                return;
            }
            Terrain[,] terrains = GetActiveTerrains();
            int res = terrains[0, 0].terrainData.heightmapResolution;

            foreach (var terrain in terrains)
            {
                var right = terrain.Right();
                if (right != null)
                {
                    float[,] h = right.terrainData.GetHeights(0, 0, 1, res);
                    terrain.terrainData.SetHeights(res - 1, 0, h);
                }

                var bottom = terrain.Bottom();
                if (bottom != null)
                {
                    float[,] h = bottom.terrainData.GetHeights(0, res - 1, res, 1);
                    terrain.terrainData.SetHeights(0, 0, h);
                }
            }

            float[,][,] heights = new float[terrains.GetLength(0), terrains.GetLength(1)][,];

            for (int i = 0; i < terrains.GetLength(0); i++)
            {
                for (int j = 0; j < terrains.GetLength(1); j++)
                {
                    heights[i, j] = terrains[i, j].terrainData.GetHeights(0, 0, res, res);
                }
            }
        }

        #endregion
    }
}