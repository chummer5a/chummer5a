using System;
using System.Linq.Expressions;

namespace Chummer.Backend.Options
{
    public class OptionConstaint
    {
        public LambdaExpression Ex { get; }

        protected OptionConstaint(LambdaExpression ex)
        {
            Ex = ex;
        }
    }

    public class OptionConstaint<T> : OptionConstaint
    {
        public OptionConstaint(Expression<Func<T, bool>> constaint) : base(constaint)
        {

        }
    }
}