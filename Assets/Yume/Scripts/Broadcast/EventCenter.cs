using System;
using System.Collections.Generic;

namespace Yume.Broadcast {
  public class EventCenter {
    private static Dictionary<EventType, Delegate> s_EventTable = new Dictionary<EventType, Delegate>();

    private static void _OnListenerAdding(EventType eventType, Delegate callBack) {
      if (!s_EventTable.ContainsKey(eventType)) {
        s_EventTable.Add(eventType, null);
      }
      Delegate d = s_EventTable[eventType];
      if (d != null && d.GetType() != callBack.GetType()) {
        throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件所对应的委托是{1}，要添加的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
      }
    }

    private static void _OnListenerRemoving(EventType eventType, Delegate callBack) {
      if (s_EventTable.ContainsKey(eventType)) {
        Delegate d = s_EventTable[eventType];
        if (d == null) {
          throw new Exception(string.Format("移除监听错误：事件{0}没有对应的委托", eventType));
        } else if (d.GetType() != callBack.GetType()) {
          throw new Exception(string.Format("移除监听错误：尝试为事件{0}移除不同类型的委托，当前委托类型为{1}，要移除的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
        }
      } else {
        throw new Exception(string.Format("移除监听错误：没有事件码{0}", eventType));
      }
    }

    private static void _OnListenerRemoved(EventType eventType) {
      if (s_EventTable[eventType] == null) {
        s_EventTable.Remove(eventType);
      }
    }

    //no parameters
    public static void AddListener(EventType eventType, CallBack callBack) {
      _OnListenerAdding(eventType, callBack);
      s_EventTable[eventType] = (CallBack)s_EventTable[eventType] + callBack;
    }

    //Single parameters
    public static void AddListener<T>(EventType eventType, CallBack<T> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T>)s_EventTable[eventType] + callBack;
    }

    //two parameters
    public static void AddListener<T, X>(EventType eventType, CallBack<T, X> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X>)s_EventTable[eventType] + callBack;
    }

    //three parameters
    public static void AddListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X, Y>)s_EventTable[eventType] + callBack;
    }

    //four parameters
    public static void AddListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X, Y, Z>)s_EventTable[eventType] + callBack;
    }

    //five parameters
    public static void AddListener<T, X, Y, Z, W>(EventType eventType, CallBack<T, X, Y, Z, W> callBack) {
      _OnListenerAdding(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X, Y, Z, W>)s_EventTable[eventType] + callBack;
    }

    //no parameters
    public static void RemoveListener(EventType eventType, CallBack callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_EventTable[eventType] = (CallBack)s_EventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //single parameters
    public static void RemoveListener<T>(EventType eventType, CallBack<T> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T>)s_EventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //two parameters
    public static void RemoveListener<T, X>(EventType eventType, CallBack<T, X> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X>)s_EventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //three parameters
    public static void RemoveListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X, Y>)s_EventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //four parameters
    public static void RemoveListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X, Y, Z>)s_EventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //five parameters
    public static void RemoveListener<T, X, Y, Z, W>(EventType eventType, CallBack<T, X, Y, Z, W> callBack) {
      _OnListenerRemoving(eventType, callBack);
      s_EventTable[eventType] = (CallBack<T, X, Y, Z, W>)s_EventTable[eventType] - callBack;
      _OnListenerRemoved(eventType);
    }

    //no parameters
    public static void Broadcast(EventType eventType) {
      Delegate d;
      if (s_EventTable.TryGetValue(eventType, out d)) {
        CallBack callBack = d as CallBack;
        if (callBack != null) {
          callBack();
        } else {
          throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
        }
      }
    }

    //single parameters
    public static void Broadcast<T>(EventType eventType, T arg) {
      Delegate d;
      if (s_EventTable.TryGetValue(eventType, out d)) {
        CallBack<T> callBack = d as CallBack<T>;
        if (callBack != null) {
          callBack(arg);
        } else {
          throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
        }
      }
    }

    //two parameters
    public static void Broadcast<T, X>(EventType eventType, T arg1, X arg2) {
      Delegate d;
      if (s_EventTable.TryGetValue(eventType, out d)) {
        CallBack<T, X> callBack = d as CallBack<T, X>;
        if (callBack != null) {
          callBack(arg1, arg2);
        } else {
          throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
        }
      }
    }

    //three parameters
    public static void Broadcast<T, X, Y>(EventType eventType, T arg1, X arg2, Y arg3) {
      Delegate d;
      if (s_EventTable.TryGetValue(eventType, out d)) {
        CallBack<T, X, Y> callBack = d as CallBack<T, X, Y>;
        if (callBack != null) {
          callBack(arg1, arg2, arg3);
        } else {
          throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
        }
      }
    }

    //four parameters
    public static void Broadcast<T, X, Y, Z>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4) {
      Delegate d;
      if (s_EventTable.TryGetValue(eventType, out d)) {
        CallBack<T, X, Y, Z> callBack = d as CallBack<T, X, Y, Z>;
        if (callBack != null) {
          callBack(arg1, arg2, arg3, arg4);
        } else {
          throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
        }
      }
    }

    //five parameters
    public static void Broadcast<T, X, Y, Z, W>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4, W arg5) {
      Delegate d;
      if (s_EventTable.TryGetValue(eventType, out d)) {
        CallBack<T, X, Y, Z, W> callBack = d as CallBack<T, X, Y, Z, W>;
        if (callBack != null) {
          callBack(arg1, arg2, arg3, arg4, arg5);
        } else {
          throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
        }
      }
    }
  }
}