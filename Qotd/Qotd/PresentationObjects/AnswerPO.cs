using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;
using Qotd.Utils;

namespace Qotd.PresentationObjects
{
    public class AnswerPO
    {
        private string _userProfileImageUrl;

        public Answer Answer { get; set; }

        public bool HasUserVoted { get; set; }

        public CommentPO[] Comments { get; set; }

        public UserPO User { get; set; }

        public string[] Tags
        {
            get { return Answer.TagValues.Split(' '); }
        }

        //public string UserDisplayName { get; set; }

        //public string UserProfileImageUrl
        //{
        //    get
        //    {
        //        if (_userProfileImageUrl.StartsWith("\\"))
        //            return Config.UploadImagesUrl + _userProfileImageUrl;
        //        return _userProfileImageUrl;
        //    }
        //    set
        //    {
        //        _userProfileImageUrl = value;
        //    }
        //}
    }
}
