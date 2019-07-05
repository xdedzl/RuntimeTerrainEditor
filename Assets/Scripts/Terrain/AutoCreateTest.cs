using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XFramework;

public class AutoCreateTest : ProcedureBase
{
    // Start is called before the first frame update
    public override void Init()
    {
        Game.UIModule.Open(UIName.TerrainModifier);
        Game.FsmModule.GetFsm<MouseFsm>().StartFsm<MouseTerrainModifierState>();
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TDSaveMgr.Instance.ReadTerrainInfo("2019-03-25 14-33-15");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Game.TerrainModule.ConfigActiveTerrains();
            Game.TerrainModule.ConfigTerrainData();
        }
    }
}