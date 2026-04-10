using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chummer.Tests
{
    [TestClass]
    public class ImprovementManagerSyncWrapperTests
    {
        private static readonly string s_strImprovementManagerPath = FindImprovementManagerPath();

        [TestMethod]
        public void DoSelectSkill_sync_wrapper_calls_sync_core()
        {
            AssertCoreInvocationExists("DoSelectSkill", "DoSelectSkillCore(");
        }

        [TestMethod]
        public void DoSelectSkillGroup_sync_wrapper_calls_sync_core()
        {
            AssertCoreInvocationExists("DoSelectSkillGroup", "DoSelectSkillGroupCore(");
        }

        [TestMethod]
        public void RemoveImprovements_sync_wrapper_uses_sync_flag_true()
        {
            Assert.AreEqual("true", GetFirstArgumentText("RemoveImprovements", "RemoveImprovementsCoreAsync"));
        }

        [TestMethod]
        public void RemoveImprovements_sync_duplicate_check_does_not_call_AnyAsync()
        {
            string strMethodBody = FindMethod("RemoveImprovementsCoreAsync").Body?.ToFullString() ?? string.Empty;
            Match objMatch = Regex.Match(strMethodBody,
                @"bool\s+blnHasDuplicate;\s*if\s*\(blnSync\)\s*\{(?<syncBlock>.*?)\}\s*else\s+if\s*\(blnAllowDuplicatesFromSameSource\)",
                RegexOptions.Singleline);

            Assert.IsTrue(objMatch.Success, "Could not isolate the sync duplicate-check branch in RemoveImprovementsCoreAsync.");
            StringAssert.DoesNotContain(objMatch.Groups["syncBlock"].Value, "AnyAsync(");
        }

        private static void AssertCoreInvocationExists(string wrapperMethodName, string invocationFragment)
        {
            MethodDeclarationSyntax objMethod = FindMethod(wrapperMethodName);
            bool blnFound = objMethod.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(x => x.ToString().Contains(invocationFragment));

            Assert.IsTrue(blnFound, $"Expected {wrapperMethodName} to invoke {invocationFragment}.");
        }

        private static string GetFirstArgumentText(string wrapperMethodName, string coreMethodName)
        {
            InvocationExpressionSyntax objInvocation = FindCoreInvocation(wrapperMethodName, coreMethodName);
            return objInvocation.ArgumentList.Arguments[0].Expression.ToString();
        }

        private static InvocationExpressionSyntax FindCoreInvocation(string wrapperMethodName, string coreMethodName)
        {
            MethodDeclarationSyntax objMethod = FindMethod(wrapperMethodName, coreMethodName);
            return objMethod.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Single(x => x.Expression.ToString().Contains(coreMethodName));
        }

        private static MethodDeclarationSyntax FindMethod(string methodName, string coreMethodName = null)
        {
            Assert.IsTrue(File.Exists(s_strImprovementManagerPath),
                "Could not locate ImprovementManager.cs at " + s_strImprovementManagerPath + '.');

            var objTree = CSharpSyntaxTree.ParseText(File.ReadAllText(s_strImprovementManagerPath));
            IQueryable<MethodDeclarationSyntax> lstMethods = objTree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(x => x.Identifier.ValueText == methodName)
                .AsQueryable();

            if (!string.IsNullOrEmpty(coreMethodName))
            {
                lstMethods = lstMethods.Where(x => x.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Any(y => y.Expression.ToString().Contains(coreMethodName)));
            }

            return lstMethods.Single();
        }

        private static string FindImprovementManagerPath()
        {
            DirectoryInfo objDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (objDir != null)
            {
                string strCandidate = Path.Combine(objDir.FullName, "Chummer", "Backend", "Static", "Managers",
                    "ImprovementManager.cs");
                if (File.Exists(strCandidate))
                    return strCandidate;
                objDir = objDir.Parent;
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImprovementManager.cs"));
        }
    }
}
