namespace Yume.UI {
  public interface IViewInit<TData> {
    void OnInit(TData data);
  }
}