using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Yume.Broadcast;
using Yume.MailSystem;
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

    private static string localUserDBPath {
      get {
        return Path.Combine(Application.persistentDataPath, "savedata");
      }
    }
    private static UserDB s_userDB;
    public static string userAccount {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.account;
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
    public static string userHashCode {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB.hashCode;
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
    public static List<Mail> userMails {
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
    private static PlayMode s_playMode;
    private static List<JToken> s_itemInfos;
    private static List<JToken> itemInfos {
      get {
        if (s_itemInfos == null) {
          JObject json = JObject.Parse(Resources.Load<TextAsset>(StringMap.ITEMS).text);
          s_itemInfos = json[StringMap.ITEMS].Value<JArray>().ToList();
        }
        return s_itemInfos;
      }
    }
    private static JObject s_taskInfos;
    private static JObject taskInfos {
      get {
        if (s_taskInfos == null) {
          JObject json = JObject.Parse(Resources.Load<TextAsset>(StringMap.TASKS).text);
          s_taskInfos = json[StringMap.TASKS].Value<JObject>();
        }
        return s_taskInfos;
      }
    }

#if TEST
    public static string setUserHashCode {
      set {
        s_userDB.hashCode = value;
      }
    }
#endif

    private DBManager() { }

    public static ItemInfo GetItemInfo(string id) {
      JToken itemInfo = itemInfos.Find(j => j[StringMap.ID].Value<string>() == id);
      return JsonConvert.DeserializeObject<ItemInfo>(itemInfo.ToString());
    }

    public static TaskInfo GetTaskInfo(TaskType taskType, string id) {
      JToken taskInfo = taskInfos[taskType.ToString()].Value<JArray>().ToList().Find(
      j => j[StringMap.ID].Value<string>() == id);

      return JsonConvert.DeserializeObject<TaskInfo>(taskInfo.ToString());
    }

    public static ItemDB GetItemDB(int index) {
      if (index < 0 || index >= userItems.Count) {
        return null;
      }
      return userItems[index];
    }

    public static ItemDB GetItemDB(string id) {
      return userItems.Find(item => item.id == id);
    }

    public static List<TaskDB> GetTaskList(TaskType taskType) {
      return userTasks[taskType.ToString()];
    }
    
    public static void DeleteMail(Mail mail) {
      userMails.Remove(mail);
      SaveUserMails();
    }    
    
    public static void AddMail(Mail mail) {
      userMails.Add(mail);
      EventCenter.Broadcast(Broadcast.EventType.ADD_MAIL);
      SaveUserMails();
    }

    public static void SaveUserMails() {
      _UpdateUserData(new { account = userAccount, mails = userMails, types = new string[] { "mails" } });
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
        s_userDB = JsonConvert.DeserializeObject<UserDB>(json.ToString());
      }
    }

    public static Dictionary<string, List<TaskDB>> GetDefaultTasks() {
      JObject taskJsons = JObject.Parse(Resources.Load<TextAsset>(StringMap.TASKS).text);
      return JsonConvert.DeserializeObject<Dictionary<string, List<TaskDB>>>(taskJsons[StringMap.TASKS].ToString());
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
      //更新任务进度
      if (isUsed/*只有真正使用了道具才能更新*消耗道具类型任务*的进度*/) {
        foreach (var item in items) {
          TaskManager.UpdateTaskState(RequestType.PROP_NUM, item.id, -item.num);
        }
        //任务和道具一起同步
        _UpdateUserData(new { account = userAccount, items = userItems, tasks = userTasks, types = new string[] { "items", "tasks" } });
      } else {
        //只同步道具
        _UpdateUserData(new { account = userAccount, items = userItems, types = new string[] { "items" } });
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
      //更新任务进度
      foreach (var item in items) {
        TaskManager.UpdateTaskState(RequestType.PROP_NUM, item.id, item.num);
      }
      //任务和道具一起同步
      _UpdateUserData(new { account = userAccount, items = userItems, tasks = userTasks, types = new string[] { "items", "tasks" } });
    }

    public static void CompleteTask(TaskDB task, Action onCompleted = null) {
      if (task.state == TaskState.COMPLETED) {
        task.Complete();
        TaskManager.UpdateTaskState(RequestType.TASK, task.id, 1);
        _UpdateUserData(new { account = userAccount, tasks = userTasks, types = new string[] { "tasks" } }, onCompleted);
      }
    }

    private static void _UpdateUserData<T>(T datas, Action onSucceed = null) where T : class {
      if (s_playMode == PlayMode.REMOTE) {
        ServerController.instance.UpdateUserData(datas,
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
              UIController.instance.OpenDifference(error, StringMap.RETRY,
              StringMap.RETURN_TO_LOGIN, () => _UpdateUserData(datas, onSucceed), 
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