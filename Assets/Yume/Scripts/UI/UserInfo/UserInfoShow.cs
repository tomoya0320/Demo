using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.Math;
using Yume.String;

namespace Yume.UI {
  public class UserInfoShow : MonoBehaviour {
    [SerializeField]
    private Text _apText;
    [SerializeField]
    private Text _diamondNumText;
    [SerializeField]
    private Text _goldNumText;
    [SerializeField]
    private Text _lvText;
    [SerializeField]
    private Text _nameText;
    [SerializeField]
    private Text _expText;
    [SerializeField]
    private Image _expImg;

    private void Start() {
      EventCenter.AddListener(Broadcast.EventType.UPDATE_USER_INFO, _UpdateUserInfo);
      _UpdateUserInfo();
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(Broadcast.EventType.UPDATE_USER_INFO, _UpdateUserInfo);
    }

    private void _UpdateUserInfo() {
      _apText.text = DBManager.userAp + "/" + DBManager.userMaxAp;
      _goldNumText.text = DBManager.GetItemDB(StringMap.GOLD_ID).num.ToString();
      _diamondNumText.text = DBManager.GetItemDB(StringMap.DIAMOND_ID).num.ToString();

      _lvText.text = DBManager.userLv.ToString();
      _nameText.text = DBManager.userName;

      float needEXP = MathUtil.CalNeededExp(DBManager.userLv);
      _expText.text = DBManager.userExp + "/" + needEXP;
      _expImg.fillAmount = DBManager.userExp / needEXP;
    }
  }
}