namespace Yume.Broadcast {
  public enum EventType {
    /// <summary>
    /// 只更新目前已接受的任务进度
    /// </summary>
    UPDATE_TASK_PROCESS,
    REFRESH_MAIL_VIEW,
    /// <summary>
    /// 有新任务添加时调用
    /// </summary>
    REFRESH_TASK_VIEW,
    UPDATE_ITEM_INFO,
    UPDATE_USER_INFO,
  }
}