using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace ControllerHttpAttributeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ControllerHttpAttributeAnalyzerCodeFixProvider)), Shared]
    public class ControllerHttpAttributeAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string HTTPGET_ATTRIBUTE = "HttpGet";
        private const string HTTPPOST_ATTRIBUTE = "HttpPost";

        private const string CODEFIX_TITLE_HTTPGET  = "Add [HttpGet] Attribute";
        private const string CODEFIX_TITLE_HTTPPOST = "Add [HttpPost] Attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ControllerHttpAttributeAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the method declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CODEFIX_TITLE_HTTPGET,
                    createChangedSolution: c => AddAttributeAsync(context.Document, declaration, HTTPGET_ATTRIBUTE, c),
                    equivalenceKey: CODEFIX_TITLE_HTTPGET),
                diagnostic);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CODEFIX_TITLE_HTTPPOST,
                    createChangedSolution: c => AddAttributeAsync(context.Document, declaration, HTTPPOST_ATTRIBUTE, c),
                    equivalenceKey: CODEFIX_TITLE_HTTPPOST),
                diagnostic);
        }

        private async Task<Solution> AddAttributeAsync(Document document, MethodDeclarationSyntax methodDecl, string attribute, CancellationToken cancellationToken)
        {
            // Construct the new attribute list
            var name = SyntaxFactory.ParseName(attribute);
            var newAttribute = SyntaxFactory.Attribute(name);
            var list = SyntaxFactory.AttributeList();  
            
            // Add new attribute to the methods attribute list
            var attributes = methodDecl.AttributeLists.Add(list.AddAttributes(newAttribute));

            // Replace the existing method declaration with this one
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(methodDecl, methodDecl.WithAttributeLists(attributes))).Project.Solution;
        }
    }
}