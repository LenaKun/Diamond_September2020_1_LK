using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public class IdNamePair<TId, TName>
    {
        public TId Id { get; set; }
        public TName Name { get; set; }

    }
}
