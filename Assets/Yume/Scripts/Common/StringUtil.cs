using System;

namespace Yume.String {
  public class StringMap {
    public const string EMPTY = "";
    public const string NONE = "无";
    public const string DESCRIPTION = "description";
    public const string SWITCH = "Switch";
    public const string VALUE = "Value";
    public const string NAME = "name";
    public const string KEY = "key";
    public const string ID = "id";
    #region datetime
    public const string TODAY = "今天";
    public const string SOME_DAY_BEFORE = "{0}天前";
    public const string SOME_WEEK_BEFORE = "{0}周前";
    public const string SOME_MONTH_BEFORE = "{0}月前";
    public const string SOME_YEAR_BEFORE = "{0}年前";
    #endregion
    public const string MESSAGE = "message";
    public const string LOADING = "Loading";
    public const string QUALITY = "quality";
    public const string PRELOAD = "Preload";
    public const string ASSETS_DOWNLOADING = "下载资源中...{0}%(总大小:{1}MB)";
    public const string ACCOUNT = "account";
    public const string TASK_COMPLETED = "任务[{0}]已完成！";
    public const string ITEMS = "items";
    public const string ICON_FILE_NAME = "iconFileName";
  }

  public class StringUtil {
    public static string IntegerToString(int num) {
      if(num < 10000) {
        return num.ToString();
      }
      string result = (num / 10000) + "万";
      num %= 10000;

      return num > 0 ? result + num : result;
    }
  }
}