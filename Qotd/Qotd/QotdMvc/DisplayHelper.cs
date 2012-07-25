using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qotd.Entities;

namespace QotdMvc
{
    public static class DisplayHelper
    {
        public static string AnswerButtonClass(QuestionType type, bool? pickedSide)
        {
            if (type == QuestionType.Open || pickedSide.GetValueOrDefault())
                return "btn btn-primary pull-right";
            else
                return "btn btn-primary disabled pull-right";
        }
    }
}