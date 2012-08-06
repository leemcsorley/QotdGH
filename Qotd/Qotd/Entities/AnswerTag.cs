using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class AnswerTag
    {
        public virtual Guid AnswerId { get; set; }

        public virtual Answer Answer { get; set; }

        public virtual Guid TagId { get; set; }

        public virtual Tag Tag { get; set; }
    }
}
