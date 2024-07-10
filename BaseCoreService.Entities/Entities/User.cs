using BaseCoreService.Entities;
using BaseCoreService.Entities.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BaseCoreService.Entities
{
    [TableConfig(tableName: "`user`")]
    public class User : BaseEntity
    {
        [Key]
        public Guid UserID { get; set; }

        public string? UserCode { get; set; }
        public string? UserName { get; set; }

        [JsonIgnore]
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
        public string Status { get; set; }


        [NotMapped]
        [JsonIgnore]
        public List<UserRole>? UserRoles { get; set; }

        [NotMapped]
        public List<Role>? Roles { get; set; }



        public User()
        {
            this.EntityDetailConfigs = new List<EntityDetailConfig>()
            {
                new EntityDetailConfig
                {
                     PropertyNameOnMaster = "UserRoles",
                     ForeignKeyName = "UserID"
                }
            };
        }
    }
}
