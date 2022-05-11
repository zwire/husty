using System.Text.Json;
using Husty.Extensions;

var ary = new[,,] { { { 0, 1 }, { 2, 3 }, { 4, 5 } } };
var ary2 = ary.Transpose(2, 0, 1);