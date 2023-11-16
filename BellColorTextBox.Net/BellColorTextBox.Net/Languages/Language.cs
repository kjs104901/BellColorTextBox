using System.Text.RegularExpressions;
using Bell.Data;
using Bell.Inputs;
using Bell.Themes;

namespace Bell.Languages;

public partial class Language
{
    internal enum TokenType
    {
        LineComment,
        BlockCommentStart,
        BlockCommentEnd,
        MultilineStringStart,
        MultilineStringEnd,
        String,
        FoldingStart,
        FoldingEnd
    }
    internal readonly Dictionary<TokenType, List<string>> Tokens = new()
    {
        { TokenType.LineComment, new() },
        { TokenType.BlockCommentStart, new() },
        { TokenType.BlockCommentEnd, new() },
        { TokenType.MultilineStringStart, new() },
        { TokenType.MultilineStringEnd, new() },
        { TokenType.String, new() },
        { TokenType.FoldingStart, new() },
        { TokenType.FoldingEnd, new() },
    };

    internal Dictionary<Regex, Theme.Token> PatternsStyle = new();
    
    public void AddLineComment(string str)
    {
        Tokens[TokenType.LineComment].Add(str);
    }

    public void AddBlockComment(string startStr, string endStr)
    {
        Tokens[TokenType.BlockCommentStart].Add(startStr);
        Tokens[TokenType.BlockCommentEnd].Add(endStr);
    }
    
    public void AddString(string str)
    {
        Tokens[TokenType.String].Add(str);
    }
    
    public void AddMultilineString(string startStr, string endStr)
    {
        Tokens[TokenType.MultilineStringStart].Add(startStr);
        Tokens[TokenType.MultilineStringEnd].Add(endStr);
    }
    
    public void AddFolding(string startStr, string endStr)
    {
        Tokens[TokenType.FoldingStart].Add(startStr);
        Tokens[TokenType.FoldingEnd].Add(endStr);
    }
    
    public void AddPattern(string regex, Theme.Token token, RegexOptions regexOptions = RegexOptions.None)
    {
        PatternsStyle.Add(new Regex(regex, regexOptions | RegexOptions.Compiled), token);
    }
    
    internal bool FindMatching(string source, int charIndex, out Token matchedToken)
    {
        matchedToken = new Token();

        foreach (TokenType tokenType in Tokens.Keys.OrderBy(k => k))
        {
            List<string> tokenStringList = Tokens[tokenType];
            for (int tokenIndex = 0; tokenIndex < tokenStringList.Count; tokenIndex++)
            {
                string tokenString = tokenStringList[tokenIndex];
                
                if (charIndex < 0 || charIndex + tokenString.Length > source.Length)
                    continue;

                bool isSame = true;
                for (int i = 0; i < tokenString.Length; i++)
                {
                    if (source[charIndex + i] != tokenString[i])
                    {
                        isSame = false;
                        break;
                    }
                }

                if (isSame)
                {
                    matchedToken.Type = tokenType;
                    matchedToken.TokenIndex = tokenIndex;
                    matchedToken.TokenString = tokenString;
                    
                    matchedToken.CharIndex = charIndex;
                    
                    return true;
                }
            }
        }
        return false;
    }
    
    internal struct Token : IEquatable<Token>
    {
        internal TokenType Type;
        internal int TokenIndex;
        internal string TokenString;
        
        internal int CharIndex;

        public bool Equals(Token other)
        {
            return Type == other.Type && TokenIndex == other.TokenIndex && CharIndex == other.CharIndex;
        }

        public override bool Equals(object? obj)
        {
            return obj is Token other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, TokenIndex, CharIndex);
        }
    }
}