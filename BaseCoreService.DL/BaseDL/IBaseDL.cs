using BaseCoreService.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.DL
{
    public interface IBaseDL
    {
        IDbConnection GetDbConnection(string connectionString);
        T GetByID<T>(int ID);

        T GetByID<T>(string ID);

        List<T> GetByFilter<T>(string filter);

        Task<List<T>> GetAll<T>();

        List<BaseEntity> GetAllBase();

        T UpdateOneByID<T>(T entity);

        bool UpdateEntities<T>(List<T> entities);

        bool SaveChanges<T>(List<T> entities);

        bool DeleteEntities<T>(List<T> entities);

        #region Excecute Scalar
        T ExecuteScalarUsingStoredProcedure<T>(string storedProcedure, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction);

        T ExecuteScalarUsingCommandText<T>(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction);

        Task<int> ExecuteAsyncUsingStoredProcedure(string storedProcedure, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction);

        Task<bool> ExecuteAsyncUsingCommandText(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction);

        Task<T> ExecuteAsyncUsingCommandText<T>(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction);

        Task<T> ExecuteScalarAsyncUsingCommandText<T>(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction);

        Task<T> ExecuteScalarAsyncUsingStoredProcedure<T>(string storedProcedure, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction);

        #endregion

        #region Query Async
        Task<List<T>> QueryAsyncUsingCommandText<T>(string commandText, IDictionary<string, object>? parameters, IDbConnection connection = null, IDbTransaction transaction = null);

        Task<List<object>> QueryAsyncUsingCommandText(Type type, string commandText, IDictionary<string, object> parameters, IDbConnection connection = null, IDbTransaction transaction = null);

        Task<List<List<object>>> QueryMultipleAsyncUsingCommandText(List<Type> types, string commandText, IDictionary<string, object> parameters, IDbConnection connection = null, IDbTransaction transaction = null);
        #endregion

        string GetInsertProcedureName(BaseEntity entity);

        string GetUpdateProcedureName(BaseEntity entity);

        string GetDeleteProcedureName(BaseEntity entity);




    }
}
