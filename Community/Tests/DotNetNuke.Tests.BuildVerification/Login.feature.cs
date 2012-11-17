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
namespace DotNetNuke.Tests.BuildVerification
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Site Login")]
    public partial class SiteLoginFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Login.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Site Login", "In order to login to the site\r\nAs a user of the site\r\nI want to be able to enter " +
                    "my credentials and login", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Login With Default Admin Password Forces Update")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentialsForceUpdate")]
        public virtual void LoginWithDefaultAdminPasswordForcesUpdate()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Login With Default Admin Password Forces Update", new string[] {
                        "MustBeDefaultAdminCredentialsForceUpdate"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I am on the site home page");
#line 9
 testRunner.And("I have pressed Login");
#line 10
 testRunner.And("I have entered the default Admin Username and the default password");
#line 11
 testRunner.When("I press Login");
#line 12
 testRunner.Then("I should be forced to enter a new password to proceed");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Login With Default Admin Password Update Password")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentialsForceUpdate")]
        public virtual void LoginWithDefaultAdminPasswordUpdatePassword()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Login With Default Admin Password Update Password", new string[] {
                        "MustBeDefaultAdminCredentialsForceUpdate"});
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("I am on the site home page");
#line 18
 testRunner.And("I have pressed Login");
#line 19
 testRunner.And("I have entered the default Admin Username and the default password");
#line 20
 testRunner.And("I press Login");
#line 21
 testRunner.When("I enter and confirm my new password");
#line 22
 testRunner.Then("I should be logged in as the admin user");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Login With Default Host Credentials")]
        [NUnit.Framework.CategoryAttribute("MustBeHostDefaultCredentials")]
        public virtual void LoginWithDefaultHostCredentials()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Login With Default Host Credentials", new string[] {
                        "MustBeHostDefaultCredentials"});
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I am on the site home page");
#line 27
 testRunner.And("I have pressed Login");
#line 28
 testRunner.When("I enter the default host username");
#line 29
 testRunner.And("I enter the default host password");
#line 30
 testRunner.And("I press Login");
#line 31
 testRunner.Then("I should be logged in as the Host user");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Login Fails With Invalid Credentials")]
        public virtual void LoginFailsWithInvalidCredentials()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Login Fails With Invalid Credentials", ((string[])(null)));
#line 33
this.ScenarioSetup(scenarioInfo);
#line 34
 testRunner.Given("I am on the site home page");
#line 35
 testRunner.And("I have pressed Login");
#line 36
 testRunner.When("I enter an Invalid username");
#line 37
 testRunner.And("I enter an Invalid Password");
#line 38
 testRunner.And("I press Login");
#line 39
 testRunner.Then("I should see a login error");
#line 40
 testRunner.And("I should not be logged in");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
