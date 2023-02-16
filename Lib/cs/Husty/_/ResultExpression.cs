namespace Husty;

public record struct ResultExpression<T>(bool HasValue, T Value);