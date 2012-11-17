﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.239
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace DotNetNuke.Website.Specs.Modules.Html
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("html rich editor")]
    public partial class HtmlRichEditorFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TelerikEditor.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "html rich editor", "In order to edit html content in rich editor\r\nAs an Admin\r\nI want to work correct" +
                    "ly in rich html editor", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Insert a hyper link to file which file name contains space")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        [NUnit.Framework.CategoryAttribute("MustHaveKnownFileInFileManager")]
        public virtual void InsertAHyperLinkToFileWhichFileNameContainsSpace()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Insert a hyper link to file which file name contains space", new string[] {
                        "MustBeDefaultAdminCredentials",
                        "MustHaveKnownFileInFileManager"});
#line 8
this.ScenarioSetup(scenarioInfo);
#line 9
 testRunner.Given("I am on the site home page");
#line 10
 testRunner.And("I have logged in as the admin");
#line 11
 testRunner.When("I edit one of the html module content");
#line 12
 testRunner.And("I enter Hello and click hyper link manager button in rad text editor");
#line 13
 testRunner.And("I click the Telerik Editor HyperLink button");
#line 14
 testRunner.And("I insert a document which file name contains space");
#line 15
 testRunner.And("Insert link with the file");
#line 16
 testRunner.Then("I should see the hyper link insert in rad text editor");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Save Template Command must work correctly")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        public virtual void SaveTemplateCommandMustWorkCorrectly()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Save Template Command must work correctly", new string[] {
                        "MustBeDefaultAdminCredentials"});
#line 19
this.ScenarioSetup(scenarioInfo);
#line 20
 testRunner.Given("I am on the site home page");
#line 21
 testRunner.And("I have logged in as the admin");
#line 22
 testRunner.When("I edit one of the html module content");
#line 23
 testRunner.And("I click SaveTemplate toolbar button");
#line 24
 testRunner.Then("SaveTemplate Dialog must open");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
