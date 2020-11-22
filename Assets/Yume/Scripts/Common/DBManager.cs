using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Yume.Battle;
using Yume.Broadcast;
using Yume.MailSystem;
using Yume.Math;
using Yume.Scene;
using Yume.Server;
using Yume.String;
using Yume.Task;
using Yume.UI;

namespace Yume.DB {
  public class DBManager {
    private enum PlayMode {
      LOCAL, REMOTE
    }

    private static PlayMode s_playMode;
    private static string localUserDBPath {
      get {
        return Path.Combine(Application.persistentDataPath, "savedata");
      }
    }

    #region User Data
    private static UserDB s_userDB;
    public static string userToken { private set; get; }
    public static string userAccount {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.account;
      }
    }
    public static string userPassword {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.password;
      }
    }
    public static string userName {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.name;
      }
    }    
    public static int userAp {
      private set {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        s_userDB.ap = value;
      }
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.ap;
      }
    }    
    public static int userMaxAp {
      private set {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        s_userDB.maxAp = value;
      }
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.maxAp;
      }
    }    
    public static int userLv {
      private set {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        s_userDB.lv = value;
      }
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.lv;
      }
    }  
    public static int userExp {
      private set {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        s_userDB.exp = value;
      }
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.exp;
      }
    }
    public static List<ItemDB> userItems {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.items;
      }
    }
    public static List<CardDB> userCards {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.cards;
      }
    }
    public static List<MailDB> userMails {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.mails;
      }
    }
    public static Dictionary<string, List<TaskDB>> userTasks {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.tasks;
      }
    }
    #endregion

    #region Json
    private static JObject s_itemInfos;
    private static JObject itemInfos {
      get {
        if (s_itemInfos == null) {
          s_itemInfos = JObject.Parse(GlobalJsonHub.instance.jsonAsset.items.text);
        }
        return s_itemInfos;
      }
    }
    private static JObject s_taskInfos;
    private static JObject taskInfos {
      get {
        if (s_taskInfos == null) {
          s_taskInfos = JObject.Parse(GlobalJsonHub.instance.jsonAsset.tasks.text);
        }
        return s_taskInfos;
      }
    }
    private static JObject s_lvMaxApMap;
    private static JObject lvMaxApMap {
      get {
        if (s_lvMaxApMap == null) {
          s_lvMaxApMap = JObject.Parse(GlobalJsonHub.instance.jsonAsset.lvMaxApMap.text);
        }
        return s_lvMaxApMap;
      }
    }
    private static JObject s_cardInfos;
    private static JObject cardInfos {
      get {
        if (s_cardInfos == null) {
          s_cardInfos = JObject.Parse(GlobalJsonHub.instance.jsonAsset.cards.text);
        }
        return s_cardInfos;
      }
    }
    #endregion

#if TEST
    public static string setUserToken {
      set {
        userToken = value;
      }
    }
#endif

    private DBManager() { }

    public static ItemInfo GetItemInfo(string id) {
      if (itemInfos.ContainsKey(id)) {
        JObject itemInfo = itemInfos[id].Value<JObject>();
        return JsonConvert.DeserializeObject<ItemInfo>(itemInfo.ToString());
      } else {
        throw new Exception(string.Format("item id:{0} is not found", id));
      }
    }

    public static CardInfo GetCardInfo(string id) {
      if (cardInfos.ContainsKey(id)) {
        JObject cardInfo = cardInfos[id].Value<JObject>();
        return JsonConvert.DeserializeObject<CardInfo>(cardInfo.ToString());
      } else {
        throw new Exception(string.Format("card id:{0} is not found", id));
      }
    }

    public static TaskInfo GetTaskInfo(TaskType taskType, string id) {
      if (taskInfos.ContainsKey(taskType.ToString())) {
        JObject tempJObject = taskInfos[taskType.ToString()].Value<JObject>();
        if (tempJObject.ContainsKey(id)) {
          JObject taskInfo = tempJObject[id].Value<JObject>();
          return JsonConvert.DeserializeObject<TaskInfo>(taskInfo.ToString());
        } else {
          throw new Exception(string.Format("task id:{0} is not found", id));
        }
      } else {
        throw new Exception(string.Format("task type:{0} is not found", taskType.ToString()));
      }    
    }

    public static Dictionary<string, List<TaskDB>> GetDefaultTasks() {
      Dictionary<string, List<TaskDB>> defaultTasks = new Dictionary<string, List<TaskDB>>();

      foreach(TaskType type in Enum.GetValues(typeof(TaskType))) {
        List<TaskDB> tasks = new List<TaskDB>();
        foreach(var info in taskInfos[type.ToString()].Values()) {
          tasks.Add(JsonConvert.DeserializeObject<TaskDB>(info.ToString()));
        }
        defaultTasks.Add(type.ToString(), tasks);
      }

      return defaultTasks;
    }

    public static ItemDB GetItemDB(string id) {
      return userItems.Find(obj => obj.id == id);
    } 
    
    public static int GetItemDBNum(string id) {
      ItemDB itemDB = userItems.Find(obj => obj.id == id);
      return itemDB == null ? 0 : itemDB.num;
    }

    public static List<TaskDB> GetTaskList(TaskType taskType) {
      return userTasks[taskType.ToString()];
    }

    public static TaskDB GetTaskDB(TaskType taskType,string taskId) {
      return userTasks[taskType.ToString()].Find(obj => obj.id == taskId);
    }
    
    public static void DeleteMail(MailDB mail) {
      userMails.Remove(mail);
      UpdateUserMails();
    }    
    
    public static void AddMail(MailDB mail) {
      userMails.Add(mail);
      EventCenter.Broadcast(Broadcast.EventType.ADD_MAIL_VIEW);
      UpdateUserMails();
    }

    public static void UpdateUserMails() {
      _UpdateUserData(new { mails = userMails, types = new string[] { "mails" } });
    }

    public static void LoadUserDB(JObject json = null) {
      if (json == null) {
        s_playMode = PlayMode.LOCAL;
        FileStream fs;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(localUserDBPath)) {
          fs = new FileStream(localUserDBPath, FileMode.Open);
          s_userDB = (UserDB)bf.Deserialize(fs);
        } else {
          s_userDB = new UserDB();
          fs = File.Open(localUserDBPath, FileMode.Create);
          //序列化     
          bf.Serialize(fs, s_userDB);
        }
        fs.Close();
      } else {
        s_playMode = PlayMode.REMOTE;
        s_userDB = JsonConvert.DeserializeObject<UserDB>(json["userDB"].ToString());
        userToken = json["token"].Value<string>();
      }
    }

    public static void ConsumeItems(bool isUsed = true, params ItemDB[] items) {
      //更新本地数据
      ItemDB tempItem;
      for (int i = 0; i < items.Length; i++) {
        tempItem = GetItemDB(items[i].id);
        if (tempItem != null && tempItem.num >= items[i].num) {
          tempItem.num -= items[i].num;
          if (tempItem.num == 0) {
            userItems.Remove(tempItem);
          }
        }
      }
      //更新UI显示
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_USER_INFO);
      //更新任务进度
      if (isUsed/*只有真正使用了道具才能更新*消耗道具类型任务*的进度*/) {
        foreach (var item in items) {
          TaskManager.UpdateTaskState(RequestType.PROP_NUM, item.id, -item.num);
        }
        //任务和道具一起同步
        _UpdateUserData(new { items = userItems, tasks = userTasks, types = new string[] { "items", "tasks" } });
      } else {
        //只同步道具
        _UpdateUserData(new { items = userItems, types = new string[] { "items" } });
      }
    }

    public static void AddItems(params ItemDB[] items) {
      //更新本地数据
      ItemDB tempItem;
      for (int i = 0; i < items.Length; i++) {
        tempItem = GetItemDB(items[i].id);
        if (tempItem == null) {
          userItems.Add(items[i]);
        } else {
          tempItem.num += items[i].num;
        }
      }
      //更新UI显示
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_USER_INFO);
      //更新任务进度
      foreach (var item in items) {
        TaskManager.UpdateTaskState(RequestType.PROP_NUM, item.id, item.num);
      }
      //任务和道具一起同步
      _UpdateUserData(new { items = userItems, tasks = userTasks, types = new string[] { "items", "tasks" } });
    }

    public static void AddExp(int exp) {
      userExp += exp;
      UIController.instance.messageManager.AddMessage(new Message(Message.Style.FADE, string.Format(StringMap.GET_EXP, exp)), true);
      int oLevel = userLv;
      while (userExp >= MathUtil.CalNeededExp(userLv)) {
        userExp -= MathUtil.CalNeededExp(userLv);
        userLv++;
      }
      //更新任务状态
      if (userLv > oLevel) {
        string tip = string.Format(StringMap.LEVEL_UP_TO, userLv);
        UIController.instance.messageManager.AddMessage(new Message(Message.Style.FADE, tip));
        TaskManager.UpdateTaskState(RequestType.LV_UP_TO, StringMap.EMPTY, userLv);
        if (lvMaxApMap.ContainsKey(userLv.ToString())) {
          userMaxAp = lvMaxApMap[userLv.ToString()].Value<int>();
          userAp += userMaxAp;
          _UpdateUserData(new {
            exp = userExp, lv = userLv, ap = userAp, maxAp = userMaxAp, tasks = userTasks,
            types = new string[] { "exp", "lv", "ap", "maxAp", "tasks" }
          });
        } else {
          _UpdateUserData(new {
            exp = userExp, lv = userLv, tasks = userTasks,
            types = new string[] { "exp", "lv", "tasks" }
          });
        }
      } else {
        _UpdateUserData(new {
          exp = userExp, lv = userLv, types = new string[] { "exp", "lv" }
        });
      }
      //更新UI显示
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_USER_INFO);
    }

    public static void CompleteTask(TaskDB task, Action onCompleted = null) {
      if (task.state == TaskState.COMPLETED) {
        task.Complete();
        TaskInfo taskInfo = GetTaskInfo(task.type, task.id);
        if (!string.IsNullOrEmpty(taskInfo.activeTaskId)) {
          TaskDB nextTask = GetTaskDB(taskInfo.activeTaskType, taskInfo.activeTaskId);
          if (nextTask != null && nextTask.state == TaskState.INACTIVE) {
            nextTask.state = TaskState.INPROCESS;
            nextTask.OnInit();
            EventCenter.Broadcast(Broadcast.EventType.ADD_TASK_VIEW, nextTask);
          }
        }
        TaskManager.UpdateTaskState(RequestType.TASK, task.id, 1);
        _UpdateUserData(new { tasks = userTasks, types = new string[] { "tasks" } }, onCompleted);
      }
    }

    private static void _UpdateUserData<T>(T data, Action onSucceed = null) where T : class {
      if (s_playMode == PlayMode.REMOTE) {
        ServerController.instance.UpdateUserData(data,
          request => {
            string error = StringMap.EMPTY;
            if (!string.IsNullOrEmpty(request.error)) {
              error = "连接出错！" + request.error;
            } else {
              var json = JObject.Parse(request.downloadHandler.text);
              int resultCode = json["resultCode"].Value<int>();
              //这里的错误就不分类了
              if (resultCode != 1) {
                error = "更新用户数据失败！";
              }
            }
            if (string.IsNullOrEmpty(error)) {
              onSucceed?.Invoke();
            } else {
              UIController.instance.OpenConfirmationBox(error, StringMap.RETRY,
              StringMap.RETURN_TO_LOGIN, () => _UpdateUserData(data, onSucceed),
              () => SceneLoader.GoToNextScene(StringMap.LOGIN));
            }
          });
      }else if(s_playMode == PlayMode.LOCAL) {
        onSucceed?.Invoke();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Open(localUserDBPath, FileMode.Create);
        //序列化     
        bf.Serialize(fs, s_userDB);
        fs.Close();
      }
    }
  }
}