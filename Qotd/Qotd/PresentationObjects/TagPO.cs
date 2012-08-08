using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.PresentationObjects
{
    public class TagPO
    {
        public Tag Tag { get; set; }

        public bool IsFollowedByCurrent { get; set; }
    }
}
