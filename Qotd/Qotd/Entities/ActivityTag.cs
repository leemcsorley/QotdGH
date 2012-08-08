using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class ActivityTag
    {
        public virtual Activity Activity { get; set; }

        public virtual Guid ActivityId { get; set; }

        public virtual Tag Tag { get; set; }

        public virtual Guid TagId { get; set; }
    }
}
