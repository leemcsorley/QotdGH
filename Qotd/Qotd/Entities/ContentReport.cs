using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class ContentReport : EntityBase
    {
        public virtual Guid? AnswerId { get; set; }

        public virtual Answer Answer { get; set; }

        public virtual Guid? QuestionId { get; set; }

        public virtual Question Question { get; set; }

        public virtual Guid UserId { get; set; }

        public virtual User User { get; set; }

        public virtual DateTime CreatedOn { get; set; }
    }
}
