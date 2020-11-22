using UnityEngine.UI;

namespace Yume.UI {
  public class RaycastNoDraw : Graphic {
    public override void SetMaterialDirty() {

    }

    public override void SetVerticesDirty() {

    }

    protected override void OnPopulateMesh(VertexHelper vh) {
      vh.Clear();
    }
  }
}