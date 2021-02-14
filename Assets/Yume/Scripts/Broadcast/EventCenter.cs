using System;
using System.Collections.Generic;

namespace Yume.Broadcast {
  public static class EventCenter {
    private static Dictionary<EventType, Delegate> s_eventTable = new Dictionary<EventType, Delegate>();

    private static void _OnListenerAdding(EventType eventType, Delegate callBack) {
      if (!s_eventTable.ContainsKey(eventType)) {
        s_eventTable.Add(eventType, null);
      }
      Delegate d = s_eventTable[eventType];
      if (d != null && d.GetType() != callBack.GetType()) {
        throw new Exception($"尝试为事件{eventType}添加不同类型的委托，当前事件所对应的委托是{d.GetType()}，要添加的委托类型为{callBack.GetType()}");
      }
    }

    private static void _OnListenerRemoving(EventType eventType, Delegate callBack) {
      if (s_eventTable.ContainsKey(eventType)) {
        Delegate d = s_eventTable[eventType];
        if (d == null) {
          throw new Exception($"移除监听错误：事件{eventType}没有对应的委托");
        } else if (d.GetType() != callBack.GetType()) {
          throw new Exception($"移除监听错误：尝试为事件{eventType}移除不同类型的委托，当前委托类型为{d.GetType()}，要移除的委托类型为{callBack.GetType()}");
        }
      } else {
        throw new Exception($"移除监听错误：没有事件码{eventType}");
      }
    }

    private static void _OnListenerRemoved(EventType eventType) {
      if (s_eventTable[eventType] == null) {
        s_eventTable.Remove(eventType);
      }
    }

    //no parameters
    public static void AddListener(EventType eventType, CallBack callBack) {
      _OnListenerAdding(eventType, callBack);
      s_eventTable[eventType] = (CallBack)s_eventTable[eventType] + callBack;
    }

    //Single parameters
    public static void AddListener<T>(EventType eventType, CallBack<T> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T>)s_eventTable[eventType] + callBack;
    }

    //two parameters
    public static void AddListener<T, X>(EventType eventType, CallBack<T, X> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X>)s_eventTable[eventType] + callBack;
    }

    //three parameters
    public static void AddListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X, Y>)s_eventTable[eventType] + callBack;
    }

    //four parameters
    public static void AddListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X, Y, Z>)s_eventTable[eventType] + callBack;
    }

    //five parameters
    public static void AddListener<T, X, Y, Z, W>(EventType eventType, CallBack<T, X, Y, Z, W> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X, Y, Z, W>)s_eventTable[eventType] + callBack;
    }

    //no parameters
    public static void RemoveListener(EventType eventType, CallBack callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_eventTable[eventType] = (CallBack)s_eventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //single parameters
    public static void RemoveListener<T>(EventType eventType, CallBack<T> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T>)s_eventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //two parameters
    public static void RemoveListener<T, X>(EventType eventType, CallBack<T, X> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X>)s_eventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //three parameters
    public static void RemoveListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X, Y>)s_eventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //four parameters
    public static void RemoveListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X, Y, Z>)s_eventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //five parameters
    public static void RemoveListener<T, X, Y, Z, W>(EventType eventType, CallBack<T, X, Y, Z, W> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_eventTable[eventType] = (CallBack<T, X, Y, Z, W>)s_eventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //no parameters
    public static void Broadcast(EventType eventType) {
      if (s_eventTable.TryGetValue(eventType, out Delegate d)) {
        if (d is CallBack callBack) {
          callBack();
        } else {
          throw new Exception($"广播事件错误：事件{eventType}对应委托具有不同的类型");
        }
      }
    }

    //single parameters
    public static void Broadcast<T>(EventType eventType, T arg) {
      if (s_eventTable.TryGetValue(eventType, out Delegate d)) {
        if (d is CallBack<T> callBack) {
          callBack(arg);
        } else {
          throw new Exception($"广播事件错误：事件{eventType}对应委托具有不同的类型");
        }
      }
    }

    //two parameters
    public static void Broadcast<T, X>(EventType eventType, T arg1, X arg2) {
      if (s_eventTable.TryGetValue(eventType, out Delegate d)) {
        if (d is CallBack<T, X> callBack) {
          callBack(arg1, arg2);
        } else {
          throw new Exception($"广播事件错误：事件{eventType}对应委托具有不同的类型");
        }
      }
    }

    //three parameters
    public static void Broadcast<T, X, Y>(EventType eventType, T arg1, X arg2, Y arg3) {
      if (s_eventTable.TryGetValue(eventType, out Delegate d)) {
        if (d is CallBack<T, X, Y> callBack) {
          callBack(arg1, arg2, arg3);
        } else {
          throw new Exception($"广播事件错误：事件{eventType}对应委托具有不同的类型");
        }
      }
    }

    //four parameters
    public static void Broadcast<T, X, Y, Z>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4) {
      if (s_eventTable.TryGetValue(eventType, out Delegate d)) {
        if (d is CallBack<T, X, Y, Z> callBack) {
          callBack(arg1, arg2, arg3, arg4);
        } else {
          throw new Exception($"广播事件错误：事件{eventType}对应委托具有不同的类型");
        }
      }
    }

    //five parameters
    public static void Broadcast<T, X, Y, Z, W>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4, W arg5) {
      if (s_eventTable.TryGetValue(eventType, out Delegate d)) {
        if (d is CallBack<T, X, Y, Z, W> callBack) {
          callBack(arg1, arg2, arg3, arg4, arg5);
        } else {
          throw new Exception($"广播事件错误：事件{eventType}对应委托具有不同的类型");
        }
      }
    }
  }
}