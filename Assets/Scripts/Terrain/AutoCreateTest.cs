using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCreateTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.PushPanel(UIPanelType.TerrainModifier);
        Debug.Log(TerrainModule.Instance.mouseTerrainModifierState);
        MouseEvent.Instance.ChangeState(TerrainModule.Instance.mouseTerrainModifierState);
        //TerrainUtility.ConfigActiveTerrains();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TDSaveMgr.Instance.ReadTerrainInfo("2019-03-25 14-33-15");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            TerrainUtility.ConfigActiveTerrains();
            TerrainUtility.ConfigTerrainData();
        }
    }
}