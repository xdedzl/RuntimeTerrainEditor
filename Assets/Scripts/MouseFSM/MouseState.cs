#define Unit

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MouseStateType
{
    /// <summary>
    /// 默认空状态
    /// </summary>
    DefaultState,
    /// <summary>
    /// 地形编辑
    /// </summary>
    TerrainModifier,
}

public class MouseState
{
    /// <summary>
    /// 是否已经初始化
    /// </summary>
    public bool isInited { get; private set; }
    /// <summary>
    /// 检测射线信息
    /// </summary>
    protected RaycastHit hitInfo;

    public MouseState() { isInited = false; }
    /// <summary>
    /// 状态初始化，只会执行一次
    /// </summary>
    public virtual void OnInit() { isInited = true; }
    /// <summary>
    /// 状态激活时
    /// </summary>
    /// <param name="para"></param>
    public virtual void OnActive(object para = null, params object[] args) { }
    /// <summary>
    /// 状态结束时
    /// </summary>
    public virtual void OnDisactive() { }
    /// <summary>
    /// 左键按下
    /// </summary>
    public virtual void OnLeftButtonDown() { }
    /// <summary>
    /// 左键保持按下状态
    /// </summary>
    public virtual void OnLeftButtonHold() { }
    /// <summary>
    /// 右键保持按下状态
    /// </summary>
    public virtual void OnRightButtonHold() { }
    /// <summary>
    /// 左键抬起
    /// </summary>
    public virtual void OnLeftButtonUp() { }
    /// <summary>
    /// 右键按下
    /// </summary>
    public virtual void OnRightButtonDown() { }
    /// <summary>
    /// 右键抬起
    /// </summary>
    public virtual void OnRightButtonUp() { }
    /// <summary>
    /// 每帧调用
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// 发射一次射线更新hitInfo并返回当前鼠标接触到的物体
    /// </summary>
    /// <param name="layer">射线层级</param>
    /// <returns></returns>
    protected GameObject SendRay(int layer)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.MaxValue, layer))
        {
            return hitInfo.collider.gameObject;
        }
        else
        {
            return null;
        }
    }
}

/// <summary>
/// 默认鼠标状态
/// </summary>
public class MouseDefaultState : MouseState { }