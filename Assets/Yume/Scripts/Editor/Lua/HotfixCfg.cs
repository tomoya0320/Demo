using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XLua;

namespace Yume.Editor {
  //如果涉及到Assembly-CSharp.dll之外的其它dll，如下代码需要放到Editor目录
  public static class HotfixCfg {
    [Hotfix] // UI部分的代码容易发生改动
    public static List<Type> UI_TYPES {
      get {
        return (from type in Assembly.GetExecutingAssembly().GetTypes()
                where type.Namespace == "Yume.UI" select type).ToList();
      }
    }
  }
}