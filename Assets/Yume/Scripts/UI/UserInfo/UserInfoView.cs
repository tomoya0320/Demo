using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.Math;
using Yume.String;

namespace Yume.UI {
  public class UserInfoView : MonoBehaviour {
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
      _goldNumText.text = DBManager.instance.GetItemDBNum(StringMap.GOLD_ID).ToString();
      _diamondNumText.text = DBManager.instance.GetItemDBNum(StringMap.DIAMOND_ID).ToString();

      _apText.text = DBManager.instance.userAp + "/" + DBManager.instance.userMaxAp;
      _lvText.text = DBManager.instance.userLv.ToString();
      _nameText.text = DBManager.instance.userName;

      float needEXP = MathUtil.CalNeededExp(DBManager.instance.userLv);
      _expImg.fillAmount = DBManager.instance.userExp / needEXP;
    }
  }
}