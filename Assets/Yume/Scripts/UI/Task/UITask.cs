using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.RedPointSystem;
using Yume.Resource;
using Yume.Task;

namespace Yume.UI {
  public class UITask : UIBase {
    private class UITaskTerm : UIChildBase, IScrollTerm<TaskDB> {
      private Text m_title;
      private TaskDB m_task;
      private Text m_description;
      private YButton m_getReward;
      private UIRedPoint m_taskRedPoint;
      public bool isValid => m_task != null;
      private RectTransform m_rewardContent;
      private RectTransform m_processContent;
      public Transform transform => gameObject.transform;

      public UITaskTerm(TaskType type, GameObject root) : base(root) {
        m_title = GetComponent<Text>("title");
        m_description = GetComponent<Text>("description");
        m_getReward = GetComponent<YButton>("getReward");
        m_rewardContent = GetComponent<RectTransform>("rewardContent");
        m_processContent = GetComponent<RectTransform>("processContent");

        var taskRedPointRoot = root.FindGo(StringMap.UI_RED_POINT);
        m_taskRedPoint = new UIRedPoint(RedPointTreeType.MAIN_TASK, m_task?.id, type.GetDescription(), taskRedPointRoot);

        _AddListener();

        EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _UpdateTaskProcess);
      }

      private void _AddListener() {
        m_getReward.onClick.AddListener(_GetReward);
      }

      private void _UpdateTaskProcess() {
        if (m_task != null) {
          for (int i = 0; i < m_processContent.childCount; i++) {
            var tempGo = m_processContent.GetChild(i).gameObject;
            if (i < m_task.taskRequests.Count) {
              tempGo.SetActiveEx(true);
              tempGo.GetComponentInChildren<Text>().text = string.Format("{0}:{1}/{2}", m_task.taskRequests[i].description,
              m_task.taskRequests[i].currentNum, m_task.taskRequests[i].requiredNum);
            } else {
              tempGo.SetActiveEx(false);
            }
          }

          m_getReward.interactable = m_task.state == TaskState.COMPLETED;
          m_processContent.gameObject.SetActiveEx(m_task.state == TaskState.INPROCESS);
          m_getReward.gameObject.SetActiveEx(m_task.state == TaskState.COMPLETED || m_task.state == TaskState.FINAL);
          m_getReward.textComponent.text = m_task.state == TaskState.FINAL ? StringMap.COMPLETED : StringMap.CLICK_GET;
          m_getReward.image.color = m_task.state == TaskState.FINAL ? Colors.DISABLE_BUTTON : m_getReward.colors.normalColor;
        }
      }

      private void _GetReward() {
        m_getReward.interactable = false;
        m_getReward.image.color = Colors.DISABLE_BUTTON;
        m_getReward.textComponent.text = StringMap.COMPLETED;

        DBManager.instance.CompleteTask(m_task);
        DBManager.instance.UpdateUserData();
      }

      public async void UpdateScrollTerm(TaskDB data) {
        m_task = data;

        m_taskRedPoint.Update(m_task?.id);
        gameObject.SetActiveEx(m_task != null);

        if (m_task != null) {
          TaskInfo taskInfo = DBManager.instance.GetTaskInfo(m_task.type, m_task.id);
          m_title.text = taskInfo.title;
          m_description.text = taskInfo.description;

          //初始化任务奖励
          for (int i = 0; i < m_rewardContent.childCount; i++) {
            var tempGo = m_rewardContent.GetChild(i).gameObject;
            if (i < taskInfo.taskRewards.items.Length) {
              tempGo.SetActiveEx(true);
              tempGo.GetComponentInChildren<Image>().sprite = await ResourceManager.LoadAssetAsync<Sprite>
                     (DBManager.instance.GetItemInfo(taskInfo.taskRewards.items[i].id).iconFileName);
              tempGo.GetComponentInChildren<Text>().text = taskInfo.taskRewards.items[i].num.ToString();
            } else {
              tempGo.SetActiveEx(false);
            }
          }

          //刷新任务进度
          _UpdateTaskProcess();
        }
      }

      public override void Destroy() {
        m_taskRedPoint.Destroy();
        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _UpdateTaskProcess);
      }
    }

    private class UITaskGroup : UIChildBase {
      private List<TaskDB> m_tasks;
      private UIRedPoint m_taskRedPoint;
      private ScrollRect m_taskScrollRect;
      private List<TaskDB> m_taskShowList = new List<TaskDB>();
      private UIScrollView<TaskDB, UITaskTerm> m_taskScrollView;
      private List<UITaskTerm> m_taskTermList = new List<UITaskTerm>();

      public UITaskGroup(TaskType taskType, GameObject root) : base(root) {
        m_taskScrollRect = GetComponent<ScrollRect>("ScrollRect");

        m_tasks = DBManager.instance.GetTaskList(taskType);

        foreach (TaskDB task in m_tasks) {
          if (task.state != TaskState.INACTIVE) {
            m_taskShowList.Add(task);
          }
        }
        //初始化任务排序
        m_taskShowList.Sort((a, b) => a.state.CompareTo(b.state));

        for (int i = 0; i < m_taskScrollRect.content.childCount; i++) {
          m_taskTermList.Add(new UITaskTerm(taskType, m_taskScrollRect.content.GetChild(i).gameObject));
        }

        m_taskScrollView = new UIScrollView<TaskDB, UITaskTerm>(m_taskScrollRect, m_taskShowList, m_taskTermList);

        var taskRedPointRoot = GetGameObject("Button").FindGo(StringMap.UI_RED_POINT);
        m_taskRedPoint = new UIRedPoint(RedPointTreeType.MAIN_TASK, taskType.GetDescription(), MainTaskRedPointTree.MAIN_TASK, taskRedPointRoot);

        EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _ResortTasks);
        EventCenter.AddListener(Broadcast.EventType.REFRESH_TASK_VIEW, _RefreshTaskView);
      }

      public override void OnUpdate() {
        m_taskScrollView.OnUpdate();
      }

      public override void Destroy() {
        foreach (var taskTerm in m_taskTermList) {
          taskTerm.Destroy();
        }
        m_taskRedPoint.Destroy();

        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _ResortTasks);
        EventCenter.RemoveListener(Broadcast.EventType.REFRESH_TASK_VIEW, _RefreshTaskView);
      }

      private void _RefreshTaskView() {
        m_taskShowList.Clear();
        foreach (TaskDB task in m_tasks) {
          if (task.state != TaskState.INACTIVE) {
            m_taskShowList.Add(task);
          }
        }
        m_taskScrollView.UpdateView();
      }

      private void _ResortTasks() {
        m_taskShowList.Sort((a, b) => a.state.CompareTo(b.state));
        m_taskScrollView.UpdateView();
      }
    }

    private UIGroupSwitch m_taskGroupSwitch;
    private List<UITaskGroup> m_taskGroupList = new List<UITaskGroup>();

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_taskGroupSwitch = GetComponent<UIGroupSwitch>("task group");

      foreach (TaskType type in Enum.GetValues(typeof(TaskType))) {
        m_taskGroupList.Add(new UITaskGroup(type, GetGameObject($"{type.GetDescription()}/root")));
      }

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      m_taskGroupSwitch.OnOpen();
    }

    public override void OnUpdate() {
      foreach (var taskGroup in m_taskGroupList) {
        taskGroup.OnUpdate();
      }
    }

    public override void Destroy() {
      base.Destroy();
      foreach (var taskGroup in m_taskGroupList) {
        taskGroup.Destroy();      
      }
    }
  }
}