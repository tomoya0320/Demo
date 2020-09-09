using System.Collections.Generic;
using Yume.Singleton;

namespace Yume.Task {
  public class TaskManager :SingletonNoMono<TaskManager> {
    private Dictionary<TaskType, List<Task>> m_tasksDict;

    protected override void OnInit() {
      base.OnInit();
      //TODO:获得用户当前所有任务的进度 m_tasks = ?
    }

    public void UpdateTaskState(RequirementType type, string id, int num) {
      foreach (var keyValuePair in m_tasksDict) {
        foreach (var task in keyValuePair.Value) {
          if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
            task.Update(type, id, num);
          }
        }
      }
      //TODO:更新任务状态的UI
    }
  }
}