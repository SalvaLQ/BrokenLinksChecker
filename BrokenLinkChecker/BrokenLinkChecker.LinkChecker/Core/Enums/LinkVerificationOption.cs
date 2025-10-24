using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker.LinkChecker.Core.Enums
{
    public enum LinkVerificationOption
    {
        CheckAllNoFollowExternals,
        CheckInternalOnly,
        CheckCurrentPageOnly
    }
}
