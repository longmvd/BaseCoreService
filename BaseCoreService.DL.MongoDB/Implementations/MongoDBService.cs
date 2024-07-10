using BaseCoreService.BL.MongoDB;
using BaseCoreService.BL.MongoDB.Entities;
using BaseCoreService.Entities.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL.MongoDB
{
    public class MongoDBService<T> : IMongoDBService<T> where T : MongoDBEntity
    {
        private readonly IConfiguration _configuration;

        private string _collectionName;

        private readonly IMongoCollection<T> _collection;

        private readonly IMongoDatabase _database;

        private readonly MongoClient _client;

        public MongoDBService(IConfiguration configuration)
        {
            _configuration = configuration;
            _collectionName = typeof(T).Name.ToLower();
            _client = new MongoClient(DatabaseContext.ConnectionString);
            _database = _client.GetDatabase(_configuration["MongoDB:Database"]);
            _collection = _database.GetCollection<T>(_collectionName);
        }


        protected string CollectionName
        {
            get { return _collectionName; }
            set
            {
                _collectionName = value;
            }
        }


        public virtual async Task<List<T>> GetAllAsync(FilterDefinition<T> filter, FindOptions<T> findOptions = null)
        {
            var res = await _collection.FindAsync<T>(filter, findOptions);
            _collection.FindAsync(_ => true);
            return res.ToList();
        }

        public virtual async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, FindOptions<T> findOptions = null)
        {
            var res = await _collection.FindAsync<T>(filter, findOptions);
            return res.ToList();
        }

        public virtual async Task<T> GetOneAsync(FilterDefinition<T> filter)
        {
            var res = await this.GetAllAsync(filter);
            return res.ToList().FirstOrDefault();
        }

        public virtual async Task<T> GetByID(string id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.ID.ToString(), id);
            return await GetOneAsync(filter);
        }

        public virtual async Task InsertManyAsync(List<T> baseModels)
        {
            await _collection.InsertManyAsync(baseModels);
        }

        public virtual async Task InsertOneAsync(T baseModel)
        {
            await _collection.InsertOneAsync(baseModel);
        }

        public virtual async Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter)
        {
            var res = await _collection.DeleteManyAsync(filter);
            return res;
        }

        public virtual async Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter)
        {
            var res = await _collection.DeleteOneAsync(filter);
            return res;
        }
    }
}
