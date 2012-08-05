using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class Tag : EntityBase
    {
        public virtual string Value { get; set; }
    }
}
