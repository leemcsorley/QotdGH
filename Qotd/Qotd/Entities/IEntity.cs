using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public interface IEntity
    {
        Guid Id { get; }
    }
}
