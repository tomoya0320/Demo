using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.Resource;
using Yume.String;
using Yume.Task;

namespace Yume.UI {
  public class UITask : UIBase {
    private class UITaskTerm : IScrollTerm<TaskDB> {
      private Text m_title;
      private TaskDB m_task;
      private GameObject m_root;
      private Text m_description;
      private YButton m_getReward;
      public TaskDB task => m_task;
      private RectTransform m_rewardContent;
      private RectTransform m_processContent;
      public GameObject gameObject => m_root;
      public Transform transform => m_root.transform;

      public UITaskTerm(GameObject root) {
        m_root = root;
        var uiObject = root.GetComponent<UIObject>();
        m_title = uiObject.GetComponent<Text>("title");
        m_description = uiObject.GetComponent<Text>("description");
        m_getReward = uiObject.GetComponent<YButton>("getReward");
        m_rewardContent = uiObject.GetComponent<RectTransform>("rewardContent");
        m_processContent = uiObject.GetComponent<RectTransform>("processContent");

        _AddListener();

        EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _UpdateTaskProcess);
      }

      private void _AddListener() {
        m_getReward.onClick.AddListener(_GetReward);
      }

      private void _UpdateTaskProcess() {
        if (task != null) {
          for (int i = 0; i < m_processContent.childCount; i++) {
            var tempGo = m_processContent.GetChild(i).gameObject;
            if (i < task.taskRequests.Count) {
              tempGo.SetActiveEx(true);
              tempGo.GetComponentInChildren<Text>().text = string.Format("{0}:{1}/{2}", task.taskRequests[i].description,
              task.taskRequests[i].currentNum, task.taskRequests[i].requiredNum);
            } else {
              tempGo.SetActiveEx(false);
            }
          }

          m_getReward.interactable = task.state == TaskState.COMPLETED;
          m_processContent.gameObject.SetActiveEx(task.state == TaskState.INPROCESS);
          m_getReward.gameObject.SetActiveEx(task.state == TaskState.COMPLETED || task.state == TaskState.FINAL);
          m_getReward.textComponent.text = task.state == TaskState.FINAL ? StringMap.COMPLETED : StringMap.CLICK_GET;
          m_getReward.image.color = task.state == TaskState.FINAL ? 0.5f * Color.white : m_getReward.colors.normalColor;
        }
      }

      private void _GetReward() {
        m_getReward.interactable = false;
        m_getReward.image.color = 0.5f * Color.white;
        m_getReward.textComponent.text = StringMap.COMPLETED;

        DBManager.instance.CompleteTask(task);
        DBManager.instance.UpdateUserData();
      }

      public void Destroy() {
        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _UpdateTaskProcess);
      }

      public async void Update(TaskDB data) {
        m_task = data;
        if (task == null) {
          gameObject.SetActiveEx(false);
        } else {
          gameObject.SetActiveEx(true);
          TaskInfo taskInfo = DBManager.instance.GetTaskInfo(task.type, task.id);
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
    }

    private class UITaskGroup {
      private TaskType m_taskType;
      private List<TaskDB> m_tasks;
      private GridLayoutGroup m_taskGLG;
      private ScrollRect m_taskScrollRect;
      private List<TaskDB> m_taskShowList = new List<TaskDB>();
      private UIScrollView<TaskDB, UITaskTerm> m_taskScrollView;
      private List<UITaskTerm> m_taskTermList = new List<UITaskTerm>();

      public UITaskGroup(TaskType type, ScrollRect taskScrollRect) {
        m_taskType = type;
        m_taskScrollRect = taskScrollRect;
        m_taskGLG = m_taskScrollRect.content.GetComponent<GridLayoutGroup>();

        m_tasks = DBManager.instance.GetTaskList(m_taskType);

        foreach (TaskDB task in m_tasks) {
          if (task.state != TaskState.INACTIVE) {
            m_taskShowList.Add(task);
          }
        }
        //初始化任务排序
        m_taskShowList.Sort((a, b) => a.state.CompareTo(b.state));

        for (int i = 0; i < m_taskScrollRect.content.childCount; i++) {
          m_taskTermList.Add(new UITaskTerm(m_taskScrollRect.content.GetChild(i).gameObject));
        }

        m_taskScrollView = new UIScrollView<TaskDB, UITaskTerm>(m_taskScrollRect, m_taskGLG, m_taskShowList, m_taskTermList);

        EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _ResortTasks);
        EventCenter.AddListener(Broadcast.EventType.REFRESH_TASK_VIEW, _RefreshTaskView);
      }

      public void Destroy() {
        foreach (var taskTerm in m_taskTermList) {
          taskTerm.Destroy();
        }
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
        m_taskScrollView.OnRefresh(m_taskShowList);
      }

      private void _ResortTasks() {
        m_taskShowList.Sort((a, b) => a.state.CompareTo(b.state));
        m_taskScrollView.OnRefresh(m_taskShowList);
      }
    }

    private List<UITaskGroup> m_taskGroupList = new List<UITaskGroup>();

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      foreach (TaskType type in System.Enum.GetValues(typeof(TaskType))) {
        var scrollRect = GetComponent<ScrollRect>($"{type.GetDescription()}/scrollRect");       
        m_taskGroupList.Add(new UITaskGroup(type, scrollRect));
      }

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      foreach (var taskGroup in m_taskGroupList) {
        taskGroup.Destroy();      
      }
    }
  }
}