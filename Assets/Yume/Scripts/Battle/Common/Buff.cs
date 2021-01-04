using System;

namespace Yume.Battle {
  [Serializable]
  public struct BuffData {
    public string id;
  }

  public class BuffContainer {
    private Unit m_owner;

    public BuffContainer(Unit owner) {
      m_owner = owner;
    }

    public bool AddBuffs(params BuffData[] buffData) {
      //TODO
      return true;
    }
  }
}