using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public static class TagExtensions
    {
        public static string GetTagString(this TagEntry[] tags)
        {
            return String.Join(" ", tags.Select(t => t.Value));
        }
    }

    [Serializable]
    public class TagEntry
    {
        public string Value { get; set; }

        public bool Approved { get; set; }

        public Guid TagId { get; set; }
    }

    public class Tag : EntityBase
    {
        public virtual string Value { get; set; }

        public virtual int NumFollowing { get; set; }

        public virtual int NumFollowingThisPeriod { get; set; }

        public virtual int NumAnswers { get; set; }

        public virtual int NumAnswersThisPeriod { get; set; }

        public virtual int NumQuestions { get; set; }

        public virtual int NumQuestionsThisPeriod { get; set; }

        public virtual int NumAnswersWon { get; set; }

        public virtual int NumAnswersWonThisPeriod { get; set; }

        public virtual int NumQuestionsWon { get; set; }

        public virtual int NumQuestionsWonThisPeriod { get; set; }

        public virtual bool Approved { get; set; }
    }
}
