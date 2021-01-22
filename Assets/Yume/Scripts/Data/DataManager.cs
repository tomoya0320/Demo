using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Yume.Broadcast;
using Yume.RedPointSystem;
using Yume.Resource;
using Yume.Server;
using Yume.UI;
using EventType = Yume.Broadcast.EventType;

namespace Yume.Data {
  public static class DataManager {
    private enum PlayMode {
      LOCAL, REMOTE
    }

    private static PlayMode m_playMode;
    private static string localUserDBPath {
      get {
        return Path.Combine(Application.persistentDataPath, "savedata");
      }
    }

    #region User
    private static UserData s_user;
    public static bool isLogged => s_user != null;
    public static string userToken { private set; get; }
    public static string userAccount {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.account;
      }
    }
    public static string userPassword {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.password;
      }
    }
    public static string userName {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.name;
      }
    }
    public static int userAp {
      private set {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        s_user.ap = value;
      }
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.ap;
      }
    }
    public static int userMaxAp {
      private set {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        s_user.maxAp = value;
      }
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.maxAp;
      }
    }
    public static int userLv {
      private set {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        s_user.lv = value;
      }
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.lv;
      }
    }
    public static int userExp {
      private set {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        s_user.exp = value;
      }
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.exp;
      }
    }
    public static List<ItemData> userItems {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.items;
      }
    }
    public static List<CardData> userCards {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.cards;
      }
    }
    public static List<TeamData> userTeams {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.teams;
      }
    }
    public static List<UnitData> userChars {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.chars;
      }
    }
    public static List<MailData> userMails {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.mails;
      }
    }
    public static Dictionary<TaskType, List<TaskData>> userTasks {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.tasks;
      }
    }
    #endregion

    #region Json
    private static JObject s_itemJson;
    private static JObject s_taskJson;
    private static JObject s_cardJson;
    private static JObject s_unitJson;
    private static JObject s_lvMaxApMapJson;
    private static List<AttribCurve> s_attribCurveList;
    #endregion

    #region Template
    public static ItemTemplate GetItemTemplate(string id) {
      if (s_itemJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<ItemTemplate>(s_itemJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("item id:{0} is not found", id));
      }
    }

    public static CardTemplate GetCardTemplate(string id) {
      if (s_cardJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<CardTemplate>(s_cardJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("card id:{0} is not found", id));
      }
    }

    public static TaskTemplate GetTaskTemplate(TaskType taskType, string id) {
      if (s_taskJson.ContainsKey(taskType.ToString())) {
        JObject tempJObject = s_taskJson[taskType.ToString()].Value<JObject>();
        if (tempJObject.ContainsKey(id)) {
          return JsonConvert.DeserializeObject<TaskTemplate>(tempJObject[id].Value<JObject>().ToString());
        } else {
          throw new Exception(string.Format("task id:{0} is not found", id));
        }
      } else {
        throw new Exception(string.Format("task type:{0} is not found", taskType.ToString()));
      }
    }

    public static UnitTemplate GetUnitTemplate(string id) {
      if (s_unitJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<UnitTemplate>(s_unitJson[id].Value<JObject>().ToString());
      }
      throw new Exception($"unit is not found! id:{id}");
    }
    #endregion

    #region Data
    public static Dictionary<TaskType, List<TaskData>> GetDefaultTasks() {
      Dictionary<TaskType, List<TaskData>> defaultTasks = new Dictionary<TaskType, List<TaskData>>();

      foreach (TaskType type in Enum.GetValues(typeof(TaskType))) {
        List<TaskData> tasks = new List<TaskData>();
        foreach (var info in s_taskJson[type.ToString()].Values()) {
          tasks.Add(JsonConvert.DeserializeObject<TaskData>(info.ToString()));
        }
        defaultTasks.Add(type, tasks);
      }

      return defaultTasks;
    }

    public static bool TryGetMaxAp(int level, out int maxAp) {
      maxAp = 0;
      if (s_lvMaxApMapJson.ContainsKey(level.ToString())) {
        maxAp = s_lvMaxApMapJson[level.ToString()].Value<int>();
        return true;
      }
      return false;
    }

    public static AnimationCurve GetAttribCurve(string id) {
      int index = s_attribCurveList.FindIndex(ac => ac.id == id);
      if (index >= 0) {
        return s_attribCurveList[index].curve;
      } else {
        throw new Exception($"找不到属性曲线！id:{id}");
      }
    }

    public static ItemData GetItem(string id) {
      return userItems.Find(obj => obj.id == id);
    }

    public static int GetItemCount(string id) {
      ItemData itemDB = userItems.Find(obj => obj.id == id);
      return itemDB == null ? 0 : itemDB.count;
    }

    public static List<TaskData> GetTaskList(TaskType taskType) {
      return userTasks[taskType];
    }

    public static TaskData GetTask(TaskType taskType, string taskId) {
      return userTasks[taskType].Find(obj => obj.id == taskId);
    }

    public static CardData GetCard(string cardId) {
      //TODO:解决Id重复的问题
      return userCards.Find(obj => obj.id == cardId);
    }

    public static UnitData GetCharacter(string charId) {
      return userChars.Find(obj => obj.id == charId);
    }
    #endregion

    #region Operate
    #region Mail
    //邮件比较特殊，需要是同一个对象
    public static void DeleteMail(MailData mail) {
      if (mail.isReceived) {
        userMails.Remove(mail);
        EventCenter.Broadcast(EventType.REFRESH_MAIL_VIEW);
      }
    }

    public static void GetAttachment(MailData mail) {
      if (!mail.isReceived) {
        mail.isReceived = true;
        foreach (var item in mail.attachment) {
          AddItem(item);
        }
        RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_MAIL, mail.id, MainMailRedPointTree.MAIN_MAIL, 0);
      }
    }

    public static void AddMail(MailData mail) {
      userMails.Add(mail);
      if (!mail.isReceived) {
        RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_MAIL, mail.id, MainMailRedPointTree.MAIN_MAIL, 1);
      }
      EventCenter.Broadcast(EventType.REFRESH_MAIL_VIEW);
    }
    #endregion

    #region Item
    public static void ConsumeItem(ItemData item, bool isUsed = true) {
      ItemData tempItem = GetItem(item.id);
      if (tempItem != null && tempItem.count >= item.count) {
        tempItem.count -= item.count;
        if (tempItem.count == 0) {
          userItems.Remove(tempItem);
        }
        _UpdateTaskState(RequestType.ITEM_NUM, item.id, isUsed ? -item.count : 0);

        RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_ITEM, tempItem.id, MainItemRedPointTree.MAIN_ITEM, 0);

        //更新UI显示
        EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
        EventCenter.Broadcast(EventType.UPDATE_USER_INFO);
      }
    }

    public static void AddItem(ItemData item) {
      ItemData tempItem = GetItem(item.id);
      if (tempItem == null) {
        tempItem = item;
        userItems.Add(tempItem);
      } else {
        tempItem.isViewed = false;
        tempItem.count += item.count;
      }

      RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_ITEM, tempItem.id, MainItemRedPointTree.MAIN_ITEM, 1);

      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);

      //更新任务进度
      _UpdateTaskState(RequestType.ITEM_NUM, item.id, item.count);
    }
    #endregion

    #region Task
    private static void _UpdateTaskState(RequestType type, string reqId, int reqCount) {
      foreach (var taskList in userTasks.Values) {
        foreach (var task in taskList) {
          if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
            foreach (TaskRequestData taskRequest in task.taskRequests) {
              if (taskRequest.type == type && (taskRequest.requirementId == reqId || taskRequest.ignoreId)) {
                switch (taskRequest.method) {
                  case Method.ADD: {
                      taskRequest.currentCount += reqCount > 0 ? reqCount : 0;
                      break;
                    }
                  case Method.CONSUME: {
                      taskRequest.currentCount += reqCount < 0 ? -reqCount : 0;
                      break;
                    }
                  case Method.HAD: {
                      _SyncTaskHadCount(taskRequest);
                      break;
                    }
                }
              }
            }
            _CheckTask(task);
          }
        }
      }
      EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
    }

    private static void _ActivateTask(TaskData task) {
      if (task.state == TaskState.INACTIVE) {
        task.state = TaskState.INPROCESS;
        foreach (var taskRequest in task.taskRequests) {
          if (taskRequest.method == Method.HAD) {
            _SyncTaskHadCount(taskRequest);
          }
        }
        _CheckTask(task);
        EventCenter.Broadcast(EventType.REFRESH_TASK_VIEW);
      }
    }

    private static void _CheckTask(TaskData task) {
      if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
        bool completed = true;
        foreach (TaskRequestData taskRequest in task.taskRequests) {
          completed &= taskRequest.currentCount >= taskRequest.requiredCount;
        }

        if (completed && task.state == TaskState.INPROCESS) {
          task.state = TaskState.COMPLETED;
          TaskTemplate taskTemplate = GetTaskTemplate(task.type, task.id);
          RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, task.type.GetDescription(), 1);
          MessageManager.AddMessage(new Message(Message.Style.FADE, string.Format(StringMap.TASK_COMPLETED, taskTemplate.title)));
        } else if (!completed && task.state == TaskState.COMPLETED) {
          task.state = TaskState.INPROCESS;
          RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, task.type.GetDescription(), 0);
        }
      }
    }

    private static void _SyncTaskHadCount(TaskRequestData taskRequest) {
      if (taskRequest.method == Method.HAD) {
        switch (taskRequest.type) {
          case RequestType.ITEM_NUM: {
              taskRequest.currentCount = GetItemCount(taskRequest.requirementId);
              break;
            }
          case RequestType.LV_UP_TO: {
              taskRequest.currentCount = userLv;
              break;
            }
        }
      }
    }

    public async static void CompleteTask(TaskData task) {
      if (task.state == TaskState.COMPLETED) {
        task.state = TaskState.FINAL;
        TaskTemplate taskTemplate = GetTaskTemplate(task.type, task.id);
        foreach (ItemData item in taskTemplate.taskRewards.items) {
          AddItem(item);
          ItemTemplate itemTemplate = GetItemTemplate(item.id);
          Sprite itemIcon = await ResourceManager.LoadAssetAsync<Sprite>(itemTemplate.iconFileName);
          MessageManager.AddMessage(new Message(Message.Style.FADE,
          string.Format(StringMap.GET + "{0}*{1}", itemTemplate.name, item.count), itemIcon));
        }

        foreach (TaskRequestData taskRequest in task.taskRequests) {
          if (taskRequest.consumable && taskRequest.type == RequestType.ITEM_NUM) {
            ConsumeItem(new ItemData(taskRequest.requirementId, taskRequest.requiredCount), false);
          }
        }

        RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, task.type.GetDescription(), 0);

        if (!string.IsNullOrEmpty(taskTemplate.activeTaskId)) {
          TaskData nextTask = GetTask(taskTemplate.activeTaskType, taskTemplate.activeTaskId);
          if (nextTask != null && nextTask.state == TaskState.INACTIVE) {
            _ActivateTask(nextTask);
          }
        }
        //任务排序用
        EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
        _UpdateTaskState(RequestType.TASK, task.id, 1);
      }
    }
    #endregion

    #region Exp&Lv
    public static void AddExp(int exp) {
      userExp += exp;
      MessageManager.AddMessage(new Message(Message.Style.FADE, string.Format(StringMap.GET_EXP, exp)));
      int oLevel = userLv;
      while (userExp >= MathUtil.CalNeededExp(userLv)) {
        userExp -= MathUtil.CalNeededExp(userLv);
        userLv++;
      }
      //更新任务状态
      if (userLv > oLevel) {
        string tip = string.Format(StringMap.LEVEL_UP_TO, userLv);
        MessageManager.AddMessage(new Message(Message.Style.FADE, tip));
        _UpdateTaskState(RequestType.LV_UP_TO, StringMap.EMPTY, userLv);
        if (TryGetMaxAp(userLv, out int maxAp)) {
          userMaxAp = maxAp;
          userAp += userMaxAp;
        }
      }
      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);
    }
    #endregion

    #region Load&Save
    public static void Init() {
      s_itemJson = AssetLoader.GetValue<JObject>("itemJson");
      s_taskJson = AssetLoader.GetValue<JObject>("taskJson");
      s_cardJson = AssetLoader.GetValue<JObject>("cardJson");
      s_unitJson = AssetLoader.GetValue<JObject>("unitJson");
      s_lvMaxApMapJson = AssetLoader.GetValue<JObject>("lvMaxApMapJson");
      s_attribCurveList = AssetLoader.GetValue<List<AttribCurve>>("attribCurves");
    }

    public static void LoadUserDB(JObject json = null) {
      if (json == null) {
        m_playMode = PlayMode.LOCAL;
        FileStream fs;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(localUserDBPath)) {
          fs = new FileStream(localUserDBPath, FileMode.Open);
          s_user = (UserData)bf.Deserialize(fs);
        } else {
          s_user = new UserData();
          fs = File.Open(localUserDBPath, FileMode.Create);
          //序列化     
          bf.Serialize(fs, s_user);
        }
        fs.Close();
      } else {
        m_playMode = PlayMode.REMOTE;
        s_user = JsonConvert.DeserializeObject<UserData>(json["user"].ToString());
        userToken = json["token"].Value<string>();
      }
    }

    public static void SaveData(Action onSucceed = null) {
      JObject data = JObject.FromObject(s_user);
      data.Merge(JObject.FromObject(new { types = typeof(UserData).GetFieldNames() }));

      if (m_playMode == PlayMode.REMOTE) {
        ServerManager.UpdateUserData(data,
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
            if (!string.IsNullOrEmpty(error)) {
              Action ok = () => SaveData(onSucceed);
              Action cancel = () => UIManager.CloseAllAndOpenOne("login");
              UIManager.OpenOne("confirmation", null, error, StringMap.RETRY,
                                         StringMap.RETURN_TO_LOGIN, ok, cancel);
            } else {
              onSucceed?.Invoke();
            }
          });
      } else if (m_playMode == PlayMode.LOCAL) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Open(localUserDBPath, FileMode.Create);
        //序列化     
        bf.Serialize(fs, s_user);
        fs.Close();

        onSucceed?.Invoke();
      }
    }
    #endregion
    #endregion
  }
}