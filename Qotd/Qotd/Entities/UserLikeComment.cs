using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class UserLikeComment
    {
        public virtual Guid UserId { get; set; }

        public virtual User User { get; set; }

        public virtual Guid CommentId { get; set; }

        public virtual Comment Comment { get; set; }
    }
}
