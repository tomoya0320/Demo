using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public abstract class UIGridPage<DataType, ViewType> : MonoBehaviour where ViewType : IViewInit<DataType> where DataType : class {
    [SerializeField]
    private Text _pageText;
    [SerializeField]
    private YButton _turnPageBackBtn;
    [SerializeField]
    private YButton _turnPageForwardBtn;
    [SerializeField]
    protected List<ViewType> viewList;
    protected List<DataType> data;
    protected int maxNumInOnePage => viewList.Count;
    protected int currentPage = 1;
    protected int maxPage;
    protected int additionalIndex => (currentPage - 1) * maxNumInOnePage;

    protected virtual void Start() {
      if(_turnPageBackBtn !=null && _turnPageForwardBtn != null) {
        _turnPageBackBtn.onClick.AddListener(OnTurnPageBack);
        _turnPageForwardBtn.onClick.AddListener(OnTurnPageForward);
      }
    }

    protected virtual void ReCalMaxPageAndRefresh() {
      maxPage = data.Count > 0 ? data.Count / maxNumInOnePage : 1;
      if (data.Count % maxNumInOnePage > 0) {
        maxPage++;
      }
      if (currentPage > maxPage) {
        currentPage = maxPage;
      }
      UpdatePageText();
      RefreshPage();
    }

    protected virtual void RefreshPage() {
      for (int i = 0; i < viewList.Count; i++) {
        if (i + additionalIndex < data.Count) {
          viewList[i].OnInit(data[i + additionalIndex]);
        } else {
          viewList[i].OnInit(null);
        }
      }
    }

    protected virtual void UpdatePageText() {
      if (_pageText != null) {
        _pageText.text = currentPage + "/" + maxPage;
      }
    }

    protected virtual void OnTurnPageForward() {
      if (currentPage < maxPage) {
        currentPage++;
        UpdatePageText();
        RefreshPage();
      }
    }

    protected virtual void OnTurnPageBack() {
      if (currentPage > 1) {
        currentPage--;
        UpdatePageText();
        RefreshPage();
      }
    }
  }
}