using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class ScoreEntry : ActionEntry, IEntity
    {
        public virtual Guid Id { get; set; }

        public virtual Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
