using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public abstract class EntityBase : IEntity
    {
        public virtual Guid Id
        {
            get;
            set;
        }
    }
}
