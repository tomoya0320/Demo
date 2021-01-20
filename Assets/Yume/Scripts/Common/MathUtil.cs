using System;
using UnityEngine;
using Yume.Battle;
using Yume.DB;
using Yume.String;

namespace Yume.Math {
  public class MathUtil {
    /// <summary>
    /// 在XOZ平面上旋转一个Vector3
    /// </summary>
    public static Vector3 RotateVector3(Vector3 v3, float angle) {
      angle = -angle;
      float x = v3.x * Mathf.Cos(Mathf.Deg2Rad * angle) - v3.z * Mathf.Sin(Mathf.Deg2Rad * angle);
      float z = v3.x * Mathf.Sin(Mathf.Deg2Rad * angle) + v3.z * Mathf.Cos(Mathf.Deg2Rad * angle);
      return new Vector3(x, 0, z);
    }
    /// <summary>
    /// 计算两个向量之间的夹角，带符号,n为旋转轴
    /// </summary>
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) {
      return Mathf.Atan2(
          Vector3.Dot(n, Vector3.Cross(v1, v2)),
          Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }
    /// <summary>
    /// 计算给定的时期（不超过现在的时间）到现在过去了多久
    /// </summary>
    public static string CalDateDeltaFromNow(DateTime dateTime) {
      TimeSpan timeSpan = DateTime.Now - dateTime;
      if (timeSpan.Days == 0) {
        return StringMap.TODAY;
      } else if (timeSpan.Days < 7) {
        return string.Format(StringMap.SOME_DAY_BEFORE, timeSpan.Days);
      } else if (timeSpan.Days < 30) {
        return string.Format(StringMap.SOME_WEEK_BEFORE, timeSpan.Days / 7);
      } else if (timeSpan.Days < 365) {
        return string.Format(StringMap.SOME_MONTH_BEFORE, timeSpan.Days / 30);
      } else {
        return string.Format(StringMap.SOME_YEAR_BEFORE, timeSpan.Days / 365);
      }
    }
    /// <summary>
    /// 计算当前等级还需要多少经验才能升级
    /// </summary>
    public static int CalNeededExp(int lv) {
      return 50 + lv * 50;
    }

    public static Attrib[] CalculateAttribs(string id, int level, int maxLevel, Attrib[] minAttribs, Attrib[] maxAttribs) {

      if (minAttribs.Length != maxAttribs.Length) {
        throw new Exception("最小属性数组和最大属性数组长度不一致！");
      }

      float p = (float)level / maxLevel;
      Attrib[] attribs = new Attrib[minAttribs.Length];
      var curve = DBManager.instance.GetAttribCurve(id);
      for (int i = 0; i < attribs.Length; i++) {
        attribs[i] = minAttribs[i] + (maxAttribs[i] - minAttribs[i]) * curve.Evaluate(p);
      }

      return attribs;
    }
  }
}