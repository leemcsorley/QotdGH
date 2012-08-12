using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;
using Qotd.Utils;

namespace Qotd.PresentationObjects
{
    public class ActivityPO
    {
        public Activity Activity { get; set; }

        public string UserDisplayName { get; set; }

        public string UserProfileImageUrl { get; set; }

        public string TargetUserDisplayName { get; set; }

        public string TargetUserProfileImageUrl { get; set; }

        public QuestionPO Question { get; set; }

        public AnswerPO Answer { get; set; }

        public CommentPO Comment { get; set; }
    }
}
