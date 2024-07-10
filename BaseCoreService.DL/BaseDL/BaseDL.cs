using BaseCoreService.Entities;
using BaseCoreService.Entities.Attributes;
using BaseCoreService.Entities.Contants;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.DL
{
    public partial class BaseDL : IBaseDL
    {
        protected IDbConnection? mySqlConnection;

        public bool DeleteEntities<T>(List<T> entities)
        {
            throw new NotImplementedException();
        }

        #region Get
        public virtual T GetByID<T>(int ID)
        {
            throw new NotImplementedException();
        }

        public virtual T GetByID<T>(string ID)
        {
            throw new NotImplementedException();
        }


       
        #endregion

        public List<BaseEntity> GetAllBase()
        {
            //string storedProcedure = String.Format(Procedure.GET_ALL, this.Curr);
            //OpenDB();
            //var result = mySqlConnection.Query<T>(storedProcedure, commandType: CommandType.StoredProcedure);
            //CloseDB();
            //return result;
            throw new NotImplementedException();
        }

        public List<T> GetByFilter<T>(string filter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Khởi tạo và mở connection tới database
        /// </summary>
        public void OpenDB()
        {
            mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString);
            mySqlConnection.Open();
        }

        /// <summary>
        /// Đóng connection tới database
        /// </summary>
        public void CloseDB()
        {
            if (mySqlConnection != null)
            {
                mySqlConnection.Close();
            }
        }

        public IDbConnection GetDbConnection(string connectionString) 
        {
            if (connectionString != null)
            {
                return new MySqlConnection(connectionString);
            }
            return new MySqlConnection(DatabaseContext.ConnectionString);
        }

        public T UpdateOneByID<T>(T entity)
        {
            throw new NotImplementedException();
        }

        public bool UpdateEntities<T>(List<T> entities)
        {
            throw new NotImplementedException();
        }

        public bool SaveChanges<T>(List<T> entities)
        {
            throw new NotImplementedException();
        }


        public string GetInsertProcedureName(BaseEntity entity)
        {
            var tableConfig = entity.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
            if (tableConfig != null)
            {
                if (!string.IsNullOrWhiteSpace(tableConfig.InsertStoredProcedure))
                {
                    return tableConfig.InsertStoredProcedure;
                }
                else if (!string.IsNullOrWhiteSpace(tableConfig.TableName))
                {
                    var tableName = tableConfig.TableName;
                    string prefix = tableConfig.StoreProcedurePrefixName ?? string.Empty;
                    return string.Format(Procedure.INSERT, tableName);

                }
            }
            return null;
        }

        public string GetUpdateProcedureName(BaseEntity entity)
        {
            var tableConfig = entity.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
            if(tableConfig != null)
            {
                if (!string.IsNullOrWhiteSpace(tableConfig.UpdateStoredProcedure))
                {
                    return tableConfig.UpdateStoredProcedure;
                }else if (!string.IsNullOrWhiteSpace(tableConfig.TableName))
                {
                    var tableName = tableConfig.TableName;
                    string prefix = tableConfig.StoreProcedurePrefixName ?? string.Empty ;
                    return string.Format(Procedure.UPDATE, tableName);

                }
            }
            return null;
        }

        public string GetDeleteProcedureName(BaseEntity entity)
        {
            var tableConfig = entity.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
            if (tableConfig != null)
            {
                if (!string.IsNullOrWhiteSpace(tableConfig.DeleteStoredProcedure))
                {
                    return tableConfig.DeleteStoredProcedure;
                }
                else if (!string.IsNullOrWhiteSpace(tableConfig.TableName))
                {
                    var tableName = tableConfig.TableName;
                    string prefix = tableConfig.StoreProcedurePrefixName ?? string.Empty;
                    return string.Format(Procedure.DELETE_BY_ID, tableName);

                }
            }
            return null;
        }


        public T ExecuteScalarUsingCommandText<T>(string sql, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            return ExecuteScalarAsyncUsingStoredProcedure<T>(sql, parameters, connection, transaction).Result;
        }

        public T ExecuteScalarUsingStoredProcedure<T>(string storedProcedure, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
