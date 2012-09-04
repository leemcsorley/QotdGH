using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QotdMvc.Models
{
    public class Stars
    {
        public string Heading { get; set; }

        public int NumStars { get; set; }

        public int Rating { get; set; }

        public bool IncludeHalf { get; set; }

        public bool Invert { get; set; }

        public string GetIconClass(int star)
        {
            string cls;
            if (IncludeHalf)
            {
                if (Rating % 2 == 1 && star == (Rating + 1) / 2)
                {
                    cls = "icon-star-empty icon-star-half star-half";
                }
                else 
                {
                    if (Rating / 2 >= star)
                        cls = "icon-star";
                    else
                        cls = "icon-star-empty";
                }
            }
            else
            {
                if (Rating >= star)
                    cls = "icon-star";
                else
                    cls = "icon-star-empty";
            }

            if ((IncludeHalf && (Rating / 2 <= NumStars / 5)) ||
                (!IncludeHalf && (Rating <= NumStars / 5)))
            {
                cls += " stars-low";
            }
            else if ((IncludeHalf && (Rating / 2 >= NumStars - 1)) ||
                (!IncludeHalf && (Rating >= NumStars - 1)))
            {
                cls += " stars-high";
            }
            return cls;
        }
    }
}