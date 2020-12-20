using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.DB;
using Yume.Server;
using Yume.String;

namespace Yume.UI {
  public class UILogin : UIBase {
    private InputField m_nameInput;
    private InputField m_accountInput;
    private InputField m_passwordInput;
    private InputField m_newPasswordInput;
    private Text m_result;
    private GameObject m_main;
    private GameObject m_second;

    private void _OnLogin() {
      if (!string.IsNullOrEmpty(m_accountInput.text) && !string.IsNullOrEmpty(m_passwordInput.text)) {
        m_result.text = StringMap.EMPTY;
        ServerManager.instance.Login(m_accountInput.text, m_passwordInput.text, request => {
          if (!string.IsNullOrEmpty(request.error)) {
            m_result.text = "连接出错！" + request.error;
          } else {
            var json = JObject.Parse(request.downloadHandler.text);
            int resultCode = json["resultCode"].Value<int>();
            switch (resultCode) {
              case 0: {
                  m_result.text = "用户不存在！";
                  break;
                }
              case -1: {
                  m_result.text = "密码错误！";
                  break;
                }
              case 1: {
                  _GoToMain(json);
                  break;
                }
            }
          }
        });
      } else {
        m_result.text = "请输入完整的账号和密码";
      }
    }

    private void _OnRegister() {
      if (!string.IsNullOrEmpty(m_nameInput.text) && !string.IsNullOrEmpty(m_passwordInput.text) &&
          !string.IsNullOrEmpty(m_accountInput.text)) {

        m_result.text = StringMap.EMPTY;
        ServerManager.instance.Register(m_nameInput.text, m_accountInput.text, m_passwordInput.text, request => {
          if (!string.IsNullOrEmpty(request.error)) {
            m_result.text = "连接出错！" + request.error;
          } else {
            var json = JObject.Parse(request.downloadHandler.text);
            int resultCode = json["resultCode"].Value<int>();
            switch (resultCode) {
              case -1: {
                  m_result.text = "用户已存在！";
                  break;
                }
              case 1: {
                  m_result.text = "注册成功！";
                  break;
                }
            }
          }
        });
      } else {
        m_result.text = "请输入完整的昵称、账号和密码";
      }
    }

    private void _OnAlterPassword() {
      if (!string.IsNullOrEmpty(m_accountInput.text) && !string.IsNullOrEmpty(m_passwordInput.text) &&
          !string.IsNullOrEmpty(m_newPasswordInput.text)) {
        m_result.text = StringMap.EMPTY;
        ServerManager.instance.AlterPassword(m_accountInput.text, m_passwordInput.text, m_newPasswordInput.text,
          request => {
            if (!string.IsNullOrEmpty(request.error)) {
              m_result.text = "连接出错！" + request.error;
            } else {
              var json = JObject.Parse(request.downloadHandler.text);
              int resultCode = json["resultCode"].Value<int>();
              switch (resultCode) {
                case 0: {
                    m_result.text = "用户不存在！";
                    break;
                  }
                case -1: {
                    m_result.text = "原密码错误！";
                    break;
                  }
                case 1: {
                    m_result.text = "修改密码成功！";
                    break;
                  }
              }
            }
          });
      } else {
        m_result.text = "请输入完整的账号和新旧密码";
      }
    }

    private void _OnPlayInLocal() {
      _GoToMain();
    }

    private void _GoToMain(JObject json = null) {
      DBManager.instance.LoadUserDB(json);
      UIManager.instance.OpenOne("blackMask", null, () => UIManager.instance.CloseAllAndOpenOne("main"));
    }

    private void _ClearInput() {
      m_nameInput.text = StringMap.EMPTY;
      m_accountInput.text = StringMap.EMPTY;
      m_passwordInput.text = StringMap.EMPTY;
      m_newPasswordInput.text = StringMap.EMPTY;
    }

    private void _AddListener() {
      GetComponent<Button>("Login").onClick.AddListener(_OnLogin);
      GetComponent<Button>("Register").onClick.AddListener(_OnRegister);
      GetComponent<Button>("Local").onClick.AddListener(_OnPlayInLocal);
      GetComponent<Button>("AlterPsd").onClick.AddListener(_OnAlterPassword);
      GetComponent<Button>("SwitchAlterPsd").onClick.AddListener(() => {
        _ClearInput();
        m_main.SetActive(false);
        m_second.SetActive(true);
      });
      GetComponent<Button>("Back").onClick.AddListener(() => {
        _ClearInput();
        m_main.SetActive(true);
        m_second.SetActive(false);
      });
    }

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_nameInput = GetComponent<InputField>("NameInput");
      m_accountInput = GetComponent<InputField>("AccountInput");
      m_passwordInput = GetComponent<InputField>("PsdInput");
      m_newPasswordInput = GetComponent<InputField>("NewPsdInput");
      m_result = GetComponent<Text>("Result");
      m_main = GetGameObject("Main");
      m_second = GetGameObject("Second");

      _AddListener();
    }

    public override UIBase Recycle() {
      base.Recycle();
      _ClearInput();
      return this;
    }

    public override void Open(Action onOpenStart = null, Action onOpenEnd = null, params object[] args) {
      onOpenStart = onOpenStart.AddAction(() => {
        UIManager.instance.SetMask(true);
        root.SetActive(true);
      });
      onOpenEnd = onOpenEnd.AddAction(() => UIManager.instance.SetMask(false));

      base.Open(onOpenStart, onOpenEnd, args);
    }

    public override void Close(Action onCloseStart = null, Action onCloseEnd = null, params object[] args) {
      onCloseEnd = onCloseEnd.AddAction(() => root.SetActive(false));
      base.Close(onCloseStart, onCloseEnd, args);
    }
  }
}