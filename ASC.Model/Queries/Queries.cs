using ASC.Model.Models;
using System.Linq.Expressions;

namespace ASC.Model.Queries
{
    public static class Queries
    {
        public static Expression<Func<ServiceRequest, bool>> GetDashboardQuery(
            DateTime? requestedDate,
            List<string>? status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            Expression<Func<ServiceRequest, bool>> query = serviceRequest => !serviceRequest.IsDeleted;

            if (requestedDate.HasValue)
            {
                query = And(query, serviceRequest => serviceRequest.RequestedDate >= requestedDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = And(query, serviceRequest => serviceRequest.PartitionKey == email);
            }

            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                query = And(query, serviceRequest => serviceRequest.ServiceEngineer == serviceEngineerEmail);
            }

            if (status != null && status.Count > 0)
            {
                Expression<Func<ServiceRequest, bool>> statusQuery = serviceRequest => false;

                foreach (var state in status)
                {
                    var currentState = state;
                    statusQuery = Or(statusQuery, serviceRequest => serviceRequest.Status == currentState);
                }

                query = And(query, statusQuery);
            }

            return query;
        }

        private static Expression<Func<T, bool>> And<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = expr1.Parameters[0];
            var right = new ParameterReplaceVisitor(expr2.Parameters[0], parameter).Visit(expr2.Body);

            // Dùng chung parameter để EF Core dịch được biểu thức lọc động.
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(expr1.Body, right!),
                parameter);
        }

        private static Expression<Func<T, bool>> Or<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = expr1.Parameters[0];
            var right = new ParameterReplaceVisitor(expr2.Parameters[0], parameter).Visit(expr2.Body);

            // Gom các trạng thái bằng OR trước khi ghép với điều kiện chính.
            return Expression.Lambda<Func<T, bool>>(
                Expression.Or(expr1.Body, right!),
                parameter);
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
