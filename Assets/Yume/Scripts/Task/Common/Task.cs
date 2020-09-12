using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Yume.DB;
using Yume.String;
using Yume.UI;

namespace Yume.Task {
  public enum TaskState {
    INACTIVE,
    INPROCESS,
    COMPLETED,
    FINAL
  }

  public enum TaskType {
    DAILY
  }

  public enum RequestType {
    PROP_NUM,
    TASK
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
    public bool isConsumable;
    /// <summary>
    /// 是否忽略需求的ID
    /// </summary>
    public bool isIgnoreId;
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
    public List<ItemDB> items;
  }

  public struct TaskInfo {
    public string id;
    public TaskType type;
    public string activeTaskId;
    public TaskType activeTaskType;
    public string title;
    public string description;
    public bool includeDialogue;
    public bool autoComplete;
    public TaskRewards taskRewards;
  }

  [Serializable]
  public class TaskDB {
    public string id;
    public TaskType type;
    public TaskState state;
    public List<TaskRequestDB> taskRequests;

    public void Update(RequestType type, string id, int num) {
      foreach (TaskRequestDB taskRequest in taskRequests) {
        if (taskRequest.type == type && (taskRequest.requirementId == id || taskRequest.isIgnoreId)) {
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
                //同步现有的数量
                //TODO:switch (requirement.type)
                break;
              }
          }
        }
      }
      Check();
    }

    public void Check() {
      if (state == TaskState.INPROCESS || state == TaskState.COMPLETED) {
        bool completed = true;
        foreach (TaskRequestDB taskRequest in taskRequests) {
          completed = completed && taskRequest.Check();
        }

        if (completed && state == TaskState.INPROCESS) {
          TaskInfo taskInfo = DBManager.GetTaskInfo(type, id);
          state = TaskState.COMPLETED;
          UIController.instance.messageManager.AddMessage(new Message
          (Message.Style.FADE, string.Format(StringMap.TASK_COMPLETED, taskInfo.title)));

          if (taskInfo.autoComplete) {
            DBManager.CompleteTask(this);
          }

        } else if (!completed && state == TaskState.COMPLETED) {
          state = TaskState.INPROCESS;
        }
      }
    }

    public async void Complete() {
      state = TaskState.FINAL;
      TaskInfo taskInfo = DBManager.GetTaskInfo(type, id);
      foreach (ItemDB item in taskInfo.taskRewards.items) {
        DBManager.AddItems(item);
        var itemInfo = DBManager.GetItemInfo(item.id);
        Sprite itemIcon = await Addressables.LoadAssetAsync<Sprite>(itemInfo.iconFileName).Task;
        UIController.instance.messageManager.AddMessage(new Message(Message.Style.FADE,
        string.Format(StringMap.GET + "{0}*{1}", itemInfo.name, item.num), itemIcon));
      }

      if (!string.IsNullOrEmpty(taskInfo.activeTaskId)) {
        //TODO:激活下一个任务
      }

      foreach (TaskRequestDB taskRequest in taskRequests) {
        if (taskRequest.isConsumable && taskRequest.type == RequestType.PROP_NUM) {
          DBManager.ConsumeItems(false, new ItemDB(taskRequest.requirementId, taskRequest.requiredNum));
        }
      }
    }
  }
}