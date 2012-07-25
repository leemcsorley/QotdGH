using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.PresentationObjects
{
    public class CommentPO
    {
        public Comment Comment { get; set; }

        public bool HasUserLiked { get; set; }

        public string UserDisplayName { get; set; }

        public string UserProfileImageUrl { get; set; }
    }
}
