// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-12-04 08:45:05
// 版本： V 1.0
// ==========================================
using UnityEngine;
using XFramework;
using XFramework.Fsm;

public class TerrainTest : ProcedureBase
{
    public override void Init()
    {
        Game.UIModule.Open(UIName.TerrainModifier);

        Game.FsmModule.GetFsm<MouseFsm>().ChangeState<MouseTerrainModifierState>();
        Game.TerrainModule.ConfigActiveTerrains();

        float[,,] a = Terrain.activeTerrain.terrainData.GetAlphamaps(0, 500, 10, 10);
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                a[i, j, 1] = 1;
            }
        }
        a[0, 0, 1] = 1;
        Terrain.activeTerrain.terrainData.SetAlphamaps(0, 500, a);
    }

    public override void OnUpdate()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    RaycastHit hit = Utility.SendRay(LayerMask.GetMask("Terrain"));
        //    Game.TerrainModule.SetTexture(hit.point, 10, 1);
        //}
    }
}