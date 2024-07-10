
using BaseCoreService.Entities.Attributes;
using BaseCoreService.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BaseCoreService.Entities
{
    public class BaseEntity : ICloneable
    {
        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ModelState State { get; set; }

        private List<EntityDetailConfig> _entityDetailConfigs;

        [NotMapped]
        [JsonIgnore]
        public List<EntityDetailConfig> EntityDetailConfigs { get
            {
                return _entityDetailConfigs;
            } set
            {
                _entityDetailConfigs = value;
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public object? GetPropertyByKey(string propretyName)
        {
            if (this != null && string.IsNullOrEmpty(propretyName))
            {
                return this?.GetType()?.GetProperty(propretyName)?.GetValue(this, null);

            }
            return null;
        }

        public bool HasProperty(string propretyName)
        {
            if (this != null && !string.IsNullOrEmpty(propretyName))
            {
                return this?.GetType()?.GetProperty(propretyName) != null;

            }
            return false;
        }

        public bool IsValidColumn(string columnName)
        {
            return HasProperty(columnName);
        }

        public void SetPrimaryKey(string value)
        {
            var properties = this.GetType().GetProperties();

            PropertyInfo propertyKeyInfo = null;

            if (properties != null)
            {
                propertyKeyInfo = properties.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);

                if (propertyKeyInfo != null)
                {
                    if (propertyKeyInfo.PropertyType == typeof(long))
                    {
                        propertyKeyInfo.SetValue(this, long.Parse(value));
                    }
                    else if (propertyKeyInfo.PropertyType == typeof(Int32))
                    {
                        propertyKeyInfo.SetValue(this, Int32.Parse(value));
                    }
                    else if (propertyKeyInfo.PropertyType == typeof(Guid))
                    {
                        propertyKeyInfo.SetValue(this, Guid.Parse(value));
                    }
                    else
                    {
                        propertyKeyInfo.SetValue(this, value);
                    }
                }
            }


        }

        public void SetAutoPrimaryKey()
        {
            var properties = this.GetType().GetProperties();

            PropertyInfo propertyKeyInfo = null;

            if (properties != null)
            {
                propertyKeyInfo = properties.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);

                if (propertyKeyInfo != null)
                {
                    if (propertyKeyInfo.PropertyType == typeof(long))
                    {

                    }
                    else if (propertyKeyInfo.PropertyType == typeof(Int32))
                    {

                    }
                    else if (propertyKeyInfo.PropertyType == typeof(Guid))
                    {
                        propertyKeyInfo.SetValue(this, Guid.NewGuid());
                    }
                    else
                    {

                    }
                }
            }


        }

        public object? GetValueByAttribute(Type attributeType)
        {
            var properties = this.GetType().GetProperties();

            PropertyInfo propertyKeyInfo = null;

            if (properties != null)
            {
                propertyKeyInfo = properties.SingleOrDefault(p => p.GetCustomAttribute(attributeType, true) != null);

            }
            return propertyKeyInfo.GetValue(this);
        }


        public void SetValueByAttribute(Type attributeType, object? value)
        {
            var properties = this.GetType().GetProperties();

            PropertyInfo propertyKeyInfo = null;

            if (properties != null)
            {
                propertyKeyInfo = properties.SingleOrDefault(p => p.GetCustomAttribute(attributeType, true) != null);
            }

            if (propertyKeyInfo != null) {
                propertyKeyInfo.SetValue(this, value);
            }
            
        }

        public object GetValueOfPrimaryKey()
        {
            return GetValueByAttribute(typeof(KeyAttribute));
        }

        public PropertyInfo GetPropertyByAttribute(Type attributeType)
        {
            var properties = this.GetType().GetProperties();

            PropertyInfo propertyKeyInfo = null;

            if (properties != null)
            {
                propertyKeyInfo = properties.SingleOrDefault(p => p.GetCustomAttribute(attributeType, true) != null);
            }

            return propertyKeyInfo;
        }

        public PropertyInfo GetKeyProperty()
        {
            var result = this.GetPropertyByAttribute(typeof(KeyAttribute));
            return result;
        }


        public object GetPrimaryKeyType()
        {
            var properties = this.GetType().GetProperties();

            PropertyInfo propertyKeyInfo = null;

            if (properties != null)
            {
                propertyKeyInfo = properties.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);

                if (propertyKeyInfo != null)
                {
                    return propertyKeyInfo.PropertyType;
                }
            }
            return null;
        }

        public TableConfigAttribute GetTableConfig()
        {
            var tableConfig = this?.GetType()?.GetCustomAttributes<TableConfigAttribute>(true)?.FirstOrDefault();
            return tableConfig;
        }

        public BaseEntity()
        {
            this.EntityDetailConfigs = [];
        }

        //public object? GetMasterTableName()
        //{
        //    this.GetType().Get
        //}


    }
}
