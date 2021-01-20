using Yume.Broadcast;
using Yume.DB;

namespace Yume.Task {
  public class TaskManager : Singleton<TaskManager> {
    public void UpdateTaskState(RequestType type, string id, int num) {
      foreach (var keyValuePair in DBManager.instance.userTasks) {
        foreach (var task in keyValuePair.Value) {
          if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
            task.Update(type, id, num);
          }
        }
      }
      EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
    }
  }
}