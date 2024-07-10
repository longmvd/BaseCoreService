using BaseCoreService.Entities;
using BaseCoreService.Entities.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Common
{
    public class SecurityUtils
    {
        public static string GetColumns(string? columnNames, BaseEntity entity)
        {
            var columns = columnNames.Split(",").Select(col => col.Trim());
            foreach(var column in columns)
            {
                if (!entity.IsValidColumn(column))
                {
                    Console.WriteLine(column + " is invalid.");
                    throw new Exception(column + " is invalid.");
                }
            }
            return string.Join(",", columns);
            

        }

        public static string SafetyCharsForLIKEOperator(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("\\", "\\\\\\\\").Replace("%", "\\%");
        }

        public static bool IsValidColumn(Type type, string column)
        {
            var result =  type.GetProperty(column);
            return result != null && !IsNotMappedColumn(type, column);
        }

        public static bool IsValidFilterColumn(Type type, string column)
        {
            
            return IsValidColumn(type, column) || IsFilterColumn(type, column);
        }

        public static bool IsNotMappedColumn(Type type, string column)
        {
            var propertyInfo = type.GetProperty(column);
            return propertyInfo != null && propertyInfo.GetCustomAttributes<NotMappedAttribute>().Any();
        }

        public static bool IsFilterColumn(Type type, string column)
        {
            var propertyInfo = type.GetProperty(column);
            return propertyInfo != null && propertyInfo.GetCustomAttributes<FilterColumnAttribute>().Any();
        }
    }
}
