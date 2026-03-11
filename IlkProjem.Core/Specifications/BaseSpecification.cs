using System.Linq.Expressions;

namespace IlkProjem.Core.Specifications;

// The BASE CLASS: provides default implementation so you don't repeat yourself
// Every specific spec (like CustomerCursorSpecification) inherits from this
public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; }
    public Expression<Func<T, object>> OrderBy { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    // Constructor: every spec MUST provide a filter (criteria)
    protected BaseSpecification(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    // Helper methods for child classes to use
    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}