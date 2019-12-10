using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XFramework;

public class ZTest : MonoBehaviour
{
    MapGenerator mapGenerator;

    public bool a;
    public int index;

    void Start()
    {
        mapGenerator = GetComponent<MapGenerator>();
        StartCoroutine(ChangeMesh());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var info = Utility.SendRay();

            if(info.collider != null)
            {
                if (!a)
                {
                    Game.TerrainModule.SetTextureNoMix(info.point, 20, index);
                }
                else
                {
                    Game.TerrainModule.SetTexture(info.point, 20, index, 1);
                }
            }
        }
    }

    IEnumerator ChangeMesh()
    {
        while (true)
        {
            mapGenerator.seed++;
            mapGenerator.DrawMapInEdior();
            yield return new WaitForSeconds(0.5f);
        }
    }
}