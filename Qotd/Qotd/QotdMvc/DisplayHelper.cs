using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qotd.Entities;
using Qotd.Utils;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Qotd.PresentationObjects;

namespace QotdMvc
{
    public enum SingleType
    {
        Answer,
        Question
    }

    public enum SearchGroupType
    {
        User,
        Answer,
        Question
    }

    public static class DisplayHelper
    {
        public static string RatingHelper(double rating)
        {
            int r = (int)rating;
            switch (r)
            {
                case 1:
                    return "<span class=\"color-low\">Bottom 10%</span>";
                case 2:
                    return "<span class=\"color-low\">Bottom 20%</span>";
                case 3:
                    return "<span class=\"color-low\">Bottom 30%</span>";
                case 4:
                    return "<span class=\"color-med\">Bottom 40%</span>";
                case 5:
                    return "<span class=\"color-med\">Bottom 50%</span>";
                case 6:
                    return "<span class=\"color-med\">Top 50%</span>";
                case 7:
                    return "<span class=\"color-med\">Top 40%</span>";
                case 8:
                    return "<span class=\"color-high\">Top 30%</span>";
                case 9:
                    return "<span class=\"color-high\">Top 20%</span>";
                case 10:
                    return "<span class=\"color-high\">Top 10%</span>";
            }
            return "";
        }

        public static SearchGroupType GetGroupType(SearchItemType type)
        {
            switch (type)
            {
                case SearchItemType.User:
                    return SearchGroupType.User;
                case SearchItemType.AnswerTags:
                    return SearchGroupType.Answer;
                case SearchItemType.AnswerContent:
                    return SearchGroupType.Answer;
                case SearchItemType.QuestionTags:
                    return SearchGroupType.Question;
                case SearchItemType.QuestionContent:
                    return SearchGroupType.Question;
            }
            throw new System.NotImplementedException();
        }

        public static string SearchItemTypeString(SearchGroupType type)
        {
            switch (type)
            {
                case SearchGroupType.User:
                    return "Users";
                case SearchGroupType.Answer:
                    return "Answers";
                case SearchGroupType.Question:
                    return "Questions";
            }
            return "";
        }

        public static string UserString(Notification notification)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<b>");
            sb.Append(notification.SourceUser1);
            sb.Append("</b>");
            if (!String.IsNullOrEmpty(notification.SourceUser2) && String.IsNullOrEmpty(notification.SourceUser3))
            {
                sb.Append(" and <b>");
                sb.Append(notification.SourceUser2);
                sb.Append("</b>");
            }
            else if (!String.IsNullOrEmpty(notification.SourceUser3) && (!notification.OtherUserCount.HasValue))
            {
                sb.Append(", <b>");
                sb.Append(notification.SourceUser2);
                sb.Append("</b> and <b>");
                sb.Append(notification.SourceUser3);
                sb.Append("</b>");
            }
            else if (!String.IsNullOrEmpty(notification.SourceUser3) && notification.OtherUserCount.HasValue)
            {
                sb.Append(", <b>");
                sb.Append(notification.SourceUser2);
                sb.Append("</b>, <b>");
                sb.Append(notification.SourceUser3);
                sb.Append("</b> and <b>");
                sb.Append(notification.OtherUserCount);
                sb.Append(" others</b>");
            }
            return sb.ToString();
        }

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