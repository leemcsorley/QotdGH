using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.PresentationObjects
{
    public class AnswerPO
    {
        public Answer Answer { get; set; }

        public bool HasUserVoted { get; set; }

        public CommentPO[] Comments { get; set; }

        public string UserDisplayName { get; set; }

        public string UserProfileImageUrl { get; set; }
    }
}
