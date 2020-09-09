using System.Collections.Generic;
using Yume.Singleton;

namespace Yume.Task {
  public class TaskManager :SingletonNoMono<TaskManager> {
    private List<List<Task>> m_tasks;

    protected override void OnInit() {
      base.OnInit();
      //TODO:获得用户当前所有任务的进度 m_tasks = ?
      foreach (List<Task> taskList in m_tasks) {
        foreach (Task task in taskList) {
          if (task.state == TaskState.INPROCESS) {
            InitTask(task);
          }
        }
      }
    }

    public void InitTask(Task task) {
      foreach (TaskRequirement requirement in task.taskRequirements) {
        if (requirement.flag == 0) {
          task.SyncTaskRequirement(requirement);
          task.Check();
        }
      }
    }

    public void UpdateTaskState(string type, string id, int num) {
      foreach (List<Task> taskList in m_tasks) {
        foreach (Task task in taskList) {
          if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
            task.Update(type, id, num);
          }
        }
      }
      //TODO:更新任务状态的UI
    }
  }
}