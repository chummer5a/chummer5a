using System;
using System.Linq.Expressions;

namespace Chummer.Backend.Options
{
    public class OptionConstraint
    {
        public LambdaExpression Ex { get; }

        protected OptionConstraint(LambdaExpression ex)
        {
            Ex = ex;
        }
    }

    public class OptionConstraint<T> : OptionConstraint
    {
        public OptionConstraint(Expression<Func<T, bool>> constaint) : base(constaint)
        {

        }
    }
}