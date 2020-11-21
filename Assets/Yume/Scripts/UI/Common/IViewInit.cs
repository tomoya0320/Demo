namespace Yume.UI {
  public interface IViewInit<DataType> {
    void OnInit(DataType data);
  }
}