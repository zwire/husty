using Husty.OpenCvSharp.DatasetFormat;
using Annot.Attributes;

namespace Annot.Utils;

internal class AttributeWindowFactory
{

    private readonly string _attributeType;
    private readonly int _labelCount;
    private readonly int _standardLineWidth;
    private readonly int _boldLineWidth;
    private readonly int _tolerance;
    private readonly Func<double> _getRatio;
    private readonly double _wheelSpeed;


    public AttributeWindowFactory(
        string attributeType,
        int labelCount,
        int standardLineWidth,
        int boldLineWidth,
        int tolerance,
        Func<double> getRatio,
        double wheelSpeed
    )
    {
        _attributeType = attributeType;
        _labelCount = labelCount;
        _standardLineWidth = standardLineWidth;
        _boldLineWidth = boldLineWidth;
        _tolerance = tolerance;
        _getRatio = getRatio;
        _wheelSpeed = wheelSpeed;
    }


    public IWpfInteractiveWindow GetInstance(
        IEnumerable<AnnotationData> ann,
        string imagePath,
        int labelIndex
    )
    {
        return _attributeType switch
        {
            "box"       => new BoxAttributeWindow(ann, imagePath, labelIndex, _labelCount, _standardLineWidth, _boldLineWidth, _tolerance, _getRatio, _wheelSpeed),
            "polygon"   => new PolygonAttributeWindow(ann, imagePath, labelIndex, _labelCount, _standardLineWidth, _boldLineWidth, _tolerance, _getRatio, _wheelSpeed),
            _           => throw new NotImplementedException()
        };
    }

}
