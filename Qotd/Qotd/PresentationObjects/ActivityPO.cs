using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.PresentationObjects
{
    public class ActivityPO
    {
        public Activity Activity { get; set; }

        public string UserDisplayName { get; set; }

        public string UserProfileImageUrl { get; set; }
    }
}
