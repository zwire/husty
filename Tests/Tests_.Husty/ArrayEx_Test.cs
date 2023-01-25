using Husty.Extensions;

namespace Tests_.Husty;

public class ArrayEx_Test
{
    [Fact]
    public void Reshape()
    {
        var first = new[] { 1, 2, 3, 4, 5, 6 };
        first.To2DArray(2, 3);
        first.To2DJaggedArray(2, 3);
        first.To3DArray(1, 2, 3);
        first.To3DJaggedArray(1, 2, 3);
        var second = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
        second.To1DArray();
        second.To2DArray();
        second.Transpose();
        var second2 = new[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 } };
        second2.To1DArray();
        second2.To2DJaggedArray();
        second2.Transpose();
        var third = new[] { new[] { new[] { 1, 2, 3, 4 }, new[] { 4, 5, 6, 7 }, new[] { 7, 8, 9, 10 } } };
        third.To1DArray();
        third.To3DArray();
        third.Transpose(0, 1, 2);
        third.Transpose(0, 2, 1);
        third.Transpose(1, 0, 2);
        third.Transpose(1, 2, 0);
        third.Transpose(2, 0, 1);
        third.Transpose(2, 1, 0);
        var third2 = new[, ,] { { { 1, 2, 3, 4 }, { 4, 5, 6, 7 }, { 7, 8, 9, 10 } } };
        third2.To1DArray();
        third2.To3DJaggedArray();
        third2.Transpose(0, 1, 2);
        third2.Transpose(0, 2, 1);
        third2.Transpose(1, 0, 2);
        third2.Transpose(1, 2, 0);
        third2.Transpose(2, 0, 1);
        third2.Transpose(2, 1, 0);
    }

}