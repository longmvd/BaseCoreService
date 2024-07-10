using BaseCoreService.Entities.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL.MongoDB
{
    public interface IMongoDBService<T> where T : IEntity
    {

        #region Get
        Task<List<T>> GetAllAsync(FilterDefinition<T>? filter, FindOptions<T> findOptions = null);

        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, FindOptions<T> findOptions = null);

        Task<T> GetOneAsync(FilterDefinition<T> filter);

        Task<T> GetByID(string id);
        #endregion

        #region Insert

        Task InsertOneAsync(T baseModel);

        Task InsertManyAsync(List<T> baseModels);

        #endregion

        #region Delete
        Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter);

        Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter);

        #endregion

    }
}
