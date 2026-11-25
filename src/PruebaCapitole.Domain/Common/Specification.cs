using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PruebaCapitole.Domain.Common
{
    public abstract class Specification<T>
    {
        public abstract Expression<Func<T, bool>> ToExpression();

    }
}
