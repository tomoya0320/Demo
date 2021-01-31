using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Yume.Asset {
  public abstract class YScriptableObject : SerializedScriptableObject {
    public abstract Dictionary<(string, Type), object> GetActualData();
  }
}