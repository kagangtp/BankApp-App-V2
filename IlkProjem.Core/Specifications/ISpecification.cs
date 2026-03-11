using System.Linq.Expressions;

namespace IlkProjem.Core.Specifications;

// The CONTRACT: "Any specification must provide these 5 things"
// Think of it as a checklist that every query rule must fill out
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }       // WHERE clause
    Expression<Func<T, object>> OrderBy { get; }      // ORDER BY clause
    int Take { get; }                                  // LIMIT (how many rows)
    int Skip { get; }                                  // OFFSET (skip this many)
    bool IsPagingEnabled { get; }                      // Should we paginate at all?
}