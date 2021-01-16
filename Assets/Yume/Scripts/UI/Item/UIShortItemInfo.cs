using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  public class UIShortItemInfo :UIChildBase, IScrollTerm<ItemDB> {
    private ItemDB m_item;
    private Text m_numText;
    private Image m_iconImg;
    public Image iconImg => m_iconImg;
    public bool isValid => m_item != null;

    public UIShortItemInfo(GameObject root) : base(root) {
      m_numText = GetComponent<Text>("NumText");
      m_iconImg = GetComponent<Image>("IconImage");
    }

    public async void Update(ItemDB data) {
      m_item = data;

      gameObject.SetActiveEx(m_item != null);

      if (m_item != null) {
        m_numText.text = m_item.num.ToString();
        m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(DBManager.instance.GetItemInfo(m_item.id).iconFileName);
      }
    }
  }
}