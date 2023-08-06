using System.Windows.Media.Imaging;
using Husty.OpenCvSharp;
using Husty.OpenCvSharp.DatasetFormat;
using OpenCvSharp;

namespace Annot.Attributes;

internal interface IWpfInteractiveWindow : IInteractiveWindow
{

  public List<AnnotationData> History { get; }

  public AnnotationData Annotation { get; }

  public int ImageId { get; }

  public int LabelIndex { get; }

  public Scalar[] LabelColors { get; }

  public bool DrawMode { get; }

  public void Back();

  public void SetColors(int[][] colors);

  public void SetLabelIndex(int index);

  public void SetDrawMode(bool on);

  public abstract void DeleteLast();

  public abstract void DeleteSelected();

  public abstract void Clear();

  public BitmapSource GetViewImage();

  internal BitmapSource InputMouseWheel(System.Windows.Point p, bool up);

  internal BitmapSource InputLeftMouseDown(System.Windows.Point p);

  internal BitmapSource InputLeftMouseUp(System.Windows.Point p);

  internal BitmapSource InputRightMouseDown(System.Windows.Point p);

  internal BitmapSource InputMouseLeave(System.Windows.Point p);

  internal BitmapSource InputMouseMove(System.Windows.Point p);

  internal void AcceptOtherKeyInput(string key);

}
