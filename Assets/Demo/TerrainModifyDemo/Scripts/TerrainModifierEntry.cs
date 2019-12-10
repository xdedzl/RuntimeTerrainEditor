using UnityEngine;
using XFramework.Fsm;

public class TerrainModifierEntry : MonoBehaviour
{
    public void Start()
    {
        Game.UIModule.Open(UIName.TerrainModifier);

        Game.FsmModule.GetFsm<MouseFsm>().ChangeState<MouseTerrainModifierState>();
        Game.TerrainModule.ConfigActiveTerrains();
    }
}