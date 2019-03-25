using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCreateTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.PushPanel(UIPanelType.TerrainModifier);
        MouseEvent.Instance.ChangeState(MouseStateType.TerrainModifier);
        //TerrainUtility.ConfigActiveTerrains();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TDSaveMgr.Instance.ReadTerrainInfo("2019-03-25 14-33-15");
        }
    }
}
