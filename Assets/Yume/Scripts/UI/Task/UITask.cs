using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;
using Yume.Resource;
using Yume.String;
using Yume.Task;

namespace Yume.UI {
  public class UITask : UIBase {
    public class UITaskTerm {
      private TaskDB m_task;
      public TaskDB task => m_task;
      private Text m_title;
      private GameObject m_root;
      private Text m_description;
      private YButton m_getReward;
      private RectTransform m_rewardContent;
      private RectTransform m_processContent;
      public Transform transform => m_root.transform;
      private static GameObject s_rewardPrefab;
      private static GameObject rewardPrefab {
        get {
          if (s_rewardPrefab == null) {
            s_rewardPrefab = ResourceManager.LoadAsset<GameObject>("UI/Prefab/reward");
          }
          return s_rewardPrefab;
        }
      }
      private static GameObject s_processTextPrefab;
      private static GameObject processTextPrefab {
        get {
          if (s_processTextPrefab == null) {
            s_processTextPrefab = ResourceManager.LoadAsset<GameObject>("UI/Prefab/processText");
          }
          return s_processTextPrefab;
        }
      }
      private List<Text> m_processTexts = new List<Text>();

      public UITaskTerm(TaskDB task, GameObject root) {
        m_task = task;
        m_root = root;

        var uiObject = root.GetComponent<UIObject>();
        m_title = uiObject.GetComponent<Text>("title");
        m_description = uiObject.GetComponent<Text>("description");
        m_getReward = uiObject.GetComponent<YButton>("getReward");
        m_rewardContent = uiObject.GetComponent<RectTransform>("rewardContent");
        m_processContent = uiObject.GetComponent<RectTransform>("processContent");

        _OnInitInternal();
      }

      private async void _OnInitInternal() {
        TaskInfo taskInfo = DBManager.instance.GetTaskInfo(task.type, task.id);
        m_title.text = taskInfo.title;
        m_description.text = taskInfo.description;

        foreach (ItemDB item in taskInfo.taskRewards.items) {
          GameObject temp = Object.Instantiate(rewardPrefab, m_rewardContent);
          temp.GetComponent<Image>().sprite = await ResourceManager.LoadAssetAsync<Sprite>
                                                    (DBManager.instance.GetItemInfo(item.id).iconFileName);
          temp.transform.GetChild(0).GetComponent<Text>().text = item.num.ToString();
        }

        for (int i = 0; i < task.taskRequests.Count; i++) {
          GameObject temp = Object.Instantiate(processTextPrefab, m_processContent);
          m_processTexts.Add(temp.GetComponent<Text>());
        }

        if (task.state == TaskState.COMPLETED) {
          transform.SetAsFirstSibling();
        }

        _AddListener();
        _UpdateTaskInfo();

        EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_INFO, _UpdateTaskInfo);
      }

      private void _AddListener() {
        m_getReward.onClick.AddListener(_GetReward);
      }

      private void _UpdateTaskInfo() {
        if (m_processContent) {
          for (int i = 0; i < task.taskRequests.Count; i++) {
            m_processTexts[i].text = string.Format("{0}:{1}/{2}", task.taskRequests[i].description,
            task.taskRequests[i].currentNum, task.taskRequests[i].requiredNum);
          }
          m_processContent.gameObject.SetActive(task.state == TaskState.INPROCESS);
        }

        m_getReward.gameObject.SetActive(task.state == TaskState.COMPLETED || task.state == TaskState.FINAL);
        m_getReward.interactable = task.state == TaskState.COMPLETED;
        m_getReward.textComponent.text = task.state == TaskState.FINAL ? StringMap.COMPLETED : StringMap.CLICK_GET;
      }

      private void _GetReward() {
        m_getReward.textComponent.text = StringMap.COMPLETED;
        m_getReward.interactable = false;
        transform.SetAsLastSibling();
        DBManager.instance.CompleteTask(task);
        DBManager.instance.UpdateUserData();
      }

      public void Destroy() {
        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_TASK_INFO, _UpdateTaskInfo);
      }
    }

    private class UITaskTermGroup {
      private TaskType m_taskType;
      private RectTransform m_taskContent;
      private static GameObject s_taskItemPrefab;
      private static GameObject taskItemPrefab {
        get {
          if (s_taskItemPrefab == null) {
            s_taskItemPrefab = ResourceManager.LoadAsset<GameObject>("UI/Prefab/taskTerm");
          }
          return s_taskItemPrefab;
        }
      }
      private float m_length;
      private List<TaskDB> m_tasks;
      private List<UITaskTerm> m_taskItemList = new List<UITaskTerm>();

      public UITaskTermGroup(TaskType type, RectTransform content) {
        m_taskType = type;
        m_taskContent = content;
        m_tasks = DBManager.instance.GetTaskList(m_taskType);
        GridLayoutGroup grid = m_taskContent.GetComponent<GridLayoutGroup>();
        m_length = grid.cellSize.y + grid.spacing.y;
        int taskViewNum = 0;

        foreach (TaskDB task in m_tasks) {
          TaskInfo taskInfo = DBManager.instance.GetTaskInfo(task.type, task.id);
          if (task.state != TaskState.INACTIVE) {
            taskViewNum++;
            GameObject taskItemRoot = Object.Instantiate(taskItemPrefab, m_taskContent);
            m_taskItemList.Add(new UITaskTerm(task, taskItemRoot));
          }
        }

        foreach (UITaskTerm taskView in m_taskItemList) {
          if (taskView.task.state == TaskState.FINAL) {
            taskView.transform.SetAsLastSibling();
          }
        }

        m_taskContent.sizeDelta = new Vector2(m_taskContent.sizeDelta.x, m_length * taskViewNum - grid.spacing.y);

        EventCenter.AddListener<TaskDB>(Broadcast.EventType.ADD_TASK_VIEW, AddTaskViewBy);
      }

      public void AddTaskViewBy(TaskDB task) {
        if (task.type == m_taskType && task.state != TaskState.INACTIVE) {
          m_taskContent.sizeDelta += new Vector2(0, m_length);
          GameObject taskTermRoot= Object.Instantiate(taskItemPrefab, m_taskContent);
          var taskTerm = new UITaskTerm(task, taskTermRoot);
          taskTerm.transform.SetAsFirstSibling();
          m_taskItemList.Add(taskTerm);
        }
      }

      public void Destroy() {
        foreach (var taskTerm in m_taskItemList) {
          taskTerm.Destroy();
        }
        EventCenter.RemoveListener<TaskDB>(Broadcast.EventType.ADD_TASK_VIEW, AddTaskViewBy);
      }
    }

    private List<UITaskTermGroup> m_taskGroupList = new List<UITaskTermGroup>();

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      foreach (TaskType type in System.Enum.GetValues(typeof(TaskType))) {
        var content = GetComponent<RectTransform>($"{type.GetDescription()}/content");       
        m_taskGroupList.Add(new UITaskTermGroup(type, content));
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