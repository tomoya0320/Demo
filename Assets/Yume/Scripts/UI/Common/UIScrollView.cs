using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Math;

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
    private Rect m_scrollRectRect;
    private List<TData> m_dataList;
    private List<TView> m_viewList;
    private ScrollRect m_scrollRect;
    private Vector2Int m_maxCellNum;
    private GridLayoutGroup m_gridLayoutGroup;
    private RectTransform m_scrollRectTransform;

    public UIScrollView(ScrollRect scrollRect, List<TData> dataList, List<TView> viewList) {
      m_dataList = dataList;
      m_viewList = viewList;
      m_scrollRect = scrollRect;
      m_scrollRectTransform = scrollRect.GetComponent<RectTransform>();
      m_gridLayoutGroup = scrollRect.content.GetComponent<GridLayoutGroup>();

      m_cellSize = m_gridLayoutGroup.cellSize + m_gridLayoutGroup.spacing;
      m_scrollRectRect = m_scrollRectTransform.rect;
      m_maxCellNum = new Vector2Int {
        x = (int)(m_scrollRectRect.size.x / m_cellSize.x + MathUtil.SCROLL_EPSILON),
        y = (int)(m_scrollRectRect.size.y / m_cellSize.y + MathUtil.SCROLL_EPSILON)
      };
      m_originPos = m_scrollRect.content.anchoredPosition;

      _AddListener();
      OnRefresh();
    }

    private void _AddListener() {
      m_scrollRect.onValueChanged.AddListener(_OnScrollPosChanged);
    }

    public void OnRefresh(TData[] data) {
      m_dataList.Clear();
      m_dataList.AddRange(data);
      OnRefresh();
    }

    public void OnRefresh() {
      if (m_scrollRect.vertical) {
        int temp = m_dataList.Count / m_maxCellNum.x;
        if (m_dataList.Count % m_maxCellNum.x > 0) {
          temp++;
        }
        m_gridLayoutGroup.padding.top = 0;
        m_scrollRect.content.SetSizeDeltaY(Mathf.Max(m_cellSize.y * temp, m_scrollRectRect.height));
      } else if (m_scrollRect.horizontal) {
        int temp = m_dataList.Count / m_maxCellNum.y;
        if (m_dataList.Count % m_maxCellNum.y > 0) {
          temp++;
        }
        m_gridLayoutGroup.padding.left = 0;
        m_scrollRect.content.SetSizeDeltaX(Mathf.Max(m_cellSize.x * temp, m_scrollRectRect.width));
      }

      m_initDataIndex = 0;
      m_scrollRect.StopMovement();
      m_scrollRect.content.anchoredPosition = m_originPos;
      LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);

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
        if (m_scrollRect.vertical && pos.y + MathUtil.SCROLL_EPSILON >= 0 && pos.y <= 1 + MathUtil.SCROLL_EPSILON) {
          int row = (int)((m_scrollRect.content.anchoredPosition.y - m_originPos.y) / m_cellSize.y + MathUtil.SCROLL_EPSILON);
          int newInitDataIndex = row * m_maxCellNum.x;
          if (m_initDataIndex != newInitDataIndex) {
            m_initDataIndex = newInitDataIndex;
            m_gridLayoutGroup.padding.top = (int)(m_cellSize.y * row);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
            
            UpdateViewData();
          }
        } else if (m_scrollRect.horizontal && pos.x + MathUtil.SCROLL_EPSILON >= 0 && pos.x <= 1 + MathUtil.SCROLL_EPSILON) {
          //注意，左右的方向是和上下的相反的
          int column = (int)((m_originPos.x - m_scrollRect.content.anchoredPosition.x) / m_cellSize.x + MathUtil.SCROLL_EPSILON);
          int newInitDataIndex = column * m_maxCellNum.y;
          if (m_initDataIndex != newInitDataIndex) {
            m_initDataIndex = newInitDataIndex;
            m_gridLayoutGroup.padding.left = (int)(m_cellSize.x * column);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
            
            UpdateViewData();
          }
        }
      }
    }
  }
}