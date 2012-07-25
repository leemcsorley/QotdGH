using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public enum QuestionType
    {
        Open,
        Debate
    }

    public class Question : EntityBase
    {
        public virtual string MainText { get; set; }

        public virtual string SubText { get; set; }

        public virtual string Details { get; set; }

        public virtual User User { get; set; }

        public virtual Guid UserId { get; set; }

        public virtual DateTime CreatedOn { get; set; }

        public virtual DateTime DateFor { get; set; }

        public virtual int VotesTotal { get; set; }

        public virtual int VotesUp { get; set; }

        public virtual int VotesDown { get; set; }

        public virtual QuestionType QuestionType
        {
            get { return (QuestionType)QuestionTypeValue; }
            set { QuestionTypeValue = (byte)value; }
        }

        public virtual byte QuestionTypeValue { get; set; }

        public virtual bool? WinningQuestion { get; set; }

        public virtual Guid? WinningAnswer1Id { get; set; }

        public virtual Answer WinningAnswer1 { get; set; }

        public virtual Guid? WinningAnswer2Id { get; set; }

        public virtual Answer WinningAnswer2 { get; set; }

        public virtual Guid? WinningAnswer3Id { get; set; }

        public virtual Answer WinningAnswer3 { get; set; }

        // denormalised ---

        public virtual string denorm_User_DisplayName { get; set; }

        public virtual string denorm_User_ProfileImageUrl { get; set; }
    }
}
