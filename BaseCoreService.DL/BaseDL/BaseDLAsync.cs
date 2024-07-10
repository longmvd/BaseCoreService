using BaseCoreService.Entities.Contants;
using Dapper;
using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace BaseCoreService.DL
{
    public partial class BaseDL : IBaseDL
    {

        public async Task<int> ExecuteAsyncUsingStoredProcedure(string storedProcedure, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            var result = await connection.ExecuteAsync(storedProcedure, commandType: CommandType.StoredProcedure, param: parameters, transaction: transaction);
            return result;
        }

        public async Task<T> ExecuteScalarAsyncUsingStoredProcedure<T>(string storedProcedure, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            T result = default(T);
            var cd = new CommandDefinition();

            try
            {
                var con = transaction != null ? transaction.Connection : connection;
                if (con != null)
                {
                    result = await con.ExecuteScalarAsync<T>(storedProcedure, parameters, transaction, commandType: CommandType.StoredProcedure);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<T> ExecuteScalarAsyncUsingCommandText<T>(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            T result = default(T);
            var cd = new CommandDefinition();

            try
            {
                var con = transaction != null ? transaction.Connection : connection;
                if (con != null)
                {
                    result = await con.ExecuteScalarAsync<T>(commandText, parameters, transaction, commandType: CommandType.Text);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> ExecuteAsyncUsingCommandText(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<T> ExecuteAsyncUsingCommandText<T>(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        #region Query async
        public async Task<List<T>> QueryAsyncUsingCommandText<T>(string commandText, IDictionary<string, object> parameters, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            var result = default(IEnumerable<T>);
            var cd = new CommandDefinition();
            try
            {
                var con = (transaction != null ? transaction.Connection : connection) ?? this.GetDbConnection(null);
                if (con != null)
                {
                    result = await con.QueryAsync<T>(commandText, parameters, transaction, commandType: CommandType.Text);
                }
                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<object>> QueryAsyncUsingCommandText(Type type, string commandText, IDictionary<string, object> parameters, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            var result = default(IEnumerable<object>);
            var cd = new CommandDefinition();
            try
            {
                var con = transaction != null ? transaction.Connection : connection;
                if (con != null)
                {
                    result = await con.QueryAsync(type, commandText, parameters, transaction, commandType: CommandType.Text);
                }
                return result.ToList();
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.StackTrace);
                throw;
            }
        }

        public async Task<List<List<object>>> QueryMultipleAsyncUsingCommandText(List<Type> types, string commandText, IDictionary<string, object> parameters, IDbConnection connection = null, IDbTransaction transaction = null)
        {

            //var cd = new CommandDefinition();
            //IEnumerable<IEnumerable<object>> result = new List<List<object>>();
            List<List<object>> result = new();
            GridReader reader = null;
            var con = transaction != null ? transaction.Connection : connection != null ? connection : this.GetDbConnection(null) ;
            try
            {
                if (con != null)
                {
                    reader = await con.QueryMultipleAsync(commandText, parameters, transaction, null, CommandType.Text);
                    int index = 0;
                    do
                    {
                        if (types != null && types.Count > index)
                        {
                            var ans = await reader.ReadAsync(types[index]);
                            result.Add(ans.ToList());
                        }
                        else
                        {
                            result.Append(await reader.ReadAsync<dynamic>());
                        }
                        index++;

                    } while (!reader.IsConsumed);
                }
                return result;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.StackTrace);
                throw;
            }
            finally { reader?.Dispose(); transaction?.Dispose(); con?.Dispose(); }
        }
        #endregion

        #region Get
        public async Task<List<T>> GetAll<T>()
        {
            string storedProcedure = String.Format(Procedure.GET_ALL, typeof(T).Name);
            OpenDB();
            var result = await mySqlConnection.QueryAsync<T>(storedProcedure, commandType: CommandType.StoredProcedure);
            CloseDB();
            return result.ToList();
        }

        //Task<IEnumerable<object>> GetPaginAsync(Type type, PagingRequest pagingRequest, ref ServiceResponse response)
        //{
        //    var result = default(IEnumerable<object>);
        //    var commandText = Utils.GetStringQuery("BaseBL_GetPaging");
        //    var instance = (BaseEntity)Activator.CreateInstance(type);
        //    var columns = "*";
        //    if (pagingRequest.Columns != null)
        //    {
        //        columns = SecurityUtils.GetColumns(pagingRequest.Columns, instance);

        //    }
        //    var condition = Buil

        //    commandText = String.Format(commandText, new object[] {columns, instance.GetTableConfig()?.TableName ?? instance.GetType().Name}, condition, sort);
        //}



        #endregion
    }
}
