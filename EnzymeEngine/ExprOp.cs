using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Enzyme
{
    class ExprOp
    {
        public Expression ValueExpression { get; set; }
        public string Operation { get; set; }
        public ExprOp Next { get; set; }
    }
}
