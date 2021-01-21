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
using Yume.RedPointSystem;
using Yume.Resource;
using Yume.Server;
using Yume.Task;
using Yume.UI;

namespace Yume.DB {
  public class DBManager : Singleton<DBManager> {
    private enum PlayMode {
      LOCAL, REMOTE
    }

    private PlayMode m_playMode;
    private string localUserDBPath {
      get {
        return Path.Combine(Application.persistentDataPath, "savedata");
      }
    }

    #region User Data
    private UserDB m_userDB;
    public bool isLogged => m_userDB != null;
    public string userToken { private set; get; }
    public string userAccount {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.account;
      }
    }
    public string userPassword {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.password;
      }
    }
    public string userName {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.name;
      }
    }    
    public int userAp {
      private set {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_userDB.ap = value;
      }
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.ap;
      }
    }    
    public int userMaxAp {
      private set {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_userDB.maxAp = value;
      }
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.maxAp;
      }
    }    
    public int userLv {
      private set {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_userDB.lv = value;
      }
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.lv;
      }
    }  
    public int userExp {
      private set {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_userDB.exp = value;
      }
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.exp;
      }
    }
    public List<ItemDB> userItems {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.items;
      }
    }
    public List<CardDB> userCards {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.cards;
      }
    }    
    public List<TeamDB> userTeams {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.teams;
      }
    }   
    public List<UnitDB> userChars {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.chars;
      }
    }
    public List<MailDB> userMails {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.mails;
      }
    }
    public Dictionary<TaskType, List<TaskDB>> userTasks {
      get {
        if (m_userDB == null) {
          m_userDB = new UserDB();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_userDB.tasks;
      }
    }
    #endregion

    #region Json
    private JObject m_itemJson;
    private JObject m_taskJson;
    private JObject m_cardJson;
    private JObject m_unitJson;
    private JObject m_lvMaxApMapJson;
    private List<AttribCurve> m_attribCurveList;
    #endregion

    protected override void OnInit() {
      m_itemJson = AssetLoader.instance.GetValue<JObject>("itemJson");
      m_taskJson = AssetLoader.instance.GetValue<JObject>("taskJson");
      m_cardJson = AssetLoader.instance.GetValue<JObject>("cardJson");
      m_unitJson = AssetLoader.instance.GetValue<JObject>("unitJson");
      m_lvMaxApMapJson = AssetLoader.instance.GetValue<JObject>("lvMaxApMapJson");
      m_attribCurveList = AssetLoader.instance.GetValue<List<AttribCurve>>("attribCurves");
    }

    public ItemInfo GetItemInfo(string id) {
      if (m_itemJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<ItemInfo>(m_itemJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("item id:{0} is not found", id));
      }
    }

    public CardInfo GetCardInfo(string id) {
      if (m_cardJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<CardInfo>(m_cardJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("card id:{0} is not found", id));
      }
    }

    public TaskInfo GetTaskInfo(TaskType taskType, string id) {
      if (m_taskJson.ContainsKey(taskType.ToString())) {
        JObject tempJObject = m_taskJson[taskType.ToString()].Value<JObject>();
        if (tempJObject.ContainsKey(id)) {
          return JsonConvert.DeserializeObject<TaskInfo>(tempJObject[id].Value<JObject>().ToString());
        } else {
          throw new Exception(string.Format("task id:{0} is not found", id));
        }
      } else {
        throw new Exception(string.Format("task type:{0} is not found", taskType.ToString()));
      }    
    }

    public Dictionary<TaskType, List<TaskDB>> GetDefaultTasks() {
      Dictionary<TaskType, List<TaskDB>> defaultTasks = new Dictionary<TaskType, List<TaskDB>>();

      foreach(TaskType type in Enum.GetValues(typeof(TaskType))) {
        List<TaskDB> tasks = new List<TaskDB>();
        foreach(var info in m_taskJson[type.ToString()].Values()) {
          tasks.Add(JsonConvert.DeserializeObject<TaskDB>(info.ToString()));
        }
        defaultTasks.Add(type, tasks);
      }

      return defaultTasks;
    }

    public bool TryGetMaxApBy(int level, out int maxAp) {
      maxAp = 0;
      if (m_lvMaxApMapJson.ContainsKey(level.ToString())) {
        maxAp = m_lvMaxApMapJson[level.ToString()].Value<int>();
        return true;
      }
      return false;
    }

    public AnimationCurve GetAttribCurve(string id) {
      int index = m_attribCurveList.FindIndex(ac => ac.id == id);
      if (index >= 0) {
        return m_attribCurveList[index].curve;
      } else {
        throw new Exception($"找不到属性曲线！id:{id}");
      }
    }

    public ItemDB GetItemDB(string id) {
      return userItems.Find(obj => obj.id == id);
    } 
    
    public int GetItemDBNum(string id) {
      ItemDB itemDB = userItems.Find(obj => obj.id == id);
      return itemDB == null ? 0 : itemDB.num;
    }

    public List<TaskDB> GetTaskList(TaskType taskType) {
      return userTasks[taskType];
    }

    public TaskDB GetTaskDB(TaskType taskType,string taskId) {
      return userTasks[taskType].Find(obj => obj.id == taskId);
    }

    public CardDB GetCardDB(string cardId) {
      return userCards.Find(obj => obj.id == cardId);
    }

    public UnitDB GetCharDB(string charId) {
      return userChars.Find(obj => obj.id == charId);
    }

    public UnitInfo GetUnitInfo(string id) {
      if (m_unitJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<UnitInfo>(m_unitJson[id].Value<JObject>().ToString());
      }
      throw new Exception($"unit is not found! id:{id}");
    }

    //邮件比较特殊，需要是同一个对象
    public void DeleteMails(params MailDB[] mails) {
      foreach (var mail in mails) {
        userMails.Remove(mail);
      }
      EventCenter.Broadcast(Broadcast.EventType.REFRESH_MAIL_VIEW);
    }

    public void AddMails(params MailDB[] mails) {
      userMails.AddRange(mails);
      foreach (var mail in mails) {
        if (!mail.isReceived) {
          //更新RedPoint
          RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_MAIL, mail.id, MainMailRedPointTree.MAIN_MAIL, 1);
        }
      }
      EventCenter.Broadcast(Broadcast.EventType.REFRESH_MAIL_VIEW);
    }

    public void LoadUserDB(JObject json = null) {
      if (json == null) {
        m_playMode = PlayMode.LOCAL;
        FileStream fs;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(localUserDBPath)) {
          fs = new FileStream(localUserDBPath, FileMode.Open);
          m_userDB = (UserDB)bf.Deserialize(fs);
        } else {
          m_userDB = new UserDB();
          fs = File.Open(localUserDBPath, FileMode.Create);
          //序列化     
          bf.Serialize(fs, m_userDB);
        }
        fs.Close();
      } else {
        m_playMode = PlayMode.REMOTE;
        m_userDB = JsonConvert.DeserializeObject<UserDB>(json["userDB"].ToString());
        userToken = json["token"].Value<string>();
      }
    }

    public void ConsumeItems(bool isUsed = true, params ItemDB[] items) {
      //更新本地数据
      ItemDB tempItem;
      for (int i = 0; i < items.Length; i++) {
        tempItem = GetItemDB(items[i].id);
        if (tempItem != null && tempItem.num >= items[i].num) {
          tempItem.num -= items[i].num;
          if (tempItem.num == 0) {
            userItems.Remove(tempItem);
          }
          if (isUsed/*只有真正使用了道具才能更新*消耗道具类型任务*的进度*/) {
            foreach (var item in items) {
              TaskManager.instance.UpdateTaskState(RequestType.ITEM_NUM, item.id, -item.num);
            }
          }
          //更新RedPoint
          RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_ITEM, tempItem.id, MainItemRedPointTree.MAIN_ITEM, 0);
        }
      }
      //更新UI显示
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_USER_INFO);
    }

    public void AddItems(params ItemDB[] items) {
      //更新本地数据
      ItemDB tempItem;
      for (int i = 0; i < items.Length; i++) {
        tempItem = GetItemDB(items[i].id);
        if (tempItem == null) {
          tempItem = items[i];
          userItems.Add(tempItem);
        } else {
          tempItem.isViewed = false;
          tempItem.num += items[i].num;
        }
        //更新RedPoint
        RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_ITEM, tempItem.id, MainItemRedPointTree.MAIN_ITEM, 1);
      }
      //更新UI显示
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_USER_INFO);
      //更新任务进度
      foreach (var item in items) {
        TaskManager.instance.UpdateTaskState(RequestType.ITEM_NUM, item.id, item.num);
      }
    }

    public void AddCards(params CardDB[] cards) {
      userCards.AddRange(cards);
    }

    //卡牌比较特殊，需要是同一个对象
    public void RemoveCards(params CardDB[] cards) {
      foreach (var card in cards) {
        userCards.Remove(card);
      }
    }

    public void AddExp(int exp) {
      userExp += exp;
      MessageManager.instance.AddMessage(new Message(Message.Style.FADE, string.Format(StringMap.GET_EXP, exp)));
      int oLevel = userLv;
      while (userExp >= MathUtil.CalNeededExp(userLv)) {
        userExp -= MathUtil.CalNeededExp(userLv);
        userLv++;
      }
      //更新任务状态
      if (userLv > oLevel) {
        string tip = string.Format(StringMap.LEVEL_UP_TO, userLv);
        MessageManager.instance.AddMessage(new Message(Message.Style.FADE, tip));
        TaskManager.instance.UpdateTaskState(RequestType.LV_UP_TO, StringMap.EMPTY, userLv);
        if (TryGetMaxApBy(userLv, out int maxAp)) {
          userMaxAp = maxAp;
          userAp += userMaxAp;
        }
      }
      //更新UI显示
      EventCenter.Broadcast(Broadcast.EventType.UPDATE_USER_INFO);
    }

    public void CompleteTask(TaskDB task) {
      if (task.state == TaskState.COMPLETED) {
        task.Complete();
        TaskInfo taskInfo = GetTaskInfo(task.type, task.id);
        if (!string.IsNullOrEmpty(taskInfo.activeTaskId)) {
          TaskDB nextTask = GetTaskDB(taskInfo.activeTaskType, taskInfo.activeTaskId);
          if (nextTask != null && nextTask.state == TaskState.INACTIVE) {
            nextTask.state = TaskState.INPROCESS;
            nextTask.OnInit();
            EventCenter.Broadcast(Broadcast.EventType.REFRESH_TASK_VIEW);
          }
        }
        //任务排序用
        EventCenter.Broadcast(Broadcast.EventType.UPDATE_TASK_PROCESS);
        TaskManager.instance.UpdateTaskState(RequestType.TASK, task.id, 1);
      }
    }

    public void UpdateUserData(Action onSucceed = null) {
      //全量更新，以后再优化
      JObject data = JObject.FromObject(m_userDB);
      data.Merge(JObject.FromObject(new { types = typeof(UserDB).GetFieldNames() }));
      _UpdateUserDataInternal(data, onSucceed);
    }

    private void _UpdateUserDataInternal(JObject data, Action onSucceed = null) {
      if (m_playMode == PlayMode.REMOTE) {
        ServerManager.instance.UpdateUserData(data,
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
              Action ok = () => UpdateUserData(onSucceed);
              Action cancel = () => UIManager.instance.CloseAllAndOpenOne("login");
              UIManager.instance.OpenOne("confirmation", null, error, StringMap.RETRY,
                                         StringMap.RETURN_TO_LOGIN, ok, cancel);
            }
          });
      }else if(m_playMode == PlayMode.LOCAL) {
        onSucceed?.Invoke();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Open(localUserDBPath, FileMode.Create);
        //序列化     
        bf.Serialize(fs, m_userDB);
        fs.Close();
      }
    }
  }
}