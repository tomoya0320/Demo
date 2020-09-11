using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Yume.Scene;
using Newtonsoft.Json;
using Yume.DB;
using System.Linq;
using Yume.String;
using System.Collections.Generic;
using Yume.UI;

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
        _result.text = string.Empty;
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
                  DBManager.userDB.account = json[StringMap.ACCOUNT].Value<string>();
                  DBManager.userDB.itemDBs = json[StringMap.ITEMS].ToObject<List<ItemDB>>();
                  _sceneJump.GoToNextScene();
                  UIController.instance.messageManager.AddMessage(new Message(
                  Message.Style.SLIDE, "用户：" + DBManager.userDB.account));
                  break;
                }
            }
          }
        });
      }
    }

    public void OnRegister() {
      if (!string.IsNullOrEmpty(_accountInput.text) && !string.IsNullOrEmpty(_passwordInput.text)) {
        _result.text = string.Empty;
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
            }
          }
        });
      }
    }
  }
}