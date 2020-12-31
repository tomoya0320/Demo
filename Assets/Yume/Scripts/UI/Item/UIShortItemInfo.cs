using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  public class UIShortItemInfo : IScrollTerm<ItemDB> {
    private ItemDB m_item;
    private Text m_numText;
    private Image m_iconImg;
    private GameObject m_root;
    public Image iconImg => m_iconImg;
    public bool isValid => m_item != null;
    public GameObject gameObject => m_root;

    public UIShortItemInfo(GameObject root) {
      m_root = root;
      var uiObject = root.GetComponent<UIObject>();
      m_numText = uiObject.GetComponent<Text>("NumText");
      m_iconImg = uiObject.GetComponent<Image>("IconImage");
    }

    public async void Update(ItemDB data) {
      m_item = data;
      if (m_item == null) {
        gameObject.SetActiveEx(false);
      } else {
        gameObject.SetActiveEx(true);
        m_numText.text = data.num.ToString();
        m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(DBManager.instance.GetItemInfo(data.id).iconFileName);
      }
    }

    public void AddRedPointNumChangedListener() {

    }

    public void RemoveRedPointNumChangedListener() {

    }
  }
}