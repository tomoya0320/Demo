using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  /// <summary>
  /// 作为ScrollView里的子项UI需要实现这个接口
  /// </summary>
  /// <typeparam name="TData"></typeparam>
  public interface IScrollTerm<TData> {
    void Update(TData data);
  }

  public class UIScrollView<TData, TView> where TData : class where TView : IScrollTerm<TData> {
    private Vector2 m_cellSize;
    private Vector2 m_originPos;
    private int m_initDataIndex;
    private Vector2 m_maxCellNum;
    private List<TData> m_dataList;
    private List<TView> m_viewList;
    private ScrollRect m_scrollRect;
    private GridLayoutGroup m_gridLayoutGroup;
    private RectTransform m_scrollRectTransform;

    public UIScrollView(ScrollRect scrollRect, GridLayoutGroup gridLayoutGroup, List<TData> dataList, List<TView> viewList) {
      m_dataList = dataList;
      m_viewList = viewList;
      m_scrollRect = scrollRect;
      m_gridLayoutGroup = gridLayoutGroup;

      m_cellSize = m_gridLayoutGroup.cellSize + m_gridLayoutGroup.spacing;
      m_scrollRectTransform = m_scrollRect.GetComponent<RectTransform>();
      m_maxCellNum = m_scrollRectTransform.rect.size / m_cellSize;

      m_scrollRect.onValueChanged.AddListener(_OnScrollPosChanged);

      OnRefresh();
    }

    public void OnRefresh(List<TData> dataList) {
      m_dataList = dataList;
      OnRefresh();
    }

    public void OnRefresh() {
      if (m_scrollRect.vertical) {
        int temp = m_dataList.Count / (int)m_maxCellNum.x;
        if (m_dataList.Count % (int)m_maxCellNum.x > 0) {
          temp++;
        }
        m_scrollRect.content.sizeDelta = new Vector2(m_scrollRect.content.sizeDelta.x, m_cellSize.y * temp);
        m_scrollRect.content.anchoredPosition = new Vector2(0, m_scrollRectTransform.rect.height * 0.5f);
      } else if (m_scrollRect.horizontal) {
        int temp = m_dataList.Count / (int)m_maxCellNum.y;
        if (m_dataList.Count % (int)m_maxCellNum.y > 0) {
          temp++;
        }
        m_scrollRect.content.sizeDelta = new Vector2(m_cellSize.x * temp, m_scrollRect.content.sizeDelta.y);
        //注意，左右的方向是和上下的相反的
        m_scrollRect.content.anchoredPosition = new Vector2(-m_scrollRectTransform.rect.width * 0.5f, 0);
      }
      m_originPos = m_scrollRect.content.anchoredPosition;
      m_initDataIndex = 0;
      UpdateViewList();
    }

    public void UpdateViewList() {
      for (int i = 0; i < m_viewList.Count; i++) {
        if (m_initDataIndex + i >= 0 && m_initDataIndex + i < m_dataList.Count) {
          m_viewList[i].Update(m_dataList[m_initDataIndex + i]);
        } else {
          m_viewList[i].Update(null);
        }
      }
    }

    private void _OnScrollPosChanged(Vector2 pos) {
      if (m_scrollRect.vertical) {
        int row = (int)((m_scrollRect.content.anchoredPosition.y - m_originPos.y) / m_cellSize.y);
        m_initDataIndex = (int)(row * m_maxCellNum.x);
        m_gridLayoutGroup.padding.top = (int)(m_cellSize.y * row);
      } else if (m_scrollRect.horizontal) {
        //注意，左右的方向是和上下的相反的
        int column = (int)((m_originPos.x - m_scrollRect.content.anchoredPosition.x) / m_cellSize.x);
        m_initDataIndex = (int)(column * m_maxCellNum.y);
        m_gridLayoutGroup.padding.left = (int)(m_cellSize.x * column);
      }
      //Grid显示bug，不会马上更新
      LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRect.content);
      //----------------------
      UpdateViewList();
    }
  }
}