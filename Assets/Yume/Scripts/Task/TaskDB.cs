using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Battle;
using Yume.DB;
using Yume.String;
using Yume.UI;
using Yume.Resource;
using System.ComponentModel;
using Yume.RedPointSystem;

namespace Yume.Task {
  /// <summary>
  /// 数值代表排序的优先级，值越小越靠前
  /// </summary>
  public enum TaskState {
    /// <summary>
    /// 不会参与排序
    /// </summary>
    INACTIVE = 0,
    COMPLETED = 1,
    INPROCESS = 2,
    FINAL = 3
  }

  public enum TaskType {
    [Description("daily")]
    DAILY,
    [Description("main")]
    MAIN
  }

  public enum RequestType {
    ITEM_NUM,
    TASK,
    LV_UP_TO
  }

  public enum Method {
     HAD, CONSUME, ADD
  }

  [Serializable]
  public class TaskRequestDB {
    public Method method;
    /// <summary>
    /// 完成任务后是否消耗所需物品
    /// </summary>
    public bool consumable;
    /// <summary>
    /// 是否忽略需求的ID
    /// </summary>
    public bool ignoreId;
    public RequestType type;
    public string requirementId;
    public string description;
    public int requiredNum;
    public int currentNum;

    public bool Check() {
      return currentNum >= requiredNum;
    }
  }

  [Serializable]
  public struct TaskRewards {
    public ItemDB[] items;
  }

  public struct TaskInfo {
    public string id;
    public TaskType type;
    public string activeTaskId;
    public TaskType activeTaskType;
    public string title;
    public string description;
    public TaskRewards taskRewards;
  }

  [Serializable]
  public class TaskDB {
    public string id;
    public TaskType type;
    public TaskState state;
    public List<TaskRequestDB> taskRequests;

    public void OnInit() {
      foreach (TaskRequestDB taskRequest in taskRequests) {
        if(taskRequest.method == Method.HAD) {
          _SyncHadNum(taskRequest);
        }
      }
      _CheckInternal();
    }

    private void _SyncHadNum(TaskRequestDB taskRequest) {
      //同步现有的数量
      switch (taskRequest.type) {
        case RequestType.ITEM_NUM: {
            taskRequest.currentNum = DBManager.instance.GetItemDBNum(id);
            break;
          }
        case RequestType.LV_UP_TO: {
            taskRequest.currentNum = DBManager.instance.userLv;
            break;
          }
      }
    }

    public void Update(RequestType type, string id, int num) {
      foreach (TaskRequestDB taskRequest in taskRequests) {
        if (taskRequest.type == type && (taskRequest.requirementId == id || taskRequest.ignoreId)) {
          switch (taskRequest.method) {
            case Method.ADD: {
                taskRequest.currentNum += num > 0 ? num : 0;
                break;
              }
            case Method.CONSUME: {
                taskRequest.currentNum += num < 0 ? -num : 0;
                break;
              }
            case Method.HAD: {
                _SyncHadNum(taskRequest);
                break;
              }
          }
        }
      }
      _CheckInternal();
    }

    private void _CheckInternal() {
      if (state == TaskState.INPROCESS || state == TaskState.COMPLETED) {
        bool completed = true;
        foreach (TaskRequestDB taskRequest in taskRequests) {
          completed = completed && taskRequest.Check();
        }

        if (completed && state == TaskState.INPROCESS) {
          TaskInfo taskInfo = DBManager.instance.GetTaskInfo(type, id);
          state = TaskState.COMPLETED;
          //更新RedPoint
          RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_TASK, id, type.GetDescription(), 1);
          MessageManager.instance.AddMessage(new Message
          (Message.Style.FADE, string.Format(StringMap.TASK_COMPLETED, taskInfo.title)));
        } else if (!completed && state == TaskState.COMPLETED) {
          state = TaskState.INPROCESS;
          //更新RedPoint
          RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_TASK, id, type.GetDescription(), 0);
        }
      }
    }

    public async void Complete() {
      state = TaskState.FINAL;
      TaskInfo taskInfo = DBManager.instance.GetTaskInfo(type, id);
      foreach (ItemDB item in taskInfo.taskRewards.items) {
        DBManager.instance.AddItems(item);
        var itemInfo = DBManager.instance.GetItemInfo(item.id);
        Sprite itemIcon = await ResourceManager.LoadAssetAsync<Sprite>(itemInfo.iconFileName);
        MessageManager.instance.AddMessage(new Message(Message.Style.FADE,
        string.Format(StringMap.GET + "{0}*{1}", itemInfo.name, item.num), itemIcon));
      }

      foreach (TaskRequestDB taskRequest in taskRequests) {
        if (taskRequest.consumable && taskRequest.type == RequestType.ITEM_NUM) {
          DBManager.instance.ConsumeItems(false, new ItemDB(taskRequest.requirementId, taskRequest.requiredNum));
        }
      }
      //更新RedPoint
      RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_TASK, id, type.GetDescription(), 0);
      DBManager.instance.UpdateUserData();
    }
  }
}