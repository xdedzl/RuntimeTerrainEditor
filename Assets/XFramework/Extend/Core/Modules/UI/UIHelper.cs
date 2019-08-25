using System;
using UnityEngine;
using UnityEngine.UI;
using XFramework.UI;
using XFramework;

/// <summary>
/// UI管理助手
/// </summary>
public class UIHelper : IGameModule
{
    /// <summary>
    /// UI管理器
    /// </summary>
    private readonly UIMgrDicType m_UIManager;

    private Transform canvasTransform;
    private Transform CanvasTransform
    {
        get
        {
            if (canvasTransform == null)
            {
                canvasTransform = GameObject.Find("Canvas")?.transform;
            }
            return canvasTransform;
        }
    }


    /// <summary>
    /// 提示
    /// </summary>
    private RectTransform tipRect;
    /// <summary>
    /// 提示
    /// </summary>
    private Text tipText;

    /// <summary>
    /// 确认操作的委托
    /// </summary>
    private Action VerifyOperate;

    public UIHelper()
    {
        m_UIManager = new UIMgrDicType();
        InitTip();
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    public void Open(string name, bool closable = false, object arg = null)
    {
        m_UIManager.OpenPanel(name, closable, arg);
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="name"></param>
    public void Close(string name)
    {
        m_UIManager.ClosePanel(name);
    }

    /// <summary>
    /// 获取面板
    /// </summary>
    public BasePanel GetPanel(string name)
    {
        return m_UIManager.GetPanel(name);
    }

    /// <summary>
    /// 关闭最近打开的面板
    /// </summary>
    public void CloseTopPanel()
    {
        m_UIManager.CloseTopPanel();
    }

    /// <summary>
    /// 初始化提示
    /// </summary>
    private void InitTip()
    {
        //tipRect = CanvasTransform.Find("Tip").GetComponent<RectTransform>();
        //tipText = tipRect.GetComponent<Text>();
    }

    /// <summary>
    /// 显示提示
    /// </summary>
    public void ShowTip(string content)
    {
        //tipRect.localPosition = Vector3.zero;
        //tipText.color = Color.red;
        //tipText.text = content;
    }

    private void InitVerify()
    {
    }

    /// <summary>
    /// 执行确认委托
    /// 除了VerifyPanel面板 不要调用
    /// </summary>
    public void ExecuteVerifyOperate()
    {
        VerifyOperate?.Invoke();
    }

    /// <summary>
    /// 打开确认面板
    /// </summary>
    /// <param name="showText"></param>
    /// <param name="action"></param>
    public void OpenVerifyOperateTip(string showText, Action action)
    {
        VerifyOperate = action;
        //Open(UIName.Verify, showText);
    }

    public int Priority { get { return m_UIManager.Priority; } }

    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        m_UIManager.Update(elapseSeconds, realElapseSeconds);
    }

    public void Shutdown()
    {
        m_UIManager.Shutdown();
    }
}