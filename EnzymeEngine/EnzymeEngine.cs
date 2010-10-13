using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParticleLexer.StandardTokens;
using Enzyme.Tokens;
using ParticleLexer;
using System.Linq.Expressions;
using System.Reflection;

namespace Enzyme
{
    public class EnzymeEngine
    {

        /// <summary>
        /// This is a private context for the internal variables.
        /// </summary>
        readonly EnzymeDynamicContext _EnginePrivateContext = new EnzymeDynamicContext();

        readonly IEnzymeContext _CurrentFieldsContext;

        public IEnzymeContext CurrentFieldsContext
        {
            get
            {
                return _CurrentFieldsContext;
            }
        }

        public EnzymeEngine()
        {
        }

        public EnzymeEngine(IEnzymeContext fieldsProvider)
        {
            _CurrentFieldsContext = fieldsProvider;
        }

        /// <summary>
        /// Convert the text into tokens.
        /// </summary>
        /// <param name="codeLine"></param>
        /// <returns></returns>
        public Token ParseToTokens(string codeLine)
        {
            var tokens = Token.ParseText(codeLine);

            tokens = tokens.DiscoverQsTextTokens();

            // assemble all spaces
            tokens = tokens.MergeTokens<MultipleSpaceToken>();

            tokens = tokens.MergeMultipleWordTokens(
                typeof(EqualityToken),
                typeof(InEqualityToken),
                typeof(LessThanOrEqualToken),
                typeof(GreaterThanOrEqualToken)
                );

            tokens = tokens.MergeTokens<WordToken>();                 //Discover words

            tokens = tokens.MergeMultipleWordTokens(
                    typeof(IfWordToken),
                    typeof(ThenWordToken),
                    typeof(ElseWordToken),
                    typeof(AndWordToken),
                    typeof(OrWordToken)
                );

            // merge the $ + Word into Symbolic and get the symbolic variables.
            tokens = tokens.MergeSequenceTokens<SymbolicToken>(typeof(DollarToken), typeof(WordToken));

            tokens = tokens.MergeTokens<NumberToken>();               //discover the numbers

            // remove spaces before making grouping 
            // because we don't need the spaces here :)
            tokens = tokens.RemoveSpaceTokens();                     //remove all spaces

            tokens = tokens.MergeTokensInGroups(
                new ParenthesisGroupToken()                          //  group (--()-) parenthesis
                );

            tokens = tokens.DiscoverCalls(
                new CallTokenClass[] { new ParenthesisCallToken() }
                );

            return tokens;
        }


        /// <summary>
        /// The primary function that make the expression that will be compiled then evaluated.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public Expression FormTheExpression(Token tokens)
        {
            Expression quantityExpression = null;
            ExprOp eop = null;

            ExprOp FirstEop = null;

            int ix = 0;                 //this is the index in the discovered tokens
            while (ix < tokens.Count)
            {
                string q = tokens[ix].TokenValue;
                string op = ix + 1 < tokens.Count ? tokens[ix + 1].TokenValue : string.Empty;

                if (q == "+" || q == "-")
                {
                    // unary prefix operator.

                    //consume another token for number

                    if (q == "+")
                    {
                        //q = tokens[ix].TokenValue;
                        quantityExpression = Expression.Constant(1.0);
                    }
                    else
                    {
                        quantityExpression = Expression.Constant(-1.0);
                    }

                    op = "_h*";
                    ix--;
                    goto ExpressionCompleted;

                }

                if (tokens[ix].TokenClassType == typeof(IfWordToken))
                {
                    // this is an If Word so we need to take all the tokens after it until we find THEN token
                    // and form expression.
                    int ifClosingIndex;
                    Token IfBodyToken = tokens.SubTokens(ix + 1, typeof(ThenWordToken), out ifClosingIndex);

                    int thenClosingIndex;
                    Token ThenBodyToken = null;

                    int elseClosingIndex;
                    Token ElseBodyToken = null;

                    ix = ifClosingIndex;

                    if (ix < tokens.Count)
                    {
                        if (tokens[ifClosingIndex].TokenClassType == typeof(ThenWordToken))
                        {
                            // True part evaluation
                            ThenBodyToken = tokens.SubTokens(ix + 1, typeof(ElseWordToken), out thenClosingIndex);
                            ix = thenClosingIndex;

                            if (ix < tokens.Count)
                            {
                                if (tokens[thenClosingIndex].TokenClassType == typeof(ElseWordToken))
                                {
                                    // True part evaluation
                                    ElseBodyToken = tokens.SubTokens(ix + 1, typeof(ElseWordToken), out elseClosingIndex);

                                    ix = elseClosingIndex;
                                }
                            }
                        }
                    }


                    Expression TestPart = FormTheExpression(IfBodyToken);
                    Expression TruePart;
                    Expression FalsePart;

                    if (ThenBodyToken != null)
                        TruePart = Expression.Convert(FormTheExpression(ThenBodyToken), typeof(object));
                    else
                        TruePart = Expression.Constant(true, typeof(object));

                    if (ElseBodyToken != null)
                        FalsePart = Expression.Convert(FormTheExpression(ElseBodyToken), typeof(object));
                    else
                        FalsePart = Expression.Constant(false, typeof(object)); 

                    quantityExpression = Expression.Condition(TestPart, TruePart, FalsePart);
                    
                }
                else if (tokens[ix].TokenClassType == typeof(ParenthesisCallToken))
                {
                    quantityExpression = FunctionCallExpression(tokens[ix]);
                }
                else if (tokens[ix].TokenClassType == typeof(ParenthesisGroupToken))
                {
                    // take the inner tokens and send it to be parsed again.
                    quantityExpression = FormTheExpression(tokens[ix].RemoveSpaceTokens().TrimTokens(1, 1));

                }
                else if (tokens[ix].TokenClassType == typeof(SymbolicToken))
                {
                    // 
                    quantityExpression = ExternalFieldHandleExpression(tokens[ix]);
                }
                else if (tokens[ix].TokenClassType == typeof(NumberToken))
                {
                    // 
                    quantityExpression = Expression.Constant(double.Parse(tokens[ix].TokenValue));
                }
                else if (tokens[ix].TokenClassType == typeof(WordToken))
                {
                    // we are taking about internal variable storage
                    quantityExpression = InternalFieldHandleExpression(tokens[ix]);
                }
                else if (tokens[ix].TokenClassType == typeof(TextToken))
                {
                    quantityExpression = Expression.Constant(tokens[ix].TrimTokens(1, 1).TokenValue);
                }
                else
                {
                    throw new UnRecognizedException("Token: " + tokens[ix].TokenValue + " was not identified");
                }

        ExpressionCompleted:
                if (eop == null)
                {
                    //firs time creation
                    FirstEop = new ExprOp();

                    eop = FirstEop;
                }
                else
                {
                    //use the next object to be eop.
                    eop.Next = new ExprOp();
                    eop = eop.Next;
                }

                eop.Operation = op;
                eop.ValueExpression = quantityExpression;

                ix += 2;

            }


            //then form the calculation expression
            return ConstructExpression(FirstEop);

        }

        /// <summary>
        /// Generate a function call expression from the provider
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private Expression FunctionCallExpression(Token token)
        {
            Token functionNameToken = token[0];
            Token args = token[1];

            List<Expression> Arguments = new List<Expression>();

            //discover parameters
            for (int ai = 1; ai < args.Count - 1; ai++)
            {
                if (args[ai].TokenClassType == typeof(ParameterToken))
                {
                    Expression x = FormTheExpression(args[ai]);
                    if (x is ConstantExpression)
                    {
                        var y = (ConstantExpression)x;
                        if (y.Type == typeof(FieldHandle))
                        {
                            Arguments.Add(Expression.Constant(((FieldHandle)((ConstantExpression)x).Value).GetValue(), typeof(object)));
                        }
                        else
                            Arguments.Add(x);
                    }
                    else
                        Arguments.Add(x);
                }
            }

            Type t = CurrentFieldsContext.GetType();
            var mi = t.GetMethod(functionNameToken.TokenValue, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);



            Expression v =Expression.Call(
                Expression.Constant(CurrentFieldsContext),
                mi,
                Arguments.ToArray());


            return v;
        }

        public Expression GetEvaluationExpression(string codeLine)
        {
            Token tokens = ParseToTokens(codeLine);
            return FormTheExpression(tokens);
        }

        public object Evaluate(string codeLine)
        {
            Expression final = GetEvaluationExpression(codeLine);
            Expression ResultExpression = Expression.Convert(final, typeof(object));

            // Construct Lambda function which return one object.
            Expression<Func<object>> cq = Expression.Lambda<Func<object>>(ResultExpression);

            // compile the function
            Func<object> aqf = cq.Compile();

            // execute the function
            object result = aqf();

            // return the result
            return result;

        }


        #region Language Expressions

        /// <summary>
        /// Generate Expression with the field value.
        /// </summary>
        /// <param name="fieldToken"></param>
        /// <returns></returns>
        Expression ExternalFieldHandleExpression(Token fieldToken)
        {
            FieldHandle handle = CurrentFieldsContext.GetFieldHandle(fieldToken[1].TokenValue);
            return Expression.Constant(handle);
        }

        Expression InternalFieldHandleExpression(Token fieldToken)
        {
            FieldHandle handle = _EnginePrivateContext.GetFieldHandle(fieldToken.TokenValue);
            return Expression.Constant(handle);
        }


        #endregion


        #region Arithmatic Expressions

        /// <summary>
        /// Just take the left and right expression with the operator and make arithmatic expression.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="op"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Expression ArithExpression(ExprOp eop, out short skip)
        {
            Expression NativeLeft = eop.ValueExpression;
            Expression left;
            if (NativeLeft.Type == typeof(FieldHandle))
            {
                //left = Expression.Constant(CurrentFieldsContext.GetFieldValue(((FieldHandle)((ConstantExpression)NativeLeft).Value).Name));
                left = Expression.Constant(((FieldHandle)((ConstantExpression)NativeLeft).Value).GetValue());
            }
            else
                left = NativeLeft;

            string op = eop.Operation;

            Expression NativeRight = eop.Next.ValueExpression;
            Expression right;

            if (NativeRight.Type == typeof(FieldHandle))
            {
                //right = Expression.Constant(CurrentFieldsContext.GetFieldValue(((FieldHandle)((ConstantExpression)NativeRight).Value).Name));
                right = Expression.Constant(((FieldHandle)((ConstantExpression)NativeRight).Value).GetValue());
            }
            else
                right = NativeRight;

            // also try to convert the right type to the left type if there is a suitable conversion.
            if (left.Type.IsPrimitive)
            {
                // left is primitive
            }

            skip = 1;

            Type aqType = typeof(object);

            if (op == "_h*") return Expression.Multiply(left, right);

            if (op == "^") return Expression.Power(left, right, aqType.GetMethod("Pow"));

            if (op == "*") return Expression.Multiply(left, right);

            if (op == "/") return Expression.Divide(left, right);
            if (op == "%") return Expression.Modulo(left, right);
            if (op == "+") return Expression.Add(left, right);
            if (op == "-") return Expression.Subtract(left, right);


            if (op == "<") return Expression.LessThan(left, right);
            if (op == ">") return Expression.GreaterThan(left, right);
            if (op == "<=") return Expression.LessThanOrEqual(left, right);
            if (op == ">=") return Expression.GreaterThanOrEqual(left, right);

            if (op == "==") return Expression.Equal(left, right);
            if (op == "!=") return Expression.NotEqual(left, right);

            if (op.Equals("and", StringComparison.OrdinalIgnoreCase))
                return Expression.And(left, right);

            if (op.Equals("or", StringComparison.OrdinalIgnoreCase))
                return Expression.Or(left, right);

            if (op == "=")
            {
                //MethodInfo mi = CurrentFieldsContext.GetType().GetMethod("SetField");
                MethodInfo mi = typeof(FieldHandle).GetMethod("SetValue");

                //return Expression.Call(Expression.Constant(CurrentFieldsContext), mi, NativeLeft, Expression.Convert(right, typeof(object)));
                return Expression.Call(Expression.Constant((FieldHandle)((ConstantExpression)NativeLeft).Value), mi, Expression.Convert(right, typeof(object)));
            }
          

            throw new NotSupportedException("Not Supported Operator '" + op + "'");
        }


        /// <summary>
        /// Takes the linked list of formed expressions and construct the arithmatic expressions based
        /// on the priority of calculation operators.
        /// Passes:
        /// 1- { "^" }
        /// 2- { "*", "/", "%" /*modulus*/ }
        /// 3- { "+", "-" }
        /// </summary>
        /// <param name="FirstEop"></param>
        /// <returns></returns>
        private Expression ConstructExpression(ExprOp FirstEop)
        {
            //Treat operators as groups
            //  means * and /  are in the same pass
            //  + and - are in the same pass

            // passes depends on priorities of operators.

            // Internal Higher Priority Group
            string[] HigherGroup = { "_h*" /* Higher Multiplication priority used internally in 
                                           * the case of -4  or 5^-3
                                             To be treated like -1_h*4   or 5^-1_h*4
                                           */};

            string[] Group = { "^"    /* Power for normal product '*' */ };


            string[] Group1 = { "*"   /* normal multiplication */, 
                                "/"   /* normal division */, 
                                "%"   /* modulus */ };

            string[] Group2 = { "+", "-" };


            string[] RelationalGroup = { "<", ">", "<=", ">=" };
            string[] EqualityGroup = { "==", "!=" };
            string[] AndGroup = { "and" };
            string[] OrGroup = { "or" };

            string[] AssignmentGroup = { "=" };


            /// Operator Groups Ordered by Priorities.
            string[][] OperatorGroups = { HigherGroup, Group, Group1, Group2, RelationalGroup, EqualityGroup, AndGroup, OrGroup, AssignmentGroup };

            foreach (var opg in OperatorGroups)
            {
                ExprOp eop = FirstEop;

                //Pass for '[op]' and merge it  but from top to child :)  {forward)
                while (eop.Next != null)
                {
                    //if the operator in node found in the opg (current operator group) then execute the logic

                    if (opg.Count(c => c.Equals(eop.Operation, StringComparison.OrdinalIgnoreCase)) > 0)
                    {
                        short skip;
                        eop.ValueExpression = ArithExpression(eop, out skip);

                        //drop eop.Next
                        if (eop.Next.Next != null)
                        {
                            while (skip > 0)
                            {
                                eop.Operation = eop.Next.Operation;

                                eop.Next = eop.Next.Next;

                                skip--;
                            }
                        }
                        else
                        {
                            //no more nodes exit the loop

                            eop.Next = null;      //last item were processed.
                            eop.Operation = string.Empty;
                        }
                    }
                    else
                    {
                        eop = eop.Next;
                    }
                }
            }

            return FirstEop.ValueExpression;
        }


        #endregion

    }
}
