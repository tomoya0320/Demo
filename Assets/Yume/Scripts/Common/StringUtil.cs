namespace Yume.String {
  public class StringMap {
    #region datetime
    public const string TODAY = "今天";
    public const string SOME_DAY_BEFORE = "{0}天前";
    public const string SOME_WEEK_BEFORE = "{0}周前";
    public const string SOME_MONTH_BEFORE = "{0}月前";
    public const string SOME_YEAR_BEFORE = "{0}年前";
    #endregion
    public const string EMPTY = "";
    public const string NONE = "无";
    public const string SWITCH = "Switch";
    public const string VALUE = "Value";
    public const string ID = "id";
    public const string MESSAGE = "message";
    public const string PRELOAD = "Preload";
    public const string ASSETS_DOWNLOADING = "下载资源中...{0}%(总大小:{1}MB)";
    public const string TASK_COMPLETED = "任务[{0}]已完成！";
    public const string ITEMS = "items";
    public const string TASKS = "tasks";
    public const string QUIT_GAME_TIP = "确认退出游戏？";
    public const string QUIT = "退出";
    public const string CANCEL = "取消";
    public const string DOWNLOAD_ERROR_TIP = "下载出错，是否重试？";
    public const string RETRY = "重试";
    public const string RETURN_TO_LOGIN = "返回登录界面";
    public const string COMPLETED = "已完成";
    public const string CLICK_GET = "点击领取";
    public const string PROCESSTEXT_PREF_NAME = "processTextPrefab";
    public const string REWARD_PREF_NAME = "rewardPrefab";
    public const string TASK_SHOW = "taskShow";
    public const string GET = "获得";
    public const string LOGIN = "Login";
    public const string HOME = "Home";
    public const string LOADING = "Loading";
    public const string LOGIN_BGM = "Bgm_5";
    public const string HOME_BGM = "Bgm_1";
    public const string DEFAULT_CLICK_SOUND = "点击2";
    public const string MAIL_IN_LIST = "mailInList";
    public const string ATTACHMENT_PREFAB = "attachmentPrefab";
  }

  public class StringUtil {
    public static string IntegerToString(int num) {
      if (num < 10000) {
        return num.ToString();
      } else if (num < 100000000) {
        return (num / 10000) + "万";
      } else {
        return (num / 100000000) + "亿";
      }
    }
  }
}