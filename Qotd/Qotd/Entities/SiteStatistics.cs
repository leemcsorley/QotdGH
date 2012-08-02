using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class SiteStatistics : EntityBase
    {
        public virtual DateTime Date { get; set; }

        public virtual int NumUsers { get; set; }

        public virtual int MaxNumFollows { get; set; }

        public virtual int MaxNumFollowing { get; set; }

        public virtual int MaxNumComments { get; set; }

        public virtual int MaxNumAnswers { get; set; }

        public virtual int MaxNumQuestions { get; set; }

        public virtual int MaxNumAnswersVoted { get; set; }

        public virtual int MaxNumQuestionsVoted { get; set; }

        public virtual int MaxNumAnswersWon { get; set; }

        public virtual int MaxNumQuestionsWon { get; set; }

        public virtual int MaxNumAnswersSecond { get; set; }

        public virtual int MaxNumAnswersThird { get; set; }

        public virtual int MaxTotalAnswerVotes { get; set; }

        public virtual int MaxTotalQuestionVotes { get; set; }

        public virtual int MaxNumFollowsThisPeriod { get; set; }

        public virtual int MaxNumFollowingThisPeriod { get; set; }

        public virtual int MaxNumQuestionsWonThisPeriod { get; set; }

        public virtual int MaxNumAnswersWonThisPeriod { get; set; }

        public virtual int MaxNumAnswersSecondThisPeriod { get; set; }

        public virtual int MaxNumAnswersThirdThisPeriod { get; set; }

        public virtual int MaxNumAnswersThisPeriod { get; set; }

        public virtual int MaxNumQuestionsThisPeriod { get; set; }

        public virtual int MaxNumCommentsThisPeriod { get; set; }

        public virtual int MaxNumAnswersVotedThisPeriod { get; set; }

        public virtual int MaxNumQuestionsVotedThisPeriod { get; set; }

        public virtual int MaxTotalAnswerVotesThisPeriod { get; set; }

        public virtual int MaxTotalQuestionVotesThisPeriod { get; set; }
    }
}
