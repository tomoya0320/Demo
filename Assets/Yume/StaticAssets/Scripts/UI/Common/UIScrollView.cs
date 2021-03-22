using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  /// <summary>
  /// 作为ScrollView里的子项UI需要实现这个接口
  /// </summary>
  /// <typeparam name="TData">数据类型</typeparam>
  public interface IScrollTerm<TData> where TData : class {
    bool isValid { get; }
    IScrollTerm<TData> Clone();
    GameObject gameObject { get; }
    void UpdateScrollTerm(TData data);
  }

  public interface IScrollGroup<TData, TView> where TData : class where TView : IScrollTerm<TData> {
    List<TData> dataList { get; }
    List<TView> viewList { get; }
  }

  public class UIScrollView<TData, TView> where TData : class where TView : class, IScrollTerm<TData> {
    private int m_row; // 行
    private int m_column; // 列
    private Vector2 m_cellSize;
    private Vector2 m_originPos;
    private int m_initDataIndex;
    private Vector2 m_displayRange;
    private ScrollRect m_scrollRect;
    private Vector2Int m_maxCellNum;
    private GridLayoutGroup m_gridLayoutGroup;
    private RectTransform m_scrollRectTransform;
    private IScrollGroup<TData, TView> m_scrollGroup;

    public UIScrollView(ScrollRect scrollRect, IScrollGroup<TData, TView> scrollGroup) {
      m_scrollRect = scrollRect;
      m_scrollGroup = scrollGroup;
      m_scrollRectTransform = scrollRect.GetComponent<RectTransform>();
      m_gridLayoutGroup = scrollRect.content.GetComponent<GridLayoutGroup>();

      m_cellSize = m_gridLayoutGroup.cellSize + m_gridLayoutGroup.spacing;
      m_displayRange = m_scrollRectTransform.rect.size;
      m_maxCellNum = new Vector2Int {
        x = (int)(m_displayRange.x / m_cellSize.x),
        y = (int)(m_displayRange.y / m_cellSize.y)
      };

      // 适配
      m_scrollRectTransform.SetSizeX(m_maxCellNum.x * m_cellSize.x);
      m_scrollRectTransform.SetSizeY(m_maxCellNum.y * m_cellSize.y);
      m_displayRange = m_scrollRectTransform.rect.size;

      m_originPos = m_scrollRect.content.anchoredPosition;

      // ScrollTerm原型复制
      if (m_scrollGroup.viewList.Count > 1) {
        for (int i = m_scrollGroup.viewList.Count - 1; i > 0; i--) {
          Object.DestroyImmediate(m_scrollGroup.viewList[i].gameObject);
          m_scrollGroup.viewList.RemoveAt(i);
        }
        Debug.LogWarning("冗余的ScrollView原型");
      }
      var prototype = m_scrollGroup.viewList.First();
      if (prototype == null) {
        throw new System.Exception("ScrollView原型为空！");
      }

      int extra = 0;
      if (m_scrollRect.vertical) {
        extra = m_maxCellNum.x;
      } else if (m_scrollRect.horizontal) {
        extra = m_maxCellNum.y;
      }
      for (int i = 1; i < m_maxCellNum.x * m_maxCellNum.y + extra; i++) {
        m_scrollGroup.viewList.Add(prototype.Clone() as TView);
      }
    }

    /// <summary>
    /// 更新视图
    /// </summary>
    /// <param name="recalLen">是否重新计算内容的长度</param>
    public void UpdateView(bool recalLen = true) {
      if (recalLen) {
        if (m_scrollRect.vertical) {
          m_row = (m_scrollGroup.dataList.Count - 1) / m_maxCellNum.x + 1;
          m_scrollRect.content.SetSizeY(Mathf.Max(m_cellSize.y * m_row, m_displayRange.y));
        } else if (m_scrollRect.horizontal) {
          m_column = (m_scrollGroup.dataList.Count - 1) / m_maxCellNum.y + 1;
          m_scrollRect.content.SetSizeX(Mathf.Max(m_cellSize.x * m_column, m_displayRange.x));
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
        _ForceRefreshScrollBar();
      }

      _UpdateViewData();
    }

    private void _UpdateViewData() {
      for (int i = 0; i < m_scrollGroup.viewList.Count; i++) {
        if (m_initDataIndex + i >= 0 && m_initDataIndex + i < m_scrollGroup.dataList.Count) {
          m_scrollGroup.viewList[i].UpdateScrollTerm(m_scrollGroup.dataList[m_initDataIndex + i]);
        } else {
          m_scrollGroup.viewList[i].UpdateScrollTerm(null);
        }
      }
    }

    /// <summary>
    /// 强制刷新ScrollBar
    /// </summary>
    private void _ForceRefreshScrollBar() {
      if (m_scrollRect.verticalScrollbar != null) {
        m_scrollRect.verticalScrollbar.value = m_scrollRect.normalizedPosition.y;
      }
      if (m_scrollRect.horizontalScrollbar != null) {
        m_scrollRect.horizontalScrollbar.value = m_scrollRect.normalizedPosition.x;
      }
    }

    public void OnUpdate() {
      _UpdateInitDataIndex();
    }

    private void _UpdateInitDataIndex() {
      if (m_scrollRect.vertical) {
        int row = Mathf.Max(0, (int)((m_scrollRect.content.anchoredPosition.y - m_originPos.y) / m_cellSize.y));
        int newInitDataIndex = row * m_maxCellNum.x;
        if (m_initDataIndex != newInitDataIndex) {
          m_initDataIndex = newInitDataIndex;
          m_gridLayoutGroup.padding.top = (int)(m_cellSize.y * row);
          LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
          _UpdateViewData();
        }
      } else if (m_scrollRect.horizontal) {
        //注意，左右的方向是和上下的相反的
        int column = Mathf.Max(0, (int)((m_originPos.x - m_scrollRect.content.anchoredPosition.x) / m_cellSize.x));
        int newInitDataIndex = column * m_maxCellNum.y;
        if (m_initDataIndex != newInitDataIndex) {
          m_initDataIndex = newInitDataIndex;
          m_gridLayoutGroup.padding.left = (int)(m_cellSize.x * column);
          LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRectTransform);
          _UpdateViewData();
        }
      }
    }
    
    public void SetAsFirstDisplay(int index) {
      // 这里的行列参考索引的方式下标是从0开始的需要注意
      if (m_scrollRect.vertical) {
        if (m_scrollGroup.dataList.Count > m_maxCellNum.x * m_maxCellNum.y) {
          int row = index / m_maxCellNum.x;
          float posY = 1.0f - row * m_cellSize.y / (m_scrollRect.content.rect.height - m_scrollRectTransform.rect.height);
          m_scrollRect.verticalNormalizedPosition = Mathf.Clamp(posY, 0, 1.0f);
          _UpdateInitDataIndex();
          _UpdateViewData();
        }
      } else if (m_scrollRect.horizontal) {
        if (m_scrollGroup.dataList.Count > m_maxCellNum.x * m_maxCellNum.y) {
          int column = index / m_maxCellNum.y;
          float posX = 1.0f - column * m_cellSize.x / (m_scrollRect.content.rect.width - m_scrollRectTransform.rect.width);
          m_scrollRect.horizontalNormalizedPosition =  Mathf.Clamp(posX, 0, 1.0f);
          _UpdateInitDataIndex();
          _UpdateViewData();
        }
      }
    }

    public void ResetNormalizedPosition() {
      // 注意(1, 1)才是最开始的位置
      m_scrollRect.normalizedPosition = Vector2.one;
      UpdateView();
    }
  }
}