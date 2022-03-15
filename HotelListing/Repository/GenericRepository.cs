using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using X.PagedList;

namespace HotelListing.Repository {
	public class GenericRepository<T> : IGenericRepository<T> where T : class {
		private readonly DatabaseContext _context;
		private readonly DbSet<T> _db;

		public GenericRepository(DatabaseContext context) {
			_context = context;
			_db = _context.Set<T>();
		}

		public async Task DeleteAsync(int id) {
			var entity = await _db.FindAsync(id);
			if (entity != null) {
				_db.Remove(entity);
			}
		}

		public void DeleteRange(IEnumerable<T> entities) {
			_db.RemoveRange(entities);
		}

		public async Task<IList<T>> GetAllAsync(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null) {
			IQueryable<T> query = _db;

			if (expression != null) {
				query = query.Where(expression);
			}
			if (include != null) {
				query = include(query);
			}

			if (orderBy != null) {
				query = orderBy(query);
			}

			return await query.AsNoTracking().ToListAsync();
		}

		public async Task<IPagedList<T>> GetAllAsync(RequestParams requestParams, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null) {
			IQueryable<T> query = _db;

			if (include != null) {
				query = include(query);
			}

			return await query.AsNoTracking().ToPagedListAsync(requestParams.PageNumber, requestParams.PageSize);
		}

		public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null) {
			IQueryable<T> query = _db;
			if (include != null) {
				query = include(query);
			}

			return await query.AsNoTracking().FirstOrDefaultAsync(expression);
		}

		public async Task InsertAsync(T entity) {
			await _db.AddAsync(entity);
		}

		public async Task InsertRangeAsync(IEnumerable<T> entities) {
			await _db.AddRangeAsync(entities);
		}

		public void Update(T entity) {
			_db.Attach(entity);
			_context.Entry(entity).State = EntityState.Modified;
			//_db.Update(entity);
		}
	}
}
