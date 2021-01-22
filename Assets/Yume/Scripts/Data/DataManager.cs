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
  public class DataManager : Singleton<DataManager> {
    private enum PlayMode {
      LOCAL, REMOTE
    }

    private PlayMode m_playMode;
    private string localUserDBPath {
      get {
        return Path.Combine(Application.persistentDataPath, "savedata");
      }
    }

    protected override void OnInit() {
      m_itemJson = AssetLoader.instance.GetValue<JObject>("itemJson");
      m_taskJson = AssetLoader.instance.GetValue<JObject>("taskJson");
      m_cardJson = AssetLoader.instance.GetValue<JObject>("cardJson");
      m_unitJson = AssetLoader.instance.GetValue<JObject>("unitJson");
      m_lvMaxApMapJson = AssetLoader.instance.GetValue<JObject>("lvMaxApMapJson");
      m_attribCurveList = AssetLoader.instance.GetValue<List<AttribCurve>>("attribCurves");
    }

    #region User
    private UserData m_user;
    public bool isLogged => m_user != null;
    public string userToken { private set; get; }
    public string userAccount {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.account;
      }
    }
    public string userPassword {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.password;
      }
    }
    public string userName {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.name;
      }
    }
    public int userAp {
      private set {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_user.ap = value;
      }
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.ap;
      }
    }
    public int userMaxAp {
      private set {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_user.maxAp = value;
      }
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.maxAp;
      }
    }
    public int userLv {
      private set {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_user.lv = value;
      }
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.lv;
      }
    }
    public int userExp {
      private set {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        m_user.exp = value;
      }
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.exp;
      }
    }
    public List<ItemData> userItems {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.items;
      }
    }
    public List<CardData> userCards {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.cards;
      }
    }
    public List<TeamData> userTeams {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.teams;
      }
    }
    public List<UnitData> userChars {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.chars;
      }
    }
    public List<MailData> userMails {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.mails;
      }
    }
    public Dictionary<TaskType, List<TaskData>> userTasks {
      get {
        if (m_user == null) {
          m_user = new UserData();
          Debug.LogWarning("用户未登录，创建新的存档！");
        }
        return m_user.tasks;
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

    #region Prototype
    public ItemTemplate GetItemTemplate(string id) {
      if (m_itemJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<ItemTemplate>(m_itemJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("item id:{0} is not found", id));
      }
    }

    public CardTemplate GetCardTemplate(string id) {
      if (m_cardJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<CardTemplate>(m_cardJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("card id:{0} is not found", id));
      }
    }

    public TaskTemplate GetTaskTemplate(TaskType taskType, string id) {
      if (m_taskJson.ContainsKey(taskType.ToString())) {
        JObject tempJObject = m_taskJson[taskType.ToString()].Value<JObject>();
        if (tempJObject.ContainsKey(id)) {
          return JsonConvert.DeserializeObject<TaskTemplate>(tempJObject[id].Value<JObject>().ToString());
        } else {
          throw new Exception(string.Format("task id:{0} is not found", id));
        }
      } else {
        throw new Exception(string.Format("task type:{0} is not found", taskType.ToString()));
      }
    }

    public UnitTemplate GetUnitTemplate(string id) {
      if (m_unitJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<UnitTemplate>(m_unitJson[id].Value<JObject>().ToString());
      }
      throw new Exception($"unit is not found! id:{id}");
    }
    #endregion

    #region Data
    public Dictionary<TaskType, List<TaskData>> GetDefaultTasks() {
      Dictionary<TaskType, List<TaskData>> defaultTasks = new Dictionary<TaskType, List<TaskData>>();

      foreach (TaskType type in Enum.GetValues(typeof(TaskType))) {
        List<TaskData> tasks = new List<TaskData>();
        foreach (var info in m_taskJson[type.ToString()].Values()) {
          tasks.Add(JsonConvert.DeserializeObject<TaskData>(info.ToString()));
        }
        defaultTasks.Add(type, tasks);
      }

      return defaultTasks;
    }

    public bool TryGetMaxAp(int level, out int maxAp) {
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

    public ItemData GetItem(string id) {
      return userItems.Find(obj => obj.id == id);
    }

    public int GetItemCount(string id) {
      ItemData itemDB = userItems.Find(obj => obj.id == id);
      return itemDB == null ? 0 : itemDB.count;
    }

    public List<TaskData> GetTaskList(TaskType taskType) {
      return userTasks[taskType];
    }

    public TaskData GetTask(TaskType taskType, string taskId) {
      return userTasks[taskType].Find(obj => obj.id == taskId);
    }

    public CardData GetCard(string cardId) {
      //TODO:解决Id重复的问题
      return userCards.Find(obj => obj.id == cardId);
    }

    public UnitData GetChar(string charId) {
      return userChars.Find(obj => obj.id == charId);
    }
    #endregion

    #region Operate
    //邮件比较特殊，需要是同一个对象
    public void DeleteMail(MailData mail) {
      if (mail.isReceived) {
        userMails.Remove(mail);
        EventCenter.Broadcast(EventType.REFRESH_MAIL_VIEW);
      }
    }

    public void GetAttachment(MailData mail) {
      if (!mail.isReceived) {
        mail.isReceived = true;
        foreach (var item in mail.attachment) {
          AddItem(item);
        }
        RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_MAIL, mail.id, MainMailRedPointTree.MAIN_MAIL, 0);
      }
    }

    public void AddMail(MailData mail) {
      userMails.Add(mail);
      if (!mail.isReceived) {
        RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_MAIL, mail.id, MainMailRedPointTree.MAIN_MAIL, 1);
      }
      EventCenter.Broadcast(EventType.REFRESH_MAIL_VIEW);
    }

    public void ConsumeItem(ItemData item, bool isUsed = true) {
      ItemData tempItem = GetItem(item.id);
      if (tempItem != null && tempItem.count >= item.count) {
        tempItem.count -= item.count;
        if (tempItem.count == 0) {
          userItems.Remove(tempItem);
        }
        _UpdateTaskState(RequestType.ITEM_NUM, item.id, isUsed ? -item.count : 0);

        RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_ITEM, tempItem.id, MainItemRedPointTree.MAIN_ITEM, 0);

        //更新UI显示
        EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
        EventCenter.Broadcast(EventType.UPDATE_USER_INFO);
      }
    }

    public void AddItem(ItemData item) {
      ItemData tempItem = GetItem(item.id);
      if (tempItem == null) {
        tempItem = item;
        userItems.Add(tempItem);
      } else {
        tempItem.isViewed = false;
        tempItem.count += item.count;
      }

      RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_ITEM, tempItem.id, MainItemRedPointTree.MAIN_ITEM, 1);

      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);

      //更新任务进度
      _UpdateTaskState(RequestType.ITEM_NUM, item.id, item.count);
    }

    public void _UpdateTaskState(RequestType type, string reqId, int reqCount) {
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
        _UpdateTaskState(RequestType.LV_UP_TO, StringMap.EMPTY, userLv);
        if (TryGetMaxAp(userLv, out int maxAp)) {
          userMaxAp = maxAp;
          userAp += userMaxAp;
        }
      }
      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);
    }

    public void ActivateTask(TaskData task) {
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

    private void _CheckTask(TaskData task) {
      if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
        bool completed = true;
        foreach (TaskRequestData taskRequest in task.taskRequests) {
          completed &= taskRequest.currentCount >= taskRequest.requiredCount;
        }

        if (completed && task.state == TaskState.INPROCESS) {
          task.state = TaskState.COMPLETED;
          TaskTemplate taskTemplate = GetTaskTemplate(task.type, task.id);
          RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, task.type.GetDescription(), 1);
          MessageManager.instance.AddMessage(new Message(Message.Style.FADE, string.Format(StringMap.TASK_COMPLETED, taskTemplate.title)));
        } else if (!completed && task.state == TaskState.COMPLETED) {
          task.state = TaskState.INPROCESS;
          RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, task.type.GetDescription(), 0);
        }
      }
    }

    private void _SyncTaskHadCount(TaskRequestData taskRequest) {
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

    public async void CompleteTask(TaskData task) {
      if (task.state == TaskState.COMPLETED) {
        task.state = TaskState.FINAL;
        TaskTemplate taskTemplate = GetTaskTemplate(task.type, task.id);
        foreach (ItemData item in taskTemplate.taskRewards.items) {
          AddItem(item);
          ItemTemplate itemTemplate = GetItemTemplate(item.id);
          Sprite itemIcon = await ResourceManager.LoadAssetAsync<Sprite>(itemTemplate.iconFileName);
          MessageManager.instance.AddMessage(new Message(Message.Style.FADE,
          string.Format(StringMap.GET + "{0}*{1}", itemTemplate.name, item.count), itemIcon));
        }

        foreach (TaskRequestData taskRequest in task.taskRequests) {
          if (taskRequest.consumable && taskRequest.type == RequestType.ITEM_NUM) {
            ConsumeItem(new ItemData(taskRequest.requirementId, taskRequest.requiredCount), false);
          }
        }

        RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_TASK, task.id, task.type.GetDescription(), 0);

        if (!string.IsNullOrEmpty(taskTemplate.activeTaskId)) {
          TaskData nextTask = GetTask(taskTemplate.activeTaskType, taskTemplate.activeTaskId);
          if (nextTask != null && nextTask.state == TaskState.INACTIVE) {
            ActivateTask(nextTask);
          }
        }
        //任务排序用
        EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
        _UpdateTaskState(RequestType.TASK, task.id, 1);
      }
    }

    public void LoadUserDB(JObject json = null) {
      if (json == null) {
        m_playMode = PlayMode.LOCAL;
        FileStream fs;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(localUserDBPath)) {
          fs = new FileStream(localUserDBPath, FileMode.Open);
          m_user = (UserData)bf.Deserialize(fs);
        } else {
          m_user = new UserData();
          fs = File.Open(localUserDBPath, FileMode.Create);
          //序列化     
          bf.Serialize(fs, m_user);
        }
        fs.Close();
      } else {
        m_playMode = PlayMode.REMOTE;
        m_user = JsonConvert.DeserializeObject<UserData>(json["user"].ToString());
        userToken = json["token"].Value<string>();
      }
    }

    public void SaveData(Action onSucceed = null) {
      JObject data = JObject.FromObject(m_user);
      data.Merge(JObject.FromObject(new { types = typeof(UserData).GetFieldNames() }));

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
            if (!string.IsNullOrEmpty(error)) {
              Action ok = () => SaveData(onSucceed);
              Action cancel = () => UIManager.instance.CloseAllAndOpenOne("login");
              UIManager.instance.OpenOne("confirmation", null, error, StringMap.RETRY,
                                         StringMap.RETURN_TO_LOGIN, ok, cancel);
            } else {
              onSucceed?.Invoke();
            }
          });
      } else if (m_playMode == PlayMode.LOCAL) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Open(localUserDBPath, FileMode.Create);
        //序列化     
        bf.Serialize(fs, m_user);
        fs.Close();

        onSucceed?.Invoke();
      }
    }
    #endregion
  }
}