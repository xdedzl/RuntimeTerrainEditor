// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-11-26 09:08:29
// 版本： V 1.0
// ==========================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XFramework;

public enum ModifierType
{
    Terrain,  // 地形
    Tree,     // 种树
    Detial,   // 种草
    Texture,  // 地表纹理
}

public enum TerrainModifierType
{
    Up,          // 上升高度
    Down,        // 降低高度
    Smooth,      // 平滑地面
}

public class TerrainModifierPanel : MonoBehaviour
{
    /// <summary>
    /// 范围的滑动器输入框组合
    /// </summary>
    private SliderMix slider1;
    /// <summary>
    /// 力度的滑动器输入框组合
    /// </summary>
    private SliderMix slider2;

    /// <summary>
    /// 卷轴内容的第一层父物体
    /// </summary>
    private Transform scrollContent;

    private ModifierType m_ModifierType = ModifierType.Terrain;
    /// <summary>
    /// 地形修改模式
    /// </summary>
    public ModifierType ModifierType 
    {
        get
        {
            return m_ModifierType;
        }
        private set 
        {
            m_ModifierType = value;
            OnModifierTypeChange();
        }
    }

    private TerrainModifierType m_TerrainModifierType = TerrainModifierType.Up;
    /// <summary>
    /// 地形修改模式
    /// </summary>
    public TerrainModifierType TerrainModifierType
    {
        get
        {
            return m_TerrainModifierType;
        }
        private set
        {
            m_TerrainModifierType = value;
        }
    }

    /// <summary>
    /// 原型索引
    /// </summary>
    private int prototypeIndex;

    /// <summary>
    /// 卷轴类容预制
    /// </summary>
    private GameObject toogleImage;
    private Dictionary<ModifierType, Sprite[]> textureDic;

    public void Awake()
    {
        InitTextureDic();

        // 初始化组件

        GameObject paintTerrainDp = transform.Find("Menu/PaintTerrainDp").gameObject;

        transform.Find("Menu/PaintToggles/Terrain").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
        {
            ModifierType = ModifierType.Terrain;
            paintTerrainDp.SetActive(value);
        });

        var dp = paintTerrainDp.GetComponent<Dropdown>();
        dp.ClearOptions();
        dp.AddOptions(new List<string> { "Up", "Down", "Smooth" });
        dp.onValueChanged.AddListener((value) =>
        {
            TerrainModifierType = (TerrainModifierType)value;
        });

        transform.Find("Menu/PaintToggles/Tree").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
        {
            ModifierType = ModifierType.Tree;
        });
        transform.Find("Menu/PaintToggles/Detial").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
        {
            ModifierType = ModifierType.Detial;
        });
        transform.Find("Menu/PaintToggles/Texture").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
        {
            ModifierType = ModifierType.Texture;
        });

        slider1 = transform.Find("Menu/Sliders/Slider1").gameObject.AddComponent<SliderMix>();
        slider2 = transform.Find("Menu/Sliders/Slider2").gameObject.AddComponent<SliderMix>();

        scrollContent = transform.Find("TextureScroll/Viewport/Content");
        toogleImage = transform.Find("ToggleImageTemplate").gameObject;

        ModifierType = ModifierType.Terrain;
        RefeshToogleList();
    }

    private void OnModifierTypeChange()
    {
        switch (ModifierType)
        {
            case ModifierType.Terrain:
                slider1.Title = "半径";
                slider1.SetMinMax(1, 50);
                slider2.Title = "力度";
                slider2.SetMinMax(0, 10);
                break;
            case ModifierType.Tree:
                slider1.Title = "半径";
                slider1.SetMinMax(1, 50);
                slider2.Title = "数量";
                slider2.SetMinMax(1, 10);
                break;
            case ModifierType.Detial:
                slider1.Title = "半径";
                slider1.SetMinMax(1, 50);
                slider2.Title = "密度";
                slider2.SetMinMax(1, 10);
                break;
            case ModifierType.Texture:
                slider1.Title = "半径";
                slider1.SetMinMax(1, 50);
                slider2.Title = "力度";
                slider2.SetMinMax(1, 10);
                break;
            default:
                break;
        }

        // 自动设置卷轴贴图
        RefeshToogleList();
    }

    /// <summary>
    /// 初始化可选择的贴图
    /// </summary>
    private void InitTextureDic()
    {
        textureDic = new Dictionary<ModifierType, Sprite[]>
        {
            { ModifierType.Terrain, Resources.LoadAll<Sprite>("Terrain/Brushs") },
            { ModifierType.Tree, Resources.LoadAll<Sprite>("Terrain/Sprites/Trees") },
            { ModifierType.Detial, Resources.LoadAll<Sprite>("Terrain/Details")},
            { ModifierType.Texture, TexsToSprites(Resources.LoadAll<Texture2D>("Terrain/Textures")) },
        };
    }

    /// <summary>
    /// 适配Toogle的数量
    /// </summary>
    private void RefeshToogleList()
    {
        textureDic.TryGetValue(ModifierType, out Sprite[] sprits);
        int count = sprits == null ? 0 : sprits.Length;
        int differ = count - scrollContent.childCount;
        // 根据现有按钮数量补齐或删除
        if (differ >= 0)
        {
            for (int i = scrollContent.childCount, length = count; i < length; i++)
            {
                Transform btn = Object.Instantiate(toogleImage).transform;
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
        var toggles = scrollContent.GetComponentsInChildren<Toggle>();
        foreach (var item in toggles)
        {
            item.onValueChanged.AddListener((a) =>
            {
                if (a == true)
                {
                    // 其在父物体中的索引当作原型索引
                    prototypeIndex = item.transform.GetSiblingIndex();
                }
            });
        }
        toggles[0].isOn = true;

        // 为Image赋贴图
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            scrollContent.GetChild(i).GetComponent<Image>().sprite = sprits[i];
        }
    }

    #region 编辑逻辑

    /// <summary>
    /// 是否是增加状态
    /// </summary>
    private bool isAdd = true;
    /// <summary>
    /// 碰撞信息缓存
    /// </summary>
    private RaycastHit hitInfo;

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
                projector.material = Resources.Load<Material>("Projector/ProjectorMat");
            }
            return projector;
        }
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
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
                Projection.orthographicSize = slider1.Value;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            OnLeftButtonDown();
        }
        if (Input.GetMouseButton(0))
        {
            OnLeftButtonHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnLeftButtonUp();
        }

        if(Input.GetKey(KeyCode.Z))
        {
            Game.TerrainModule.Undo();
        }
    }

    public void OnLeftButtonDown()
    {
        
    }

    /// <summary>
    /// 左键按住
    /// </summary>
    public void OnLeftButtonHold()
    {
        hitInfo = Utility.SendRay(LayerMask.GetMask("Terrain"));
        if (hitInfo.Equals(default(RaycastHit)))
            return;

        switch (ModifierType)
        {
            case ModifierType.Terrain:
                switch (TerrainModifierType)
                {
                    case TerrainModifierType.Up:
                        Game.TerrainModule.ChangeHeightWithBrush(hitInfo.point, slider1.Value, slider2.Value, prototypeIndex, true, true);
                        break;
                    case TerrainModifierType.Down:
                        Game.TerrainModule.ChangeHeightWithBrush(hitInfo.point, slider1.Value, slider2.Value, prototypeIndex, false, true);
                        break;
                    case TerrainModifierType.Smooth:
                        Game.TerrainModule.Smooth(hitInfo.point, slider1.Value, slider2.Value, regesterUndo:true);
                        break;
                    default:
                        break;
                }
                break;
            case ModifierType.Detial:
                if (isAdd)
                    Game.TerrainModule.AddDetial(hitInfo.point, slider1.Value, (int)slider2.Value, prototypeIndex, true);
                else
                    Game.TerrainModule.RemoveDetial(hitInfo.point, slider1.Value, (int)slider2.Value, true);
                break;
            case ModifierType.Texture:
                Game.TerrainModule.SetTexture(hitInfo.point, slider1.Value, prototypeIndex, 0.05f * slider2.Value, true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 左键抬起
    /// </summary>
    private void OnLeftButtonUp()
    {
        hitInfo = Utility.SendRay(LayerMask.GetMask("Terrain"));
        if (hitInfo.collider != null) 
        {
            switch (ModifierType)
            {
                case ModifierType.Terrain:
                    Game.TerrainModule.Refresh();
                    break;
                case ModifierType.Tree:
                    if (isAdd)
                        Game.TerrainModule.CreatTree(hitInfo.point, (int)slider2.Value, (int)slider1.Value, prototypeIndex);
                    else
                        Game.TerrainModule.RemoveTree(hitInfo.point, (int)slider2.Value, prototypeIndex);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// texture转sprite
    /// </summary>
    /// <param name="texs"></param>
    /// <returns></returns>
    private Sprite[] TexsToSprites(Texture2D[] texs)
    {
        var sprites = new Sprite[texs.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = TexToSprite(texs[i]);
        }
        return sprites;
    }

    /// <summary>
    /// texture转sprite
    /// </summary>
    /// <param name="tex"></param>
    /// <returns></returns>
    private Sprite TexToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    #endregion
}

public class SliderMix : MonoBehaviour
{
    private Slider slider;
    private Text title;

    public float Value
    {
        get
        {
            return slider.value;
        }
    }

    public string Title
    {
        set
        {
            title.text = value;
        }
    }

    public void Awake()
    {
        slider = transform.Find("slider").GetComponent<Slider>();
        title = transform.Find("title").GetComponent<Text>();
    } 

    public void SetMinMax(float min, float max)
    {
        slider.minValue = min;
        slider.maxValue = max;
    }
}