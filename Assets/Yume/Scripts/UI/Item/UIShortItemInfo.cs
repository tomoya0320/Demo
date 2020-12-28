using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  public class UIShortItemInfo : IScrollTerm<ItemDB> {
    private Image m_iconImg;
    public Image iconImg => m_iconImg;
    private Text m_numText;
    private GameObject m_root;
    public GameObject gameObject => m_root;

    public UIShortItemInfo(GameObject root) {
      m_root = root;
      var uiObject = root.GetComponent<UIObject>();
      m_numText = uiObject.GetComponent<Text>("NumText");
      m_iconImg = uiObject.GetComponent<Image>("IconImage");
    }

    public async void Update(ItemDB data) {
      if (data == null) {
        gameObject.SetActiveEx(false);
      } else {
        gameObject.SetActiveEx(true);
        m_numText.text = data.num.ToString();
        m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(DBManager.instance.GetItemInfo(data.id).iconFileName);
      }
    }
  }
}