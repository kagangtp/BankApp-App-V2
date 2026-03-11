namespace IlkProjem.Core.Specifications;

// The ENGINE: takes an IQueryable + a Specification, and builds the final query
// This is where IQueryable gets its WHERE, ORDER BY, and LIMIT applied
public class SpecificationEvaluator<TEntity> where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
    {
        var query = inputQuery;

        // Apply WHERE (e.g. "Name contains 'kagan' AND Id > 50")
        if (spec.Criteria != null) query = query.Where(spec.Criteria);

        // Apply ORDER BY (e.g. "order by Id ascending")
        if (spec.OrderBy != null) query = query.OrderBy(spec.OrderBy);

        // Apply LIMIT (e.g. "take only 10 rows")
        if (spec.IsPagingEnabled) query = query.Skip(spec.Skip).Take(spec.Take);

        // Return the built query — NO SQL has executed yet!
        return query;
    }
}