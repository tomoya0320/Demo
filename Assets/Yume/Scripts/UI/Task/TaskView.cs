using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.Battle;
using Yume.String;
using Yume.Task;
using Yume.DB;

namespace Yume.UI {
  public class TaskView : MonoBehaviour {
    public TaskDB task { get; private set; }
    [SerializeField]
    private Text _title;
    [SerializeField]
    private Text _description;
    [SerializeField]
    private YButton _getRewardButton;
    [SerializeField]
    private RectTransform _processContent;
    [SerializeField]
    private RectTransform _rewardContent;
    [SerializeField]
    private GameObject _rewardPrefab;
    [SerializeField]
    private GameObject _processTextPrefab;
    private List<Text> m_processTexts;

    private void Start() {
      EventCenter.AddListener(Broadcast.EventType.UPDATE_TASK_INFO, UpdateTaskInfo);
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(Broadcast.EventType.UPDATE_TASK_INFO, UpdateTaskInfo);
    }

    public async void OnInit(TaskDB task) {
      this.task = task;

      m_processTexts = new List<Text>();

      TaskInfo taskInfo = DBManager.GetTaskInfo(task.type, task.id);
      _title.text = taskInfo.title;
      _description.text = taskInfo.description;

      foreach (ItemDB item in taskInfo.taskRewards.items) {
        GameObject temp = Instantiate(_rewardPrefab, _rewardContent);
        temp.GetComponent<Image>().sprite = await Addressables.LoadAssetAsync<Sprite>
        (DBManager.GetItemInfo(item.id).iconFileName).Task;
        temp.transform.GetChild(0).GetComponent<Text>().text = item.num.ToString();
      }

      for (int i = 0; i < task.taskRequests.Count; i++) {
        GameObject temp = Instantiate(_processTextPrefab, _processContent);
        m_processTexts.Add(temp.GetComponent<Text>());
      }

      if (task.state == TaskState.COMPLETED) {
        transform.SetAsFirstSibling();
      }

      UpdateTaskInfo();
    }

    protected virtual void UpdateTaskInfo() {
      if (_processContent) {
        for (int i = 0; i < task.taskRequests.Count; i++) {
          m_processTexts[i].text = string.Format("{0}:{1}/{2}", task.taskRequests[i].description,
          task.taskRequests[i].currentNum, task.taskRequests[i].requiredNum);
        }
        _processContent.gameObject.SetActive(task.state == TaskState.INPROCESS);
      }

      _getRewardButton.gameObject.SetActive(task.state == TaskState.COMPLETED || task.state == TaskState.FINAL);
      _getRewardButton.interactable = task.state == TaskState.COMPLETED;
      _getRewardButton.textComponent.text = task.state == TaskState.FINAL ? StringMap.COMPLETED : StringMap.CLICK_GET;
    }

    public virtual void GetReward() {
      DBManager.CompleteTask(task, () => {
        _getRewardButton.textComponent.text = StringMap.COMPLETED;
        _getRewardButton.interactable = false;
        transform.SetAsLastSibling();
      });
    }
  }
}