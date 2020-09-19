using Yume.Broadcast;
using Yume.DB;

namespace Yume.Task {
  public class TaskManager {
    private TaskManager() { }

    public static void UpdateTaskState(RequestType type, string id, int num) {
      foreach (var keyValuePair in DBManager.userTasks) {
        foreach (var task in keyValuePair.Value) {
          if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
            task.Update(type, id, num);
          }
        }
      }
      EventCenter.Broadcast(EventType.UPDATE_TASK_INFO);
    }
  }
}