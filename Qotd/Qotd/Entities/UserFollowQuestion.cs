using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class UserFollowQuestion
    {
        public virtual User SourceUser { get; set; }

        public virtual Guid SourceUserId { get; set; }

        public virtual Question Question { get; set; }

        public virtual Guid QuestionId { get; set; }

        public virtual FollowSource Source
        {
            get { return (FollowSource)FollowSourceValue; }
            set { FollowSourceValue = (byte)value; }
        }

        public virtual byte FollowSourceValue { get; set; }
    }
}
