using HotelListing.Core.Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using X.PagedList;

namespace HotelListing.Core.IRepository {
	public interface IGenericRepository<T> where T : class {
		Task<IList<T>> GetAllAsync(
			Expression<Func<T, bool>>? expression = null,
			Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
			Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null
		);
		Task<IPagedList<T>> GetAllAsync(
			RequestParams requestParams,
			Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null
		);

		Task<T?> GetSingleAsync(
			Expression<Func<T, bool>> expression,
			Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null
		);

		Task InsertAsync(T entity);
		Task InsertRangeAsync(IEnumerable<T> entities);
		Task DeleteAsync(int id);
		void DeleteRange(IEnumerable<T> entities);
		void Update(T entity);
	}
}
