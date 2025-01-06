using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);//for get all category     
        T Get(Expression<Func<T, bool>> filter , string? includeProperties = null); //for get category based on Id

        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);// it will delete multiple entities with single column.
    }
}
