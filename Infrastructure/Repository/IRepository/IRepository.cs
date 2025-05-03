using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repository.IRepository
{
   
        public interface IRepository<T> where T : class
        {
            Task<IEnumerable<T>> GetAsync(Expression<Func<T, object>>[]? includeProps = null, Expression<Func<T, bool>>? expression = null, bool tracked = true);
            Task<T?> GetOneAsync(Expression<Func<T, object>>[]? includeProps = null, Expression<Func<T, bool>>? expression = null, bool tracked = true);
            Task CreateAsync(T entity);
            void Edit(T entity);
            void Delete(T entity);
            void DeleteRange(IEnumerable<T> entities);

            Task CommitAsync();
        }
    
}
