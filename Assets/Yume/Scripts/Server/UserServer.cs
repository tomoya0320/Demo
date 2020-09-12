using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Yume.Scene;
using Yume.DB;
using Yume.UI;
using Yume.String;

namespace Yume.Server {
  public class UserServer : MonoBehaviour {
    [SerializeField]
    private InputField _accountInput;
    [SerializeField]
    private InputField _passwordInput;
    [SerializeField]
    private InputField _newPasswordInput;
    [SerializeField]
    private Text _result;
    [SerializeField]
    private SceneLoader _sceneJump;

    public void OnLogin() {
      if (!string.IsNullOrEmpty(_accountInput.text) && !string.IsNullOrEmpty(_passwordInput.text)) {
        _result.text = StringMap.EMPTY;
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
                  DBManager.LoadUserDB(json);
                  _sceneJump.GoToNextScene();
#if TEST
                  UIController.instance.messageManager.AddMessage(new Message(
                  Message.Style.SLIDE, "用户：" + DBManager.userAccount));
#endif
                  break;
                }
            }
          }
        });
      } else {
        _result.text = "请输入完整的账号和密码";
      }
    }

    public void OnRegister() {
      if (!string.IsNullOrEmpty(_accountInput.text) && !string.IsNullOrEmpty(_passwordInput.text)) {
        _result.text = StringMap.EMPTY;
        ServerConnection.instance.Register(_accountInput.text, _passwordInput.text, request => {
          if (!string.IsNullOrEmpty(request.error)) {
            _result.text = "连接出错！" + request.error;
          } else {
            var json = JObject.Parse(request.downloadHandler.text);
            int code = json["code"].Value<int>();
            switch (code) {
              case -1: {
                  _result.text = "用户已存在！";
                  break;
                }
              case 1: {
                  _result.text = "注册成功！";
                  break;
                }      
              case -2: {
                  _result.text = "注册失败！";
                  break;
                }
            }
          }
        });
      } else {
        _result.text = "请输入完整的账号和密码";
      }
    }

    public void OnAlterPassword() {
      if (!string.IsNullOrEmpty(_accountInput.text) && !string.IsNullOrEmpty(_passwordInput.text) &&
          !string.IsNullOrEmpty(_newPasswordInput.text)) {
        _result.text = StringMap.EMPTY;
        ServerConnection.instance.AlterPassword(_accountInput.text, _passwordInput.text, _newPasswordInput.text,
          request => {
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
                    _result.text = "原密码错误！";
                    break;
                  }
                case 1: {
                    _result.text = "修改密码成功！";
                    break;
                  }
                case -2: {
                    _result.text = "修改密码失败！";
                    break;
                  }
              }
            }
          });
      } else {
        _result.text = "请输入完整的账号和新旧密码";
      }
    }

    public void OnPlayInLocal() {
      DBManager.LoadUserDB();
      _sceneJump.GoToNextScene();
#if TEST
      UIController.instance.messageManager.AddMessage(new Message(
      Message.Style.SLIDE, "用户：" + DBManager.userAccount));
#endif
    }
  }
}