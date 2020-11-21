using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public abstract class UIGridPage<DataType, ShowType> : MonoBehaviour where ShowType : IViewInit<DataType> where DataType : class {
    [SerializeField]
    private Text _pageText;
    [SerializeField]
    protected List<ShowType> showList;
    protected List<DataType> data;
    protected int maxNumInOnePage => showList.Count;
    protected int currentPage = 1;
    protected int maxPage;
    protected int additionalIndex => (currentPage - 1) * maxNumInOnePage;

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
      for (int i = 0; i < showList.Count; i++) {
        if (i + additionalIndex < data.Count) {
          showList[i].OnInit(data[i + additionalIndex]);
        } else {
          showList[i].OnInit(null);
        }
      }
    }

    protected virtual void UpdatePageText() {
      _pageText.text = currentPage + "/" + maxPage;
    }

    public virtual void TurnPageForward() {
      if (currentPage < maxPage) {
        currentPage++;
        UpdatePageText();
        RefreshPage();
      }
    }

    public virtual void TurnPageBack() {
      if (currentPage > 1) {
        currentPage--;
        UpdatePageText();
        RefreshPage();
      }
    }
  }
}