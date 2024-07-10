using BaseCoreService.Common;
using BaseCoreService.Common.Resources;
using BaseCoreService.Entities;
using BaseCoreService.Entities.DTO;
using BaseCoreService.Entities.Enums;
using BaseCoreService.Entities.Extension;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BaseCoreService.BL
{
    public partial class BaseBL : IBaseBL
    {
        protected User _userInfo;
        public virtual async Task<T> GetByID<T>(Type type, string id) where T : BaseEntity
        {
            var instance = (BaseEntity)Activator.CreateInstance(type);
            var keyName = instance.GetKeyProperty()?.Name;
            if (keyName != null)
            {
                var pagingRequset = new PagingRequest() { CustomFilter = $"[\"{keyName}\", \"=\", \"{id}\"]" };
                var result = (await GetPagingAsync(type, pagingRequset)).PageData as IEnumerable<T>;
                return (T)result.FirstOrDefault();
            }
            return default(T);
        }

        public virtual async Task<ServiceResponse> SaveAsync(BaseEntity entity)
        {
            var response = new ServiceResponse();
            IDbConnection connection = null;
            IDbTransaction transaction = null;
            try
            {

                //validate
                var validateResults = new List<ValidateResult>();

                await ValidateBeforeSaveAsync(entity, validateResults);


                if (validateResults.Count > 0)
                {
                    response.OnError(new ErrorResponse() { Data = validateResults });
                    return response;

                }

                //before save
                await this.BeforeSaveAsync(entity);

                connection = _baseDL.GetDbConnection(_connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();

                //save
                var result = await this.DoSaveAsync(entity, transaction);

                if (result)
                {
                    this.AfterSave(entity, transaction);
                    transaction.Commit();

                    //log
                }
                else
                {
                    transaction.Rollback();
                    response.IsSuccess = false;
                }
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    response.IsSuccess = false;

                }
                throw;

            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            if (response.IsSuccess)
            {
                this.AfterCommit(entity, response);
            }

            return response;
        }

        public virtual async Task<ServiceResponse> SaveChangesAsync(BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual Task ValidateBeforeSaveAsync(BaseEntity baseModel, List<ValidateResult> validateResults)
        {
            return Task.CompletedTask;
        }


        public virtual async Task<ServiceResponse> SaveChangesAsync(BaseEntity entity, List<EntityFieldUpdate> fieldUpdates)
        {
            var response = new ServiceResponse();
            IDbConnection connection = null;
            IDbTransaction transaction = null;
            try
            {

                //validate


                //before save



                connection = _baseDL.GetDbConnection(_connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();

                //save


                var result = await DoSaveChangesAsync(entity, fieldUpdates, transaction);

                if (result)
                {
                    this.AfterSaveChanges(entity, fieldUpdates, transaction);
                    transaction.Commit();

                    //log
                }
                else
                {
                    transaction.Rollback();
                    response.IsSuccess = false;
                }
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    response.IsSuccess = false;

                }
                throw;

            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            if (response.IsSuccess)
            {
                this.AfterCommitSaveChanges(entity, fieldUpdates, response);
            }

            return response;


        }

        public virtual async Task<ServiceResponse> SaveListAsync(IEnumerable<BaseEntity> entities)
        {
            var response = new ServiceResponse();
            IDbConnection connection = null;
            IDbTransaction transaction = null;
            try
            {

                //validate
                var validateResult = new List<ValidateResult>();

                if (validateResult.Count > 0)
                {
                    response.IsSuccess = false;
                    return response;

                }

                //before save
                await this.BeforeSaveListAsync(entities);

                connection = _baseDL.GetDbConnection(_connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();

                //save
                var result = await this.DoSaveListAsyncPrototype(entities, transaction);

                if (result)
                {
                    await this.AfterSaveListAsync(entities, transaction);
                    if (response.IsSuccess)
                    {
                        transaction.Commit();
                    }

                    //log
                }
                else
                {
                    transaction.Rollback();
                    response.IsSuccess = false;
                }
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    response.IsSuccess = false;

                }
                throw;

            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            if (response.IsSuccess)
            {
                this.AfterCommitSaveList(entities, response);
            }

            return response;

        }

        public async virtual Task<bool> DoSaveAsync(BaseEntity entity, IDbTransaction transaction)
        {
            var dic = new Dictionary<string, object>();
            entity.SetAutoPrimaryKey();
            string commandText = GetCommandTextByModelState(entity, ref dic);
            //var dic = Converter.ConvertDatabaseParam(entity);

            if (entity.GetPrimaryKeyType() == typeof(int) || entity.GetPrimaryKeyType() == typeof(long))
            {
                var primaryKey = await ExecuteScalarAsyncUsingCommandText<int>(commandText, transaction, dic);
                if (entity.State == ModelState.Insert || entity.State == ModelState.Dupplicate)
                {
                    entity.SetValueByAttribute(typeof(KeyAttribute), primaryKey);
                }
            }
            else
            {
                if (entity.State == ModelState.Insert || entity.State == ModelState.Dupplicate)
                {
                    entity.SetAutoPrimaryKey();
                }
                await this.ExecuteScalarAsyncUsingCommandText<object>(commandText, transaction, dic);
            }

            if (entity.EntityDetailConfigs?.Count > 0)
            {
                foreach (var config in entity.EntityDetailConfigs.Where(c => !string.IsNullOrWhiteSpace(c.PropertyNameOnMaster)))
                {
                    IList detailObjects = entity.Get<IList>(config.PropertyNameOnMaster);
                    if (detailObjects != null)
                    {
                        foreach (BaseEntity detailObject in detailObjects)
                        {
                            if (detailObject.State == ModelState.Insert ||
                                detailObject.State == ModelState.Update ||
                                detailObject.State == ModelState.Dupplicate)
                            {
                                detailObject.Set(config.ForeignKeyName, entity.GetValueOfPrimaryKey());
                                if (detailObject.State == ModelState.Insert ||
                                detailObject.State == ModelState.Dupplicate)
                                {
                                    detailObject.CreatedDate = DateTime.Now;
                                    detailObject.CreatedBy = _userName;
                                    if (detailObject.GetValueOfPrimaryKey() == null)
                                    {
                                        detailObject.SetAutoPrimaryKey();
                                    }
                                }
                                detailObject.ModifiedDate = DateTime.Now;
                                detailObject.ModifiedBy = _userName;
                                await DoSaveAsync(detailObject, transaction);
                            }
                            else if (detailObject.State == ModelState.Delete)
                            {
                                await DoDeleteAsync(detailObject, transaction);
                            }
                        }

                    }
                }
            }
            return true;
        }

        public virtual async Task<bool> DoSaveChangesAsync(BaseEntity entity, List<EntityFieldUpdate> fieldUpdates, IDbTransaction transaction)
        {
            var param = new Dictionary<string, object>();
            var sql = BuildUpdateFieldsCommandText(entity, fieldUpdates, ref param, true);
            return await this.ExecuteScalarAsyncUsingCommandText<bool>(sql, transaction, param);
        }

        public async virtual Task<bool> DoDeleteAsync(BaseEntity entity, IDbTransaction transaction)
        {
            var param = new Dictionary<string, object>();
            var query = BuildDeleteCommandText<BaseEntity>(entity, ref param);
            var isSuccess = false;
            if (query != null)
            {
                isSuccess = await ExecuteScalarAsyncUsingCommandText<bool>(query, transaction, param);
            }
            return isSuccess;
        }


        public virtual async Task<bool> DoSaveListAsync(IEnumerable<BaseEntity> entities, IDbTransaction transaction)
        {
            var isSuccess = false;
            foreach (var entity in entities)
            {
                isSuccess = await DoSaveAsync(entity, transaction);
                if (!isSuccess)
                {
                    break;
                }
            }
            return isSuccess;
        }

        public virtual void BeforeSaveList(IEnumerable<BaseEntity> entities)
        {
            // TODOS
            foreach (var entity in entities)
            {
                BeforeSave(entity);
            }

        }

        public virtual async Task BeforeSaveListAsync(IEnumerable<BaseEntity> entities)
        {
            // TODOS
            foreach (var entity in entities)
            {
                await BeforeSaveAsync(entity);
            }

        }

        public virtual void AfterCommitSaveList(IEnumerable<BaseEntity> entities, ServiceResponse response)
        {
            // TODOS

        }

        public virtual void AfterSaveList(IEnumerable<BaseEntity> entities, IDbTransaction transaction)
        {

        }

        public virtual async Task AfterSaveListAsync(IEnumerable<BaseEntity> entities, IDbTransaction transaction)
        {

        }


        public virtual async Task<T> ExecuteScalarAsyncUsingCommandText<T>(string commandText, IDbTransaction transaction, object param)
        {
            return await this.ExecuteScalarAsyncUsingCommandText<T>(commandText, (IDictionary<string, object>)param, null, transaction);
        }

        public virtual async Task<T> ExecuteScalarAsyncUsingCommandText<T>(string commandText, object param)
        {
            IDbConnection connection = null;
            IDbTransaction transaction = null;
            var result = default(T);
            try
            {
                connection = _baseDL.GetDbConnection(_connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();
                result = await this.ExecuteScalarAsyncUsingCommandText<T>(commandText, (IDictionary<string, object>)param, connection, transaction);
                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();

                }
                throw;

            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            return result;

        }


        public virtual async Task<T> ExecuteScalarAsyncUsingCommandText<T>(string commandText, IDictionary<string, object> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            return await _baseDL.ExecuteScalarAsyncUsingCommandText<T>(commandText, parameters, connection, transaction);
        }

        private string GetCommandTextByModelState(BaseEntity entity, ref Dictionary<string, object> param)
        {
            switch (entity.State)
            {
                case ModelState.Insert:
                    return BuildInsertCommandText<BaseEntity>(entity, ref param);

                case ModelState.Update:
                    return BuildUpdateCommandText<BaseEntity>(entity, ref param);

                case ModelState.Delete:
                    return BuildDeleteCommandText<BaseEntity>(entity, ref param);
                default:
                    return BuildInsertCommandText<BaseEntity>(entity, ref param);

            }
        }


        /// <summary>
        /// Hàm đang phát triển 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual async Task<bool> DoSaveListAsyncPrototype(IEnumerable<BaseEntity> entities, IDbTransaction transaction)
        {
            var isSuccess = false;
            var listForInsert = new List<BaseEntity>(entities.Where(entity => entity.State == ModelState.Insert));
            var listForUpdate = new List<BaseEntity>(entities.Where(entity => entity.State == ModelState.Update));
            var listForDelete = new List<BaseEntity>(entities.Where(entity => entity.State == ModelState.Delete));
            var dic = new Dictionary<string, object>();
            var insertStatement = BuildInsertCommandText(listForInsert, ref dic, useRowCount: false);
            if (!string.IsNullOrWhiteSpace(insertStatement))
            {
                isSuccess = true;
                // mysql return the id of the first entity in inserted list not last entity
                var lastInsertId = await ExecuteScalarAsyncUsingCommandText<int>(insertStatement, transaction, dic);
                if (lastInsertId != null)
                {
                    int masterID = lastInsertId;
                    foreach (var entity in listForInsert)
                    {
                        if (entity.GetPrimaryKeyType() == typeof(Int32) || entity.GetPrimaryKeyType() == typeof(long))
                        {
                            entity.SetPrimaryKey((masterID++).ToString());
                        }
                        if (entity.EntityDetailConfigs?.Count > 0)
                        {
                            foreach (var config in entity.EntityDetailConfigs.Where(c => !string.IsNullOrWhiteSpace(c.PropertyNameOnMaster)).GroupBy(g => g.DetailTableName).Select(grp => grp.First()))
                            {
                                IEnumerable<BaseEntity> detailObjects = entity.Get<IEnumerable<BaseEntity>>(config.PropertyNameOnMaster);
                                if (detailObjects != null)
                                {
                                    foreach (BaseEntity detailObject in detailObjects)
                                    {

                                        if (detailObject.State == ModelState.Insert ||
                                            detailObject.State == ModelState.Update ||
                                            detailObject.State == ModelState.Dupplicate)
                                        {
                                            detailObject.Set(config.ForeignKeyName, entity.GetValueOfPrimaryKey());
                                            if (detailObject.State == ModelState.Insert ||
                                            detailObject.State == ModelState.Dupplicate)
                                            {
                                                detailObject.CreatedDate = DateTime.Now;
                                                detailObject.CreatedBy = _userName;
                                                if (detailObject.GetValueOfPrimaryKey() == null)
                                                {
                                                    detailObject.SetAutoPrimaryKey();
                                                }
                                            }
                                            detailObject.ModifiedDate = DateTime.Now;
                                            detailObject.ModifiedBy = _userName;
                                        }
                                    }
                                    await DoSaveListAsyncPrototype(detailObjects, transaction);

                                }
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            dic.Clear();
            var updateStatement = BuildUpdateCommandText(listForUpdate, ref dic, useRowCount: true);
            if (!string.IsNullOrWhiteSpace(updateStatement))
            {
                isSuccess = true;
                var updatedRows = await ExecuteScalarAsyncUsingCommandText<int>(updateStatement, transaction, dic);
                if (updatedRows == listForUpdate.Count)
                {
                    foreach (var entity in listForUpdate)
                    {
                        if (entity.EntityDetailConfigs?.Count > 0)
                        {
                            foreach (var config in entity.EntityDetailConfigs.Where(c => !string.IsNullOrWhiteSpace(c.PropertyNameOnMaster)))
                            {
                                IEnumerable<BaseEntity> detailObjects = entity.Get<IEnumerable<BaseEntity>>(config.PropertyNameOnMaster);
                                if (detailObjects != null)
                                {
                                    foreach (BaseEntity detailObject in detailObjects)
                                    {

                                        if (detailObject.State == ModelState.Insert ||
                                            detailObject.State == ModelState.Update ||
                                            detailObject.State == ModelState.Dupplicate)
                                        {
                                            detailObject.Set(config.ForeignKeyName, entity.GetValueOfPrimaryKey());
                                            if (detailObject.State == ModelState.Insert ||
                                            detailObject.State == ModelState.Dupplicate)
                                            {
                                                detailObject.CreatedDate = DateTime.Now;
                                                detailObject.CreatedBy = _userName;
                                                if (detailObject.GetValueOfPrimaryKey() == null)
                                                {
                                                    detailObject.SetAutoPrimaryKey();
                                                }
                                            }
                                            detailObject.ModifiedDate = DateTime.Now;
                                            detailObject.ModifiedBy = _userName;
                                        }
                                    }
                                    await DoSaveListAsyncPrototype(detailObjects, transaction);

                                }
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }

            }
            dic.Clear();
            var deleteStatement = BuildDeleteCommandText(listForDelete, ref dic, useRowCount: true);
            if (!string.IsNullOrWhiteSpace(deleteStatement))
            {
                isSuccess = true;
                var deletedRows = await ExecuteScalarAsyncUsingCommandText<int>(deleteStatement, transaction, dic);
                if (deletedRows == listForDelete.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return isSuccess;
        }

        #region Get

        public async Task<IEnumerable<T>> GetAllAsync<T>(PagingRequest request)
        {
            var response = new PagingResponse();
            var commandText = BaseEntityQuery.BaseBL_GetPaging;
            GetPagingCommandText(ref commandText);
            var instance = (BaseEntity)Activator.CreateInstance(typeof(T));
            var columns = "*";
            if (request.Columns != null)
            {
                columns = SecurityUtils.GetColumns(request.Columns, instance);

            }
            var param = new Dictionary<string, object>();
            var condition = BuildSelectCommandText(typeof(T), request, ref param);
            var tableName = instance.GetTableConfig()?.TableName ?? instance.GetType().Name;
            commandText = string.Format(commandText, new object[] { columns, tableName, condition });
            OnBeforeExecutePagingQuery(ref commandText, request);
            var result = await QueryAsyncUsingCommandText<T>(commandText, param);
            return result;

        }

        public async Task<List<T>> GetAllObjectAsync<T>(PagingRequest request)
        {
            var res = await GetAllAsync<T>(request);
            return res.ToList();
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : BaseEntity
        {
            var res = default(IEnumerable<T>);
            var entity = (T)Activator.CreateInstance(typeof(T));
            var query = BaseEntityQuery.BaseBL_GetAll;
            query = string.Format(query, entity.GetTableConfig().TableName);
            try
            {
                using IDbConnection connection = _baseDL.GetDbConnection(_connectionString);
                var entities = await _baseDL.QueryAsyncUsingCommandText(typeof(T), query, null, connection);
                res = entities.OfType<T>();
            }
            catch
            {
                throw;
            }
            return res;
        }

        public virtual async Task<IEnumerable<T>> GetAll<T>(Type type) where T : BaseEntity
        {
            var res = default(IEnumerable<T>);
            var entity = (T)Activator.CreateInstance(type);
            var query = BaseEntityQuery.BaseBL_GetAll;
            query = string.Format(query, entity.GetTableConfig().TableName);
            try
            {
                using IDbConnection connection = _baseDL.GetDbConnection(_connectionString);
                var entities = await _baseDL.QueryAsyncUsingCommandText(type, query, null, connection);
                res = entities.OfType<T>();
            }
            catch
            {
                throw;
            }
            return res;
        }

        public virtual async Task<PagingResponse> GetPagingAsync(Type type, PagingRequest pagingRequest)
        {
            var response = new PagingResponse();
            var commandText = BaseEntityQuery.BaseBL_GetPaging;
            GetPagingCommandText(ref commandText);
            var instance = (BaseEntity)Activator.CreateInstance(type);
            var columns = "*";
            if (pagingRequest.Columns != null)
            {
                columns = SecurityUtils.GetColumns(pagingRequest.Columns, instance);

            }
            var param = new Dictionary<string, object>();
            var condition = BuildPagingCommandText(type, pagingRequest, ref param);
            var tableName = instance.GetTableConfig()?.TableName ?? instance.GetType().Name;
            commandText = string.Format(commandText, new object[] { columns, tableName, condition });
            OnBeforeExecutePagingQuery(ref commandText, pagingRequest);
            var result = await QueryMultipleAsyncUsingCommandText(new List<Type>() { type, typeof(int) }, commandText, param);
            result = await OnAfterExecutePagingQueryAsync(result);
            response.PageData = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result?.ElementAt(0)), typeof(List<>).MakeGenericType(result?.ElementAtOrDefault(0)?.FirstOrDefault()?.GetType() ?? type));
            response.Total = JsonConvert.DeserializeObject<int>(JsonConvert.SerializeObject(result?.ElementAt(1).FirstOrDefault()));
            return response;
        }

        #endregion

        #region Query Async
        public async Task<IEnumerable<T>> QueryAsyncUsingCommandText<T>(string commandText, IDictionary<string, object>? parameters, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            IEnumerable<T>? res;
            try
            {
                if (connection == null)
                {
                    connection = _baseDL.GetDbConnection(_connectionString);
                }
                var entities = await _baseDL.QueryAsyncUsingCommandText<T>(commandText, parameters, connection, transaction);
                res = entities.OfType<T>();
                connection.Close();

            }
            catch
            {
                if (connection != null)
                {
                    connection.Close();
                }
                throw;
            }
            return res;

        }

        public async Task<IEnumerable<object>> QueryAsyncUsingCommandText(Type type, string commandText, IDictionary<string, object> parameters, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            IEnumerable<object>? res;
            try
            {
                if (connection == null)
                {
                    connection = _baseDL.GetDbConnection(_connectionString);
                }
                res = await _baseDL.QueryAsyncUsingCommandText(type, commandText, parameters, connection, transaction);
                connection.Close();

            }
            catch
            {
                if (connection != null)
                {
                    connection.Close();
                }
                throw;
            }
            return res;
        }

        public async Task<IEnumerable<IEnumerable<object>>> QueryMultipleAsyncUsingCommandText(List<Type> types, string commandText, IDictionary<string, object> parameters, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            var res = await _baseDL.QueryMultipleAsyncUsingCommandText(types, commandText, parameters, connection, transaction);
            return res;
        }

        #endregion

        #region Delete
        public async Task<ServiceResponse> DeleteListAsync(IEnumerable<BaseEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.State = ModelState.Delete;
            }
            var response = await SaveListAsync(entities);
            return response;
        }

        #endregion

        public async virtual Task<ServiceResponse> UpdateOneAsync(BaseEntity entity)
        {
            var response = new ServiceResponse();
            if (entity == null)
            {
                return response.OnError(new ErrorResponse() { ErrorMessage = Resource.DEV_NullRequestObject });
            }
            else
            {
                entity.State = ModelState.Update;
            }
            return await this.SaveAsync(entity);
        }

        public virtual void GetPagingCommandText(ref string commandText)
        {
            // to do assign command text
        }

        public virtual void OnBeforeExecutePagingQuery(ref string commandText, PagingRequest request)
        {
            // to do assign command text

        }

        public virtual async Task<IEnumerable<IEnumerable<object>>> OnAfterExecutePagingQueryAsync(IEnumerable<IEnumerable<object>> result)
        {
            // to do handle result
            return await Task.FromResult(result);
        }

        public string GetTokenFromHeader()
        {

            var bearer = _serviceCollection.HttpContextBL.GetRequestHeaderValue("Authorization");
            var token = bearer.Split(" ").ElementAtOrDefault(1);
            return token;


        }

        public void SetUserInfor(User user)
        {
            if (user != null)
            {
                _userInfo = user;
                _userName = user.UserName ?? user.FullName ?? "";
            }
        }

        public User GetUserInfor()
        {
            var token = GetTokenFromHeader();
            if (token != null)
            {
                var authService = _serviceCollection.ServiceProvider.GetService<IAuthBL>();
                var userOject = authService?.GetInfoFromToken(token);

                var roles = new List<Role>();
                var hasRole = (userOject as JwtPayload).TryGetValue("Roles", out var strRole);
                if (hasRole)
                {
                    var stringRoles = strRole?.ToString()?.Split();
                    foreach (var stringRole in stringRoles)
                    {
                        roles.Add(new Role() { RoleCode = stringRole.Trim() });
                    }


                }
                (userOject as JwtPayload).TryGetValue("UserID", out var UserID);
                (userOject as JwtPayload).TryGetValue("Email", out var Email);
                (userOject as JwtPayload).TryGetValue("FullName", out var FullName);
                var user = new User()
                {
                    Roles = roles,
                    UserID = new Guid(UserID.ToString()),
                    FullName = FullName.ToString(),
                    Email = Email.ToString()

                };

                return user;
            }
            return null;
        }
    }
}
