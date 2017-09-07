using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using ControllerHttpAttributeAnalyzer;

namespace ControllerHttpAttributeAnalyzer.Test
{
	[TestClass]
	public class DiagnosticAnalyzerUnitTests : DiagnosticVerifier
	{
		private const string DIAGNOSTIC_ID = "ControllerHttpAttributeAnalyzer";
		private const string MESSAGE_FORMAT = "Controller method '{0}' does not specify a HTTP verb attribute";

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new ControllerHttpAttributeAnalyzer();
		}

		[TestMethod]
		public void NoCode_RaisesNoDiagnostics()
		{
			//Arrange
			var test = @"";

			//Act & Assert
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void PrivateMethod_RaisesNoDiagnostics()
		{
			//Arrange
			var test = @"
				using Microsoft.AspNetCore.Mvc;

				namespace WebApplication1.Controllers
				{
					public class HomeController : Controller
					{
						private IActionResult Index()
						{
							return View();
						}
					}
				}";

			//Act & Assert
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void NonControllerBaseClass_RaisesNoDiagnostics()
		{
			//Arrange
			var test = @"
				using Microsoft.AspNetCore.Mvc;

				namespace WebApplication1.Controllers
				{
					public class HomeController
					{
						public IActionResult Index()
						{
							return View();
						}
					}
				}";

			//Act & Assert
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void MultipleAttributesIncludingHttpGetVerb_RaisesNoDiagnostics()
		{
			//Arrange
			var test = @"
				using System.Web.Mvc;

				namespace WebApplication1.Controllers
				{
					public class HomeController : Controller
					{
						[HandleError]
						[HttpGet]                        
						public IActionResult Index()
						{
							return View();
						}
					}
				}";

			//Act & Assert
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void AspNetMvcHttpGetAttribute_RaisesNoDiagnostics()
		{
			//Arrange
			var test = @"
				using System.Web.Mvc;

				namespace WebApplication2.Controllers
				{
					public class HomeController : Controller
					{
						[HttpGet]						
						public ActionResult Index()
						{
							return View();
						}
					}
				}";

			//Act & Assert
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void AspNetMvcAcceptVerbsAttribute_RaisesNoDiagnostics()
		{
			//Arrange
			var test = @"
				using System.Web.Mvc;

				namespace WebApplication2.Controllers
				{
					public class HomeController : Controller
					{
						[AcceptVerbs(HttpVerbs.Get)]						
						public ActionResult Index()
						{
							return View();
						}
					}
				}";

			//Act & Assert
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void HttpAttributeFromDifferentBaseClass_RaisesDiagnostics()
		{
			//Arrange
			var test = @"
				using System;
				using Microsoft.AspNetCore.Mvc;

				namespace WebApplication1.Controllers
				{
					public class HttpTest: Attribute
					{
					}
					
					public class HomeController : Controller
					{
						[HttpTest]
						public IActionResult Index()
						{
							return View();
						}
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = DIAGNOSTIC_ID,
				Message = String.Format(MESSAGE_FORMAT, "Index"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 28) }
			};

			//Act & Assert
			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void AspNetCoreControllerPublicMethod_RaisesDiagnostics()
		{
			//Arrange
			var test = @"
				using Microsoft.AspNetCore.Mvc;

				namespace WebApplication1.Controllers
				{
					public class HomeController : Controller
					{
						public IActionResult Index()
						{
							return View();
						}
					}
				}";
			
			var expected = new DiagnosticResult
			{
				Id = DIAGNOSTIC_ID,
				Message = String.Format(MESSAGE_FORMAT, "Index"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 28) }
			};

			//Act & Assert
			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void AspNetMvcControllerPublicMethod_RaisesDiagnostics()
		{
			//Arrange
			var test = @"
				using System.Web.Mvc;

				namespace WebApplication2.Controllers
				{
					public class HomeController : Controller
					{
						public ActionResult Index()
						{
							return View();
						}
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = DIAGNOSTIC_ID,
				Message = String.Format(MESSAGE_FORMAT, "Index"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 27) }
			};

			//Act & Assert
			VerifyCSharpDiagnostic(test, expected);
		}
	}
}