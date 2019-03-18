using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseEvent : Singleton<MouseEvent>
{
    /// <summary>
    /// 当前鼠标状态
    /// </summary>
    public MouseState CurrentState { get; private set; }

    /// <summary>
    /// 存储状态枚举和状态类对应关系的字典
    /// </summary>
    private Dictionary<MouseStateType, MouseState> stateDic;

    public MouseStateType CurrentStateType { get; private set; }

    /// <summary>
    /// 鼠标在上一帧的位置
    /// </summary>
    private Vector3 lastPosition;
    /// <summary>
    /// 鼠标是否移动
    /// </summary>
    public bool MouseMove { get; private set; }

    public MouseEvent()
    {
        InitDic();
        CurrentState = stateDic[MouseStateType.DefaultState];
        MonoEvent.Instance.UPDATE += Update;
    }

    void Update()
    {
        //处理鼠标事件 当点击UI面板时不处理
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                CurrentState.OnLeftButtonDown();
            }
            if (Input.GetMouseButton(0))
            {
                CurrentState.OnLeftButtonHold();
            }
            if (Input.GetMouseButton(1))
            {
                CurrentState.OnRightButtonHold();
            }
            if (Input.GetMouseButtonUp(0))
            {
                CurrentState.OnLeftButtonUp();
            }

            if (Input.GetMouseButtonDown(1))
            {
                CurrentState.OnRightButtonDown();
            }
            if (Input.GetMouseButtonUp(1))
            {
                CurrentState.OnRightButtonUp();
            }

            if (Input.GetMouseButtonDown(2))
            {
                //OnMouseRollDown();
            }
        }

        CurrentState.Update();

        if (Input.mousePosition != lastPosition)
        {
            lastPosition = Input.mousePosition;
            MouseMove = true;
        }
        else
        {
            MouseMove = false;
        }
    }

    /// <summary>
    /// 改变当前鼠标状态(带参数: 实体单位)
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="para"></param>
    public void ChangeState(MouseStateType _type, object para = null, params object[] args)
    {
        // 状态未改变
        if (CurrentStateType == _type)
        {
            CurrentState.OnActive(para, args);
            return;
        }

        CurrentState.OnDisactive();
        CurrentState = stateDic[_type];          // 更新状态

        if (!CurrentState.isInited)
            CurrentState.OnInit();
        CurrentState.OnActive(para, args);
        CurrentStateType = _type;                // 赋值当前状态类型
    }

    /// <summary>
    /// 获取特定类型的状态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public T GetState<T>(MouseStateType type) where T : MouseState
    {
        return (T)stateDic[type];
    }

    /// <summary>
    /// 初始化字典
    /// </summary>
    /// TODO:派生类无法解耦，没法打成程序集，想办法改掉
    private void InitDic()
    {
        stateDic = new Dictionary<MouseStateType, MouseState>
        {
            { MouseStateType.DefaultState, new MouseDefaultState() },
            { MouseStateType.TerrainModifier, new MouseTerrainModifierState() },
        };
    }
}