using BrokenLinkChecker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker.LinkChecker.Core.Models
{
    public class LinkInfo : IEqualityComparer<LinkInfo>
    {
        public string PageName { get; set; }
        public string LinkUrl { get; set; }
        public bool Broken { get; set; }
        public string LinkText { get; set; }

         
        public bool Equals(LinkInfo x, LinkInfo y)
            => (x.LinkUrl ) ==  (y.LinkUrl );

        public int GetHashCode(LinkInfo linkInfo) =>
           linkInfo?.LinkUrl.GetHashCode() ?? throw new ArgumentNullException(nameof(linkInfo));
    }
}
