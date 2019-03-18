using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class LoomTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Loom.Initialize();

        DoLoomThread();
    }

    void DoLoomThread()
    {
        //int a = 10;
        //Loom.RunAsync(() =>
        //{
        //    Debug.Log("异步开始");
        //    Loom.QueueOnMainThread(() =>
        //    {
        //        Debug.Log("回到主线程");
        //        GameObject go = new GameObject("asd");
        //        Loom.RunAsync(() =>
        //        {
        //            Debug.Log("有一个线程");
        //            Loom.QueueOnMainThread(() =>
        //            {
        //                Debug.Log("回到主线程");
        //            });
        //        });
        //    },3);
        //});
    }
}
