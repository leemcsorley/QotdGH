using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

    public class Tag : EntityBase, IActionEntryStats
    {
        private ActionEntry[] _actionEntriesThisPeriod;

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

        public virtual int NumAnswersSecond { get; set; }

        public virtual int NumAnswersSecondThisPeriod { get; set; }

        public virtual int NumAnswersThird { get; set; }

        public virtual int NumAnswersThirdThisPeriod { get; set; }

        // complex serialized properties
        public virtual ActionEntry[] ActionEntriesThisPeriod
        {
            get
            {
                if (_actionEntriesThisPeriod == null && ActionEntriesThisPeriod_Data != null)
                {
                    using (MemoryStream ms = new MemoryStream(ActionEntriesThisPeriod_Data))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        _actionEntriesThisPeriod = (ActionEntry[])bf.Deserialize(ms);
                    }
                }
                return _actionEntriesThisPeriod;
            }
            set
            {
                _actionEntriesThisPeriod = value;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, _actionEntriesThisPeriod);
                    ActionEntriesThisPeriod_Data = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(ActionEntriesThisPeriod_Data, 0, (int)ms.Length);
                }
            }
        }

        public virtual byte[] ActionEntriesThisPeriod_Data { get; set; }
    }
}
