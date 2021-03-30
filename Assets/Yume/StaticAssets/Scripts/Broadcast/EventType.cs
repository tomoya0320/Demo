namespace Yume.Broadcast {
  public enum EventType {
    /// <summary>
    /// 只更新目前已接受的任务进度
    /// </summary>
    UPDATE_TASK_PROCESS,
    /// <summary>
    /// 有新任务添加时调用
    /// </summary>
    REFRESH_TASK_VIEW,
    UPDATE_ITEM_INFO,
    UPDATE_USER_INFO,
    UPDATE_CARD_INFO,
    UPDATE_MAIN_STORY_LEVEL,
    UPDATE_CHAR_DETAIL,
    /// <summary>
    /// 当进入玩家的回合时
    /// </summary>
    ON_ENTER_PLAYER_TURN,    
    /// <summary>
    /// 当进入第一个敌人的回合时
    /// </summary>
    ON_ENTER_FIRST_ENEMY_TURN,
    UPDATE_BATTLE_UNIT_ATTRIB,
    UPDATE_BATTLE_UNIT_BUFF,
  }
}