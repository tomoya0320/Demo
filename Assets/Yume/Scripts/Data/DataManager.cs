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
      LOCAL,
      REMOTE
    }

    private static PlayMode s_playMode;
    private static HashSet<string> s_saveDataTypes = new HashSet<string>();
    private static string s_localUserDataPath = Path.Combine(Application.persistentDataPath, "savedata");

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
    public static int userTeamIndex {
      private set {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        s_user.teamIndex = value;
      }
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.teamIndex;
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
    public static List<TaskData> userTasks {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.tasks;
      }
    }
    public static List<LevelData> userLevels {
      get {
        if (s_user == null) {
          s_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return s_user.levels;
      }
    }

    #endregion

    #region Data

    public static ItemData GetItem(string id) {
      return userItems.Find(obj => obj.id == id);
    }

    public static int GetItemCount(string id) {
      return userItems.Find(obj => obj.id == id)?.count ?? 0;
    }

    public static List<TaskData> GetTaskList(TaskType taskType) {
      return userTasks.FindAll(obj => ResourceManager.Load<TaskTemplate>(obj.id).type == taskType);
    }

    public static TaskData GetTask(string taskId) {
      return userTasks.Find(obj => obj.id == taskId);
    }

    /// <summary>
    /// 因为卡牌能重复所以只能通过guid获取
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static CardData GetCard(int guid) {
      return userCards.Find(obj => obj.guid == guid);
    }

    public static UnitData GetCharacter(string charId) {
      return userChars.Find(obj => obj.id == charId);
    }

    #endregion

    #region Operate

    public static void OnLateUpdate() {
      if (s_saveDataTypes.Count > 0) {
        JObject data = new JObject();
        foreach (var saveDataType in s_saveDataTypes) {
          object value = typeof(UserData).GetField(saveDataType).GetValue(s_user);
          data.Add(saveDataType, JToken.FromObject(value));
        }
        data.Add("account", userAccount);
        data.Add("password", userPassword);
        data.Add("types", JArray.FromObject(s_saveDataTypes));
        _SaveData(data.ToString());

        s_saveDataTypes.Clear();
      }
    }

    #region Mail

    //邮件比较特殊，需要是同一个对象
    public static void DeleteMail(MailData mail) {
      if (mail.isReceived) {
        userMails.Remove(mail);

        s_saveDataTypes.Add("mails");
      }
    }

    public static void GetAttachment(MailData mail) {
      if (!mail.isReceived) {
        mail.isReceived = true;
        foreach (var item in mail.attachment) {
          AddItem(item.id, item.count);
        }
        RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_MAIL, mail.title, MainMailRedPointTree.MAIN_MAIL, 0);
        s_saveDataTypes.Add("mails");
      }
    }

    public static void AddMail(MailData mail) {
      userMails.Add(mail);
      if (!mail.isReceived) {
        RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_MAIL, mail.title, MainMailRedPointTree.MAIN_MAIL, 1);
      }
      s_saveDataTypes.Add("mails");
    }

    #endregion

    #region Card

    public static void AddCard(string id, int phase) {
      CardData card = new CardData(id, phase);
      userCards.Add(card);
      card.guid = s_user.validGuid++;
      s_saveDataTypes.Add("cards");
      s_saveDataTypes.Add("validGuid");
    }

    #endregion

    #region Character

    public static void AddCharacter(string id, int level, int passiveSkillLevel) {
      UnitData character = new UnitData(id, level, passiveSkillLevel);
      userChars.Add(character);
      s_saveDataTypes.Add("chars");
    }

    #endregion

    #region Item

    public static void ConsumeItem(string id, int count, bool isUsed = true) {
      ItemData tempItem = GetItem(id);
      if (tempItem != null && tempItem.count >= count) {
        tempItem.count -= count;
        if (tempItem.count == 0) {
          userItems.Remove(tempItem);
        }
        _UpdateTaskState(RequestType.ITEM_NUM, id, isUsed ? -count : 0);

        //更新UI显示
        EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
        EventCenter.Broadcast(EventType.UPDATE_USER_INFO);

        s_saveDataTypes.Add("items");
      }
    }

    public static void AddItem(string id, int count) {
      ItemData tempItem = GetItem(id);
      if (tempItem == null) {
        tempItem = new ItemData(id, count);
        userItems.Add(tempItem);
      } else {
        tempItem.count += count;
      }
      
      // 提示
      ItemTemplate itemTemplate = ResourceManager.Load<ItemTemplate>(id);
      Sprite itemIcon = ResourceManager.Load<Sprite>(itemTemplate.iconFileName);
      // 小 心 引 用 类 型 (改了后没这个问题了)
      string content = $"{StringMap.GET}{itemTemplate.name}*{count}";
      MessageManager.AddMessage(new Message(Message.Style.FADE, content, itemIcon));
      
      // 经验值额外再处理一次
      if (id == StringMap.EXP_ID) {
        _AddExpInternal();
      }

      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);

      //更新任务进度
      _UpdateTaskState(RequestType.ITEM_NUM, id, count);

      s_saveDataTypes.Add("items");
    }

    #endregion

    #region Task

    private static void _UpdateTaskState(RequestType type, string reqId, int reqCount) {
      foreach (var task in userTasks) {
        if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
          var taskRequest = ResourceManager.Load<TaskTemplate>(task.id).taskRequest;
          if (taskRequest.type == type && (taskRequest.requirementId == reqId || taskRequest.ignoreId)) {
            switch (taskRequest.method) {
              case Method.ADD: {
                task.process += reqCount > 0 ? reqCount : 0;
                break;
              }
              case Method.CONSUME: {
                task.process += reqCount < 0 ? -reqCount : 0;
                break;
              }
              case Method.HAD: {
                _SyncTaskHadCount(task);
                break;
              }
            }

          }
          _CheckTask(task);
        }
      }
      EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
      s_saveDataTypes.Add("tasks");
    }

    private static void _ActivateTask(TaskData task) {
      if (task.state == TaskState.INACTIVE) {
        var taskRequest = ResourceManager.Load<TaskTemplate>(task.id).taskRequest;
        task.state = TaskState.INPROCESS;
        if (taskRequest.method == Method.HAD) {
          _SyncTaskHadCount(task);
        }
        _CheckTask(task);
        EventCenter.Broadcast(EventType.REFRESH_TASK_VIEW);
        s_saveDataTypes.Add("tasks");
      }
    }

    private static void _CheckTask(TaskData task) {
      if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
        var taskRequest = ResourceManager.Load<TaskTemplate>(task.id).taskRequest;
        bool completed = task.process >= taskRequest.requiredCount;

        if (completed && task.state == TaskState.INPROCESS) {
          task.state = TaskState.COMPLETED;
          TaskTemplate taskTemplate = ResourceManager.Load<TaskTemplate>(task.id);
          RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, taskTemplate.type.GetDescription(), 1);
          string content = string.Format(StringMap.TASK_COMPLETED, taskTemplate.title);
          MessageManager.AddMessage(new Message(Message.Style.FADE, content));
          s_saveDataTypes.Add("tasks");
        } else if (!completed && task.state == TaskState.COMPLETED) {
          task.state = TaskState.INPROCESS;
          TaskTemplate taskTemplate = ResourceManager.Load<TaskTemplate>(task.id);
          RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, taskTemplate.type.GetDescription(), 0);
          s_saveDataTypes.Add("tasks");
        }
      }
    }

    private static void _SyncTaskHadCount(TaskData task) {
      var taskRequest = ResourceManager.Load<TaskTemplate>(task.id).taskRequest;
      if (taskRequest.method == Method.HAD) {
        switch (taskRequest.type) {
          case RequestType.ITEM_NUM: {
            task.process = GetItemCount(taskRequest.requirementId);
            break;
          }
          case RequestType.LV_UP_TO: {
            task.process = userLv;
            break;
          }
        }
        s_saveDataTypes.Add("tasks");
      }
    }

    public static void CompleteTask(TaskData task) {
      if (task.state == TaskState.COMPLETED) {
        task.state = TaskState.FINAL;
        TaskTemplate taskTemplate = ResourceManager.Load<TaskTemplate>(task.id);
        foreach (ItemData item in taskTemplate.taskRewards.items) {
          AddItem(item.id, item.count);
        }

        if (taskTemplate.taskRequest.consumable && taskTemplate.taskRequest.type == RequestType.ITEM_NUM) {
          ConsumeItem(taskTemplate.taskRequest.requirementId, taskTemplate.taskRequest.requiredCount, false);
        }

        RedPointManager.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, taskTemplate.type.GetDescription(), 0);

        if (!string.IsNullOrEmpty(taskTemplate.activeTaskId)) {
          TaskData nextTask = GetTask(taskTemplate.activeTaskId);
          if (nextTask != null && nextTask.state == TaskState.INACTIVE) {
            _ActivateTask(nextTask);
          }
        }
 
        EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
        _UpdateTaskState(RequestType.TASK, task.id, 1);

        s_saveDataTypes.Add("tasks");
      }
    }

    #endregion

    #region Team

    public static void ChangeTeamIndex(int newTeamIndex) {
      userTeamIndex = newTeamIndex;
      s_saveDataTypes.Add("teamIndex");
    }

    public static void ChangeTeamName(TeamData team, string newName) {
      team.name = newName;
      s_saveDataTypes.Add("teams");
    }

    public static void ChangeTeamChar(TeamData team, string newCharId) {
      team.charId = newCharId;
      s_saveDataTypes.Add("teams");
    }

    public static void ChangeTeamCards(TeamData team, List<int> newCardGuidList) {
      team.cardGuidList = newCardGuidList;
      s_saveDataTypes.Add("teams");
    }

    #endregion

    #region Exp&Lv

    private static void _AddExpInternal() {
      int oLevel = userLv;
      ItemData expItem = GetItem(StringMap.EXP_ID);
      while (expItem.count >= MathUtil.CalNeededExp(userLv)) {
        expItem.count -= MathUtil.CalNeededExp(userLv);
        userLv++;
      }
      //更新任务状态
      if (userLv > oLevel) {
        string content = string.Format(StringMap.LEVEL_UP_TO, userLv);
        MessageManager.AddMessage(new Message(Message.Style.SLIDE, content));
        _UpdateTaskState(RequestType.LV_UP_TO, StringMap.EMPTY, userLv);
        int maxAp = ResourceManager.Load<int>($"{userLv}");
        if (maxAp > userMaxAp) {
          userMaxAp = maxAp;
          userAp += userMaxAp;
          s_saveDataTypes.Add("ap");
          s_saveDataTypes.Add("maxAp");
        }
        s_saveDataTypes.Add("lv");
      }
      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);
    }

    #endregion

    #region Level(关卡)

    public static void CompleteLevel(string id) {
      var level = userLevels.Find(l => l.id == id);
      if (level != null) {
        LevelTemplate levelTemplate = ResourceManager.Load<LevelTemplate>(id);
        if (!level.completed) {
          level.completed = true;
          foreach (var item in levelTemplate.firstCompletedRewards.items) {
            AddItem(item.id, item.count);
          }
          s_saveDataTypes.Add("levels");
          // 更新UI
          EventCenter.Broadcast(EventType.UPDATE_MAIN_STORY_LEVEL);
        }
        foreach (var item in levelTemplate.regularRewards.items) {
          AddItem(item.id, item.count);
        }
      }
    }
    
    #endregion

    #region Load&Save

    public static void LoadUserData(JObject json = null) {
      if (json == null) {
        s_playMode = PlayMode.LOCAL;
        FileStream fs;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(s_localUserDataPath)) {
          fs = new FileStream(s_localUserDataPath, FileMode.Open);
          s_user = (UserData)bf.Deserialize(fs);
        } else {
          s_user = new UserData();
          fs = File.Open(s_localUserDataPath, FileMode.Create);
          //序列化     
          bf.Serialize(fs, s_user);
        }
        fs.Close();
      } else {
        s_playMode = PlayMode.REMOTE;
        s_user = JsonConvert.DeserializeObject<UserData>(json["user"].ToString());
        userToken = json["token"].Value<string>();

        // 保存账密
        PlayerPrefs.SetString("account", userAccount);
        PlayerPrefs.SetString("password", userPassword);
      }
    }

    private static void _SaveData(string data) {
      switch (s_playMode) {
        case PlayMode.REMOTE: {
          ServerManager.UpdateUserData(
            data,
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
                Action ok = () => _SaveData(data);
                Action cancel = () => UIManager.CloseAllAndOpenOne("login");
                UIManager.OpenOne(
                  "confirmation", null, error, StringMap.RETRY,
                  StringMap.RETURN_TO_LOGIN, ok, cancel
                );
              }
            }
          );
          break;
        }
        case PlayMode.LOCAL: {
          BinaryFormatter bf = new BinaryFormatter();
          FileStream fs = File.Open(s_localUserDataPath, FileMode.Create);
          //序列化     
          bf.Serialize(fs, s_user);
          fs.Close();
          break;
        }
      }
    }

    #endregion

    #endregion
  }
}