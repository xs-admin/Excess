using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.VS
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    class Scanner : IScanner
    {
        string _source;
        IEnumerator<SyntaxToken> _tokens;
        IEnumerable<string> _keywords;

        public Scanner(IEnumerable<string> keywords)
        {
            _keywords = keywords;
        }

        public IEnumerable<string> Keywords
        {
            get { return _keywords; }
            set { _keywords = value; }
        }

        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            if (_tokens == null)
                return false;

            if (!_tokens.MoveNext())
                return false;

            var token = _tokens.Current;
            tokenInfo.Type = TokenType.Unknown;
            tokenInfo.Color = TokenColor.Text;
            tokenInfo.StartIndex = token.Span.Start;
            tokenInfo.EndIndex = token.Span.End;

            switch (token.Kind())
            {
                case SyntaxKind.TildeToken:
                case SyntaxKind.ExclamationToken:
                case SyntaxKind.DollarToken:
                case SyntaxKind.PercentToken:
                case SyntaxKind.CaretToken:
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.BarToken:
                case SyntaxKind.BackslashToken:
                case SyntaxKind.LessThanToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.QuestionToken:
                case SyntaxKind.HashToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.SlashGreaterThanToken:
                case SyntaxKind.LessThanSlashToken:
                case SyntaxKind.BarBarToken:
                case SyntaxKind.AmpersandAmpersandToken:
                case SyntaxKind.MinusMinusToken:
                case SyntaxKind.PlusPlusToken:
                case SyntaxKind.QuestionQuestionToken:
                case SyntaxKind.MinusGreaterThanToken:
                case SyntaxKind.ExclamationEqualsToken:
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.EqualsGreaterThanToken:
                case SyntaxKind.LessThanEqualsToken:
                case SyntaxKind.LessThanLessThanToken:
                case SyntaxKind.LessThanLessThanEqualsToken:
                case SyntaxKind.GreaterThanEqualsToken:
                case SyntaxKind.GreaterThanGreaterThanToken:
                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                case SyntaxKind.SlashEqualsToken:
                case SyntaxKind.AsteriskEqualsToken:
                case SyntaxKind.BarEqualsToken:
                case SyntaxKind.AmpersandEqualsToken:
                case SyntaxKind.PlusEqualsToken:
                case SyntaxKind.MinusEqualsToken:
                case SyntaxKind.CaretEqualsToken:
                case SyntaxKind.PercentEqualsToken:
                    tokenInfo.Type = TokenType.Operator;
                    break;

                case SyntaxKind.OpenParenToken:
                case SyntaxKind.CloseParenToken:
                case SyntaxKind.OpenBraceToken:
                case SyntaxKind.CloseBraceToken:
                case SyntaxKind.OpenBracketToken:
                case SyntaxKind.CloseBracketToken:
                    tokenInfo.Type = TokenType.Delimiter;
                    break;

                case SyntaxKind.BoolKeyword:
                case SyntaxKind.ByteKeyword:
                case SyntaxKind.SByteKeyword:
                case SyntaxKind.ShortKeyword:
                case SyntaxKind.UShortKeyword:
                case SyntaxKind.IntKeyword:
                case SyntaxKind.UIntKeyword:
                case SyntaxKind.LongKeyword:
                case SyntaxKind.ULongKeyword:
                case SyntaxKind.DoubleKeyword:
                case SyntaxKind.FloatKeyword:
                case SyntaxKind.DecimalKeyword:
                case SyntaxKind.StringKeyword:
                case SyntaxKind.CharKeyword:
                case SyntaxKind.VoidKeyword:
                case SyntaxKind.ObjectKeyword:
                case SyntaxKind.TypeOfKeyword:
                case SyntaxKind.SizeOfKeyword:
                case SyntaxKind.NullKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.IfKeyword:
                case SyntaxKind.ElseKeyword:
                case SyntaxKind.WhileKeyword:
                case SyntaxKind.ForKeyword:
                case SyntaxKind.ForEachKeyword:
                case SyntaxKind.DoKeyword:
                case SyntaxKind.SwitchKeyword:
                case SyntaxKind.CaseKeyword:
                case SyntaxKind.DefaultKeyword:
                case SyntaxKind.TryKeyword:
                case SyntaxKind.CatchKeyword:
                case SyntaxKind.FinallyKeyword:
                case SyntaxKind.LockKeyword:
                case SyntaxKind.GotoKeyword:
                case SyntaxKind.BreakKeyword:
                case SyntaxKind.ContinueKeyword:
                case SyntaxKind.ReturnKeyword:
                case SyntaxKind.ThrowKeyword:
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.PrivateKeyword:
                case SyntaxKind.InternalKeyword:
                case SyntaxKind.ProtectedKeyword:
                case SyntaxKind.StaticKeyword:
                case SyntaxKind.ReadOnlyKeyword:
                case SyntaxKind.SealedKeyword:
                case SyntaxKind.ConstKeyword:
                case SyntaxKind.FixedKeyword:
                case SyntaxKind.StackAllocKeyword:
                case SyntaxKind.VolatileKeyword:
                case SyntaxKind.NewKeyword:
                case SyntaxKind.OverrideKeyword:
                case SyntaxKind.AbstractKeyword:
                case SyntaxKind.VirtualKeyword:
                case SyntaxKind.EventKeyword:
                case SyntaxKind.ExternKeyword:
                case SyntaxKind.RefKeyword:
                case SyntaxKind.OutKeyword:
                case SyntaxKind.InKeyword:
                case SyntaxKind.IsKeyword:
                case SyntaxKind.AsKeyword:
                case SyntaxKind.ParamsKeyword:
                case SyntaxKind.ArgListKeyword:
                case SyntaxKind.MakeRefKeyword:
                case SyntaxKind.RefTypeKeyword:
                case SyntaxKind.RefValueKeyword:
                case SyntaxKind.ThisKeyword:
                case SyntaxKind.BaseKeyword:
                case SyntaxKind.NamespaceKeyword:
                case SyntaxKind.UsingKeyword:
                case SyntaxKind.ClassKeyword:
                case SyntaxKind.StructKeyword:
                case SyntaxKind.InterfaceKeyword:
                case SyntaxKind.EnumKeyword:
                case SyntaxKind.DelegateKeyword:
                case SyntaxKind.CheckedKeyword:
                case SyntaxKind.UncheckedKeyword:
                case SyntaxKind.UnsafeKeyword:
                case SyntaxKind.OperatorKeyword:
                case SyntaxKind.ExplicitKeyword:
                case SyntaxKind.ImplicitKeyword:
                case SyntaxKind.YieldKeyword:
                case SyntaxKind.PartialKeyword:
                case SyntaxKind.AliasKeyword:
                case SyntaxKind.GlobalKeyword:
                case SyntaxKind.AssemblyKeyword:
                case SyntaxKind.ModuleKeyword:
                case SyntaxKind.TypeKeyword:
                case SyntaxKind.FieldKeyword:
                case SyntaxKind.MethodKeyword:
                case SyntaxKind.ParamKeyword:
                case SyntaxKind.PropertyKeyword:
                case SyntaxKind.TypeVarKeyword:
                case SyntaxKind.GetKeyword:
                case SyntaxKind.SetKeyword:
                case SyntaxKind.AddKeyword:
                case SyntaxKind.RemoveKeyword:
                case SyntaxKind.WhereKeyword:
                case SyntaxKind.FromKeyword:
                case SyntaxKind.GroupKeyword:
                case SyntaxKind.JoinKeyword:
                case SyntaxKind.IntoKeyword:
                case SyntaxKind.LetKeyword:
                case SyntaxKind.ByKeyword:
                case SyntaxKind.SelectKeyword:
                case SyntaxKind.OrderByKeyword:
                case SyntaxKind.OnKeyword:
                case SyntaxKind.EqualsKeyword:
                case SyntaxKind.AscendingKeyword:
                case SyntaxKind.DescendingKeyword:
                case SyntaxKind.NameOfKeyword:
                case SyntaxKind.AsyncKeyword:
                case SyntaxKind.AwaitKeyword:
                case SyntaxKind.WhenKeyword:
                case SyntaxKind.ElifKeyword:
                case SyntaxKind.EndIfKeyword:
                case SyntaxKind.RegionKeyword:
                case SyntaxKind.EndRegionKeyword:
                case SyntaxKind.DefineKeyword:
                case SyntaxKind.UndefKeyword:
                case SyntaxKind.WarningKeyword:
                case SyntaxKind.ErrorKeyword:
                case SyntaxKind.LineKeyword:
                case SyntaxKind.PragmaKeyword:
                case SyntaxKind.HiddenKeyword:
                case SyntaxKind.ChecksumKeyword:
                case SyntaxKind.DisableKeyword:
                case SyntaxKind.RestoreKeyword:
                case SyntaxKind.ReferenceKeyword:
                    tokenInfo.Type = TokenType.Keyword;
                    tokenInfo.Color = TokenColor.Keyword;
                    break;

                case SyntaxKind.IdentifierToken:
                    if (_keywords != null && _keywords.Contains(token.ValueText))
                    {
                        tokenInfo.Type = TokenType.Keyword;
                        tokenInfo.Color = TokenColor.Keyword;
                    }
                    else
                    {
                        tokenInfo.Type = TokenType.Identifier;
                        tokenInfo.Color = TokenColor.Identifier;
                    }
                    break;

                case SyntaxKind.StringLiteralToken:
                case SyntaxKind.CharacterLiteralToken:
                    tokenInfo.Type = TokenType.String;
                    tokenInfo.Color = TokenColor.String;
                    break;

                case SyntaxKind.NumericLiteralToken:
                    tokenInfo.Type = TokenType.Literal;
                    tokenInfo.Color = TokenColor.Number;
                    break;

                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                    tokenInfo.Type = TokenType.Comment;
                    tokenInfo.Color = TokenColor.Comment;
                    break;
            }

            return true;
        }

        void IScanner.SetSource(string source, int offset)
        {
            _source = source.Substring(offset);
            _tokens = CSharp.ParseTokens(_source).GetEnumerator();
        }
    }
}
