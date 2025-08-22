using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.Application.Common
{
    public sealed record DepartmentFilter(string? NameContains = null);
}
