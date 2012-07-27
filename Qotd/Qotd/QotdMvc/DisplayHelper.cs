using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qotd.Entities;
using Qotd.Utils;

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

        public static string GetRankIconClass(int rank)
        {
            switch (rank)
            {
                case 1:
                    return "icon-trophy gold";
                case 2:
                    return "icon-trophy silver";
                case 3:
                    return "icon-trophy bronze";
            }
            return "";
        }

        public static string ImgUrl(string url)
        {
            if (url.StartsWith("\\"))
                return Config.UploadImagesUrl + url;
            return url;
        }

        public static string QuestionMainText(string text)
        {
            if (!text.EndsWith("?"))
                text = text.TrimEnd(' ', '.') + "?";
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}