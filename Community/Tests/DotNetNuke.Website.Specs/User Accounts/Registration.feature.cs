﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.269
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace DotNetNuke.Website.Specs.UserAccounts
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Registration")]
    public partial class RegistrationFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Registration.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Registration", "In order to register user\r\nAs a regular user\r\nI want to use the registration feat" +
                    "ure correctly", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("User Registration Notifcation mail should contains display name")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        [NUnit.Framework.CategoryAttribute("MustHaveEmailSetUpForSiteDumpToFolder")]
        [NUnit.Framework.CategoryAttribute("MustHaveEmptyEmailFolder")]
        [NUnit.Framework.CategoryAttribute("SiteMustRunInFullTrust")]
        public virtual void UserRegistrationNotifcationMailShouldContainsDisplayName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("User Registration Notifcation mail should contains display name", new string[] {
                        "MustBeDefaultAdminCredentials",
                        "MustHaveEmailSetUpForSiteDumpToFolder",
                        "MustHaveEmptyEmailFolder",
                        "SiteMustRunInFullTrust"});
#line 10
this.ScenarioSetup(scenarioInfo);
#line 11
 testRunner.Given("I am on the site home page");
#line 12
 testRunner.When("I click the Register link");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Control",
                        "Value"});
            table1.AddRow(new string[] {
                        "User Name",
                        "RegisterUserTest"});
            table1.AddRow(new string[] {
                        "Email",
                        "RegisterUserTest@dnn.com"});
            table1.AddRow(new string[] {
                        "Password",
                        "password"});
            table1.AddRow(new string[] {
                        "Display Name",
                        "RegisterUserTest DN"});
#line 13
 testRunner.And("I fill in the Register User form", ((string)(null)), table1);
#line 19
 testRunner.And("I click the Register button");
#line 20
 testRunner.Then("The admin notification mail should contain RegisterUserTest DN");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Password should remain its value after page post back")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        [NUnit.Framework.CategoryAttribute("CustomRegistration")]
        public virtual void PasswordShouldRemainItsValueAfterPagePostBack()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Password should remain its value after page post back", new string[] {
                        "MustBeDefaultAdminCredentials",
                        "CustomRegistration"});
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
 testRunner.Given("I am on the site home page");
#line 26
 testRunner.And("I have logged in as the host");
#line 27
 testRunner.And("I clean the cache");
#line 28
 testRunner.When("I log off");
#line 29
 testRunner.And("I click the Register link");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Control",
                        "Value"});
            table2.AddRow(new string[] {
                        "User Name",
                        "Israel"});
            table2.AddRow(new string[] {
                        "Email",
                        "israel@dnn.com"});
            table2.AddRow(new string[] {
                        "Password",
                        "password"});
            table2.AddRow(new string[] {
                        "Display Name",
                        "Israel"});
#line 30
 testRunner.And("I fill in the Register User form", ((string)(null)), table2);
#line 36
 testRunner.And("I select country as Canada");
#line 37
 testRunner.Then("Password field\'s value should be password");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
