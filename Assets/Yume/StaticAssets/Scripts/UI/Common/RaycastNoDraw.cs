using UnityEngine.UI;

namespace Yume.UI {
  /// <summary>
  /// 只需要接收射线而不需要渲染图片的时候用到
  /// </summary>
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