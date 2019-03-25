// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-12-04 08:45:05
// 版本： V 1.0
// ==========================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Experimental.TerrainAPI;

public class TerrainTest : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.PushPanel(UIPanelType.TerrainModifier); 
        MouseEvent.Instance.ChangeState(MouseStateType.TerrainModifier); 
        TerrainUtility.ConfigActiveTerrains(); 

    }

    private void Terrain()
    {
        //TerrainPaintUtility
    }
}