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
        public Comment Comment { get; set; }

        public bool HasUserLiked { get; set; }

        public UserPO User { get; set; }
    }
}
