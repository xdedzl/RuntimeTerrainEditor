// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-11-26 09:08:29
// 版本： V 1.0
// ==========================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XFramework;
using XFramework.Fsm;
using XFramework.UI;

public class TerrainModifierPanel : BasePanel
{

    /// <summary>
    /// 范围的滑动器输入框组合
    /// </summary>
    public SliderMixInput rangeMix { get; private set; }
    /// <summary>
    /// 力度的滑动器输入框组合
    /// </summary>
    public SliderMixInput opticalMix { get; private set; }
    /// <summary>
    /// 路面宽度
    /// </summary>
    public SliderMixInput widthMix { get; private set; }

    /// <summary>
    /// 用于显示原型贴图的卷轴
    /// </summary>
    private Transform textureScroll;
    /// <summary>
    /// 卷轴内容的第一层父物体
    /// </summary>
    private Transform scrollContent;

    /// <summary>
    /// 地形修改模式
    /// </summary>
    public ModifierType modifierType { get; private set; }
    /// <summary>
    /// 原型索引
    /// </summary>
    public int PrototypeIndex { get; private set; }

    /// <summary>
    /// 创建路
    /// </summary>
    private Button creatRoadBtn;
    private Button deleteRoadBtn;
    private Toggle isBrigde;
    private Button showNodeBtn;

    // TODO 路面相关
    //public WayType wayType
    //{
    //    get
    //    {
    //        if (isBrigde.isOn)
    //            return WayType.Bridge;
    //        else
    //            return WayType.Road;
    //    }
    //}

    /// <summary>
    /// 卷轴类容预制
    /// </summary>
    private GameObject toogleImage;
    private Dictionary<ModifierType, Sprite[]> textureDic;

    /// <summary>
    /// 不同的ModifierType显示效果同，管理这些可能变化的物体
    /// </summary>
    private GameObject[] configShowObjs;

    public override void Reg()
    {
        InitTextureDic();
        ConfigTerrainEditor();

        rect = gameObject.GetComponent<RectTransform>();
        
        // 初始化组件
        rangeMix = transform.Find("Sliders/Range").GetComponent<SliderMixInput>();
        opticalMix = transform.Find("Sliders/Optical").GetComponent<SliderMixInput>();
        widthMix = transform.Find("RoadUI/Width").GetComponent<SliderMixInput>();

        textureScroll = transform.Find("TextureScroll");
        scrollContent = transform.Find("TextureScroll/Viewport/Content");
        toogleImage = transform.Find("ToggleImageTemplate").gameObject;

        creatRoadBtn = transform.Find("Btns/CreatRoadBtn").GetComponent<Button>();
        deleteRoadBtn = transform.Find("Btns/DeleteRoadBtn").GetComponent<Button>();
        isBrigde = transform.Find("RoadUI/IsBridge").GetComponent<Toggle>();
        showNodeBtn = transform.Find("RoadUI/ShowNodeBtn").GetComponent<Button>();

        // TODO 路面相关
        //creatRoadBtn.onClick.AddListener(() =>
        //{
        //    // 按创建按钮生成路面
        //    RoadManager.Instance.CreateROB();           // 生成路面
        //    RoadManager.Instance.CreateROBInters();     // 生成路口
        //    if (RoadManager.Instance.WPNs.Count > 0)
        //    {
        //        RoadManager.Instance.SetShow(false); // 关闭显示
        //        RoadManager.Instance.toBeShow = true;
        //    }
        //});
        //deleteRoadBtn.onClick.AddListener(() =>
        //{
        //    // 按删除按钮清除路点
        //    if (modifierType == ModifierType.BuildRoad)
        //    {
        //        RoadManager.Instance.DeleteWPN(RoadManager.Instance.SelectWPN);
        //    }
        //});
        //showNodeBtn.onClick.AddListener(() =>
        //{
        //    if (RoadManager.Instance.WPNs.Count > 0)
        //    {
        //        RoadManager.Instance.SetShow(RoadManager.Instance.toBeShow);
        //        RoadManager.Instance.toBeShow = !RoadManager.Instance.toBeShow;
        //    }
        //});

        configShowObjs = new GameObject[]
        {
            transform.Find("Sliders").gameObject,
            textureScroll.gameObject,
            transform.Find("Btns").gameObject,
            transform.Find("RoadUI").gameObject,
        };


        rangeMix.SetMinMax(10, 1000);
        opticalMix.SetMinMax(1, 10);
        widthMix.SetMinMax(5f, 10f);
        transform.Find("Dropdown").GetComponent<Dropdown>().onValueChanged.AddListener(OnDropDownChanged);

        ConfigShow(true, true, false, false);
    }

    private void OnDropDownChanged(int type)
    {
        modifierType = (ModifierType)type;
        Sprite[] sprits;
        textureDic.TryGetValue(modifierType, out sprits);

        switch (modifierType)
        {
            case ModifierType.Up:
            case ModifierType.Down:
                opticalMix.SetMinMax(1, 10);
                ConfigShow(true, true, false, false);
                break;
            case ModifierType.Smooth:
                opticalMix.SetMinMax(0.5f, 1.5f);
                ConfigShow(true, true, false, false);
                break;
            case ModifierType.AddTree:
                opticalMix.SetMinMax(1, 100);
                ConfigShow(true, true, false, false);
                break;
            case ModifierType.AddDetial:
                opticalMix.SetMinMax(1, 10);
                ConfigShow(true, true, false, false);
                break;
            case ModifierType.BuildRoad:
                ConfigShow(false, false, true, true);
                break;
            case ModifierType.Buildings:
                ConfigShow(false, true, false, false);
                break;
            default:
                break;
        }

        // 自动设置卷轴贴图
        AutoSetToogle(sprits, toogleImage);

        // 通过类型修改投影的显示
        // MouseEvent.Instance.GetState<MouseTerrainModifierState>(MouseStateType.TerrainModifier).OnDropDownChange(type);
    }

    /// <summary>
    /// 配置地形编辑
    /// </summary>
    private void ConfigTerrainEditor()
    {
        foreach (var item in Terrain.activeTerrains)
        {
            item.terrainData.treePrototypes = new TreePrototype[0];
            item.terrainData.detailPrototypes = new DetailPrototype[0];
            item.terrainData.terrainLayers = new TerrainLayer[0];
        }
        Game.TerrainModule.InitBrushs(Game.ResModule.LoadAll<Texture2D>("Assets/ABRes/Terrain/Brushs"));
        Game.TerrainModule.InitTreePrototype(Game.ResModule.LoadAll<GameObject>("Assets/ABRes/Terrain/Trees"));
        Game.TerrainModule.InitDetailPrototype(Game.ResModule.LoadAll<Texture2D>("Assets/ABRes/Terrain/Details"));
        Game.TerrainModule.InitTerrainLayers(Game.ResModule.LoadAll<TerrainLayer>("Assets/ABRes/Terrain/TerrainLayers"));

        Game.TerrainModule.ConfigActiveTerrains();
    }

    /// <summary>
    /// 初始化可选择的贴图
    /// </summary>
    private void InitTextureDic()
    {
        textureDic = new Dictionary<ModifierType, Sprite[]>
        {
            { ModifierType.AddDetial, Game.ResModule.LoadAll<Sprite>("Assets/ABRes/Terrain/Details")},
            { ModifierType.AddTree, Game.ResModule.LoadAll<Sprite>("Assets/ABRes/Terrain/Sprites/Trees") },
            { ModifierType.Buildings, Game.ResModule.LoadAll<Sprite>("Assets/ABRes/Terrain/Sprites/Buildings")},
        };
    }

    /// <summary>
    /// 适配Toogle的数量
    /// 后期传参不是int，应该是一个Texture[]
    /// </summary>
    private void AutoSetToogle(Sprite[] sprites = null, GameObject toogleObj = null)
    {
        int count = sprites == null ? 0 : sprites.Length;
        int differ = count - scrollContent.childCount;
        // 根据现有按钮数量补齐或删除
        if (differ >= 0)
        {
            for (int i = scrollContent.childCount, length = count; i < length; i++)
            {
                Transform btn = Object.Instantiate(toogleObj).transform;
                btn.SetParent(scrollContent, false);
            }
        }
        else
        {
            for (int i = 0; i < -differ; i++)
            {
                Transform btn = scrollContent.GetChild(scrollContent.childCount - 1);
                Object.DestroyImmediate(btn.gameObject);
            }
        }

        // 注册Toogle事件
        ToggleGroup group = scrollContent.GetComponent<ToggleGroup>();
        foreach (var item in scrollContent.GetComponentsInChildren<Toggle>())
        {
            item.group = group;
            item.onValueChanged.AddListener((a) =>
            {
                if (a == true)
                {
                    // 其在父物体中的索引当作原型索引
                    PrototypeIndex = item.transform.GetSiblingIndex();
                }
            });
        }

        // 为Image赋贴图
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            scrollContent.GetChild(i).GetComponent<Image>().sprite = sprites[i];
        }
    }

    public override void OnOpen(object arg)
    {
        gameObject.SetActive(true);
        Game.FsmModule.GetFsm<MouseFsm>().ChangeState<MouseTerrainModifierState>();
    }

    public override void OnClose()
    {
        gameObject.SetActive(false);
    }

    public enum ModifierType
    {
        Up,          // 上升高度
        Down,        // 降低高度
        Smooth,      // 平滑地面 
        AddTree,     // 种树
        AddDetial,   // 种草
        BuildRoad,   // 建路/桥
        Buildings,   // 建造建筑物
    }

    /// <summary>
    /// 为不同ModifierType显示不同的UI
    /// </summary>
    /// <param name="parms"></param>
    private void ConfigShow(params bool[] parms)
    {
        if (parms.Length != configShowObjs.Length)
        {
            Debug.LogError("参数传递数量有误");
            return;
        }
        for (int i = 0; i < parms.Length; i++)
        {
            configShowObjs[i].SetActive(parms[i]);
        }
    }
}

/// <summary>
/// 用于地形修改的鼠标状态
/// </summary>
public class MouseTerrainModifierState : MouseState
{
    /// <summary>
    /// 是否是增加状态
    /// </summary>
    private bool isAdd = true;
    /// <summary>
    /// 选中的物体(建筑物或路点)
    /// </summary>
    private Transform selectObj;
    /// <summary>
    /// 碰撞信息缓存
    /// </summary>
    private RaycastHit hitInfo;

    /// <summary>
    /// 对应的地形修改面板
    /// </summary>
    private TerrainModifierPanel panel;
    private TerrainModifierPanel Panel
    {
        get
        {
            if (panel == null)
                panel = Game.UIModule.GetPanel(UIName.TerrainModifier) as TerrainModifierPanel;
            return panel;
        }
    }

    /// <summary>
    /// 投影
    /// </summary>
    private Projector projector;
    private Projector Projection
    {
        get
        {
            if (projector == null)
            {
                GameObject projectorObj = new GameObject("TerrainProjector");
                projectorObj.transform.localEulerAngles = new Vector3(90, 0, 0);
                projector = projectorObj.AddComponent<Projector>();
                projector.orthographic = true;
                projector.farClipPlane = 20000;
                projector.material = Resources.Load("Projector/ProjectorMat") as Material;
            }
            return projector;
        }
    }

    /// <summary>
    /// 初始化状态
    /// </summary>
    public override void Init()
    {
        Panel.rangeMix.onValueChange.AddListener((a) =>
        {
            Projection.orthographicSize = a;
        });
    }

    /// <summary>
    /// 下拉框状态改变
    /// </summary>
    /// <param name="index"></param>
    public void OnDropDownChange(int index)
    {
        if (index > 4)
        {
            Projection.enabled = false;
        }
        else
        {
            Projection.enabled = true;
        }
    }

    /// <summary>
    /// 状态开始
    /// </summary>
    /// <param name="para"></param>
    /// <param name="args"></param>
    public override void OnEnter()
    {
        if((int)panel.modifierType < 5)
        {
            Projection.enabled = true;
        }
    }

    /// <summary>
    /// 左键按住
    /// </summary>
    public override void OnLeftButtonHold()
    {
        hitInfo = Utility.SendRay(LayerMask.GetMask("Terrain"));
        if (hitInfo.Equals(default(RaycastHit)))
            return;

        Terrain terrain = hitInfo.collider.GetComponent<Terrain>();
        switch (Panel.modifierType)
        {
            case TerrainModifierPanel.ModifierType.Up:
                Game.TerrainModule.ChangeHeight(hitInfo.point, (int)Panel.rangeMix.Value, Panel.opticalMix.Value);
                break;
            case TerrainModifierPanel.ModifierType.Down:
                Game.TerrainModule.ChangeHeight(hitInfo.point, (int)Panel.rangeMix.Value, Panel.opticalMix.Value, false);
                break;
            case TerrainModifierPanel.ModifierType.Smooth:
                Game.TerrainModule.Smooth(hitInfo.point, (int)Panel.rangeMix.Value, Panel.opticalMix.Value);
                break;
            case TerrainModifierPanel.ModifierType.AddDetial:
                if (Game.FsmModule.GetFsm<MouseFsm>().MouseMove)
                {
                    if (isAdd)
                        Game.TerrainModule.AddDetial(hitInfo.point, Panel.rangeMix.Value, (int)(Panel.opticalMix.Value), Panel.PrototypeIndex);
                    else
                        Game.TerrainModule.RemoveDetial(hitInfo.point, Panel.rangeMix.Value, Panel.PrototypeIndex);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 左键按下
    /// </summary>
    public override void OnLeftButtonDown()
    {
        hitInfo = Utility.SendRay(LayerMask.GetMask("Terrain"));
        if (hitInfo.Equals(default(RaycastHit)))
            return;

        // TODO 路面桥梁
        //switch (Panel.modifierType)
        //{
        //    case TerrainModifierPanel.ModifierType.BuildRoad:
        //        selectObj = RoadManager.Instance.GetSelectWPN(true)?.parent;
        //        RuntimeHandle.SetTarget(selectObj);
        //        break;
        //    case TerrainModifierPanel.ModifierType.Buildings:
        //        BuildingManager.Instance.AddBuild(Panel.PrototypeIndex, hitInfo.point);
        //        RuntimeHandle.SetTarget(selectObj);
        //        break;
        //}
    }

    /// <summary>
    /// 左键抬起
    /// </summary>
    public override void OnLeftButtonUp()
    {
        hitInfo = Utility.SendRay(LayerMask.GetMask("Terrain"));
        if (hitInfo.Equals(default(RaycastHit)))
            return;

        Terrain terrain = hitInfo.collider.GetComponent<Terrain>();
        switch (Panel.modifierType)
        {
            case TerrainModifierPanel.ModifierType.Up:
            case TerrainModifierPanel.ModifierType.Down:
            case TerrainModifierPanel.ModifierType.Smooth:
                Game.TerrainModule.Refresh();
                Game.TerrainModule.AddOldData();
                break;
            case TerrainModifierPanel.ModifierType.AddTree:
                if (isAdd)
                    Game.TerrainModule.CreatTree(hitInfo.point, (int)(Panel.opticalMix.Value), (int)Panel.rangeMix.Value, Panel.PrototypeIndex);
                else
                    Game.TerrainModule.RemoveTree(hitInfo.point, (int)Panel.rangeMix.Value, Panel.PrototypeIndex);
                break;

            // TODO 路面桥梁
            //case TerrainModifierPanel.ModifierType.BuildRoad:
            //    RoadManager.Instance.ConnetWPN(panel.wayType, (int)Panel.widthMix.Value);
            //    selectObj = null;
            //    break;
            default:
                break;
        }
    }

    /// <summary>
    /// 右键点击事件
    /// </summary>
    public override void OnRightButtonDown()
    {
        // TODO 路面桥梁
        //switch (Panel.modifierType)
        //{
        //    case TerrainModifierPanel.ModifierType.BuildRoad:
        //        //RoadManager.AddWayPoints();
        //        RoadManager.Instance.AddWPN(RoadManager.Instance.LastWPN, panel.wayType, (int)Panel.widthMix.Value);
        //        break;
        //    case TerrainModifierPanel.ModifierType.Buildings:
        //        selectObj = BuildingManager.Instance.GetSelectBuild("Building");
        //        break;
        //    default:
        //        break;
        //}
    }

    /// <summary>
    /// 右键弹起事件
    /// </summary>
    public override void OnRightButtonUp()
    {
        switch (Panel.modifierType)
        {
            case TerrainModifierPanel.ModifierType.Buildings:
                selectObj = null;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 右键按住状态
    /// </summary>
    public override void OnRightButtonHold()
    {
        switch (Panel.modifierType)
        {
            case TerrainModifierPanel.ModifierType.Buildings:
                //BuildingManager.Instance.SetObjPosAndRot(selectObj);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 按键操作
    /// </summary>
    public override void OnUpdate()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            // TODO 路面桥梁
            //RoadManager.Instance.SetShow(RoadManager.Instance.toBeShow);
            //RoadManager.Instance.toBeShow = !RoadManager.Instance.toBeShow;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
            isAdd = false;

        if (Input.GetKeyUp(KeyCode.LeftShift))
            isAdd = true;

        if (Projection.enabled)
        {
            // 更新投影的位置
            hitInfo = Utility.SendRay(LayerMask.GetMask("Terrain"));
            if (!hitInfo.Equals(default(RaycastHit)))
            {
                Projection.transform.position = hitInfo.point + Vector3.up * 10000;
            }
        }
    }

    /// <summary>
    /// 状态结束
    /// </summary>
    public override void OnExit()
    {
        Projection.enabled = false;
    }

    /// <summary>
    /// 设置投影的显示
    /// </summary>
    /// <param name="isShow"></param>
    public void SetProjectorShow(bool isShow = false)
    {
        Projection.enabled = isShow;
    }
}