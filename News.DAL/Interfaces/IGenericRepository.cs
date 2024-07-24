using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace News.DAL.Interfaces
{
    public interface IGenericRepository<T> : IDisposable where T : class
    {
        Task<T> CreateAsync(T item, CancellationToken token);
        Task<bool> UpdateAsync(T item, CancellationToken token);
        Task<bool> DeleteAsync(int id, CancellationToken token);
        Task<T> GetElementByIdAsync(int id, CancellationToken token);
        Task<IEnumerable<T>> GetAllCollectionAsync(CancellationToken token);
    }
}
