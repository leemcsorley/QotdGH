using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class QuestionContainer : EntityBase
    {
        public string Name { get; set; }

        public virtual Question TodaysQuestion { get; set; }
    }
}
