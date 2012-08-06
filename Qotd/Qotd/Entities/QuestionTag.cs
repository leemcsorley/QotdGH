using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class QuestionTag
    {
        public virtual Guid QuestionId { get; set; }

        public virtual Question Question { get; set; }

        public virtual Guid TagId { get; set; }

        public virtual Tag Tag { get; set; }
    }
}
