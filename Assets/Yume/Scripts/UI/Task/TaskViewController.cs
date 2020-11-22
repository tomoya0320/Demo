using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.String;
using Yume.Task;

namespace Yume.UI {
  public class TaskViewController : MonoBehaviour {
    [SerializeField]
    private TaskType _taskType;
    [SerializeField]
    private RectTransform _taskContent;
    [SerializeField]
    private GameObject _taskViewPrefab;
    private List<TaskDB> m_tasks;
    private float m_length;

    private void Start() {
      EventCenter.AddListener<TaskDB>(Broadcast.EventType.ADD_TASK, _AddTask);

      m_tasks = DBManager.GetTaskList(_taskType);
      GridLayoutGroup grid = _taskContent.GetComponent<GridLayoutGroup>();
      m_length = grid.cellSize.y + grid.spacing.y;
      int taskViewNum = 0;
      List<TaskView> tempList = new List<TaskView>();
      foreach (TaskDB task in m_tasks) {
        TaskInfo taskInfo = DBManager.GetTaskInfo(task.type, task.id);
        if (task.state != TaskState.INACTIVE && !taskInfo.includeDialogue) {
          taskViewNum++;
          GameObject tempTaskView = Instantiate(_taskViewPrefab, _taskContent);
          tempTaskView.GetComponent<TaskView>().OnInit(task);
          tempList.Add(tempTaskView.GetComponent<TaskView>());
        }
      }

      foreach (TaskView taskView in tempList) {
        if (taskView.task.state == TaskState.FINAL) {
          taskView.transform.SetAsLastSibling();
        }
      }

      _taskContent.sizeDelta = new Vector2(_taskContent.sizeDelta.x, m_length * taskViewNum);
    }

    private void OnDestroy() {
      EventCenter.RemoveListener<TaskDB>(Broadcast.EventType.ADD_TASK, _AddTask);
    }

    private void _AddTask(TaskDB task) {
      if(task.type == _taskType) {
        TaskInfo taskInfo = DBManager.GetTaskInfo(task.type, task.id);
        if (task.state != TaskState.INACTIVE && !taskInfo.includeDialogue) {
          _taskContent.sizeDelta += new Vector2(0, m_length);
          GameObject tempTaskView = Instantiate(_taskViewPrefab, _taskContent);
          tempTaskView.GetComponent<TaskView>().OnInit(task);
          tempTaskView.transform.SetAsFirstSibling();
        }
      }
    }
  }
}