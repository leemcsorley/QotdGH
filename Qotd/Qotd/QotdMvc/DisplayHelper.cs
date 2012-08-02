using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qotd.Entities;
using Qotd.Utils;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace QotdMvc
{
    public enum SingleType
    {
        Answer,
        Question
    }

    public static class DisplayHelper
    {
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