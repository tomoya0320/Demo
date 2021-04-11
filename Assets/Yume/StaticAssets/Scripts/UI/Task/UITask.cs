using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.RedDotSystem;

namespace Yume.UI {
  public class UITask : UIGaussianBlurBg {
    private class UITaskTerm : UIChildBase, IScrollTerm<TaskData> {
      private Text m_title;
      private Text m_process;
      private TaskData m_task;
      private Text m_description;
      private TaskType m_taskType;
      private YButton m_getReward;
      private UIRedDot m_taskRedDot;
      public bool isValid => m_task != null;
      private ObjectContainer[] m_rewardObjects;

      public UITaskTerm(GameObject root, TaskType type) : base(root) {
        m_title = GetComponent<Text>("title");
        m_process = GetComponent<Text>("process");
        m_getReward = GetComponent<YButton>("getReward");
        m_description = GetComponent<Text>("description");
        m_rewardObjects = GetGameObject("rewardContent").GetComponentsInChildren<ObjectContainer>(true);

        m_taskType = type;
        m_taskRedDot = new UIRedDot(RedDotTreeType.MAIN_TASK, m_task?.id, 
          m_taskType.GetDescription(), m_getReward.transform.Find(StringMap.UI_RED_DOT));

        _AddListener();

        EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _UpdateTaskProcess);
      }

      private void _AddListener() {
        m_getReward.onClick.AddListener(_GetReward);
      }

      private void _UpdateTaskProcess() {
        if (m_task != null) {
          var taskRequest = ResourceManager.Load<TaskTemplate>(m_task.id).taskRequest;

          if (taskRequest.type == RequestType.LV_UP_TO) {
            // 等级的显示要特殊处理(+1)
            m_process.text = $"{taskRequest.description}:{m_task.process + 1}/{taskRequest.requiredCount + 1}";
          } else {
            m_process.text = $"{taskRequest.description}:{m_task.process}/{taskRequest.requiredCount}";
          }
          m_getReward.interactable = m_task.state == TaskState.COMPLETED;
          m_process.gameObject.SetActiveEx(m_task.state == TaskState.INPROCESS);
          m_getReward.gameObject.SetActiveEx(m_task.state == TaskState.COMPLETED || m_task.state == TaskState.FINAL);
          m_getReward.textComponent.text = m_task.state == TaskState.FINAL ? StringMap.COMPLETED : StringMap.CLICK_GET;
          m_getReward.image.color = m_task.state == TaskState.FINAL ? Colors.DISABLE_BUTTON : m_getReward.colors.normalColor;
        }
      }

      private void _GetReward() {
        m_getReward.interactable = false;
        m_getReward.image.color = Colors.DISABLE_BUTTON;
        m_getReward.textComponent.text = StringMap.COMPLETED;

        DataManager.CompleteTask(m_task);
      }

      public void UpdateScrollTerm(TaskData task) {
        m_task = task;

        m_taskRedDot.Update(m_task?.id);
        gameObject.SetActiveEx(m_task != null);

        if (m_task != null) {
          TaskTemplate taskTemplate = ResourceManager.Load<TaskTemplate>(m_task.id);
          m_title.text = taskTemplate.title;
          m_description.text = taskTemplate.description;

          //初始化任务奖励
          for (int i = 0; i < m_rewardObjects.Length; i++) {
            var rewardObject = m_rewardObjects[i];
            if (i < taskTemplate.taskRewards.items.Length) {
              rewardObject.gameObject.SetActiveEx(true);
              rewardObject.GetComponent<Image>("icon").SetSpriteAsync(ResourceManager.Load<ItemTemplate>(taskTemplate.taskRewards.items[i].id).iconFileName);
              rewardObject.GetComponent<Text>("num").text = taskTemplate.taskRewards.items[i].count.ToString();
            } else {
              rewardObject.gameObject.SetActiveEx(false);
            }
          }

          //刷新任务进度
          _UpdateTaskProcess();
        }
      }

      public override void Destroy() {
        m_taskRedDot.Destroy();
        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _UpdateTaskProcess);
      }

      public IScrollTerm<TaskData> Clone() {
        var clone = ObjectUtil.Instantiate(gameObject, gameObject.GetParent());
        return new UITaskTerm(clone, m_taskType);
      }
    }

    private class UITaskGroup : UIChildBase, IScrollGroup<TaskData, UITaskTerm> {
      private UIRedDot m_taskRedDot;
      private List<TaskData> m_tasks;
      private ScrollRect m_taskScrollRect;
      public List<TaskData> dataList => m_taskShowList;
      public List<UITaskTerm> viewList => m_taskTermList;
      private UIScrollView<TaskData, UITaskTerm> m_taskScrollView;
      private List<TaskData> m_taskShowList = new List<TaskData>();
      private List<UITaskTerm> m_taskTermList = new List<UITaskTerm>();

      public UITaskGroup(TaskType taskType, GameObject root) : base(root) {
        m_taskScrollRect = GetComponent<ScrollRect>("ScrollRect");

        m_tasks = DataManager.GetTaskList(taskType);

        for (int i = 0; i < m_taskScrollRect.content.childCount; i++) {
          m_taskTermList.Add(new UITaskTerm(m_taskScrollRect.content.GetChild(i).gameObject, taskType));
        }

        m_taskScrollView = new UIScrollView<TaskData, UITaskTerm>(m_taskScrollRect, this);

        m_taskRedDot = new UIRedDot(RedDotTreeType.MAIN_TASK, taskType.GetDescription(), MainTaskRedDotTree.MAIN_TASK,
          GetTransform("Button").Find(StringMap.UI_RED_DOT));

        EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _ResortTasks);
        EventCenter.AddListener(Broadcast.EventType.REFRESH_TASK_VIEW, _RefreshTaskView);
      }

      public override void OnOpen() {
        m_tasks.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_taskShowList.Clear();
        m_taskShowList.AddRange(m_tasks.FindAll(t => t.state != TaskState.INACTIVE));
        m_taskShowList.Sort((a, b) => a.state.CompareTo(b.state));
        m_taskScrollView.ResetNormalizedPos();
      }

      public override void OnUpdate() {
        m_taskScrollView.OnUpdate();
      }

      public override void Destroy() {
        foreach (var taskTerm in m_taskTermList) {
          taskTerm.Destroy();
        }
        m_taskRedDot.Destroy();

        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_TASK_PROCESS, _ResortTasks);
        EventCenter.RemoveListener(Broadcast.EventType.REFRESH_TASK_VIEW, _RefreshTaskView);
      }

      private void _RefreshTaskView() {
        m_taskShowList.Clear();
        foreach (TaskData task in m_tasks) {
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

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_taskGroupSwitch = GetComponent<UIGroupSwitch>("task group");

      foreach (TaskType type in Enum.GetValues(typeof(TaskType))) {
        m_taskGroupList.Add(new UITaskGroup(type, GetGameObject(type.GetDescription())));
      }

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      m_taskGroupSwitch.OnOpen();
      m_taskGroupList.ForEach(taskGroup => taskGroup.OnOpen());
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