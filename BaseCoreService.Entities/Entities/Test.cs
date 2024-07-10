
using BaseCoreService.Entities.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities
{
    [TableConfig(tableName: "test")]
    public class Test: BaseEntity
    {
        [Key]
         public int ID { get; set; }

        public string Name { get; set; }

        [NotMapped]
        [FilterColumn]
        public int Status { get; set; }

        public DateTime? StartDate{ get; set; }

        [NotMapped]
        public List<TestDetails> Details { get; set; }

        public Test()
        {
            EntityDetailConfigs = new List<EntityDetailConfig> { new EntityDetailConfig() { 
                DetailTableName = "test_detail",
                ForeignKeyName = "TestID",
                PropertyNameOnMaster = "Details",
            } };
        }
    }
}
