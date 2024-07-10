using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities
{
    public class EntityDetailConfig
    {
        public string DetailTableName { get; set; }

        public string ForeignKeyName { get; set; }

        /// <summary>
        /// Tên trường danh sách các con
        /// </summary>
        public string PropertyNameOnMaster { get; set; }

        public bool OnDeleteCascade { get; set; }

        public EntityDetailConfig(string detailTableName, string foreignKeyName, string propertyNameOnMaster, bool onDeleteCascade)
        {
            DetailTableName = detailTableName;
            ForeignKeyName = foreignKeyName;
            PropertyNameOnMaster = propertyNameOnMaster;
            OnDeleteCascade = onDeleteCascade;
        }

        public EntityDetailConfig()
        {
        }
    }
}
