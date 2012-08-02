using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class Notification : EntityBase
    {
        public virtual Guid UserId { get; set; }

        public virtual User User { get; set; }

        public virtual DateTime Date { get; set; }

        public virtual string SourceUser1 { get; set; }

        public virtual string SourceUser1ProfileImageUrl { get; set; }

        public virtual string SourceUser2 { get; set; }

        public virtual string SourceUser2ProfileImageUrl { get; set; }

        public virtual string SourceUser3 { get; set; }

        public virtual string SourceUser3ProfileImageUrl { get; set; }

        public virtual int? OtherUserCount { get; set; }

        public virtual bool IsRead { get; set; }

        public virtual string Text { get; set; }

        public virtual string Text2 { get; set; }

        public virtual ActivityType ActivityType
        {
            get { return (ActivityType)ActivityTypeValue; }
            set { ActivityTypeValue = (byte)value; }
        }

        public virtual byte ActivityTypeValue { get; set; }

        public virtual Guid? QuestionId { get; set; }

        public virtual Question Question { get; set; }

        public virtual Guid? AnswerId { get; set; }

        public virtual Answer Answer { get; set; }

        public virtual Guid? CommentId { get; set; }

        public virtual Comment Comment { get; set; }

        public virtual Guid? RelatedObjectId { get; set; }
    }
}
