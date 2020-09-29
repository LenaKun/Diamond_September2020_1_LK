using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
    public class NoProxiesCcContext:ccEntities
    {
        public NoProxiesCcContext()
            : base()
        {
            ContextOptions.LazyLoadingEnabled = false;
            ContextOptions.ProxyCreationEnabled = false;
        }
    }
}
