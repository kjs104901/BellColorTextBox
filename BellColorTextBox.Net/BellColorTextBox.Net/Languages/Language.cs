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
        MultilineString,
        String,
        FoldingStart,
        FoldingEnd
    }

    private readonly List<TokenType> _tokenTypes = new()
    {
        TokenType.LineComment,
        TokenType.BlockCommentStart,
        TokenType.BlockCommentEnd,
        TokenType.MultilineString,
        TokenType.String,
        TokenType.FoldingStart,
        TokenType.FoldingEnd
    };
    
    // Don't use dictionary for performance
    internal readonly List<string> LineComments = new();
    internal readonly List<string> BlockCommentStarts = new();
    internal readonly List<string> BlockCommentEnds = new();
    internal readonly List<string> MultilineStrings = new();
    internal readonly List<string> Strings = new();
    internal readonly List<string> FoldingStarts = new();
    internal readonly List<string> FoldingEnds = new();
    private List<string> GetTokenStringList(TokenType tokenType)
    {
        // Don't use default for performance
        switch (tokenType)
        {
            case TokenType.LineComment:
                return LineComments;
            case TokenType.BlockCommentStart:
                return BlockCommentStarts;
            case TokenType.BlockCommentEnd:
                return BlockCommentEnds;
            case TokenType.MultilineString:
                return MultilineStrings;
            case TokenType.String:
                return Strings;
            case TokenType.FoldingStart:
                return FoldingStarts;
            case TokenType.FoldingEnd:
                return FoldingEnds;
        }
        throw new ArgumentOutOfRangeException();
    }
    
    internal readonly Dictionary<Regex, Theme.Token> PatternsStyle = new();
    
    internal readonly List<string> MultilinePrefixes = new();
    internal readonly List<string> MultilinePostfixes = new();

    public void AddLineComment(string str)
    {
        LineComments.Add(str);
    }

    public void AddBlockComment(string startStr, string endStr)
    {
        BlockCommentStarts.Add(startStr);
        BlockCommentEnds.Add(endStr);
    }
    
    public void AddMultilineString(string str)
    {
        MultilineStrings.Add(str);
    }
    
    public void AddString(string str)
    {
        Strings.Add(str);
    }
    
    public void AddMultilinePrefix(string str)
    {
        MultilinePrefixes.Add(str);
    }
    
    public void AddFolding(string startStr, string endStr)
    {
        FoldingStarts.Add(startStr);
        FoldingEnds.Add(endStr);
    }
    
    public void AddMultilinePostfix(string str)
    {
        MultilinePostfixes.Add(str);
    }
    
    public void AddPattern(string regex, Theme.Token token, RegexOptions regexOptions = RegexOptions.None)
    {
        PatternsStyle.Add(new Regex(regex, regexOptions | RegexOptions.Compiled), token);
    }
    
    internal bool FindMatching(string source, int charIndex, out Token matchedToken)
    {
        matchedToken = new Token();

        foreach (TokenType tokenType in _tokenTypes)
        {
            List<string> tokenStringList = GetTokenStringList(tokenType);
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
                    
                    matchedToken.IsMultiline = false;

                    if (TokenType.String == tokenType)
                    {
                        foreach (string prefix in MultilinePrefixes)
                        {
                            bool isSamePrefix = true;
                            for (int i = 0; i < prefix.Length; i++)
                            {
                                int prefixIndex = charIndex - prefix.Length + i;
                                if (prefixIndex < 0 || source[prefixIndex] != prefix[i])
                                {
                                    isSamePrefix = false;
                                    break;
                                }
                            }

                            if (isSamePrefix)
                            {
                                matchedToken.IsMultiline = true;
                                matchedToken.CharIndex -= prefix.Length;
                                break;
                            }
                        }
                    }
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
        
        internal bool IsMultiline;

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