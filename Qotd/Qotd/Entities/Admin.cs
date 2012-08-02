using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class Admin : EntityBase
    {
        public virtual Guid LatestSiteStatisticsId { get; set; }

        public virtual SiteStatistics LatestSiteStatistics { get; set; }
    }
}
