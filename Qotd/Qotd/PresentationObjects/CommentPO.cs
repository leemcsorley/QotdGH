using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;
using Qotd.Utils;

namespace Qotd.PresentationObjects
{
    public class CommentPO
    {
        private string _userProfileImageUrl;

        public Comment Comment { get; set; }

        public bool HasUserLiked { get; set; }

        public string UserDisplayName { get; set; }

        public string UserProfileImageUrl
        {
            get
            {
                if (_userProfileImageUrl.StartsWith("\\"))
                    return Config.UploadImagesUrl + _userProfileImageUrl;
                return _userProfileImageUrl;
            }
            set
            {
                _userProfileImageUrl = value;
            }
        }
    }
}
