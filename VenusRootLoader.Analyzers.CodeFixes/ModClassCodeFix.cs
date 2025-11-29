using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;

namespace VenusRootLoader.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ModClassCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        // ReSharper disable once UseCollectionExpression
        ImmutableArray.Create(Descriptors.Vrl0003ModClassIsNotSealed.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is null)
            return;

        SyntaxNode node = root.FindNode(context.Span);
        if (node is not ClassDeclarationSyntax classDeclaration)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Make class sealed",
                c => ChangeDocument(context.Document, classDeclaration, c),
                $"{Descriptors.Vrl0003ModClassIsNotSealed.Id}CodeFix"),
            context.Diagnostics);
    }

    private static async Task<Document> ChangeDocument(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
            return document;

        SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
        DeclarationModifiers modifiers = generator.GetModifiers(classDeclaration);
        SyntaxNode newClassDeclaration = generator.WithModifiers(classDeclaration, modifiers.WithIsSealed(true));
        SyntaxNode newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}