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
    private Image _expImg;

    private void Start() {
      EventCenter.AddListener(Broadcast.EventType.UPDATE_USER_INFO, _UpdateUserInfo);
      _UpdateUserInfo();
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(Broadcast.EventType.UPDATE_USER_INFO, _UpdateUserInfo);
    }

    private void _UpdateUserInfo() {
      _goldNumText.text = DBManager.GetItemDBNum(StringMap.GOLD_ID).ToString();
      _diamondNumText.text = DBManager.GetItemDBNum(StringMap.DIAMOND_ID).ToString();

      _apText.text = DBManager.userAp + "/" + DBManager.userMaxAp;
      _lvText.text = DBManager.userLv.ToString();
      _nameText.text = DBManager.userName;

      float needEXP = MathUtil.CalNeededExp(DBManager.userLv);
      _expImg.fillAmount = DBManager.userExp / needEXP;
    }
  }
}