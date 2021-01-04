using System;
using System.Text;
using Yume.Battle;

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
    public const string PRELOAD = "Preload";
    public const string ASSETS_DOWNLOADING = "下载资源中...{0}%(总大小:{1}MB)";
    public const string TASK_COMPLETED = "任务[{0}]已完成！";
    public const string QUIT_GAME_TIP = "确认退出游戏？";
    public const string QUIT = "退出";
    public const string CANCEL = "取消";
    public const string DOWNLOAD_ERROR_TIP = "下载出错，是否重试？";
    public const string RETRY = "重试";
    public const string RETURN_TO_LOGIN = "返回登录界面";
    public const string COMPLETED = "已完成";
    public const string CLICK_GET = "点击领取";
    public const string GOT = "已领取";
    public const string GET = "获得";
    public const string LOGIN = "login";
    public const string HOME = "home";
    public const string LOGIN_BGM = "SOTE_Shrine_v2";
    public const string HOME_BGM = "STS_Merchant_v2";
    public const string DEFAULT_CLICK_SOUND = "点击2";
    public const string DIAMOND_ID = "item_1_diamond";
    public const string GOLD_ID = "item_2_gold";
    public const string GET_EXP = "获得经验值{0}";
    public const string LEVEL_UP_TO = "等级提升至Lv{0}";
    public const string AVG = "avg";
    public const string BG = "bg";
    public const string NAME = "name";
    public const string DIALOGUE = "dialogue";
    public const string UI_OPEN_ANIM = "ui_open";
    public const string UI_CLOSE_ANIM = "ui_close";
    public const string UI_RED_POINT = "red point";
  }

  public class StringUtil {
    /// <summary>
    /// 以万、亿等单位来显示数字并忽略小数部分，避免数字过长导致显示问题
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static string IntegerToString(int num) {
      if (num < 10000) {
        return num.ToString();
      } else if (num < 100000000) {
        return (num / 10000) + "万";
      } else {
        return (num / 100000000) + "亿";
      }
    }

    public static string StringToBase64(string str) {
      byte[] bytes = Encoding.UTF8.GetBytes(str);
      return Convert.ToBase64String(bytes);
    }

    public static string Base64ToString(string str) {
      byte[] bytes = Convert.FromBase64String(str);
      return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 将描述里类似{atk}的字符串替换成blackboard里的实际数值
    /// </summary>
    public static string GetActualDescription(string description, Blackboard blackboard) {
      StringBuilder result = new StringBuilder();
      int start = 0;
      for (int i = 0; i < description.Length; i++) {
        if (description[i] == '{') {
          result.Append(description.Substring(start, i - start));
          start = i + 1;
        } else if (description[i] == '}') {
          result.Append(blackboard.GetValueOrDefault(description.Substring(start, i - start), 0));
          start = i + 1;
        }
      }
      if (start < description.Length) {
        result.Append(description.Substring(start));
      }
      return result.ToString();
    }
  }
}