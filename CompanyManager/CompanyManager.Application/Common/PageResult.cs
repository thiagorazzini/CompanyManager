using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.Application.Common
{
    public sealed record PageResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
    {
        public bool HasNext => Page * PageSize < Total;
        public bool HasPrev => Page > 1;
    }
}
