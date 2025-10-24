using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker.Domain
{
    public class PageLink
    {
        public string PageName { get; set; }
        public string LinkUrl { get; set; }
        public bool Broken { get; set; }
        public string LinkText { get; set; }
        
    }
}
