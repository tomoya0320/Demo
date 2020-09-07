using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Yume.Scene;
using Newtonsoft.Json;
using Yume.DB;

namespace Yume.Server {
  public class UserServer : MonoBehaviour {
    [SerializeField]
    private InputField _accountInput;
    [SerializeField]
    private InputField _passwordInput;
    [SerializeField]
    private Text _result;
    [SerializeField]
    private SceneJump _sceneJump;

    public void OnLogin() {
      if (!string.IsNullOrEmpty(_accountInput.text) && !string.IsNullOrEmpty(_passwordInput.text)) {
        ServerConnection.instance.Login(_accountInput.text, _passwordInput.text, request => {
          if (!string.IsNullOrEmpty(request.error)) {
            _result.text = "连接出错！" + request.error;
          } else {
            var json = JObject.Parse(request.downloadHandler.text);
            int code = json["code"].Value<int>();
            switch (code) {
              case 0: {
                  _result.text = "用户不存在！";
                  break;
                }
              case -1: {
                  _result.text = "密码错误！";
                  break;
                }
              case 1: {
                  DBManager.userDB = JsonConvert.DeserializeObject<UserDB>(json.ToString());
                  _sceneJump.GoToNextScene();
                  break;
                }
            }
          }
        });
      }
    }

    public void Register() {

    }
  }
}