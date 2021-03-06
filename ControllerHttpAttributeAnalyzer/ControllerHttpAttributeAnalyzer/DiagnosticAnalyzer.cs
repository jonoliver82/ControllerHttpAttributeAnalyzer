using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ControllerHttpAttributeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ControllerHttpAttributeAnalyzer : DiagnosticAnalyzer
    {
        private const string CONTROLLER_BASE_TYPE_SUFFIX = "Controller";
        private const string ASPNET_CORE_ATTRIBUTE_BASE_TYPE_NAME = "HttpMethodAttribute";
        private const string ASPNET_MVC_ATTRIBUTE_BASE_TYPE_NAME = "ActionMethodSelectorAttribute";

        public const string DiagnosticId = "ControllerHttpAttributeAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Attributes";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        /// <summary>
        /// If the method is within a class that inherits from Controller, 
        /// then check it has an attribute that inherits from HttpMethodAttribute or ActionMethodSelectorAttribute
        /// </summary>
        /// <param name="context"></param>
        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (methodSymbol.DeclaredAccessibility == Accessibility.Public &&
                methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.ContainingType.BaseType.Name.EndsWith(CONTROLLER_BASE_TYPE_SUFFIX))
            {
                foreach (var attribute in methodSymbol.GetAttributes())
                {
                    if (attribute.AttributeClass.BaseType != null &&
                        (attribute.AttributeClass.BaseType.Name.Equals(ASPNET_CORE_ATTRIBUTE_BASE_TYPE_NAME) ||
                        attribute.AttributeClass.BaseType.Name.Equals(ASPNET_MVC_ATTRIBUTE_BASE_TYPE_NAME)))
                    {
                        return;
                    }
                }

                var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
