using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xaml.Generators;

internal class WindowGenerator : IGenerator
{
    public string Template { get; }

    public WindowGenerator()
    {
        Template = "";
//         Template = $$"""
//                      using Terminal.Gui.Views;
//                      namespace {{@namespace}}
//                      {
//                          public partial class {{className}}
//                          {
//                              public static Window Build()
//                              {
//                                  var root = new Window();
//                      {{children}}
//                                  return root;
//                              }
//                          }
//                      }
//                      """;
    }

    public string Generate(ElementNode node, IGeneratorFactory generators)
    {
        return CompilationUnit()
            .WithUsings(
                SingletonList(
                    UsingDirective(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName("Terminal"),
                                    IdentifierName("Gui")),
                                IdentifierName("Views")))
                        .WithUsingKeyword(
                            Token(
                                TriviaList(),
                                SyntaxKind.UsingKeyword,
                                TriviaList(
                                    Space)))
                        .WithSemicolonToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.SemicolonToken,
                                TriviaList(
                                    CarriageReturnLineFeed)))))
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    NamespaceDeclaration(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    "Xaml",
                                    TriviaList(
                                        CarriageReturnLineFeed))))
                        .WithNamespaceKeyword(
                            Token(
                                TriviaList(),
                                SyntaxKind.NamespaceKeyword,
                                TriviaList(
                                    Space)))
                        .WithOpenBraceToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.OpenBraceToken,
                                TriviaList(
                                    CarriageReturnLineFeed)))
                        .WithMembers(
                            SingletonList<MemberDeclarationSyntax>(
                                ClassDeclaration(
                                        Identifier(
                                            TriviaList(),
                                            "MyWindow",
                                            TriviaList(
                                                Space)))
                                    .WithModifiers(
                                        TokenList(Token(
                                                TriviaList(
                                                    Whitespace("    ")),
                                                SyntaxKind.PublicKeyword,
                                                TriviaList(
                                                    Space)), Token(
                                                TriviaList(),
                                                SyntaxKind.PartialKeyword,
                                                TriviaList(
                                                    Space))))
                                    .WithKeyword(
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.ClassKeyword,
                                            TriviaList(
                                                Space)))
                                    .WithBaseList(
                                        BaseList(
                                                SingletonSeparatedList<BaseTypeSyntax>(
                                                    SimpleBaseType(
                                                        QualifiedName(
                                                            QualifiedName(
                                                                QualifiedName(
                                                                    IdentifierName("Terminal"),
                                                                    IdentifierName("Gui")),
                                                                IdentifierName("Views")),
                                                            IdentifierName(
                                                                Identifier(
                                                                    TriviaList(),
                                                                    "Window",
                                                                    TriviaList(
                                                                        CarriageReturnLineFeed)))))))
                                            .WithColonToken(
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.ColonToken,
                                                    TriviaList(
                                                        Space))))
                                    .WithOpenBraceToken(
                                        Token(
                                            TriviaList(
                                                Whitespace("    ")),
                                            SyntaxKind.OpenBraceToken,
                                            TriviaList(
                                                CarriageReturnLineFeed)))
                                    .WithMembers(
                                        SingletonList<MemberDeclarationSyntax>(
                                            ConstructorDeclaration(
                                                    Identifier("MyWindow"))
                                                .WithModifiers(
                                                    TokenList(
                                                        Token(
                                                            TriviaList(
                                                                Whitespace("        ")),
                                                            SyntaxKind.PublicKeyword,
                                                            TriviaList(
                                                                Space))))
                                                .WithParameterList(
                                                    ParameterList()
                                                        .WithCloseParenToken(
                                                            Token(
                                                                TriviaList(),
                                                                SyntaxKind.CloseParenToken,
                                                                TriviaList(
                                                                    CarriageReturnLineFeed))))
                                                .WithBody(
                                                    Block(
                                                            LocalDeclarationStatement(
                                                                    VariableDeclaration(
                                                                            IdentifierName(
                                                                                Identifier(
                                                                                    TriviaList(
                                                                                        Whitespace(
                                                                                            "            ")),
                                                                                    SyntaxKind.VarKeyword,
                                                                                    "var",
                                                                                    "var",
                                                                                    TriviaList(
                                                                                        Space))))
                                                                        .WithVariables(
                                                                            SingletonSeparatedList<
                                                                                    VariableDeclaratorSyntax>(
                                                                                    VariableDeclarator(
                                                                                            Identifier(
                                                                                                TriviaList(),
                                                                                                "label0",
                                                                                                TriviaList(
                                                                                                        Space)))
                                                                                        .WithInitializer(
                                                                                            EqualsValueClause(
                                                                                                    ObjectCreationExpression(
                                                                                                            IdentifierName(
                                                                                                                    "Label"))
                                                                                                        .WithNewKeyword(
                                                                                                            Token(
                                                                                                                    TriviaList(),
                                                                                                                    SyntaxKind
                                                                                                                        .NewKeyword,
                                                                                                                    TriviaList(
                                                                                                                            Space)))
                                                                                                        .WithArgumentList(
                                                                                                            ArgumentList()))
                                                                                                .WithEqualsToken(
                                                                                                    Token(
                                                                                                        TriviaList(),
                                                                                                        SyntaxKind
                                                                                                            .EqualsToken,
                                                                                                        TriviaList(
                                                                                                                Space)))))))
                                                                .WithSemicolonToken(
                                                                    Token(
                                                                        TriviaList(),
                                                                        SyntaxKind.SemicolonToken,
                                                                        TriviaList(
                                                                            CarriageReturnLineFeed))),
                                                            ExpressionStatement(
                                                                    AssignmentExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                IdentifierName(
                                                                                    Identifier(
                                                                                        TriviaList(
                                                                                            Whitespace(
                                                                                                "            ")),
                                                                                        "label0",
                                                                                        TriviaList())),
                                                                                IdentifierName(
                                                                                    Identifier(
                                                                                        TriviaList(),
                                                                                        "Text",
                                                                                        TriviaList(
                                                                                            Space)))),
                                                                            LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                Literal("Hello")))
                                                                        .WithOperatorToken(
                                                                            Token(
                                                                                TriviaList(),
                                                                                SyntaxKind.EqualsToken,
                                                                                TriviaList(
                                                                                    Space))))
                                                                .WithSemicolonToken(
                                                                    Token(
                                                                        TriviaList(),
                                                                        SyntaxKind.SemicolonToken,
                                                                        TriviaList(
                                                                            CarriageReturnLineFeed))),
                                                            ExpressionStatement(
                                                                    InvocationExpression(
                                                                            MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                ThisExpression()
                                                                                    .WithToken(
                                                                                        Token(
                                                                                            TriviaList(
                                                                                                Whitespace(
                                                                                                        "            ")),
                                                                                            SyntaxKind.ThisKeyword,
                                                                                            TriviaList())),
                                                                                IdentifierName("Add")))
                                                                        .WithArgumentList(
                                                                            ArgumentList(
                                                                                SingletonSeparatedList<
                                                                                        ArgumentSyntax>(
                                                                                        Argument(
                                                                                            IdentifierName(
                                                                                                    "label0"))))))
                                                                .WithSemicolonToken(
                                                                    Token(
                                                                        TriviaList(),
                                                                        SyntaxKind.SemicolonToken,
                                                                        TriviaList(
                                                                            CarriageReturnLineFeed))))
                                                        .WithOpenBraceToken(
                                                            Token(
                                                                TriviaList(
                                                                    Whitespace("        ")),
                                                                SyntaxKind.OpenBraceToken,
                                                                TriviaList(
                                                                    CarriageReturnLineFeed)))
                                                        .WithCloseBraceToken(
                                                            Token(
                                                                TriviaList(
                                                                    Whitespace("        ")),
                                                                SyntaxKind.CloseBraceToken,
                                                                TriviaList(
                                                                    CarriageReturnLineFeed))))))
                                    .WithCloseBraceToken(
                                        Token(
                                            TriviaList(
                                                Whitespace("    ")),
                                            SyntaxKind.CloseBraceToken,
                                            TriviaList(
                                                CarriageReturnLineFeed)))))))
            .ToFullString();
    }
}