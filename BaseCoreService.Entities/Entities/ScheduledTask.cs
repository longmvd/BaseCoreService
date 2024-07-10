using BaseCoreService.Entities.Attributes;
using BaseCoreService.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.Entities
{
    [TableConfig("scheduled_task")]
    public class ScheduledTask: BaseEntity
    {
        [Key]
        public Guid ID { get; set; }

        public string CronExpression { get; set; }

        public string TaskName { get; set; }

        public DateTime NextExecutionTime { get; set; }

        public ScheduledTaskStatus Status { get; set; } = ScheduledTaskStatus.Waiting;
    }
}
