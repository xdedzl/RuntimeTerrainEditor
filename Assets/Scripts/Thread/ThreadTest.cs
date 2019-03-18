using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ThreadTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Thread thread = new Thread(()=> 
        {
            Debug.Log("AAA");
        });
        thread.Start();


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
