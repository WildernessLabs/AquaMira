using Meadow;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Thurston_Monitor.DT;

public static class Program
{
    private static int _mainThreadId;
    private static readonly ConcurrentQueue<Action> _mainThreadQueue = new();

    public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

    [STAThread]
    private static void Main(string[] args)
    {
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;

        _ = MeadowOS.Start(args);

        while (_mainThreadQueue.TryDequeue(out var action))
        {
            action();
        }
    }

    public static void ProcessMainThreadQueue()
    {
        while (_mainThreadQueue.TryDequeue(out var action))
        {
            action();
        }
    }

    public static void InvokeOnMainThread(Action action)
    {
        if (IsMainThread)
        {
            action();
        }
        else
        {
            _mainThreadQueue.Enqueue(action);
        }
    }
}