using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.PresentationObjects
{
    public enum ScoreEntryType
    {
        AnswerVotes,
        QuestionVotes,
        ActivityLevel,
        Sociability,
        Overall
    }

    public class ScoreEntryPO
    {
        public ActivityPO Activity { get; set; }

        public ScoreEntryDetailPO[] Details { get; set; }

        public int PointsTotal { get; set; }
    }

    public class ScoreEntryDetailPO
    {
        public string Text { get; set; }

        public int Points { get; set; }
    }
}
