using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.PresentationObjects
{
    public class QuestionPO
    {
        public Question Question { get; set; }

        public bool HasUserVoted { get; set; }
    }
}
