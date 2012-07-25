using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class User : EntityBase
    {
        public virtual string Username { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual string Email { get; set; }

        public virtual string Password { get; set; }

        public virtual string ProfileImageUrl { get; set; }

        public virtual string FacebookId { get; set; }

        public int NumComments { get; set; }

        public int NumAnswers { get; set; }

        public int NumQuestions { get; set; }

        public int NumAnswersVoted { get; set; }

        public int NumQuestionsVoted { get; set; }

        public int NumAnswersWon { get; set; }

        public int NumQuestionsWon { get; set; }

        public int NumAnswersSecond { get; set; }

        public int NumAnswersThird { get; set; }

        public int TotalAnswerVotes { get; set; }

        public int TotalQuestionVotes { get; set; }

        public int NumAnswersThisPeriod { get; set; }

        public int NumQuestionsThisPeriod { get; set; }

        public int NumCommentsThisPeriod { get; set; }

        public int NumAnswersVotedThisPeriod { get; set; }

        public int NumQuestionsVotedThisPeriod { get; set; }

        public int TotalAnswerVotesThisPeriod { get; set; }

        public int TotalQuestionVotesThisPeriod { get; set; }

        public int Score
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public int ScoreThisPeriod
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
