using Husty.Extensions;

namespace Tests_.Husty;

public class ArrayEx_Test
{
    [Fact]
    public void Test_Reshape1()
    {
        var src = new[] { 1, 2, 3, 4, 5, 6 };
        var t1 = src.To2DArray(2, 3);
        var t2 = src.To2DJaggedArray(2, 3);
        var t3 = src.To3DArray(1, 2, 3);
        var t4 = src.To3DJaggedArray(1, 2, 3);
        Assert.Equal(2, t1.GetLength(0));
        Assert.Equal(3, t1.GetLength(1));
        Assert.Equal(2, t2.Length);
        Assert.Equal(3, t2.First().Length);
        Assert.Equal(1, t3.GetLength(0));
        Assert.Equal(2, t3.GetLength(1));
        Assert.Equal(3, t3.GetLength(2));
        Assert.Equal(1, t4.Length);
        Assert.Equal(2, t4.First().Length);
        Assert.Equal(3, t4.First().First().Length);
    }

    [Fact]
    public void Test_Reshape2()
    {
        var src = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
        var t1 = src.To1DArray();
        var t2 = src.To2DArray();
        var t3 = src.Transpose();
        Assert.Equal(6, t1.Length);
        Assert.Equal(2, t2.GetLength(0));
        Assert.Equal(3, t2.GetLength(1));
        Assert.Equal(3, t3.Length);
        Assert.Equal(2, t3.First().Length);
    }

    [Fact]
    public void Test_Reshape3()
    {
        var src = new[] { new[] { new[] { 1, 2, 3, 4 }, new[] { 4, 5, 6, 7 }, new[] { 7, 8, 9, 10 } } };
        var t1 = src.To1DArray();
        var t2 = src.To3DArray();
        var t3 = src.Transpose(0, 1, 2);
        var t4 = src.Transpose(0, 2, 1);
        var t5 = src.Transpose(1, 0, 2);
        var t6 = src.Transpose(1, 2, 0);
        var t7 = src.Transpose(2, 0, 1);
        var t8 = src.Transpose(2, 1, 0);
        Assert.Equal(12, t1.Length);
        Assert.Equal(1, t2.GetLength(0));
        Assert.Equal(3, t2.GetLength(1));
        Assert.Equal(4, t2.GetLength(2));
        Assert.Equal(1, t3.Length);
        Assert.Equal(3, t3.First().Length);
        Assert.Equal(4, t3.First().First().Length);
        Assert.Equal(1, t4.Length);
        Assert.Equal(4, t4.First().Length);
        Assert.Equal(3, t4.First().First().Length);
        Assert.Equal(3, t5.Length);
        Assert.Equal(1, t5.First().Length);
        Assert.Equal(4, t5.First().First().Length);
        Assert.Equal(3, t6.Length);
        Assert.Equal(4, t6.First().Length);
        Assert.Equal(1, t6.First().First().Length);
        Assert.Equal(4, t7.Length);
        Assert.Equal(1, t7.First().Length);
        Assert.Equal(3, t7.First().First().Length);
        Assert.Equal(4, t8.Length);
        Assert.Equal(3, t8.First().Length);
        Assert.Equal(1, t8.First().First().Length);
    }

    [Fact]
    public void Test_Reshape4()
    {
        var src = new[, ,] { { { 1, 2, 3, 4 }, { 4, 5, 6, 7 }, { 7, 8, 9, 10 } } };
        var t1 = src.To1DArray();
        var t2 = src.To3DJaggedArray();
        var t3 = src.Transpose(0, 1, 2);
        var t4 = src.Transpose(0, 2, 1);
        var t5 = src.Transpose(1, 0, 2);
        var t6 = src.Transpose(1, 2, 0);
        var t7 = src.Transpose(2, 0, 1);
        var t8 = src.Transpose(2, 1, 0);
        Assert.Equal(12, t1.Length);
        Assert.Equal(1, t2.Length);
        Assert.Equal(3, t2.First().Length);
        Assert.Equal(4, t2.First().First().Length);
        Assert.Equal(1, t3.GetLength(0));
        Assert.Equal(3, t3.GetLength(1));
        Assert.Equal(4, t3.GetLength(2));
        Assert.Equal(1, t4.GetLength(0));
        Assert.Equal(4, t4.GetLength(1));
        Assert.Equal(3, t4.GetLength(2));
        Assert.Equal(3, t5.GetLength(0));
        Assert.Equal(1, t5.GetLength(1));
        Assert.Equal(4, t5.GetLength(2));
        Assert.Equal(3, t6.GetLength(0));
        Assert.Equal(4, t6.GetLength(1));
        Assert.Equal(1, t6.GetLength(2));
        Assert.Equal(4, t7.GetLength(0));
        Assert.Equal(1, t7.GetLength(1));
        Assert.Equal(3, t7.GetLength(2));
        Assert.Equal(4, t8.GetLength(0));
        Assert.Equal(3, t8.GetLength(1));
        Assert.Equal(1, t8.GetLength(2));
    }

}