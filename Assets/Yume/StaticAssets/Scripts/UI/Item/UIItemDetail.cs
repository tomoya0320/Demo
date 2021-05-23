using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIItemDetail : UIGaussianBlurBg {
    private Image m_iconImg;
    private Text m_descText;
    private Text m_nameNumText;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

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
      m_iconImg.SetSprite(itemTemplate.iconFileName);
      m_nameNumText.text = $"{itemTemplate.name}*{item.count}";
      m_descText.text = StringUtil.GetActualDescription(itemTemplate.description, itemTemplate.blackboard);

      base.Open(callback, args);
    }
  }
}