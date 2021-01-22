using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UIShortItemInfo : UIChildBase, IScrollTerm<ItemData> {
    private ItemData m_item;
    private Text m_numText;
    private Image m_iconImg;
    public Image iconImg => m_iconImg;
    public bool isValid => m_item != null;

    public UIShortItemInfo(GameObject root) : base(root) {
      m_numText = GetComponent<Text>("NumText");
      m_iconImg = GetComponent<Image>("IconImage");
    }

    public async void UpdateScrollTerm(ItemData item) {
      m_item = item;

      gameObject.SetActiveEx(m_item != null);

      if (m_item != null) {
        m_numText.text = m_item.count.ToString();
        m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(DataManager.GetItemTemplate(m_item.id).iconFileName);
      }
    }
  }
}