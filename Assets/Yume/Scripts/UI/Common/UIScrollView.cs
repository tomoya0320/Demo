using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  /// <summary>
  /// 作为ScrollView里的子项UI需要实现这个接口
  /// </summary>
  /// <typeparam name="TData"></typeparam>
  public interface IScrollTerm<TData> {
    bool isValid { get; }
    void Update(TData data);
  }

  public class UIScrollView<TData, TView> where TData : class where TView : IScrollTerm<TData> {
    private Vector2 m_cellSize;
    private Vector2 m_originPos;
    private int m_initDataIndex;
    private Vector2 m_maxCellNum;
    private Rect m_scrollRectRect;
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
      m_scrollRectRect = m_scrollRectTransform.rect;
      m_maxCellNum = m_scrollRectRect.size / m_cellSize;
      m_originPos = m_scrollRect.content.anchoredPosition;

      _AddListener();
      OnRefresh();
    }

    private void _AddListener() {
      m_scrollRect.onValueChanged.AddListener(_OnScrollPosChanged);
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
        m_scrollRect.content.SetSizeDeltaY(Mathf.Max(m_cellSize.y * temp, m_scrollRectRect.height));
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
        int row = (int)((m_scrollRect.content.anchoredPosition.y - m_originPos.y) / m_cellSize.y);
        m_initDataIndex = (int)(row * m_maxCellNum.x);
      } else if (m_scrollRect.horizontal) {
        int temp = m_dataList.Count / (int)m_maxCellNum.y;
        if (m_dataList.Count % (int)m_maxCellNum.y > 0) {
          temp++;
        }
        m_scrollRect.content.SetSizeDeltaX(Mathf.Max(m_cellSize.x * temp, m_scrollRectRect.width));
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
        //注意，左右的方向是和上下的相反的
        int column = (int)((m_originPos.x - m_scrollRect.content.anchoredPosition.x) / m_cellSize.x);
        m_initDataIndex = (int)(column * m_maxCellNum.y);
      }
      UpdateViewData();
    }

    public void UpdateViewData() {
      for (int i = 0; i < m_viewList.Count; i++) {
        if (m_initDataIndex + i >= 0 && m_initDataIndex + i < m_dataList.Count) {
          m_viewList[i].Update(m_dataList[m_initDataIndex + i]);
        } else {
          m_viewList[i].Update(null);
        }
      }
    }

    private void _OnScrollPosChanged(Vector2 pos) {
      //不这么判断的话当显示的内容没超过显示区域时pos的坐标会一直保持在0和1
      if (m_dataList.Count > m_maxCellNum.x * m_maxCellNum.y) {
        if (m_scrollRect.vertical && pos.y >= 0 && pos.y <= 1) {
          int row = (int)((m_scrollRect.content.anchoredPosition.y - m_originPos.y) / m_cellSize.y);
          m_initDataIndex = (int)(row * m_maxCellNum.x);
          int newTop = (int)(m_cellSize.y * row);
          if (m_gridLayoutGroup.padding.top != newTop) {
            m_gridLayoutGroup.padding.top = newTop;
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
            UpdateViewData();
          }
        } else if (m_scrollRect.horizontal && pos.x >= 0 && pos.x <= 1) {
          //注意，左右的方向是和上下的相反的
          int column = (int)((m_originPos.x - m_scrollRect.content.anchoredPosition.x) / m_cellSize.x);
          m_initDataIndex = (int)(column * m_maxCellNum.y);
          int newLeft = (int)(m_cellSize.x * column);
          if (m_gridLayoutGroup.padding.left != newLeft) {
            m_gridLayoutGroup.padding.left = newLeft;
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
            UpdateViewData();
          }
        }
      }
    }
  }
}