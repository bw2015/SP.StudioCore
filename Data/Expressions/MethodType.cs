using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Data.Expressions
{
    [EnumConfig(ignore: true)]
    public enum MethodType
    {
        None,
        StartsWith,
        Contains,
        EndsWith,
        OrderByDescending,
        OrderBy,
        Where,
        Select,
        Any,
        Count,
        FirstOrDefault,
        Take,
        Skip
    }
}
