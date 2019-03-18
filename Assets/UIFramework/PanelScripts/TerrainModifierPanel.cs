// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-11-26 09:08:29
// 版本： V 1.0
// ==========================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    /// 卷轴类容预制
    /// </summary>
    private GameObject toogleImage;
    private Dictionary<ModifierType, Sprite[]> textureDic;

    protected override void Awake()
    {
        InitTextureDic();
        rect = GetComponent<RectTransform>();

        // 初始化组件
        rangeMix = transform.Find("Range").GetComponent<SliderMixInput>();
        opticalMix = transform.Find("Optical").GetComponent<SliderMixInput>();

        textureScroll = transform.Find("TextureScroll");
        scrollContent = transform.Find("TextureScroll/Viewport/Content");
        toogleImage = Resources.Load("UIPrefabs/ToggleImage") as GameObject;
    }

    private void Start()
    {
        rangeMix.SetMinMax(2, 50);
        opticalMix.SetMinMax(1, 10);
        transform.Find("Dropdown").GetComponent<Dropdown>().onValueChanged.AddListener((a) =>
        {
            modifierType = (ModifierType)a;
            textureDic.TryGetValue(modifierType, out Sprite[] sprits);

            switch (modifierType)
            {
                case ModifierType.Up:
                case ModifierType.Down:
                    opticalMix.SetMinMax(1, 10);
                    SetShow(true);
                    break;
                case ModifierType.Smooth:
                    opticalMix.SetMinMax(0.5f, 1.5f);
                    SetShow(true);
                    break;
                case ModifierType.AddTree:
                    opticalMix.SetMinMax(1, 100);
                    SetShow(true);
                    break;
                case ModifierType.AddDetial:
                    opticalMix.SetMinMax(1, 10);
                    SetShow(true);
                    break;
                default:
                    break;
            }

            // 自动设置卷轴贴图
            AutoSetToogle(sprits, toogleImage);

            // 通过类型修改投影的显示
            MouseEvent.Instance.GetState<MouseTerrainModifierState>(MouseStateType.TerrainModifier).OnDropDownChange(a);
        });
    }

    /// <summary>
    /// 初始化可选择的贴图
    /// </summary>
    private void InitTextureDic()
    {
        textureDic = new Dictionary<ModifierType, Sprite[]>
        {
            { ModifierType.AddDetial, Resources.LoadAll("Terrain/Details", typeof(Sprite)).Convert<Sprite>() },
            { ModifierType.AddTree, Resources.LoadAll("Terrain/Sprites/Trees", typeof(Sprite)).Convert<Sprite>() },
            { ModifierType.AddBuilding, Resources.LoadAll("Terrain/Sprites/Buildings", typeof(Sprite)).Convert<Sprite>() },
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
                Transform btn = Instantiate(toogleObj).transform;
                btn.SetParent(scrollContent, false);
            }
        }
        else
        {
            for (int i = 0; i < -differ; i++)
            {
                Transform btn = scrollContent.GetChild(scrollContent.childCount - 1);
                DestroyImmediate(btn.gameObject);
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

    public override void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public override void OnExit()
    {
        gameObject.SetActive(false);

        // 这个是临时的
        MouseEvent.Instance.GetState<MouseTerrainModifierState>(MouseStateType.TerrainModifier).SetProjectorShow(false);
    }

    public enum ModifierType
    {
        Up,           // 上升高度
        Down,         // 降低高度
        Smooth,       // 平滑地面 
        AddTree,      // 种树
        AddDetial,    // 种草
        AddBuilding,  // 添加建筑物 
        AttachTexture,// 附加贴图
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isSlider"></param>
    public void SetShow(bool isSlider)
    {
        rangeMix.gameObject.SetActive(isSlider);
        opticalMix.gameObject.SetActive(isSlider);
    }
}

/// <summary>
/// 用于地形修改的鼠标状态
/// </summary>
public class MouseTerrainModifierState : MouseState
{
    private bool isAdd = true;
    /// <summary>
    /// 选中的建筑
    /// </summary>
    private Transform selectBuild;

    /// <summary>
    /// 对应的地形修改面板
    /// </summary>
    private TerrainModifierPanel panel;
    private TerrainModifierPanel Panel
    {
        get
        {
            if (panel == null)
                panel = UIManager.Instance.GetPanel(UIPanelType.TerrainModifier) as TerrainModifierPanel;
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
                projector.material = Resources.Load("Terrain/Materials/ProjectorMat") as Material;
            }
            return projector;
        }
    }

    /// <summary>
    /// 初始化状态
    /// </summary>
    public override void OnInit()
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
    public override void OnActive(object para = null, params object[] args)
    {
        if ((int)panel.modifierType < 5)
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
                TerrainUtility.ChangeHeight(hitInfo.point, (int)Panel.rangeMix.Value, Panel.opticalMix.Value);
                break;
            case TerrainModifierPanel.ModifierType.Down:
                TerrainUtility.ChangeHeight(hitInfo.point, (int)Panel.rangeMix.Value, Panel.opticalMix.Value, false);
                break;
            case TerrainModifierPanel.ModifierType.Smooth:
                TerrainUtility.Smooth(hitInfo.point, (int)Panel.rangeMix.Value, Panel.opticalMix.Value);
                break;
            case TerrainModifierPanel.ModifierType.AddDetial:
                if (MouseEvent.Instance.MouseMove)
                {
                    if (isAdd)
                        TerrainUtility.AddDetial(terrain, hitInfo.point, Panel.rangeMix.Value, (int)(Panel.opticalMix.Value), Panel.PrototypeIndex);
                    else
                        TerrainUtility.RemoveDetial(terrain, hitInfo.point, Panel.rangeMix.Value, Panel.PrototypeIndex);
                }
                break;
            case TerrainModifierPanel.ModifierType.AttachTexture:
                if (MouseEvent.Instance.MouseMove)
                {
                    TerrainUtility.SetTexture(hitInfo.point, 1, 1);
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
                TerrainUtility.Refresh();
                TerrainUtility.AddOldData();
                break;
            case TerrainModifierPanel.ModifierType.AddTree:
                if (isAdd)
                    TerrainUtility.CreatTree(terrain, hitInfo.point, (int)(Panel.opticalMix.Value), (int)Panel.rangeMix.Value, Panel.PrototypeIndex);
                else
                    TerrainUtility.RemoveTree(terrain, hitInfo.point, (int)Panel.rangeMix.Value, Panel.PrototypeIndex);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 按键操作
    /// </summary>
    public override void Update()
    {
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
    public override void OnDisactive()
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

    public void SetShow(bool isSlider)
    {
    }
}