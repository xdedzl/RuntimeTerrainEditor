using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZTest : MonoBehaviour
{
    MapGenerator mapGenerator;

    void Start()
    {
        mapGenerator = GetComponent<MapGenerator>();
        StartCoroutine(ChangeMesh());
    }

    IEnumerator ChangeMesh()
    {
        while (true)
        {
            mapGenerator.seed++;
            mapGenerator.GenerateMap();
            yield return new WaitForSeconds(0.5f);
        }
    }
}