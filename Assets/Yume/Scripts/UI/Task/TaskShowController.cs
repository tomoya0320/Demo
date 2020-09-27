using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.String;
using Yume.Task;

namespace Yume.UI {
  public class TaskShowController : MonoBehaviour {
    [SerializeField]
    private TaskType _taskType;
    [SerializeField]
    private RectTransform _taskContent;
    private List<TaskDB> m_tasks;
    private GameObject m_taskShow;
    private float m_length;

    private async void Start() {
      EventCenter.AddListener<TaskDB>(Broadcast.EventType.ADD_TASK, _AddTask);

      m_taskShow = await Addressables.LoadAssetAsync<GameObject>(StringMap.TASK_SHOW).Task;
      m_tasks = DBManager.GetTaskList(_taskType);
      GridLayoutGroup grid = _taskContent.GetComponent<GridLayoutGroup>();
      m_length = grid.cellSize.y + grid.spacing.y;
      int taskShowNum = 0;
      List<TaskShow> tempList = new List<TaskShow>();
      foreach (TaskDB task in m_tasks) {
        TaskInfo taskInfo = DBManager.GetTaskInfo(task.type, task.id);
        if (task.state != TaskState.INACTIVE && !taskInfo.includeDialogue) {
          taskShowNum++;
          GameObject tempTaskShow = Instantiate(m_taskShow, _taskContent);
          tempTaskShow.GetComponent<TaskShow>().OnInit(task);
          tempList.Add(tempTaskShow.GetComponent<TaskShow>());
        }
      }

      foreach (TaskShow taskShow in tempList) {
        if (taskShow.task.state == TaskState.FINAL) {
          taskShow.transform.SetAsLastSibling();
        }
      }

      _taskContent.sizeDelta = new Vector2(_taskContent.sizeDelta.x, m_length * taskShowNum);
    }

    private void OnDestroy() {
      EventCenter.RemoveListener<TaskDB>(Broadcast.EventType.ADD_TASK, _AddTask);
    }

    private void _AddTask(TaskDB task) {
      if(task.type == _taskType) {
        TaskInfo taskInfo = DBManager.GetTaskInfo(task.type, task.id);
        if (task.state != TaskState.INACTIVE && !taskInfo.includeDialogue) {
          _taskContent.sizeDelta += new Vector2(0, m_length);
          GameObject tempTaskShow = Instantiate(m_taskShow, _taskContent);
          tempTaskShow.GetComponent<TaskShow>().OnInit(task);
          tempTaskShow.transform.SetAsFirstSibling();
        }
      }
    }
  }
}