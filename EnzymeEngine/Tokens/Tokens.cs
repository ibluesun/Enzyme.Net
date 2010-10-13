using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ParticleLexer;
using ParticleLexer.StandardTokens;

namespace Enzyme.Tokens
{
    [TokenPattern(RegexPattern = "If", ExactWord = true)]
    public class IfWordToken : TokenClass
    {
    }

    [TokenPattern(RegexPattern = "Then", ExactWord = true)]
    public class ThenWordToken : TokenClass
    {
    }

    [TokenPattern(RegexPattern = "Else", ExactWord = true)]
    public class ElseWordToken : TokenClass
    {
    }

    [TokenPattern(RegexPattern = "!=", ExactWord = true)]
    public class InEqualityToken : TokenClass
    {
    }

    [TokenPattern(RegexPattern = "==", ExactWord = true)]
    public class EqualityToken : TokenClass
    {
    }

    /// <summary>
    /// Dollar Sign followed by word token. $x or $y  $ROI 
    /// </summary>
    public class SymbolicToken : TokenClass
    {
    }


    /// <summary>
    /// Mathches \"    
    /// </summary>
    [TokenPattern(RegexPattern = @"\\""", ExactWord = true)]
    public class QuotationMarkEscapeToken : TokenClass
    {

    }

    /// <summary>
    /// Text between two single qutation.
    /// </summary>
    public class TextToken : TokenClass
    {
    }




    public static class TokenExtensions
    {

        /// <summary>
        /// Get the sub token untill closing token type encountered.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="startIndex"></param>
        /// <param name="closingTokenClassType"></param>
        /// <returns></returns>
        public static Token SubTokens(this Token token, int startIndex, Type closingTokenClassType, out int closingIndex)
        {
            int idx = startIndex;
            Token result = new Token();

            while (idx < token.Count && token[idx].TokenClassType != closingTokenClassType)
            {
                result.AppendSubToken(token[idx]);
                
                idx++;
            }

            closingIndex = idx;

            return result;
        }

        /// <summary>
        /// returns the value of tokens starting from specific token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string SubTokensValue(this Token token, int startIndex)
        {
            int idx = startIndex;
            string total = string.Empty;
            while (idx < token.Count)
            {
                total += token[idx].TokenValue;
                idx++;
            }
            return total;

        }


        /// <summary>
        /// Get inner tokens from leftIndex to the rightIndex 
        /// --->   tokens &lt; -- 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="leftIndex"></param>
        /// <param name="rightIndex"></param>
        /// <returns>Return new token with sub tokens trimmed</returns>
        public static Token TrimTokens(this Token token, int leftIndex, int rightIndex)
        {
            int count = token.Count;


            Token rtk = new Token();
            for (int b = leftIndex; b < count - rightIndex; b++)
            {
                rtk.AppendSubToken(token[b]);
            }

            return rtk;

        }




        /// <summary>
        /// Extend Tokens from Left and Right and Fuse them into one Token with specific token class
        /// </summary>
        /// <param name="token"></param>
        /// <param name="leftText"></param>
        /// <param name="rightText"></param>
        /// <returns>Return token with sub tokens extended and fused</returns>
        public static Token FuseTokens<FusedTokenClass>(this Token token, string leftText, string rightText)
            where FusedTokenClass : TokenClass
        {
            int count = token.Count;

            Token rtk = new Token();

            foreach (var t in Token.ParseText(leftText))
            {
                rtk.AppendSubToken(t);
            }

            for (int b = 0; b < count; b++)
            {
                rtk.AppendSubToken(token[b]);
            }
            foreach (var t in Token.ParseText(rightText))
            {
                rtk.AppendSubToken(t);
            }

            rtk.TokenClassType = typeof(FusedTokenClass);

            Token tk = new Token();
            tk.AppendSubToken(rtk);

            return tk;
        }





        public static Token DiscoverQsTextTokens(this Token tokens)
        {

            // merge \" to be one charachter after this

            tokens = tokens.MergeTokens<QuotationMarkEscapeToken>();

            Token root = new Token();

            Token runner = root;

            //add every token until you encounter '


            int ix = 0;
            bool TextMode = false;
            while (ix < tokens.Count)
            {
                if (tokens[ix].TokenClassType == typeof(QuotationMarkToken))
                {
                    TextMode = !TextMode;

                    if (TextMode)
                    {
                        //true create the token
                        runner = new Token();
                        runner.TokenClassType = typeof(TextToken);
                        root.AppendSubToken(runner);

                        runner.AppendSubToken(tokens[ix]);

                    }
                    else
                    {
                        //false: return to root tokens
                        runner.AppendSubToken(tokens[ix]);

                        runner = root;
                    }
                }
                else
                {
                    runner.AppendSubToken(tokens[ix]);
                }


                ix++;

            }


            return root;
        }
    }

}
