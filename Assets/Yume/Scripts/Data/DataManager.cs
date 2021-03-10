using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Yume.Broadcast;
using Yume.RedDotSystem;
using Yume.Resource;
using EventType = Yume.Broadcast.EventType;

namespace Yume.Data {
  public static class DataManager {
    private static bool s_saveData;
    public const string USER_DB_KEY = "savedata";

    #region User

    private static UserDB s_userDB;
    public static bool isLogged => s_userDB != null;
    public static string playerName => s_userDB.player.name;
    public static int playerLv {
      private set => s_userDB.player.lv = value;
      get => s_userDB.player.lv;
    }
    public static int playerTeamIndex {
      private set => s_userDB.player.teamIndex = value;
      get => s_userDB.player.teamIndex;
    }
    public static List<ItemData> playerItems => s_userDB.items;
    public static List<CardData> playerCards => s_userDB.cards;
    public static List<TeamData> playerTeams => s_userDB.teams;
    public static List<UnitData> playerChars => s_userDB.chars;
    public static List<MailData> playerMails => s_userDB.mails;
    public static List<TaskData> playerTasks => s_userDB.tasks;
    public static List<LevelData> playerLevels => s_userDB.levels;

    #endregion

    #region Data

    public static ItemData GetItem(string id) {
      return playerItems.Find(obj => obj.id == id);
    }

    public static int GetItemCount(string id) {
      return playerItems.Find(obj => obj.id == id)?.count ?? 0;
    }

    public static List<TaskData> GetTaskList(TaskType taskType) {
      return playerTasks.FindAll(obj => ResourceManager.Load<TaskTemplate>(obj.id).type == taskType);
    }

    public static TaskData GetTask(string taskId) {
      return playerTasks.Find(obj => obj.id == taskId);
    }

    /// <summary>
    /// 因为卡牌能重复所以只能通过guid获取
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static CardData GetCard(int guid) {
      return playerCards.Find(obj => obj.guid == guid);
    }

    public static UnitData GetCharacter(string charId) {
      return playerChars.Find(obj => obj.id == charId);
    }

    #endregion

    #region Operate

    public static void OnLateUpdate() {
      if (s_saveData) {
        _SaveData();
        s_saveData = false;
      }
    }

    #region Mail

    //邮件比较特殊，需要是同一个对象
    public static void DeleteMail(MailData mail) {
      if (mail.isReceived) {
        playerMails.Remove(mail);

        s_saveData = true;
      }
    }

    public static void GetAttachment(MailData mail) {
      if (!mail.isReceived) {
        var mailTemplate = ResourceManager.Load<MailTemplate>(mail.id);
        foreach (var item in mailTemplate.attachments) {
          AddItem(item.id, item.count);
        }   
        mail.isReceived = true;
        RedDotManager.SetRedDotNum(RedDotTreeType.MAIN_MAIL, mail.id, MainMailRedDotTree.MAIN_MAIL, 0);
        s_saveData = true;
      }
    }

    public static void AddMail(MailData mail) {
      playerMails.Add(mail);
      if (!mail.isReceived) {
        RedDotManager.SetRedDotNum(RedDotTreeType.MAIN_MAIL, mail.id, MainMailRedDotTree.MAIN_MAIL, 1);
      }
      s_saveData = true;
    }

    #endregion

    #region Card

    public static void AddCard(string id, int phase) {
      CardData card = new CardData(id, phase);
      playerCards.Add(card);
      card.guid = s_userDB.player.validGuid++;
      s_saveData = true;
    }

    public static void UpdateCard(CardData card) {
      card.phase++;
      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_CARD_INFO);
      s_saveData = true;
    }
    
    #endregion

    #region Character

    public static void AddCharacter(string id, int level, int passiveSkillLevel) {
      UnitData character = new UnitData(id, level, passiveSkillLevel);
      playerChars.Add(character);
      s_saveData = true;
    }

    public static void AddCharExp(UnitData character, int exp) {
      var unitTemplate = ResourceManager.Load<UnitTemplate>(character.id);
      character.exp += exp;
      while (character.exp >= MathUtil.CalNeedCharExp(character.lv)) {
        character.exp -= MathUtil.CalNeedCharExp(character.lv);
        character.lv++;
        if (character.lv >= unitTemplate.maxLevel) {
          character.exp = 0;
          break;
        }
      }
      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_CHAR_DETAIL);
      s_saveData = true;
    }

    public static void UpdateCharPassiveSkill(UnitData character) {
      character.passiveSkillLv++;
      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_CHAR_DETAIL);
      s_saveData = true;
    }

    public static void IfAddCharExp(string charId, int currentLv, int currentExp, int exp, out int finalLv, out int leftExp) {
      var unitTemplate = ResourceManager.Load<UnitTemplate>(charId);
      currentExp += exp;
      while (currentExp >= MathUtil.CalNeedCharExp(currentLv)) {
        currentExp -= MathUtil.CalNeedCharExp(currentLv);
        currentLv++;
        if (currentLv >= unitTemplate.maxLevel) {
          currentExp = 0;
          break;
        }
      }
      finalLv = currentLv;
      leftExp = currentExp;
    }

    #endregion

    #region Item

    public static void ConsumeItem(string id, int count, bool isUsed = true) {
      ItemData tempItem = GetItem(id);
      if (tempItem != null && tempItem.count >= count) {
        tempItem.count -= count;
        if (tempItem.count == 0) {
          playerItems.Remove(tempItem);
        }
        _UpdateTaskState(RequestType.ITEM_NUM, id, isUsed ? -count : 0);

        //更新UI显示
        EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
        EventCenter.Broadcast(EventType.UPDATE_USER_INFO);
        s_saveData = true;
      }
    }

    public static void AddItem(string id, int count) {
      ItemData tempItem = GetItem(id);
      if (tempItem == null) {
        tempItem = new ItemData(id, count);
        playerItems.Add(tempItem);
      } else {
        tempItem.count += count;
      }

      // 提示
      ItemTemplate itemTemplate = ResourceManager.Load<ItemTemplate>(id);
      Sprite itemIcon = ResourceManager.Load<Sprite>(itemTemplate.iconFileName);
      // 小 心 引 用 类 型 (改了后没这个问题了)
      string content = $"{StringMap.GET}{itemTemplate.name}*{count}";
      MessageManager.AddMessage(new Message(Message.Style.FADE, content, true, itemIcon));

      // 经验值额外再处理一次
      if (id == StringMap.EXP_ITEM_ID) {
        _AddExpInternal();
      }

      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_ITEM_INFO);
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);

      //更新任务进度
      _UpdateTaskState(RequestType.ITEM_NUM, id, count);
      s_saveData = true;
    }

    #endregion

    #region Task

    private static void _UpdateTaskState(RequestType type, string reqId, int reqCount) {
      foreach (var task in playerTasks) {
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
            _CheckTask(task);
          }
        }
      }
      EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
      s_saveData = true;
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
        s_saveData = true;
      }
    }

    private static void _CheckTask(TaskData task) {
      if (task.state == TaskState.INPROCESS || task.state == TaskState.COMPLETED) {
        var taskRequest = ResourceManager.Load<TaskTemplate>(task.id).taskRequest;
        bool completed = task.process >= taskRequest.requiredCount;

        if (completed && task.state == TaskState.INPROCESS) {
          task.state = TaskState.COMPLETED;
          TaskTemplate taskTemplate = ResourceManager.Load<TaskTemplate>(task.id);
          RedDotManager.SetRedDotNum(RedDotTreeType.MAIN_TASK, task.id, taskTemplate.type.GetDescription(), 1);
          string content = string.Format(StringMap.TASK_COMPLETED, taskTemplate.title);
          MessageManager.AddMessage(new Message(Message.Style.FADE, content));
          s_saveData = true;
        } else if (!completed && task.state == TaskState.COMPLETED) {
          task.state = TaskState.INPROCESS;
          TaskTemplate taskTemplate = ResourceManager.Load<TaskTemplate>(task.id);
          RedDotManager.SetRedDotNum(RedDotTreeType.MAIN_TASK, task.id, taskTemplate.type.GetDescription(), 0);
          s_saveData = true;
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
            task.process = playerLv;
            break;
          }
        }
        s_saveData = true;
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

        RedDotManager.SetRedDotNum(RedDotTreeType.MAIN_TASK, task.id, taskTemplate.type.GetDescription(), 0);

        if (!string.IsNullOrEmpty(taskTemplate.activeTaskId)) {
          TaskData nextTask = GetTask(taskTemplate.activeTaskId);
          if (nextTask != null && nextTask.state == TaskState.INACTIVE) {
            _ActivateTask(nextTask);
          }
        }

        _UpdateTaskState(RequestType.TASK, task.id, 1);
        EventCenter.Broadcast(EventType.UPDATE_TASK_PROCESS);
        s_saveData = true;
      }
    }

    #endregion

    #region Team

    public static void ChangeTeamIndex(int newTeamIndex) {
      playerTeamIndex = newTeamIndex;
      s_saveData = true;
    }

    public static void ChangeTeamName(TeamData team, string newName) {
      team.name = newName;
      s_saveData = true;
    }

    public static void ChangeTeamChar(TeamData team, string newCharId) {
      team.charId = newCharId;
      s_saveData = true;
    }

    public static void ChangeTeamCards(TeamData team, List<int> newCardGuidList) {
      team.cardGuids = newCardGuidList;
      s_saveData = true;
    }

    #endregion

    #region Exp&Lv

    private static void _AddExpInternal() {
      int oLevel = playerLv;
      ItemData expItem = GetItem(StringMap.EXP_ITEM_ID);
      while (expItem.count >= MathUtil.CalNeedUserExp(playerLv)) {
        ConsumeItem(StringMap.EXP_ITEM_ID, MathUtil.CalNeedUserExp(playerLv));
        playerLv++;
      }
      //更新任务状态
      if (playerLv > oLevel) {
        string content = string.Format(StringMap.LEVEL_UP_TO, playerLv);
        MessageManager.AddMessage(new Message(Message.Style.SLIDE, content));
        _UpdateTaskState(RequestType.LV_UP_TO, StringMap.EMPTY, playerLv);
        s_saveData = true;
      }
      //更新UI显示
      EventCenter.Broadcast(EventType.UPDATE_USER_INFO);
    }

    #endregion

    #region Level(关卡)

    public static void CompleteLevel(string id) {
      var level = playerLevels.Find(l => l.id == id);
      if (level != null) {
        LevelTemplate levelTemplate = ResourceManager.Load<LevelTemplate>(id);
        if (!level.completed) {
          level.completed = true;
          foreach (var item in levelTemplate.firstCompletedRewards.items) {
            AddItem(item.id, item.count);
          }
          // 更新UI
          EventCenter.Broadcast(EventType.UPDATE_MAIN_STORY_LEVEL);
          s_saveData = true;
        }
        foreach (var item in levelTemplate.regularRewards.items) {
          AddItem(item.id, item.count);
        }
      }
    }

    #endregion

    #region Load&Save

    public static void LoadUserData() {
      if (PlayerPrefs.HasKey(USER_DB_KEY)) {
        s_userDB = JsonConvert.DeserializeObject<UserDB>(PlayerPrefs.GetString(USER_DB_KEY));
      } else {
        s_userDB = UserDB.Create();
      }
    }

    private static void _SaveData() {
      PlayerPrefs.SetString(USER_DB_KEY, JsonConvert.SerializeObject(s_userDB));
    }

    #endregion

    #endregion
  }
}