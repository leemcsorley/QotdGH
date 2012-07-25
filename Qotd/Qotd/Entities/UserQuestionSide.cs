using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class UserQuestionSide
    {
        public virtual Guid UserId { get; set; }

        public virtual User User { get; set; }

        public virtual Guid QuestionId { get; set; }

        public virtual Question Question { get; set; }

        public virtual int DebateSide { get; set; }
    }
}
