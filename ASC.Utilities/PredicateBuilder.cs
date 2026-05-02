using System.Linq.Expressions;

namespace ASC.Utilities
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = expr1.Parameters[0];
            var right = new ParameterReplaceVisitor(expr2.Parameters[0], parameter).Visit(expr2.Body);

            // Ghép hai điều kiện LINQ bằng AND để EF Core dịch được xuống SQL.
            var combined = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(expr1.Body, right!),
                parameter);

            return combined;
        }

        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = expr1.Parameters[0];
            var right = new ParameterReplaceVisitor(expr2.Parameters[0], parameter).Visit(expr2.Body);

            // Ghép hai điều kiện LINQ bằng OR, dùng cho các nhóm trạng thái.
            var combined = Expression.Lambda<Func<T, bool>>(
                Expression.Or(expr1.Body, right!),
                parameter);

            return combined;
        }

        private sealed class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _source;
            private readonly ParameterExpression _target;

            public ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
            {
                _source = source;
                _target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _source ? _target : base.VisitParameter(node);
            }
        }
    }
}
