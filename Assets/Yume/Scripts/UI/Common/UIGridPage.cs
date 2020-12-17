using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public interface IViewInit<TData> {
    void OnInit(TData data);
  }

  public abstract class UIGridPage<TData, TView> : MonoBehaviour where TView : IViewInit<TData> where TData : class {
    [SerializeField]
    private Text _pageText;
    [SerializeField]
    private YButton _turnPageBackBtn;
    [SerializeField]
    private YButton _turnPageForwardBtn;
    [SerializeField]
    protected List<TView> viewList;
    protected List<TData> dataList;
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
      maxPage = dataList.Count > 0 ? dataList.Count / maxNumInOnePage : 1;
      if (dataList.Count % maxNumInOnePage > 0) {
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
        if (i + additionalIndex < dataList.Count) {
          viewList[i].OnInit(dataList[i + additionalIndex]);
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