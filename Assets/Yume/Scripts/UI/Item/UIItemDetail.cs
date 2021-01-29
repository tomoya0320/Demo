using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UIItemDetail : UIBase {
    private Image m_iconImg;
    private Text m_descText;
    private Text m_nameNumText;

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      m_iconImg = GetComponent<Image>("Icon");
      m_descText = GetComponent<Text>("Description");
      m_nameNumText = GetComponent<Text>("NameAndNum");

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      ItemData item = args[0] as ItemData;
      ItemTemplate itemTemplate = ResourceManager.Load<ItemTemplate>(item.id);
      m_iconImg.sprite = ResourceManager.Load<Sprite>(itemTemplate.iconFileName);
      m_nameNumText.text = $"{itemTemplate.name}*{item.count}";
      m_descText.text = itemTemplate.description;

      base.Open(callback, args);
    }
  }
}