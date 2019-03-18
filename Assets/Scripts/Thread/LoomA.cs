using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

[ExecuteInEditMode]
/// <summary>
/// Multithreading support
/// </summary>
public class LoomA : MonoBehaviour
{
    private static LoomA _current;
    private int _count;
    /// <summary>
    /// Return the current instance
    /// </summary>
    public static LoomA Current
    {
        get
        {
            if (!_initialized)
                Initialize();
            return _current;
        }
    }

    static bool _initialized;
    static int _threadId = -1;
    static float time;

    public static void Initialize()
    {

        bool go = !_initialized;

        if (go && _threadId != -1 && _threadId != Thread.CurrentThread.ManagedThreadId)
            return;

        if (go)
        {
            GameObject g = new GameObject("Loom");
            //g.hideFlags = HideFlags.HideAndDontSave;  // 不在场景中展示
            DontDestroyOnLoad(g);
            _current = g.AddComponent<LoomA>();
            DontDestroyOnLoad(_current);
            _initialized = true;
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

    }

    private List<Action> _actions = new List<Action>();

    public class DelayedQueueItem
    {
        public float time;
        public Action action;
    }
    private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

    // Update is called once per frame
    void Update()
    {
        time = Time.time;
        var actions = new List<Action>();
        lock (_actions)
        {
            actions.AddRange(_actions);
            _actions.Clear();
            foreach (var a in actions)
            {
                a();
            }
        }
        lock (_delayed)
        {
            foreach (var delayed in _delayed.Where(d => d.time <= time).ToList())
            {
                _delayed.Remove(delayed);
                delayed.action();
            }
        }
    }

    void OnDestroy()
    {
        _initialized = false;
    }

    /// <summary>
    /// Queues an action on the main thread
    /// </summary>
    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0f);
    }
    /// <summary>
    /// Queues an action on the main thread after a delay
    /// </summary>
    public static void QueueOnMainThread(Action action, float _time)
    {
        if (_time != 0)
        {
            lock (Current._delayed)
            {

                Current._delayed.Add(new DelayedQueueItem { time = LoomA.time + _time, action = action });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions.Add(action);
            }
        }
    }

    /// <summary>
    /// Runs an action on another thread
    /// </summary>
    public static void RunAsync(Action action)
    {
        var t = new Thread(RunAction);
        t.Priority = System.Threading.ThreadPriority.Normal;
        t.Start(action);
    }

    private static void RunAction(object action)
    {
        ((Action)action)();
    }
}