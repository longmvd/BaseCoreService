
using BaseCoreService.Entities;
using BaseCoreService.Entities.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities
{
    [TableConfig(tableName: "test_detail")]
    public class TestDetails: BaseEntity
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        public int TestID { get; set; }
    }
}
