using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;

namespace ControllerHttpAttributeAnalyzer.Test
{
	/// <summary>
	/// Note we cant test the fix works on AspNetCore from a project of type .NET Framework (Full)
	/// </summary>
	[TestClass]
	public class CodeFixProviderUnitTests : CodeFixVerifier
	{
		private const string DIAGNOSTIC_ID = "ControllerHttpAttributeAnalyzer";
		private const string MESSAGE_FORMAT = "Controller method '{0}' does not specify a HTTP verb attribute";

		private const int CODEFIX_ID_HTTPGET = 0;
		private const int CODEFIX_ID_HTTPPOST = 1;

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new ControllerHttpAttributeAnalyzerCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new ControllerHttpAttributeAnalyzer();
		}

		[TestMethod]
		public void PostFixAddsPostAttribute()
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

			var fixtest = @"
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
	public class HomeController : Controller
	{
        [HttpPost]
        public ActionResult Index()
		{
			return View();
		}
	}
}";

			//Act & Assert
			VerifyCSharpFix(test, fixtest, CODEFIX_ID_HTTPPOST);
		}

		[TestMethod]
		public void GetFixAddsGetAttribute()
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

			var fixtest = @"
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
			VerifyCSharpFix(test, fixtest, CODEFIX_ID_HTTPGET);
		}

		[TestMethod]
		public void GetFixAppendsAttribute_WhenExistingAttributes()
		{
			//Arrange
			var test = @"
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
	public class HomeController : Controller
	{
		[HandleError]						
		public ActionResult Index()
		{
			return View();
		}
	}
}";

			var fixtest = @"
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
	public class HomeController : Controller
	{
		[HandleError]
        [HttpGet]
        public ActionResult Index()
		{
			return View();
		}
	}
}";

			//Act & Assert
			VerifyCSharpFix(test, fixtest, CODEFIX_ID_HTTPGET);
		}
	}
}
