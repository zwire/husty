using Husty.Extensions;

namespace Husty;

public class CsvContainer
{

  // ------ fields ------ //

  private string[][] _array;


  // ------ properties ------ //

  public int Rows => _array.Length;

  public int Columns => _array[0].Length;

  public CsvContainer this[Range rowRange, Range colRange]
  {
    get
    {
      var array = GetRows(rowRange).Select(row => row[colRange]).ToArray();
      return new(array);
    }
  }


  // ------ constructors ------ //

  private CsvContainer(string[][] array)
  {
    _array = array;
    try { _array.To2DArray(); }
    catch { throw new ArgumentException("input array must be defined as 2D array"); }
  }

  public CsvContainer(string filePath)
      : this(File.ReadAllLines(filePath).Select(line => line.Trim().Split(',')).ToArray()) { }


  // ------ public methods ------ //

  public static CsvContainer Create<T>(T[][] array) where T : IConvertible, IComparable
      => new(array.Select(ary => ary.Select(a => $"{a}").ToArray()).ToArray());

  public static CsvContainer Create<T>(T[,] array) where T : IConvertible, IComparable
      => Create(array.To2DJaggedArray());

  public static CsvContainer Create(string allText)
      => Create(allText.Split('\n').Select(line => line.Split(',')).ToArray());

  public override string ToString()
  {
    return string.Join('\n', _array.Select(row => string.Join(',', row)).ToArray());
  }

  public string[] GetRow(int number)
  {
    return _array[number];
  }

  public string[] GetColumn(int number)
  {
    return _array.Select(row => row[number]).ToArray();
  }

  public string[][] GetRows(Range? range = default)
  {
    return _array[range ?? ..];
  }

  public string[][] GetColumns(Range? range = default)
  {
    return _array.Transpose()[range ?? ..];
  }

  public void InsertRow(int index, string[] array)
  {
    if (array.Length != Columns)
      throw new ArgumentException(nameof(array));
    var list = _array.ToList();
    list.Insert(index, array);
    _array = list.ToArray();
  }

  public void InsertColumn(int index, string[] array)
  {
    if (array.Length != Rows)
      throw new ArgumentException(nameof(array));
    var list = _array.Transpose().ToList();
    list.Insert(index, array);
    _array = list.ToArray().Transpose();
  }

  public void AddRow(string[] array)
  {
    InsertRow(Rows, array);
  }

  public void AddColumn(string[] array)
  {
    InsertColumn(Columns, array);
  }

  public void RemoveRow(int index)
  {
    var list = _array.ToList();
    list.RemoveAt(index);
    _array = list.ToArray();
  }

  public void RemoveColumn(int index)
  {
    var list = _array.Transpose().ToList();
    list.RemoveAt(index);
    _array = list.ToArray().Transpose();
  }

  public CsvContainer MapRows(Func<string[], string[]> func)
  {
    var array = GetRows();
    for (int i = 0; i < array.Length; i++)
      array[i] = func(array[i]);
    return new(array);
  }

  public CsvContainer MapColumns(Func<string[], string[]> func)
  {
    var array = GetColumns();
    for (int i = 0; i < array.Length; i++)
      array[i] = func(array[i]);
    return new(array.Transpose());
  }

}
