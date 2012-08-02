using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qotd.PresentationObjects;

namespace QotdMvc.Models
{
    public class UserBoxes
    {
        public UserPO User { get; set; }

        public bool Overall { get; set; }
    }
}