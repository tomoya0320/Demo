using UnityEngine;
using UnityEngine.UI;
using Yume.DB;

namespace Yume.Tester {
  public class UserDBDisplay : MonoBehaviour {
    [SerializeField]
    private Text _accountText;

    private void Start() {
      _accountText.text = DBManager.userDB.account;
    }
  }
}