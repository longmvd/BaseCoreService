using BaseCoreService.Common;
using BaseCoreService.DL;
using BaseCoreService.Entities;
using BaseCoreService.Entities.Attributes;
using BaseCoreService.Entities.DTO;
using BaseCoreService.Entities.Enums;
using BaseCoreService.Entities.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace BaseCoreService.BL
{
    

    public partial class BaseBL : IBaseBL
    {
        //protected IAuthService _authService;
        protected IBaseDL _baseDL;
        protected ServiceCollection _serviceCollection;
        protected IBaseDL DL { get { return _baseDL; } set { _baseDL = value; } }

        private string _connectionString;

        public string ConnectionString { get { return _connectionString; } set { _connectionString = value; } }

        public BaseBL(IBaseDL baseDL, ServiceCollection serviceCollection )
        {
            DL = baseDL;
            _serviceCollection = serviceCollection;
            SetUserInfor(GetUserInfor());
        }

        protected string _userName = "";



        //public IEnumerable<dynamic> GetAll(Type type)
        //{
        //    return _baseDL.GetAll<typeof type>();
        //}

        public IEnumerable<BaseEntity> GetAllBase()
        {
            return _baseDL.GetAllBase();
        }
        public ServiceResponse Save(BaseEntity entity)
        {
            var response = new ServiceResponse();
            IDbConnection connection = null;
            IDbTransaction transaction = null;
            try
            {

                //validate
                var validateResult = new List<ValidateResult>();

                ValidateBeforeSave(entity, validateResult);

                if (validateResult.Count > 0)
                {
                    response.IsSuccess = false;
                    return response;

                }

                //before save
                this.BeforeSave(entity);

                connection = _baseDL.GetDbConnection(_connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();

                //save
                var result = this.DoSave(entity, transaction);

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

        public virtual void ValidateBeforeSave(BaseEntity entity, List<ValidateResult> validateResult)
        {

        }

        public virtual void AfterCommit(BaseEntity entity, ServiceResponse response)
        {
            //todo
            response.Data = entity;
            response.ServerTime = DateTime.Now;
        }

        public virtual void AfterCommitSaveChanges(BaseEntity entity, List<EntityFieldUpdate> fieldUpdates, ServiceResponse response)
        {
            //todo
            foreach (var fieldUpdate in fieldUpdates)
            {

                entity.SetProperty(fieldUpdate.FieldName, fieldUpdate.FieldValue);

            }
            response.Data = entity;
            response.ServerTime = DateTime.Now;
        }


        public virtual void AfterSave(BaseEntity entity, IDbTransaction transaction)
        {
            //todo anything after save successfully if needed
        }

        public virtual void AfterSaveChanges(BaseEntity entity, List<EntityFieldUpdate> fieldUpdates, IDbTransaction transaction)
        {
            //todo anything after save successfully if needed
        }


        public virtual bool DoSave(BaseEntity entity, IDbTransaction transaction)
        {
            var dic = new Dictionary<string, object>();
            entity.SetAutoPrimaryKey();
            string storedProcedure = entity.State == ModelState.Update ? this.BuildUpdateCommandText<BaseEntity>(entity, ref dic) : this.BuildInsertCommandText<BaseEntity>(entity, ref dic);
            //var dic = Converter.ConvertDatabaseParam(entity);

            if (entity.GetPrimaryKeyType() == typeof(int))
            {
                var primaryKey = ExecuteScalarAsyncUsingCommandText<int>(storedProcedure, transaction, dic).Result;
                if (entity.State == ModelState.Insert || entity.State == ModelState.Dupplicate)
                {
                    entity.SetValueByAttribute(typeof(KeyAttribute), primaryKey);
                }
            }
            else
            {
                this.ExecuteScalarAsyncUsingCommandText<object>(storedProcedure, transaction, dic);
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
                                DoSave(detailObject, transaction);
                            }
                            else if (detailObject.State == ModelState.Delete)
                            {
                                DoDelete(detailObject, transaction);
                            }
                        }

                    }
                }
            }
            return true;
        }

        private void DoDelete(BaseEntity detailObject, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse SaveChanges(BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse SaveList(IEnumerable<BaseEntity> entities)
        {
            return SaveListAsync(entities).Result;
        }

        public void Validate()
        {

        }


        public virtual void BeforeSave(BaseEntity entity)
        {
            if (entity.State == ModelState.Insert || entity.State == ModelState.Dupplicate)
            {
                entity.CreatedBy = _userName;
                entity.CreatedDate = DateTime.Now;
                if (entity.GetValueOfPrimaryKey() == null)
                {
                    entity.SetAutoPrimaryKey();
                }

            }

            entity.ModifiedBy = _userName;
            entity.ModifiedDate = DateTime.Now;
        }

        public async virtual Task BeforeSaveAsync(BaseEntity entity)
        {
            if (entity.State == ModelState.Insert || entity.State == ModelState.Dupplicate)
            {
                entity.CreatedBy = _userName;
                entity.CreatedDate = DateTime.Now;
                if (entity.GetValueOfPrimaryKey() == null)
                {
                    entity.SetAutoPrimaryKey();
                }

            }

            entity.ModifiedBy = _userName;
            entity.ModifiedDate = DateTime.Now;
        }

        public virtual void BeforeSaveChanges(BaseEntity entity)
        {
            if (entity.State == ModelState.Insert || entity.State == ModelState.Dupplicate)
            {
                entity.CreatedBy = _userName;
                entity.CreatedDate = DateTime.Now;
                if (entity.GetValueOfPrimaryKey() == null)
                {
                    entity.SetAutoPrimaryKey();
                }

            }

            entity.ModifiedBy = _userName;
            entity.ModifiedDate = DateTime.Now;
        }

        public void BeforeSaveListDetails()
        {

        }

        public string GetInsertProcedureName(BaseEntity entity)
        {
            return this._baseDL.GetInsertProcedureName(entity);
        }

        public string GetUpdateProcedureName(BaseEntity entity)
        {
            return this._baseDL.GetUpdateProcedureName(entity);
        }

        public string GetDeleteProcedureName(BaseEntity entity)
        {
            return this._baseDL.GetDeleteProcedureName(entity);
        }

        public T ExecuteScalarUsingStoredProcedure<T>(string storedProcedureName, IDbTransaction transaction, object param)
        {

            return ExecuteScalarAsyncUsingStoredProcedure<T>(storedProcedureName, transaction, param).Result;

        }

        public Task<T> ExecuteUsingStoredProcedure<T>(string storedProcedureName, IDbTransaction transaction, object param)
        {
            throw new NotImplementedException();
        }

        public async Task<T> ExecuteScalarAsyncUsingStoredProcedure<T>(string storedProcedureName, IDbTransaction transaction, object param)
        {
            return await _baseDL.ExecuteScalarAsyncUsingStoredProcedure<T>(storedProcedureName, (IDictionary<string, object>)param, null, transaction);
        }
        #region Build Command Text

        public string BuildInsertCommandText<T>(List<T> entities, ref Dictionary<string, object> dic, bool useRowCount = false)
        {
            StringBuilder builder = new();
            if (entities != null && entities.Count > 0)
            {
                var tableConfig = entities[0]?.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
                var param = new Dictionary<string, object>();
                var commandParams = new List<string>();
                if (tableConfig != null)
                {
                    builder.Append($"INSERT INTO {tableConfig.TableName} ( ");

                    for (int i = 0; i < entities.Count; i++)
                    {
                        var entity = entities[i];
                        var properties = entity?.GetType().GetProperties();
                        if (properties?.Length > 0)
                        {
                            StringBuilder paramBuilder = new StringBuilder("(");
                            foreach (var property in properties)
                            {

                                if (property.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() != null || (property.GetCustomAttributes<KeyAttribute>(true).FirstOrDefault() != null && (property.PropertyType == typeof(Int32) || property == typeof(long))))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(property.GetValue(entity)?.ToString()))
                                    {
                                        var columnName = property.GetCustomAttributes<ColumnAttribute>(true).FirstOrDefault()?.Name;
                                        string stringParam = property.Name;
                                        if (columnName != null)
                                        {
                                            stringParam = columnName;
                                        }
                                        if (i == 0)
                                        {
                                            builder.Append(stringParam + ",");
                                        }
                                        dic.TryAdd("@" + stringParam + i, property.GetValue(entity));
                                        paramBuilder.Append("@" + stringParam + i + ",");
                                    }
                                    else
                                    {
                                        var columnName = property.GetCustomAttributes<ColumnAttribute>(true).FirstOrDefault()?.Name;
                                        string stringParam = property.Name;
                                        if (columnName != null)
                                        {
                                            stringParam = columnName;
                                        }
                                        if (i == 0)
                                        {
                                            builder.Append(stringParam + ",");
                                        }
                                        dic.TryAdd("@" + stringParam + i, null);
                                        paramBuilder.Append("@" + stringParam + i + ",");
                                    }
                                }
                            }
                            //remove "," at the end of string builder
                            paramBuilder.Remove(paramBuilder.Length - 1, 1);
                            paramBuilder.Append(")");
                            commandParams.Add(paramBuilder.ToString());
                            if (i == 0)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                builder.Append(") VALUES ");
                            }

                        }

                    }
                    foreach (var commandParam in commandParams)
                    {
                        builder.Append(commandParam + ",");
                    }
                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(";");
                    _ = useRowCount ? builder.Append("SELECT ROW_COUNT();") : builder.Append("SELECT LAST_INSERT_ID();");
                }
            }
            return builder.ToString();
        }

        //public string BuildInsertCommandText<T>(List<T> entities, ref Dictionary<string, object> dic, string selectMode = "useRowCount")
        //{
        //    StringBuilder builder = new();
        //    if (entities != null && entities.Count > 0)
        //    {
        //        var tableConfig = entities[0]?.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
        //        var param = new Dictionary<string, object>();
        //        var commandParams = new List<string>();
        //        if (tableConfig != null)
        //        {
        //            builder.Append($"INSERT INTO {tableConfig.TableName} ( ");

        //            for (int i = 0; i < entities.Count; i++)
        //            {
        //                var entity = entities[i];
        //                var properties = entity?.GetType().GetProperties();
        //                if (properties?.Length > 0)
        //                {
        //                    StringBuilder paramBuilder = new StringBuilder("(");
        //                    foreach (var property in properties)
        //                    {

        //                        if (property.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() != null || (property.GetCustomAttributes<KeyAttribute>(true).FirstOrDefault() != null && (property.PropertyType == typeof(Int32) || property == typeof(long))))
        //                        {
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            if (!string.IsNullOrWhiteSpace(property.GetValue(entity)?.ToString()))
        //                            {
        //                                var columnName = property.GetCustomAttributes<ColumnAttribute>(true).FirstOrDefault()?.Name;
        //                                string stringParam = property.Name;
        //                                if (columnName != null)
        //                                {
        //                                    stringParam = columnName;
        //                                }
        //                                if (i == 0)
        //                                {
        //                                    builder.Append(stringParam + ",");
        //                                }
        //                                dic.TryAdd("@" + stringParam + i, property.GetValue(entity));
        //                                paramBuilder.Append("@" + stringParam + i + ",");
        //                            }
        //                        }
        //                    }
        //                    //remove "," at the end of string builder
        //                    paramBuilder.Remove(paramBuilder.Length - 1, 1);
        //                    paramBuilder.Append(")");
        //                    commandParams.Add(paramBuilder.ToString());
        //                    if (i == 0)
        //                    {
        //                        builder.Remove(builder.Length - 1, 1);
        //                        builder.Append(") VALUES ");
        //                    }

        //                }

        //            }
        //            foreach (var commandParam in commandParams)
        //            {
        //                builder.Append(commandParam + ",");
        //            }
        //            builder.Remove(builder.Length - 1, 1);
        //            builder.Append(";");
        //            _ = selectMode == "useRowCount" ? builder.Append("SELECT ROW_COUNT();") : selectMode == "useLastInsert" ? builder.Append("SELECT LAST_INSERT_ID();") : builder.Append("SELECT ROW_COUNT();SELECT LAST_INSERT_ID();");
        //        }
        //    }
        //    return builder.ToString();
        //}

        public string BuildUpdateCommandText<T>(List<T> entities, ref Dictionary<string, object> dic, bool useRowCount = false)
        {

            StringBuilder builder = new();
            if (entities != null && entities.Count > 0)
            {
                var tableConfig = entities[0]?.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
                var propertiesInfo = entities[0]?.GetType().GetProperties();
                var commandParams = new HashSet<string>();
                if (tableConfig != null && propertiesInfo != null)
                {
                    builder.Append($"Update {tableConfig.TableName} SET ");
                    var keyAttr = propertiesInfo.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                    var keyAttrName = keyAttr?.Name ?? string.Empty;
                    var keyStringParams = new List<string>();
                    foreach (var property in propertiesInfo)
                    {
                        if (property.GetCustomAttribute<KeyAttribute>(true) != null || property.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() != null || property.Name == "CreatedDate")
                        {
                            continue;
                        }
                        else
                        {
                            var tableColumn = property.Name ?? property.GetCustomAttributes<ColumnAttribute>(true).FirstOrDefault()?.Name;
                            builder.Append($"{tableColumn} = CASE ");
                            for (var i = 0; i < entities.Count; i++)
                            {
                                builder.Append($"WHEN {keyAttrName} = @{keyAttrName}{i} THEN @{tableColumn}{i} ");
                                dic.TryAdd($"@{keyAttrName}{i}", keyAttr.GetValue(entities[i]));
                                dic.TryAdd($"@{tableColumn}{i}", property.GetValue(entities[i]));
                                keyStringParams.Add($"@{keyAttrName}{i}");
                            }
                            builder.Append("END,");
                        }
                    }
                    builder.Remove(builder.Length - 1, 1);
                    var ids = entities.Select((entity, index) => $"@{keyAttrName}{index}").ToList();
                    builder.Append($" WHERE {keyAttrName} IN ({string.Join(",", ids)})");


                    builder.Append(";");
                    _ = useRowCount ? builder.Append("SELECT ROW_COUNT();") : builder.Append("");

                }
            }
            return builder.ToString();
        }

        public string BuildUpdateFieldsCommandText(BaseEntity entity, List<EntityFieldUpdate> fields, ref Dictionary<string, object> dic, bool useRowCount = false)
        {
            StringBuilder builder = new();
            if (entity != null)
            {
                var tableConfig = entity.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
                var propertiesInfo = entity.GetType().GetProperties();
                var commandParams = new HashSet<string>();
                if (tableConfig != null && propertiesInfo != null && fields?.Count > 0)
                {
                    builder.Append($"Update {tableConfig.TableName} SET ");
                    var keyAttr = propertiesInfo.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                    var keyAttrName = keyAttr?.Name ?? string.Empty;
                    bool inValidColumns = true;
                    foreach (var field in fields)
                    {

                        var tableColumn = field.FieldName;
                        if (!string.IsNullOrWhiteSpace(tableColumn)
                            && SecurityUtils.IsValidColumn(entity.GetType(), tableColumn)
                            && !keyAttr.Name.Equals(tableColumn, StringComparison.OrdinalIgnoreCase))
                        {
                            inValidColumns = false;
                            var paramString = Utils.GenerateSearchParam();
                            builder.Append($"{tableColumn} = {paramString},");
                            dic.Add($"{paramString}", field.FieldValue);
                        }

                    }
                    if (inValidColumns)
                    {
                        builder.Clear();
                    }
                    else
                    {
                        builder.Remove(builder.Length - 1, 1);
                        builder.Append($" WHERE {keyAttrName} = @ID");
                        dic.Add($"@ID", entity.GetValueOfPrimaryKey());
                        builder.Append(";");
                        _ = useRowCount ? builder.Append("SELECT ROW_COUNT();") : builder.Append("");

                    }
                }

            }
            return builder.ToString();
        }

        public string BuildDeleteCommandText<T>(List<T> entities, ref Dictionary<string, object> dic, bool useRowCount = false)
        {
            var builder = new StringBuilder();
            if (entities != null && entities.Count > 0)
            {
                var tableConfig = entities[0]?.GetType().GetCustomAttributes<TableConfigAttribute>(true).FirstOrDefault();
                var param = new Dictionary<string, object>();
                var commandParams = new List<string>();
                if (tableConfig != null)
                {
                    var propertiesInfo = entities[0]?.GetType().GetProperties();
                    var keyAttr = propertiesInfo?.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                    var keyAttrName = keyAttr?.Name ?? string.Empty;
                    builder.Append($"DELETE FROM {tableConfig.TableName} WHERE {keyAttrName} IN (");

                    for (int i = 0; i < entities.Count; i++)
                    {
                        var entity = entities[i];
                        builder.Append($"@{keyAttrName}{i},");
                        dic.Add($"@{keyAttrName}{i}", keyAttr.GetValue(entity));
                    }

                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(");");
                    _ = useRowCount ? builder.Append("SELECT ROW_COUNT();") : builder.Append("SELECT LAST_INSERT_ID();");
                }
            }
            return builder.ToString();
        }

        public string BuildInsertCommandText<T>(T entity, ref Dictionary<string, object> dic)
        {
            return this.BuildInsertCommandText<T>(new List<T>() { entity }, ref dic);
        }

        public string BuildUpdateCommandText<T>(T entity, ref Dictionary<string, object> dic)
        {
            return BuildUpdateCommandText<T>(new List<T>() { entity }, ref dic);
        }

        public string BuildDeleteCommandText<T>(T entity, ref Dictionary<string, object> dic)
        {
            return BuildDeleteCommandText<T>(new List<T>() { entity }, ref dic);
        }

        public string BuildPagingCommandText(Type type, PagingRequest pagingRequest, ref Dictionary<string, object> param, bool disabledLimit = false)
        {
            var builder = new StringBuilder();

            int pageIndex = pagingRequest.PageIndex;
            int pageSize = pagingRequest.PageSize;
            var conditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(pagingRequest.Filter))
            {
                var filter = JsonConvert.DeserializeObject<JArray>(pagingRequest.Filter);
                var condition = BuildCondition(type, filter, ref param);
                if (!string.IsNullOrEmpty(condition))
                {
                    conditions.Add(condition);
                }
            }
            if (!string.IsNullOrWhiteSpace(pagingRequest.CustomFilter))
            {
                var filter = JsonConvert.DeserializeObject<JArray>(pagingRequest.CustomFilter);
                var condition = BuildCondition(type, filter, ref param);
                if (!string.IsNullOrEmpty(condition))
                {
                    conditions.Add(condition);
                }
            }
            if (pagingRequest.QuickSearch != null)
            {
                var searchObject = pagingRequest.QuickSearch;
                var condition = BuildSearchCondition(type, searchObject, ref param);
                if (!string.IsNullOrEmpty(condition))
                {
                    conditions.Add(condition);
                }
            }
            if (conditions.Count > 0)
            {
                builder.Append(string.Join(" AND ", conditions));
            }
            else
            {
                builder.Append("1=1");
            }
            if (pagingRequest.Sort != null)
            {
                var sort = JsonConvert.DeserializeObject<List<PagingSort>>(pagingRequest.Sort ?? "");
                builder.Append(BuildSortOrder(sort));
            }

            if (!disabledLimit)
            {
                builder.Append(BuildLimit(ref pageSize, ref pageIndex));
            }

            builder.Append(BuildSelectCount(type, string.Join(" AND ", conditions)));

            var commandText = builder.ToString();



            return commandText;
        }


        public string BuildSelectCommandText(Type type, PagingRequest pagingRequest, ref Dictionary<string, object> param, bool disabledLimit = false)
        {
            var builder = new StringBuilder();

            int pageIndex = pagingRequest.PageIndex;
            int pageSize = pagingRequest.PageSize;
            var conditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(pagingRequest.Filter))
            {
                var filter = JsonConvert.DeserializeObject<JArray>(pagingRequest.Filter);
                conditions.Add(BuildCondition(type, filter, ref param));
            }
            if (!string.IsNullOrWhiteSpace(pagingRequest.CustomFilter))
            {
                var filter = JsonConvert.DeserializeObject<JArray>(pagingRequest.CustomFilter);
                conditions.Add(BuildCondition(type, filter, ref param));
            }
            if (pagingRequest.QuickSearch != null)
            {
                var searchObject = pagingRequest.QuickSearch;
                conditions.Add(BuildSearchCondition(type, searchObject, ref param));
            }
            if (conditions.Count > 0)
            {
                builder.Append(string.Join(" AND ", conditions));
            }
            else
            {
                builder.Append("1=1");
            }
            if (pagingRequest.Sort != null)
            {
                var sort = JsonConvert.DeserializeObject<List<PagingSort>>(pagingRequest.Sort ?? "");
                builder.Append(BuildSortOrder(sort));
            }
            if (!disabledLimit)
            {
                builder.Append(BuildLimit(ref pageSize, ref pageIndex));
            }

            var commandText = builder.ToString();

            return commandText;
        }


        private string BuildCondition(Type type, JToken filters, ref Dictionary<string, object> param)
        {
            if (filters != null && filters?.Count() > 0)
            {
                string whereCondition = "";
                int index = 0;
                foreach (var element in filters)
                {
                    if (element == null)
                    {
                        return whereCondition;
                    }
                    if (element.GetType() == typeof(JValue))
                    {
                        if (index == 0)
                        {
                            if (!SecurityUtils.IsValidFilterColumn(type, element.ToString()))
                            {
                                throw new Exception($"Column {element.ToString()} is not exist.");
                            }
                            else
                            {
                                var column = element.ToString();
                                OnBuildColumnFilter(ref column);
                                whereCondition += " " + column + " ";
                            }
                        }
                        else
                        if (index == 2)
                        {
                            var stringParam = Utils.GenerateSearchParam();
                            whereCondition = string.Format(whereCondition, stringParam);
                            var conditionValue = GetConditionValue(filters?[1]?.ToString() ?? "", element.ToString());
                            param.TryAdd(stringParam, conditionValue);
                        }
                        else
                        {
                            whereCondition += " " + GetCondition(element.ToString()) + " ";
                        }
                    }
                    if (element.GetType() == typeof(JArray))
                    {
                        whereCondition += this.BuildCondition(type, element, ref param);
                    }
                    index++;
                    //Console.WriteLine(element.GetType() == typeof(JArray));
                }
                return "(" + whereCondition + ")";
            }
            return "";
        }

        public virtual void OnBuildColumnFilter(ref string column)
        {
            //to do
        }
        private string GetCondition(string value)
        {
            switch (value.ToUpper())
            {
                case Operator.Equal:
                    return "= {0}";
                case Operator.StartWith:
                case Operator.EndWith:
                case Operator.Contains:
                    return "LIKE {0}";
                case Operator.NotEqual:
                    return "<> {0}";
                case Operator.LessOrEqual:
                    return "<= {0}";
                case Operator.GreaterOrEqual:
                    return ">= {0}";
                case Operator.LessThan:
                    return "< {0}";
                case Operator.GreaterThan:
                    return "> {0}";
                case Operator.And:
                    return "AND";
                case Operator.Or:
                    return "OR";
                case Operator.IsNotNull:
                    return "IS NOT NULL";
                case Operator.NotNull:
                    return "NOT NULL";
                case Operator.In:
                    return "IN {0}";
                case Operator.NotIn:
                    return "NOT IN {0}";
            }
            return "LIKE {0}";
        }

        private object GetConditionValue(string op, string value)
        {
            switch (op.ToUpper())
            {
                case Operator.Contains:
                    return $"%{SecurityUtils.SafetyCharsForLIKEOperator(value)}%";
                case Operator.StartWith:
                    //return "LIKE";
                    return $"{SecurityUtils.SafetyCharsForLIKEOperator(value)}%";
                case Operator.EndWith:
                    //return "LIKE";
                    return $"%{SecurityUtils.SafetyCharsForLIKEOperator(value)}";
                case Operator.In:
                case Operator.NotIn:
                    return GetMultipleValue(SecurityUtils.SafetyCharsForLIKEOperator(value));

            }
            return $"{SecurityUtils.SafetyCharsForLIKEOperator(value)}";
        }

        private string[] GetMultipleValue(string rawValue)
        {
            try
            {
                if (rawValue != null)
                {
                    return rawValue.Split(",");
                }

            }
            catch (Exception)
            {
                return new string[] { };
            }
            return new string[] { };

        }

        private string BuildSortOrder(IEnumerable<PagingSort> sorts)
        {

            var sortOrder = new StringBuilder();
            if (sorts != null)
            {
                sortOrder.Append(" ORDER BY");
                foreach (var sort in sorts)
                {
                    if (sort.Random)
                    {
                        sortOrder.Append(" RAND(),");
                    }
                    else
                    {
                        sortOrder.Append(" " + sort.Selector + " " + (sort.Desc ? "DESC" : "ASC") + ",");
                    }
                }

                return sortOrder.ToString().Remove(sortOrder.Length - 1);

            }
            return "";
        }

        private string BuildSearchCondition(Type typeModel, QuickSearch quickSearch, ref Dictionary<string, object> parameter)
        {
            var columns = quickSearch.Columns;
            var instance = (BaseEntity)Activator.CreateInstance(typeModel);
            var conditions = new List<string>();
            if (quickSearch != null && quickSearch.Columns != null && quickSearch.Columns.Length > 0)
            {
                parameter.Add("@SearchValue", $"%{SecurityUtils.SafetyCharsForLIKEOperator(quickSearch.SearchValue)}%");
                columns = SecurityUtils.GetColumns(columns, instance);
                if (columns != null)
                {
                    foreach (var column in columns.Split(",").Select(x => x.Trim()))
                    {
                        var type = typeModel.GetPropertyType(column.ToString());
                        if (Nullable.GetUnderlyingType(type) == typeof(DateTime))
                        {

                        }
                        else
                        {
                            conditions.Add($"( {column} LIKE @SearchValue)");
                        }
                    }
                    return "( " + string.Join(" OR ", conditions) + " )";
                }

            }
            return "";
        }

        private string BuildSelectCount(Type typeModel, string condition)
        {
            var instance = (BaseEntity)Activator.CreateInstance(typeModel);
            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;
            var tableName = instance.GetTableConfig().TableName ?? typeModel.Name;
            GetTableNamePagingCommandText(ref tableName);
            var commandText = $"SELECT COUNT(1) FROM {tableName} WHERE {condition}";
            GetTotalCountPagingCommandText(ref commandText);
            return commandText;
        }

        public virtual void GetTotalCountPagingCommandText(ref string commandText)
        {

        }

        public virtual void GetTableNamePagingCommandText(ref string tableName)
        {

        }




        public string BuildLimit(ref int pageSize, ref int pageIndex)
        {
            if (pageSize > DatabaseConstant.MaxReturnRecord || pageSize <= 0)
            {
                pageSize = DatabaseConstant.MaxReturnRecord;
            }
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            if (pageIndex > DatabaseConstant.MaxPageIndex)
            {
                pageIndex = DatabaseConstant.MaxPageIndex;
            }
            return $" LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize};";
        }



        #endregion
    }
}
