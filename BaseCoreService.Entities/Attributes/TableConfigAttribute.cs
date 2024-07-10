using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.Attributes
{
    public class TableConfigAttribute : Attribute
    {
        public string TableName { get; set; }

        public string? ViewName { get; set; }

        public string? UniqueField { get; set; }

        public bool? IsDeleted { get; set; }


        public string? InsertStoredProcedure { get; set; }
        public string? UpdateStoredProcedure { get; set; }
        public string? DeleteStoredProcedure { get; set; }

        public string? StoreProcedurePrefixName { get; set; }

        public TableConfigAttribute(string tableName)
        {
            TableName = tableName;
        }

        public TableConfigAttribute(string tableName, string UniqueField) : this(tableName)
        {
            this.UniqueField = UniqueField;
        }

        public TableConfigAttribute(string tableName, string uniqueField, string? viewName) : this(tableName, uniqueField)
        {
            this.ViewName = viewName;
        }

        public TableConfigAttribute(string tableName, string uniqueField, string? viewName, bool isDeleted): this(tableName, uniqueField, viewName)
        {
            IsDeleted = isDeleted;
        }
    }
}
