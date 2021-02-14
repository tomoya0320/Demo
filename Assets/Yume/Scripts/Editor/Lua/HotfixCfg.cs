using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XLua;
using Yume.Battle;
using Yume.UI;

namespace Yume.Editor {
  //如果涉及到Assembly-CSharp.dll之外的其它dll，如下代码需要放到Editor目录
  public static class HotfixCfg {
    [Hotfix] // UI的代码
    public static List<Type> ui =>
      (from type in Assembly.Load("Assembly-CSharp").GetTypes()
        where typeof(UIBase).IsAssignableFrom(type) || typeof(UIChildBase).IsAssignableFrom(type) 
        select type).ToList();
    
    [Hotfix] // 行为树节点的代码
    public static List<Type> behaviorNode =>
      (from type in Assembly.Load("Assembly-CSharp").GetTypes()
        where typeof(BehaviorNodeBase).IsAssignableFrom(type)
        select type).ToList();
  }
}