using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.PresentationObjects
{
    public enum SearchItemType
    {
        User,
        AnswerContent,
        QuestionContent,
        AnswerTags,
        QuestionTags
    }

    public class SearchResultPO
    {
        public Guid Id { get; set; }

        public SearchItemType Type { get; set; }

        public string DisplayText { get; set; }

        public string ImgUrl { get; set; }
    }
}
