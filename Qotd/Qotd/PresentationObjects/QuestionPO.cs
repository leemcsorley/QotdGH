using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;
using Qotd.Utils;

namespace Qotd.PresentationObjects
{
    public class QuestionPO
    {
        public Question Question { get; set; }

        public bool HasUserVoted { get; set; }

        public UserPO User { get; set; }
    }
}
