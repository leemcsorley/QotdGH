using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Qotd.Entities
{
    public class Answer : EntityBase
    {
        private TagEntry[] _tagEntries;

        public virtual Guid QuestionId { get; set; }

        public virtual Question Question { get; set; }

        public virtual Guid UserId { get; set; }

        public virtual User User { get; set; }

        public virtual string Title { get; set; }

        public virtual string Content { get; set; }

        public virtual DateTime CreatedOn { get; set; }

        public virtual int VotesTotal { get; set; }

        public virtual int VotesUp { get; set; }

        public virtual int VotesDown { get; set; }

        public virtual int DebateSide { get; set; }

        public virtual int NumComments { get; set; }

        public virtual Comment Comment1 { get; set; }

        public virtual Guid? Comment1Id { get; set; }

        public virtual Comment Comment2 { get; set; }

        public virtual Guid? Comment2Id { get; set; }

        public virtual bool? IsFirst { get; set; }

        public virtual bool? IsSecond { get; set; }

        public virtual bool? IsThird { get; set; }

        public virtual int? AnswerRank { get; set; }

        public virtual bool LinksCreated { get; set; }

        public virtual bool TagsProcessed { get; set; }

        // denormalised ---

        public virtual string denorm_User_DisplayName { get; set; }

        public virtual string denorm_User_ProfileImageUrl { get; set; }

        public virtual int denorm_User_OverallRank { get; set; }

        public virtual int denorm_User_OverallRankThisPeriod { get; set; }

        // complex serialised properties
        public virtual TagEntry[] TagEntries
        {
            get
            {
                if (_tagEntries == null && TagEntries_Data != null)
                {
                    using (MemoryStream ms = new MemoryStream(TagEntries_Data))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        _tagEntries = (TagEntry[])bf.Deserialize(ms);
                    }
                }
                return _tagEntries;
            }
            set
            {
                _tagEntries = value;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, _tagEntries);
                    TagEntries_Data = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(TagEntries_Data, 0, (int)ms.Length);
                }
            }
        }

        public virtual byte[] TagEntries_Data { get; set; }
    }
}
