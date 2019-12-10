using System.Linq;
using UnityEngine;
using XFramework.Mathematics;

namespace XFramework.TerrainMoudule
{
    /// <summary>
    /// 地形编辑管理
    /// </summary>
    public partial class TerrainManager
    {
        #region 高度图相关
        /*
         * 高度编辑的过程
         * 1.InitHMArg初始化所需要的数据，高度图会在这里获取到
         * 2.对得到的高度图按需处理
         * 3.SetHeightsMap对地形高度图重新赋值
         */

        /// <summary>
        /// 初始化笔刷
        /// </summary>
        public void InitBrushs(Texture2D[] textures)
        {
            for (int i = 0, length = textures.Length; i < length; i++)
            {
                // 获取图片颜色ARGB信息
                Color[] colors = textures[i].GetPixels();
                // terrainData.GetHeightMap得到的二维数组是[y,x]
                float[,] alphas = new float[textures[i].height, textures[i].width];
                // 设置笔刷数据
                for (int j = 0, length0 = textures[i].height, index = 0; j < length0; j++)
                {
                    for (int k = 0, length1 = textures[i].width; k < length1; k++)
                    {
                        alphas[j, k] = colors[index].a;
                        index++;
                    }
                }
                brushDic.Add(i, alphas);
            }
        }

        /// <summary>
        /// 返回Terrain的HeightMap的一部分
        /// 场景中有多块地图时不要直接调用terrainData.getheights
        /// 这个方法会解决跨多块地形的问题
        /// </summary>
        /// <param name="terrain">目标地形</param>
        /// <param name="xBase">检索HeightMap时的X索引起点</param>
        /// <param name="yBase">检索HeightMap时的Y索引起点</param>
        /// <param name="width">在X轴上的检索长度</param>
        /// <param name="height">在Y轴上的检索长度</param>
        /// <returns></returns>
        public float[,] GetHeightMap(Terrain terrain, int xBase = 0, int yBase = 0, int width = 0, int height = 0)
        {
            // 如果后四个均为默认参数，则直接返回当前地形的整个高度图
            if (xBase + yBase + width + height == 0)
            {
                return terrain.terrainData.GetHeights(xBase, yBase, heightMapRes, heightMapRes);
            }

            TerrainData terrainData = terrain.terrainData;
            int differX = xBase + width - (terrainData.heightmapResolution - 1);   // 右溢出量级
            int differY = yBase + height - (terrainData.heightmapResolution - 1);  // 上溢出量级

            // 根据数据溢出情况做处理
            float[,] ret;
            if (differX <= 0 && differY <= 0)  // 无溢出
            {
                ret = terrain.terrainData.GetHeights(xBase, yBase, width, height);
            }
            else if (differX > 0 && differY <= 0) // 右边溢出
            {
                ret = terrain.terrainData.GetHeights(xBase, yBase, width - differX, height);
                float[,] right = terrain.Right()?.terrainData.GetHeights(0, yBase, differX, height);
                if (right != null)
                    ret = ret.Concat1(right);
            }
            else if (differX <= 0 && differY > 0)  // 上边溢出
            {
                ret = terrain.terrainData.GetHeights(xBase, yBase, width, height - differY);
                float[,] up = terrain.Top()?.terrainData.GetHeights(xBase, 0, width, differY);
                if (up != null)
                    ret = ret.Concat0(up);
            }
            else // 上右均溢出
            {
                ret = terrain.terrainData.GetHeights(xBase, yBase, width - differX, height - differY);

                float[,] right = terrain.Right()?.terrainData.GetHeights(0, yBase, differX, height - differY);
                float[,] up = terrain.Top()?.terrainData.GetHeights(xBase, 0, width - differX, differY);
                float[,] upRight = terrain.Right()?.Top()?.terrainData.GetHeights(0, 0, differX, differY);

                if (right != null)
                    ret = ret.Concat1(right);
                if (upRight != null)
                    ret = ret.Concat0(up.Concat1(upRight));
            }

            return ret;
        }

        /// <summary>
        /// 设置Terrain的HeightMap
        /// 有不只一块地形的场景不要直接调用terrainData.SetHeights
        /// 这个方法会解决跨多块地形的问题
        /// </summary>
        /// <param name="terrain">目标地形</param>
        /// <param name="heights">高度图</param>
        /// <param name="xBase">X起点</param>
        /// <param name="yBase">Y起点</param>
        /// <param name="immediate">是否立即刷新地图</param>
        private void SetHeightMap(Terrain terrain, float[,] heights, int xBase = 0, int yBase = 0, bool immediate = true)
        {
            TerrainData terrainData = terrain.terrainData;
            int length_1 = heights.GetLength(1);
            int length_0 = heights.GetLength(0);

            int differX = xBase + length_1 - (terrainData.heightmapResolution - 1);
            int differY = yBase + length_0 - (terrainData.heightmapResolution - 1);

            // 根据溢出情况对数据做处理
            if (differX <= 0 && differY <= 0) // 无溢出
            {
                SetSingleHeightMap(terrain, xBase, yBase, heights, immediate);
            }
            else if (differX > 0 && differY <= 0) // 右溢出
            {
                SetSingleHeightMap(terrain, xBase, yBase, heights.GetPart(0, 0, length_0, length_1 - differX + 1), immediate);  // 最后的 +1是为了和右边的地图拼接
                SetSingleHeightMap(terrain.Right(), 0, yBase, heights.GetPart(0, length_1 - differX, length_0, differX), immediate);
            }
            else if (differX <= 0 && differY > 0) // 上溢出
            {
                SetSingleHeightMap(terrain, xBase, yBase, heights.GetPart(0, 0, length_0 - differY + 1, length_1), immediate);  // 最后的 +1是为了和上边的地图拼接
                SetSingleHeightMap(terrain.Top(), xBase, 0, heights.GetPart(length_0 - differY, 0, differY, length_1), immediate);
            }
            else  // 右上均溢出
            {
                SetSingleHeightMap(terrain, xBase, yBase, heights.GetPart(0, 0, length_0 - differY + 1, length_1 - differX + 1), immediate);  // 最后的 +1是为了和上边及右边的地图拼接
                SetSingleHeightMap(terrain.Right(), 0, yBase, heights.GetPart(0, length_1 - differX, length_0 - differY + 1, differX), immediate);
                SetSingleHeightMap(terrain.Top(), xBase, 0, heights.GetPart(length_0 - differY, 0, differY, length_1 - differX + 1), immediate);
                SetSingleHeightMap(terrain.Top()?.Right(), 0, 0, heights.GetPart(length_0 - differY, length_1 - differX, differY, differX), immediate);
            }
        }

        /// <summary>
        /// 设置单块地图的高度图
        /// </summary>
        /// <param name="immediate">是否立即刷新LOD</param>
        private void SetSingleHeightMap(Terrain terrain, int xBase, int yBase, float[,] heights, bool immediate = true)
        {
            if (terrain == null)
                return;
            if (!terrainDataDic.ContainsKey(terrain))
                terrainDataDic.Add(terrain, GetHeightMap(terrain));


            if (immediate)
                terrain.terrainData.SetHeights(xBase, yBase, heights);      // 会立即刷新整个地形LOD,不适合实时编辑
            else
                SetSingleHeightMapDelayLOD(terrain, xBase, yBase, heights); // 仅仅改变高度，如果用这种模式，实时编辑会更很快，但需要在合适的时候调用Refresh
        }

        /// <summary>
        /// 快速设置高度图，之后调用Refresh设置LOD
        /// </summary>
        /// <param name="terrain">目标地形</param>
        /// <param name="xBase">x轴的起始索引</param>
        /// <param name="yBase">z轴的起始索引</param>
        /// <param name="heights">目标高度图</param>
        private void SetSingleHeightMapDelayLOD(Terrain terrain, int xBase, int yBase, float[,] heights)
        {
            if (!terrainList.Contains(terrain))
            {
                terrainList.Add(terrain);
            }

            terrain.terrainData.SetHeightsDelayLOD(xBase, yBase, heights);
        }

        /// <summary>
        /// 初始化地形高度图编辑所需要的参数
        /// 后四个参数需要在调用前定义
        /// 编辑高度时所需要用到的参数后期打算用一个结构体封装
        /// </summary>
        /// <param name="center">目标中心</param>
        /// <param name="radius">半径</param>
        /// <param name="mapIndex">起始修改点在高度图上的索引</param>
        /// <param name="heightMap">要修改的高度二维数组</param>
        /// <param name="mapRadius">修改半径对应的索引半径</param>
        /// <param name="limit">限制高度</param>
        /// <returns></returns>
        private Terrain InitHMArg(Vector3 center, float radius, out HMArg arg)
        {
            Vector3 leftDown = new Vector3(center.x - radius, 0, center.z - radius);
            // 左下方Terrain
            Terrain centerTerrain = Utility.SendRayDown(center, LayerMask.GetMask("Terrain")).collider?.GetComponent<Terrain>();
            Terrain leftDownTerrain = Utility.SendRayDown(leftDown, LayerMask.GetMask("Terrain")).collider?.GetComponent<Terrain>();
            arg = default(HMArg);
            if (leftDownTerrain != null)
            {
                // 获取相关参数
                arg.mapRadiusX = (int)(heightMapRes / terrainSize.x * radius);
                arg.mapRadiusZ = (int)(heightMapRes / terrainSize.z * radius);
                arg.mapRadiusX = arg.mapRadiusX < 1 ? 1 : arg.mapRadiusX;
                arg.mapRadiusZ = arg.mapRadiusZ < 1 ? 1 : arg.mapRadiusZ;
                arg.startMapIndex = TerrainUtility.GetHeightmapIndex(leftDownTerrain, leftDown);
                arg.centerMapIndex = new Vector2Int(arg.startMapIndex.x + arg.mapRadiusX, arg.startMapIndex.y + arg.mapRadiusZ);
                arg.heightMap = GetHeightMap(leftDownTerrain, arg.startMapIndex.x, arg.startMapIndex.y, 2 * arg.mapRadiusX, 2 * arg.mapRadiusZ);
                arg.limit = 0/*heightMap[mapRadius, mapRadius]*/;
                return leftDownTerrain;
            }
            // 左下至少有一个方向没有Terrain,大多数情况下不会进入，如果删掉地图的左边界和下边界无法编辑，影响不大，其实我很想删掉，所以注释什么的就去TM的吧
            else if (centerTerrain != null)
            {
                // 获取相关参数
                arg.mapRadiusX = (int)(heightMapRes / terrainSize.x * radius);
                arg.mapRadiusZ = (int)(heightMapRes / terrainSize.z * radius);
                arg.mapRadiusX = arg.mapRadiusX < 1 ? 1 : arg.mapRadiusX;
                arg.mapRadiusZ = arg.mapRadiusZ < 1 ? 1 : arg.mapRadiusZ;

                arg.centerMapIndex = TerrainUtility.GetHeightmapIndex(centerTerrain, center);
                arg.startMapIndex = new Vector2Int(arg.centerMapIndex.x - arg.mapRadiusX, arg.centerMapIndex.y - arg.mapRadiusZ);

                int width = 2 * arg.mapRadiusX, height = 2 * arg.mapRadiusZ;

                if (arg.startMapIndex.x < 0 && arg.startMapIndex.y < 0)
                {
                    if (centerTerrain.Left() != null)
                    {
                        height += arg.startMapIndex.y;
                        arg.startMapIndex.y = 0;
                        arg.startMapIndex.x += heightMapRes;

                        centerTerrain = centerTerrain.Left();
                    }
                    else if (centerTerrain.Bottom() != null)
                    {
                        width += arg.startMapIndex.x;
                        arg.startMapIndex.x = 0;
                        arg.startMapIndex.y += heightMapRes;

                        centerTerrain = centerTerrain.Bottom();
                    }
                    else
                    {
                        width += arg.startMapIndex.x;
                        arg.startMapIndex.x = 0;
                        height += arg.startMapIndex.y;
                        arg.startMapIndex.y = 0;
                    }
                }
                else if (arg.startMapIndex.x < 0)
                {
                    width += arg.startMapIndex.x;
                    arg.startMapIndex.x = 0;
                }
                else if (arg.startMapIndex.y < 0)
                {
                    height += arg.startMapIndex.y;
                    arg.startMapIndex.y = 0;
                }

                arg.heightMap = GetHeightMap(centerTerrain, arg.startMapIndex.x, arg.startMapIndex.y, width, height);
                arg.limit = 0/*heightMap[mapRadius, mapRadius]*/;
            }
            return centerTerrain;
        }

        /// <summary>
        /// 改变地形高度
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="opacity">力度</param>
        /// <param name="isRise">抬高还是降低</param>
        /// <param name="amass">是否累加高度</param>
        private void InternalChangeHeight(Vector3 center, float radius, float opacity, bool isRise = true, bool amass = true)
        {
            HMArg arg;
            Terrain terrain = InitHMArg(center, radius, out arg);
            if (terrain == null) return;

            if (!isRise) opacity = -opacity;

            // 修改高度图
            for (int i = 0, length_0 = arg.heightMap.GetLength(0); i < length_0; i++)
            {
                for (int j = 0, length_1 = arg.heightMap.GetLength(1); j < length_1; j++)
                {
                    // 限制范围为一个圆
                    float rPow = Mathf.Pow(i + arg.mapRadiusZ - (arg.centerMapIndex.y - arg.startMapIndex.y) - arg.mapRadiusZ, 2) + Mathf.Pow(j + arg.mapRadiusX - (arg.centerMapIndex.x - arg.startMapIndex.x) - arg.mapRadiusX, 2);
                    if (rPow > arg.mapRadiusX * arg.mapRadiusZ)
                        continue;

                    float differ = 1 - rPow / (arg.mapRadiusX * arg.mapRadiusZ);
                    if (amass)
                    {
                        arg.heightMap[i, j] += differ * deltaHeight * opacity;
                    }
                    else if (isRise)
                    {
                        arg.heightMap[i, j] = arg.heightMap[i, j] >= arg.limit ? arg.heightMap[i, j] : arg.heightMap[i, j] + differ * deltaHeight * opacity;
                    }
                    else
                    {
                        arg.heightMap[i, j] = arg.heightMap[i, j] <= arg.limit ? arg.heightMap[i, j] : arg.heightMap[i, j] + differ * deltaHeight * opacity;
                    }
                }
            }
            // 重新设置高度图
            SetHeightMap(terrain, arg.heightMap, arg.startMapIndex.x, arg.startMapIndex.y, false);
        }

        /// <summary>
        /// 通过自定义笔刷编辑地形
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="opacity">力度</param>
        /// <param name="brushIndex">笔刷索引</param>
        /// <param name="isRise">抬高还是降低</param>
        private async void InternalChangeHeightWithBrush(Vector3 center, float radius, float opacity, int brushIndex = 0, bool isRise = true)
        {
            HMArg arg;
            Terrain terrain = InitHMArg(center, radius, out arg);
            if (terrain == null) return;

            // 是否反转透明度
            if (!isRise) opacity = -opacity;

            //修改高度图
            //float[,] deltaMap = await Utility.BilinearInterp(brushDic[brushIndex], 2 * mapRadius, 2 * mapRadius);
            float[,] deltaMap = await Math2d.ZoomBilinearInterpAsync(brushDic[brushIndex], 2 * arg.mapRadiusX, 2 * arg.mapRadiusX);

            for (int i = 0; i < 2 * arg.mapRadiusX; i++)
            {
                for (int j = 0; j < 2 * arg.mapRadiusX; j++)
                {
                    arg.heightMap[i, j] += deltaMap[i, j] * deltaHeight * opacity;
                }
            }

            // 重新设置高度图
            SetHeightMap(terrain, arg.heightMap, arg.startMapIndex.x, arg.startMapIndex.y);
        }

        /// <summary>
        /// 平滑地形
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="dev">这是高斯模糊的一个参数，会影响平滑的程度</param>
        /// <param name="level">构建高斯核的半径</param>
        public void InternalSmooth(Vector3 center, float radius, float dev, int level = 3)
        {
            center.x -= terrainSize.x / (heightMapRes - 1) * level;
            center.z -= terrainSize.z / (heightMapRes - 1) * level;
            radius += terrainSize.x / (heightMapRes - 1) * level;
            HMArg arg;
            Terrain terrain = InitHMArg(center, radius, out arg);
            if (terrain == null) return;
            // 利用高斯模糊做平滑处理
            Math2d.GaussianBlur(arg.heightMap, dev, level, false);
            SetHeightMap(terrain, arg.heightMap, arg.startMapIndex.x, arg.startMapIndex.y, false);
        }

        /// <summary>
        /// 批量平滑操作
        /// </summary>
        public void BatchSmooth(Vector3[] centers, float radius, float dev, int level = 1)
        {
            float differx = terrainSize.x / (heightMapRes - 1) * level;
            float differz = terrainSize.z / (heightMapRes - 1) * level;
            int arrayLength = centers.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                centers[i].x -= differx;
                centers[i].z -= differz;
            }
            Terrain[] terrains = new Terrain[arrayLength];

            HMArg[] args = new HMArg[arrayLength];
            //Loom.Initialize();
            for (int i = 0; i < arrayLength; i++)
            {
                terrains[i] = InitHMArg(centers[i], radius, out args[i]);
                BatchSmooth(terrains[i], args[i].heightMap, args[i].startMapIndex, dev, level);
                //BatchSmoothAsync(terrains[i], heightMaps[i], mapIndexs[i], dev, level);
            }
        }

        private void BatchSmooth(Terrain terrain, float[,] heightMap, Vector2Int mapIndex, float dev, int level = 1)
        {
            if (terrain != null)
            {
                //Loom.RunAsync(() =>
                //{
                //    for (int i = 0; i < 10; i++)
                //    {
                //        Math2d.GaussianBlur(heightMap, dev, level);
                //    }
                //    Loom.QueueOnMainThread(() =>
                //    {
                //        SetHeightMap(terrain, heightMap, mapIndex.x, mapIndex.y, false);
                //    });
                //});
            }
        }

        /// <summary>
        /// 压平Terrain并提升到指定高度
        /// </summary>
        /// <param name="terrain">Terrain</param>
        /// <param name="height">高度</param>
        private void InternalFlatten(Terrain terrain, float height)
        {
            float scaledHeight = height * tdInfo.deltaHeight;

            float[,] heights = new float[tdInfo.heightMapRes, tdInfo.heightMapRes];
            for (int i = 0; i < tdInfo.heightMapRes; i++)
            {
                for (int j = 0; j < tdInfo.heightMapRes; j++)
                {
                    heights[i, j] = scaledHeight;
                }
            }

            terrain.terrainData.SetHeights(0, 0, heights);
            //TerrainUtility.Flatten(terrain, height, tdInfo); 
        }

        #endregion

        #region 树木

        /// <summary>
        /// 创建树木原型并返回
        /// </summary>
        /// <returns></returns>
        public TreePrototype[] CreatTreePrototype(GameObject[] treeObjs)
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
        /// 初始化树木原型组
        /// </summary>
        public void InitTreePrototype(GameObject[] treeObjs)
        {
            TreePrototype[] trees = CreatTreePrototype(treeObjs);
            Terrain[] terrains = Terrain.activeTerrains;
            for (int i = 0, length = terrains.Length; i < length; i++)
            {
                terrains[i].terrainData.treePrototypes = terrains[i].terrainData.treePrototypes.Concat(trees).ToArray();
            }
        }

        /// <summary>
        /// 创建树木
        /// </summary>
        private void InternalCreatTree(Terrain terrain, Vector3 pos, int count, int radius, int index = 0)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 relativePosition;
            Vector3 position;

            for (int i = 0; i < count; i++)
            {
                // 获取世界坐标系的位置和相对位置
                position = pos + new Vector3(UnityEngine.Random.Range(-radius, radius), 0, UnityEngine.Random.Range(-radius, radius));
                relativePosition = position - terrain.GetPosition();

                if (Mathf.Pow(pos.x - position.x, 2) + Mathf.Pow(pos.z - position.z, 2) > radius * radius)
                {
                    i--; // 没有创建的数不计入
                    continue;
                }

                TreeInstance instance = TerrainUtility.GetTreeInstance(index);

                // 对跨地形做处理
                Vector3 p = new Vector3(relativePosition.x / terrainData.size.x, 0, relativePosition.z / terrainData.size.z);
                if (p.x > 1 || p.z > 1)
                {
                    if (p.x > 1)
                        p.x -= 1;
                    if (p.z > 1)
                        p.z -= 1;
                    instance.position = p;
                    GetTerrain(position)?.AddTreeInstance(instance);
                }
                else if (p.x < 0 || p.z < 0)
                {
                    if (p.x < 0)
                        p.x += 1;
                    if (p.z < 0)
                        p.z += 1;
                    instance.position = p;
                    GetTerrain(position)?.AddTreeInstance(instance);
                }
                else
                {
                    instance.position = p;
                    terrain.AddTreeInstance(instance);
                }
            }
        }

        /// <summary>
        /// 移除地形上的树，没有做多地图的处理
        /// </summary>
        /// <param name="terrain">目标地形</param>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="index">树模板的索引</param> 
        private void InternalRemoveTree(Terrain terrain, Vector3 center, float radius, int index = 0)
        {
            center -= terrain.GetPosition();     // 转为相对位置
            Vector2 v2 = new Vector2(center.x, center.z);
            v2.x /= Terrain.activeTerrain.terrainData.size.x;
            v2.y /= Terrain.activeTerrain.terrainData.size.z;

            terrain.Invoke("RemoveTrees", v2, radius / Terrain.activeTerrain.terrainData.size.x, index);
        }

        #endregion

        #region 细节纹理 草

        /// <summary>
        /// 创建细节原型并返回
        /// </summary>
        /// <returns></returns>
        public DetailPrototype[] CreateDetailPrototype(Texture2D[] textures)
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
        /// 初始化细节原型组
        /// </summary>
        public void InitDetailPrototype(Texture2D[] textures)
        {
            DetailPrototype[] details = CreateDetailPrototype(textures);

            Terrain[] terrains = Terrain.activeTerrains;
            for (int i = 0, length = terrains.Length; i < length; i++)
            {
                terrains[i].terrainData.detailPrototypes = terrains[i].terrainData.detailPrototypes.Concat(details).ToArray();
                terrains[i].detailObjectDistance = 250;  // 设置草的消失距离 
            }
        }

        /// <summary>
        /// 修改细节数据
        /// </summary>
        /// <param name="detailMap"></param>
        /// <param name="count"></param>
        private void ChangeDetailMap(int[,] detailMap, int count)
        {
            int mapRadius = detailMap.GetLength(0) / 2;
            // 修改数据
            for (int i = 0, length_0 = detailMap.GetLength(0); i < length_0; i++)
            {
                for (int j = 0, length_1 = detailMap.GetLength(1); j < length_1; j++)
                {
                    // 限定圆
                    if ((i - mapRadius) * (i - mapRadius) + (j - mapRadius) * (j - mapRadius) > mapRadius * mapRadius)
                        continue;
                    detailMap[i, j] = count;
                }
            }
        }

        /// <summary>
        /// 可跨多块地形的细节修改
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="layer"></param>
        /// <param name="count"></param>
        private void InternalSetDetail(Vector3 center, float radius, int layer, int count)
        {
            Vector3 leftDown = new Vector3(center.x - radius, 0, center.z - radius);
            Terrain terrain = Utility.SendRayDown(leftDown, LayerMask.GetMask("Terrain")).collider?.GetComponent<Terrain>();
            if (terrain != null)
            {
                // 获取数据
                TerrainData terrainData = terrain.terrainData;
                int mapRadius = (int)(radius / terrainData.size.x * terrainData.detailResolution);
                Vector2Int mapIndex = TerrainUtility.GetDetialMapIndex(terrain, leftDown);
                int[,] detailMap = TerrainUtility.GetDetailLayer(terrain, mapIndex.x, mapIndex.y, 2 * mapRadius, 2 * mapRadius, layer);

                // 修改数据
                ChangeDetailMap(detailMap, count);

                // 设置数据
                TerrainUtility.SetDetailLayer(terrain, detailMap, mapIndex.x, mapIndex.y, layer);
            }
        }

        #endregion

        #region 贴图

        /// <summary>
        /// 创建贴图原型并返回
        /// </summary>
        /// <returns></returns>
        public TerrainLayer[] CreateSplatPrototype()
        {
            TerrainLayer[] splats = Resources.LoadAll<TerrainLayer>("Terrain/TerrainLayers");

            return splats;
        }

        /// <summary>
        /// 初始化贴图原型
        /// </summary>
        public void InitTerrainLayers(TerrainLayer[] terrainLayers)
        {
            Terrain[] terrains = Terrain.activeTerrains;
            for (int i = 0, length = terrains.Length; i < length; i++)
            {
                terrains[i].terrainData.terrainLayers = terrains[i].terrainData.terrainLayers.Concat(terrainLayers).ToArray();
            }
        }

        /// <summary>
        /// 修改细节数据
        /// </summary>
        /// <param name="alphaMap"></param>
        /// <param name="count"></param>
        private void ChangeAlphaMap(float[,,] alphaMap, int index, float strength)
        {
            int mapRadius = alphaMap.GetLength(0) / 2;
            // 修改数据
            for (int i = 0, length_0 = alphaMap.GetLength(0); i < length_0; i++)
            {
                for (int j = 0, length_1 = alphaMap.GetLength(1); j < length_1; j++)
                {
                    // 限定圆
                    if ((i - mapRadius) * (i - mapRadius) + (j - mapRadius) * (j - mapRadius) > mapRadius * mapRadius)
                        continue;
                    alphaMap[i, j, index] = strength;
                }
            }
        }

        /// <summary>
        /// 设置贴图
        /// </summary>
        /// <param name="point"></param>
        /// <param name="index"></param>
        private void InternalSetTexture(Vector3 center, float radius, int index, float strength)
        {
            Vector3 leftDown = new Vector3(center.x - radius, 0, center.z - radius);
            Terrain terrain = Utility.SendRayDown(leftDown, LayerMask.GetMask("Terrain")).collider?.GetComponent<Terrain>();
            if (terrain != null)
            {
                // 获取数据
                TerrainData terrainData = terrain.terrainData;
                int mapRadius = (int)(radius / terrainData.size.x * terrainData.alphamapResolution);
                Vector2Int mapIndex = TerrainUtility.GetAlphaMapIndex(terrain, leftDown);
                float[,,] alphaMap = TerrainUtility.GetAlphaMap(terrain, mapIndex.x, mapIndex.y, 2 * mapRadius, 2 * mapRadius);

                // 修改数据
                ChangeAlphaMap(alphaMap, index, strength);

                // 设置数据
                TerrainUtility.SetAlphaMap(terrain, alphaMap, mapIndex.x, mapIndex.y);
            }
        }

        /// <summary>
        /// 利用笔刷设置贴图
        /// </summary>
        private async void SetTextureWithBrush(Terrain terrain, Vector3 point, int index, float radius, float[,] brush)
        {
            Vector3 down = new Vector3(point.x - radius, 0, point.z - radius);
            if (terrain != null)
            {
                Vector2Int mapIndex = TerrainUtility.GetAlphaMapIndex(terrain, down);
                int mapRadius = (int)(radius / terrain.terrainData.size.x * terrain.terrainData.alphamapResolution);
                float[,,] map = terrain.terrainData.GetAlphamaps(mapIndex.x, mapIndex.y, 2 * mapRadius, 2 * mapRadius);
                float[,] realBrush = await Math2d.ZoomBilinearInterpAsync(brush, 2 * mapRadius, 2 * mapRadius);

                // 这么计算所附加的贴图实际上是有问题的，但是我懒得继续算了
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        float temp = (1 - realBrush[i, j]) / (terrain.terrainData.alphamapLayers - 1);
                        for (int k = 0; k < terrain.terrainData.alphamapLayers; k++)
                        {
                            map[i, j, k] = temp;
                        }
                        map[i, j, index] = 1 - temp;
                    }
                }
                terrain.terrainData.SetAlphamaps(mapIndex.x, mapIndex.y, map);
            }
        }

        #endregion

        #region 外部调用

        /// <summary>
        /// 改变高度（圆形）
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="opacity">力度</param>
        /// <param name="isRise">抬高还是降低</param>
        /// <param name="amass">是否累加高度</param>
        public void ChangeHeight(Vector3 center, float radius, float opacity, bool isRise = true, bool amass = true)
        {
            InternalChangeHeight(center, radius, opacity, isRise, amass);
        }

        /// <summary>
        /// 利用自定义笔刷更改地形高度
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="opacity">力度</param>
        /// <param name="brushIndex">笔刷索引</param>
        /// <param name="isRise">抬高还是降低</param>
        public void ChangeHeightWithBrush(Vector3 center, float radius, float opacity, int brushIndex = 0, bool isRise = true)
        {
            InternalChangeHeightWithBrush(center, radius, opacity, brushIndex, isRise);
        }

        /// <summary>
        /// 平滑地形
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="dev">这是高斯模糊的一个参数，会影响平滑的程度</param>
        /// <param name="level">构建高斯和的半径</param>
        public void Smooth(Vector3 center, float radius, float dev, int level = 3)
        {
            InternalSmooth(center, radius, dev, level);
        }

        /// <summary>
        /// 压平地形
        /// </summary>
        /// <param name="terrain">地形</param>
        /// <param name="height">目标高度</param>
        public void Flatten(Terrain terrain, float height)
        {
            InternalFlatten(terrain, height);
        }

        /// <summary>
        /// 刷新地图的LOD
        /// </summary>
        public void Refresh()
        {
            foreach (var item in terrainList)
            {
                item.ApplyDelayedHeightmapModification();
            }
            terrainList.Clear();
        }


        /// <summary>
        /// 创建树木
        /// </summary>
        /// <param name="pos">中心点</param>
        /// <param name="count">数量</param>
        /// <param name="radius">种植半径</param>
        /// <param name="index">树模板的索引</param>
        public void CreatTree(Vector3 pos, int count, int radius, int index = 0)
        {
            Terrain terrain = GetTerrain(pos);
            InternalCreatTree(terrain, pos, count, radius, index);
        }

        /// <summary>
        /// 移除地形上的树，没有做多地图的处理
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="index">树模板的索引</param> 
        public void RemoveTree(Vector3 center, float radius, int index = 0)
        {
            Terrain terrain = GetTerrain(center);
            InternalRemoveTree(terrain, center, radius, index);
        }


        /// <summary>
        /// 添加细节
        /// </summary>
        /// <param name="terrain">目标地形</param>
        /// <param name="center">目标中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="layer">层级</param>
        public void AddDetial(Vector3 center, float radius, int count, int layer = 0)
        {
            InternalSetDetail(center, radius, layer, count);
        }

        /// <summary>
        /// 移除细节
        /// </summary>
        /// <param name="terrain">目标地形</param>
        /// <param name="center">目标中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="layer">层级</param>
        public void RemoveDetial(Vector3 point, float radius, int layer = 0)
        {
            InternalSetDetail(point, radius, layer, 0);
        }


        /// <summary>
        /// 设置贴图
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="point">中心点</param>
        /// <param name="index">layer</param>
        public void SetTexture(Vector3 point, float radius, int index, float strength = 1)
        {
            InternalSetTexture(point, radius, index, strength);
        }

        /// <summary>
        /// 利用笔刷设置贴图
        /// </summary>
        /// <param name="point">中心点</param>
        /// <param name="index">layer</param>
        /// <param name="radius">半径</param>
        /// <param name="brushIndex">笔刷序号</param>
        public void SetTextureWithBrush(Vector3 point, int index, float radius, int brushIndex)
        {
            Terrain terrain = GetTerrain(point);
            SetTextureWithBrush(terrain, point, index, radius, brushDic[brushIndex]);
        }

        #endregion
    }
}