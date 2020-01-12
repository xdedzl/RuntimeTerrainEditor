> 上一篇关于地形文章写完后（好像也有点遥远了），这个运行时地形编辑器也基本上满足自己需求了，抽空稍微整理了一下做成了一个独立的工具，unity2018.3及以上可用，一开始也是适配了2017的，后来由于自己已经完全不用2017了，就放弃了对2017的支持。
> [Github源码链接](https://github.com/xdedzl/RunTimeTerrainEditor)

 - [x] 支持跨地形编辑
 - [x] 笔刷
 - [x] 地形的抬高，降低，平滑
 - [x] 树的种植和删除
 - [x] 草的种植和删除
 - [x] 贴图编辑
 - [x] 地形高度编辑支持使用笔刷
 - [ ] 树、草的编辑支持使用笔刷
 - [x] 操作的撤回 

## 一、RuntimeTerrainEditor
&emsp;&emsp;提供了对地形高度，细节，树木，纹理四大类的修改，其中地形高度的修改支持使用笔刷，其他几种由于自已没有需要就偷了个懒，后期会补上。
#### 1.笔刷
&emsp;&emsp;之前在[自定义笔刷](https://blog.csdn.net/xdedzl/article/details/85546694)这篇文章中讲到了自定义笔刷的思路，这里就不多说了,只说怎么创建一张图片用作笔刷。
&emsp;&emsp;首先自己制作的笔刷图片一定要带透明通道，推荐使用png格式，图片的alpha值也就代表了笔刷在对应位置的强度，下面这张图片是个最常用的一种笔刷，但由于自己不会做图，实际运用时使用这张图效果不怎么好。这张图中心透明度高，越往外越低，用这张图做地形高度的修改也就是一个中间高外围低的效果。

![在这里插入图片描述](https://img-blog.csdnimg.cn/20200108233200989.png?)

&emsp;&emsp;图片制作完成后导入unity，设置图片的Import Setting如下。

![在这里插入图片描述](https://img-blog.csdnimg.cn/20200108234017454.PNG)

&emsp;&emsp;导入并设置完成后，使用自己工程加载资源的方式加载这些贴图，在构造RunTimeTerrainEditor类的时候当参数传入。

```csharp
public RuntimeTerrainEditor(Texture2D[] brushs = null)
```
#### 2.高度修改
&emsp;&emsp;高度修改提供的API如下，每个接口对应一个DelayLod版本，在使用DelayLod版本时，需要在合适的时候自行调用`ApplyDelayedHeightmapModification`方法用以刷新地图的LOD。

```csharp
/// <summary>
/// 改变高度（圆形）
/// </summary>
/// <param name="center">中心点</param>
/// <param name="radius">半径</param>
/// <param name="opacity">力度</param>
/// <param name="isRise">抬高还是降低</param>
/// <param name="regesterUndo">是否注册撤回命令</param>
public void ChangeHeight(Vector3 center, float radius, float opacity, bool isRise = true, bool regesterUndo = false)
/// <summary>
/// 利用自定义笔刷更改地形高度
 /// </summary>
 /// <param name="center">中心点</param>
 /// <param name="radius">半径</param>
 /// <param name="opacity">力度</param>
 /// <param name="brushIndex">笔刷索引</param>
 /// <param name="isRise">抬高还是降低</param>
 /// <param name="regesterUndo">是否注册撤回命令</param>
public void ChangeHeightWithBrush(Vector3 center, float radius, float opacity, int brushIndex = 0, bool isRise = true, bool regesterUndo = false)
/// <summary>
/// 平滑地形
/// </summary>
/// <param name="center">中心点</param>
/// <param name="radius">半径</param>
/// <param name="dev">这是高斯模糊的一个参数，会影响平滑的程度</param>
/// <param name="level">构建高斯和的半径</param>
/// <param name="regesterUndo">是否注册撤回命令</param>
public void Smooth(Vector3 center, float radius, float dev, int level = 3, bool regesterUndo = false)
```
#### 3.增删树
```csharp
/// <summary>
/// 创建树木
/// </summary>
/// <param name="pos">中心点</param>
/// <param name="count">数量</param>
/// <param name="radius">种植半径</param>
/// <param name="index">树模板的索引</param>
public void CreatTree(Vector3 pos, int count, int radius, int index = 0)
/// <summary>
/// 移除地形上的树，没有做多地图的处理
/// </summary>
/// <param name="center">中心点</param>
/// <param name="radius">半径</param>
/// <param name="index">树模板的索引</param> 
public void RemoveTree(Vector3 center, float radius, int index = 0)
```
#### 4.增删草
```csharp
/// <summary>
/// 添加细节
/// </summary>
/// <param name="terrain">目标地形</param>
/// <param name="center">目标中心点</param>
/// <param name="radius">半径</param>
/// <param name="layer">层级</param>
/// <param name="regesterUndo">是否注册撤回命令</param>
public void AddDetial(Vector3 center, float radius, int count, int layer = 0, bool regesterUndo = false)
/// <summary>
/// 移除细节
/// </summary>
/// <param name="terrain">目标地形</param>
/// <param name="center">目标中心点</param>
/// <param name="radius">半径</param>
/// <param name="layer">层级</param>
/// <param name="regesterUndo">是否注册撤回命令</param>
public void RemoveDetial(Vector3 point, float radius, int layer = 0, bool regesterUndo = false)
```
#### 5.修改贴图
```csharp
/// <summary>
/// 设置贴图
/// </summary>
/// <param name="radius">半径</param>
/// <param name="point">中心点</param>
/// <param name="index">层级</param>
/// <param name="regesterUndo">是否注册撤回命令</param>
public void SetTextureNoMix(Vector3 point, float radius, int index, bool regesterUndo = false)
```

#### 6.撤回操作
&emsp;&emsp;目前除了树的创建和删除以外，所有的操作都支持撤回，在使用这些API的时候设置参数`regesterUndo `为`true`，标明本次操作要注册到撤回操作中，然后在你想要撤回的时候调用`Undo()`即可。

## 二、使用前的配置
#### 1.配置笔刷
&emsp;&emsp;前文中已经说明了笔刷的配置，这里就不在赘述。
#### 2.配置treePrototypes 、detailPrototypes、terrainLayers 
&emsp;&emsp;在使用地形编辑工具前，需要对场景中的地形设置树、草、贴图的模板，可以在运行前就在场景中设置好，也可以用代码设置。
```csharp
Terrain.activeTerrain.terrainData.treePrototypes = ...
Terrain.activeTerrain.terrainData.detailPrototypes = ...
Terrain.activeTerrain.terrainData.terrainLayers = ...
```
&emsp;&emsp;在用代码设置上述模板的话，可以使用`TerrainUtility`工具类提供了几个构造模板的方法。

```csharp
/// <summary>
/// 通过树的预制体创建树模板
/// </summary>
/// <param name="treeObjs">树预制体</param>
/// <returns>模板</returns>
public static TreePrototype[] CreatTreePrototype(GameObject[] treeObjs)
/// <summary>
/// 通过贴图创建细节模板
/// </summary>
/// <param name="textures">贴图</param>
/// <returns>模板</returns>
public static DetailPrototype[] CreateDetailPrototype(Texture2D[] textures)
```
#### 3.地形基础数据的配置
&emsp;&emsp;在`RuntimeTerrainEditor`的构造函数中，已经调用了对地形基础数据的配置，但如果这个时候你的地形还没有加载完成的话，就需要在地形加载完成后再手动调用一次。
```csharp
/// <summary>
/// 使用前的配置
/// </summary>
public void Config()
```

上述步骤完成之后，就可以使用地形编辑器了。