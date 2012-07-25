using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class Comment : EntityBase
    {
        public virtual Guid AnswerId { get; set; }

        public virtual Answer Answer { get; set; }

        public virtual Guid UserId { get; set; }

        public virtual User User { get; set; }

        public virtual string Content { get; set; }

        public virtual DateTime CreatedOn { get; set; }

        public virtual int NumLikes { get; set; }

        // denormalised ---

        public virtual string denorm_User_DisplayName { get; set; }

        public virtual string denorm_User_ProfileImageUrl { get; set; }
    }
}
