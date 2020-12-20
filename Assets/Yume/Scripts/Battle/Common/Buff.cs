using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Yume.Battle {
  [Serializable]
  public struct BuffData {

  }

  public class BuffContainer {
    private Unit m_owner;

    public BuffContainer(Unit owner) {
      m_owner = owner;
    }

    public bool AddBuff(BuffData buffData) {
      //TODO
      return true;
    }
  }
}