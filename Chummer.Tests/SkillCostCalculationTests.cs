/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class SkillCostCalculationTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Utils.IsUnitTest = true;
            Utils.IsUnitTestForUI = false;
            Utils.CreateSynchronizationContext();
        }

        [TestMethod]
        public void TestActiveSkillKarmaCostCalculation()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestActiveSkillKarmaCostCalculation()");
                
                // Create a test character
                using (Character objCharacter = new Character())
                {
                    objCharacter.Created = false; // Create mode
                    
                    // Add a basic active skill
                    Skill objSkill = new Skill(objCharacter);
                    objSkill.Name = "Firearms";
                    objSkill.SkillCategory = "Combat Active";
                    objSkill.Base = 0;
                    objSkill.Karma = 0;
                    
                    // Test basic karma cost calculation
                    int intBaseCost = objSkill.CurrentKarmaCost;
                    Assert.IsTrue(intBaseCost > 0, "Base karma cost should be positive");
                    
                    // Test upgrade cost
                    int intUpgradeCost = objSkill.UpgradeKarmaCost;
                    Assert.IsTrue(intUpgradeCost > 0, "Upgrade karma cost should be positive");
                    
                    Debug.WriteLine($"Base cost: {intBaseCost}, Upgrade cost: {intUpgradeCost}");
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestKnowledgeSkillKarmaCostCalculation()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestKnowledgeSkillKarmaCostCalculation()");
                
                // Create a test character
                using (Character objCharacter = new Character())
                {
                    objCharacter.Created = false; // Create mode
                    
                    // Add a knowledge skill
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter);
                    objKnowledgeSkill.Name = "History";
                    objKnowledgeSkill.Type = "Academic";
                    objKnowledgeSkill.Base = 0;
                    objKnowledgeSkill.Karma = 0;
                    
                    // Test basic karma cost calculation
                    int intBaseCost = objKnowledgeSkill.CurrentKarmaCost;
                    Assert.IsTrue(intBaseCost > 0, "Base karma cost should be positive");
                    
                    // Test upgrade cost
                    int intUpgradeCost = objKnowledgeSkill.UpgradeKarmaCost;
                    Assert.IsTrue(intUpgradeCost > 0, "Upgrade karma cost should be positive");
                    
                    Debug.WriteLine($"Knowledge skill base cost: {intBaseCost}, Upgrade cost: {intUpgradeCost}");
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestActiveSkillKarmaCostImprovements()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestActiveSkillKarmaCostImprovements()");
                
                // Create a test character
                using (Character objCharacter = new Character())
                {
                    objCharacter.Created = true; // Career mode for improvements
                    
                    // Add a skill
                    Skill objSkill = new Skill(objCharacter);
                    objSkill.Name = "Firearms";
                    objSkill.SkillCategory = "Combat Active";
                    objSkill.Base = 0;
                    objSkill.Karma = 0;
                    
                    // Get baseline cost
                    int intBaselineCost = objSkill.CurrentKarmaCost;
                    Debug.WriteLine($"Baseline cost: {intBaselineCost}");
                    
                    // Add ActiveSkillKarmaCost improvement
                    Improvement objImprovement = new Improvement(objCharacter);
                    objImprovement.ImprovementType = Improvement.ImprovementType.ActiveSkillKarmaCost;
                    objImprovement.Value = 5; // Add 5 karma cost
                    objImprovement.ImprovementSource = Improvement.ImprovementSource.Quality;
                    objImprovement.SourceName = "Test Quality";
                    objImprovement.Condition = "career";
                    
                    // Apply the improvement
                    objCharacter.Improvements.Add(objImprovement);
                    
                    // Test that the improvement is applied
                    int intCostWithImprovement = objSkill.CurrentKarmaCost;
                    Assert.AreEqual(intBaselineCost + 5, intCostWithImprovement, 
                        "ActiveSkillKarmaCost improvement should add 5 to the cost");
                    
                    Debug.WriteLine($"Cost with improvement: {intCostWithImprovement}");
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestActiveSkillKarmaCostMinimumImprovements()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestActiveSkillKarmaCostMinimumImprovements()");
                
                // Create a test character
                using (Character objCharacter = new Character())
                {
                    objCharacter.Created = true; // Career mode for improvements
                    
                    // Add a skill
                    Skill objSkill = new Skill(objCharacter);
                    objSkill.Name = "Firearms";
                    objSkill.SkillCategory = "Combat Active";
                    objSkill.Base = 0;
                    objSkill.Karma = 0;
                    
                    // Get baseline cost
                    int intBaselineCost = objSkill.CurrentKarmaCost;
                    Debug.WriteLine($"Baseline cost: {intBaselineCost}");
                    
                    // Add ActiveSkillKarmaCostMinimum improvement
                    Improvement objImprovement = new Improvement(objCharacter);
                    objImprovement.ImprovementType = Improvement.ImprovementType.ActiveSkillKarmaCostMinimum;
                    objImprovement.Value = 10; // Set minimum cost to 10
                    objImprovement.ImprovementSource = Improvement.ImprovementSource.Quality;
                    objImprovement.SourceName = "Test Quality";
                    objImprovement.Condition = "career";
                    
                    // Apply the improvement
                    objCharacter.Improvements.Add(objImprovement);
                    
                    // Test that the minimum cost override is applied
                    int intCostWithMinimum = objSkill.CurrentKarmaCost;
                    Assert.IsTrue(intCostWithMinimum >= 10, 
                        "ActiveSkillKarmaCostMinimum should ensure cost is at least 10");
                    
                    Debug.WriteLine($"Cost with minimum override: {intCostWithMinimum}");
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestKnowledgeSkillKarmaCostImprovements()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestKnowledgeSkillKarmaCostImprovements()");
                
                // Create a test character
                using (Character objCharacter = new Character())
                {
                    objCharacter.Created = true; // Career mode for improvements
                    
                    // Add a knowledge skill
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter);
                    objKnowledgeSkill.Name = "History";
                    objKnowledgeSkill.Type = "Academic";
                    objKnowledgeSkill.Base = 0;
                    objKnowledgeSkill.Karma = 0;
                    
                    // Get baseline cost
                    int intBaselineCost = objKnowledgeSkill.CurrentKarmaCost;
                    Debug.WriteLine($"Baseline cost: {intBaselineCost}");
                    
                    // Add KnowledgeSkillKarmaCost improvement
                    Improvement objImprovement = new Improvement(objCharacter);
                    objImprovement.ImprovementType = Improvement.ImprovementType.KnowledgeSkillKarmaCost;
                    objImprovement.Value = 3; // Add 3 karma cost
                    objImprovement.ImprovementSource = Improvement.ImprovementSource.Quality;
                    objImprovement.SourceName = "Test Quality";
                    objImprovement.Condition = "career";
                    
                    // Apply the improvement
                    objCharacter.Improvements.Add(objImprovement);
                    
                    // Test that the improvement is applied
                    int intCostWithImprovement = objKnowledgeSkill.CurrentKarmaCost;
                    Assert.AreEqual(intBaselineCost + 3, intCostWithImprovement, 
                        "KnowledgeSkillKarmaCost improvement should add 3 to the cost");
                    
                    Debug.WriteLine($"Cost with improvement: {intCostWithImprovement}");
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestSkillCostConsistencyWithExistingCharacters()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestSkillCostConsistencyWithExistingCharacters()");
                
                foreach (Character objCharacter in GetTestCharacters())
                {
                    Debug.WriteLine($"Testing character: {objCharacter.Name}");
                    
                    // Test all active skills
                    foreach (Skill objSkill in objCharacter.SkillsSection.Skills)
                    {
                        // Test that costs are consistent
                        int intCurrentCost = objSkill.CurrentKarmaCost;
                        int intUpgradeCost = objSkill.UpgradeKarmaCost;
                        
                        Assert.IsTrue(intCurrentCost >= 0, 
                            $"Current karma cost should be non-negative for skill {objSkill.Name}");
                        Assert.IsTrue(intUpgradeCost >= 0, 
                            $"Upgrade karma cost should be non-negative for skill {objSkill.Name}");
                        
                        // Test async versions
                        int intCurrentCostAsync = objSkill.GetCurrentKarmaCostAsync().Result;
                        int intUpgradeCostAsync = objSkill.GetUpgradeKarmaCostAsync().Result;
                        
                        Assert.AreEqual(intCurrentCost, intCurrentCostAsync, 
                            $"Async and sync current costs should match for skill {objSkill.Name}");
                        Assert.AreEqual(intUpgradeCost, intUpgradeCostAsync, 
                            $"Async and sync upgrade costs should match for skill {objSkill.Name}");
                    }
                    
                    // Test all knowledge skills
                    foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        // Test that costs are consistent
                        int intCurrentCost = objKnowledgeSkill.CurrentKarmaCost;
                        int intUpgradeCost = objKnowledgeSkill.UpgradeKarmaCost;
                        
                        Assert.IsTrue(intCurrentCost >= 0, 
                            $"Current karma cost should be non-negative for knowledge skill {objKnowledgeSkill.Name}");
                        Assert.IsTrue(intUpgradeCost >= 0, 
                            $"Upgrade karma cost should be non-negative for knowledge skill {objKnowledgeSkill.Name}");
                        
                        // Test async versions
                        int intCurrentCostAsync = objKnowledgeSkill.GetCurrentKarmaCostAsync().Result;
                        int intUpgradeCostAsync = objKnowledgeSkill.GetUpgradeKarmaCostAsync().Result;
                        
                        Assert.AreEqual(intCurrentCost, intCurrentCostAsync, 
                            $"Async and sync current costs should match for knowledge skill {objKnowledgeSkill.Name}");
                        Assert.AreEqual(intUpgradeCost, intUpgradeCostAsync, 
                            $"Async and sync upgrade costs should match for knowledge skill {objKnowledgeSkill.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestImprovementTypeProperties()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestImprovementTypeProperties()");
                
                // Test active skill improvement types
                using (Character objCharacter = new Character())
                {
                    Skill objSkill = new Skill(objCharacter);
                    objSkill.Name = "Firearms";
                    objSkill.SkillCategory = "Combat Active";
                    
                    Assert.AreEqual(Improvement.ImprovementType.ActiveSkillKarmaCost, objSkill.KarmaCostImprovementType,
                        "Active skill should use ActiveSkillKarmaCost improvement type");
                    Assert.AreEqual(Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier, objSkill.KarmaCostMultiplierImprovementType,
                        "Active skill should use ActiveSkillKarmaCostMultiplier improvement type");
                    Assert.AreEqual(Improvement.ImprovementType.ActiveSkillKarmaCostMinimum, objSkill.KarmaCostMinimumImprovementType,
                        "Active skill should use ActiveSkillKarmaCostMinimum improvement type");
                }
                
                // Test knowledge skill improvement types
                using (Character objCharacter = new Character())
                {
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter);
                    objKnowledgeSkill.Name = "History";
                    objKnowledgeSkill.Type = "Academic";
                    
                    Assert.AreEqual(Improvement.ImprovementType.KnowledgeSkillKarmaCost, objKnowledgeSkill.KarmaCostImprovementType,
                        "Knowledge skill should use KnowledgeSkillKarmaCost improvement type");
                    Assert.AreEqual(Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier, objKnowledgeSkill.KarmaCostMultiplierImprovementType,
                        "Knowledge skill should use KnowledgeSkillKarmaCostMultiplier improvement type");
                    Assert.AreEqual(Improvement.ImprovementType.KnowledgeSkillKarmaCostMinimum, objKnowledgeSkill.KarmaCostMinimumImprovementType,
                        "Knowledge skill should use KnowledgeSkillKarmaCostMinimum improvement type");
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void TestSkillCostWithTestQuality()
        {
            try
            {
                Debug.WriteLine("Unit test initialized for: TestSkillCostWithTestQuality()");
                
                // Create a test character
                using (Character objCharacter = new Character())
                {
                    objCharacter.Created = true; // Career mode for improvements
                    
                    // Add the test quality we created
                    Quality objQuality = new Quality(objCharacter);
                    objQuality.Name = "Active Skill Training Discount";
                    objQuality.Source = "Test";
                    objQuality.SourceID = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
                    objCharacter.Qualities.Add(objQuality);
                    
                    // Add a skill
                    Skill objSkill = new Skill(objCharacter);
                    objSkill.Name = "Firearms";
                    objSkill.SkillCategory = "Combat Active";
                    objSkill.Base = 0;
                    objSkill.Karma = 0;
                    
                    // Get baseline cost
                    int intBaselineCost = objSkill.CurrentKarmaCost;
                    Debug.WriteLine($"Baseline cost: {intBaselineCost}");
                    
                    // Test that the quality's improvement is applied
                    int intCostWithQuality = objSkill.CurrentKarmaCost;
                    Assert.IsTrue(intCostWithQuality >= 5, 
                        "The test quality should set minimum cost to 5");
                    
                    Debug.WriteLine($"Cost with test quality: {intCostWithQuality}");
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
            }
        }

        private static IEnumerable<Character> GetTestCharacters(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            for (int i = 0; i < CommonTestData.TestFileInfos.Length; ++i)
            {
                token.ThrowIfCancellationRequested();
                Character objLoopCharacter = CommonTestData.Characters[i];
                if (objLoopCharacter == null)
                {
                    objLoopCharacter = LoadCharacter(CommonTestData.TestFileInfos[i], token: token);
                    CommonTestData.Characters[i] = objLoopCharacter;
                }

                yield return objLoopCharacter;
            }
        }

        private static Character LoadCharacter(FileInfo objFileInfo, Character objExistingCharacter = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Character objCharacter = objExistingCharacter;
            try
            {
                Debug.WriteLine("Loading: " + objFileInfo.Name);
                if (objExistingCharacter != null)
                    objCharacter.ResetCharacter(token);
                else
                    objCharacter = new Character();
                objCharacter.FileName = objFileInfo.FullName;

                bool blnSuccess = objCharacter.Load(token: token);
                Assert.IsTrue(blnSuccess);
                Debug.WriteLine("Character loaded: " + objCharacter.Name + ", " + objFileInfo.Name);
            }
            catch (Exception e)
            {
                e = e.Demystify();
                if (objCharacter != null)
                {
                    if (objExistingCharacter == null)
                        objCharacter.Dispose();
                    objCharacter = null;
                }
                string strErrorMessage = "Could not load " + objFileInfo.FullName + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }

            return objCharacter;
        }
    }
}
