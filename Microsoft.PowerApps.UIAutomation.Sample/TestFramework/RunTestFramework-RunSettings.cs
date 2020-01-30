using Microsoft.Dynamics365.UIAutomation.Browser;
using Microsoft.PowerApps.UIAutomation.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace Microsoft.PowerApps.UIAutomation.Sample.TestFramework
{
    [TestClass]
    public class RunTestFrameworkWithRunSettings
    {
        private static string _username = "";
        private static string _password = "";
        private static BrowserType _browserType;
        private static Uri _xrmUri;
        private static Uri _testFrameworkUri;
        private static string _resultsDirectory = "";
        private static string _driversPath = "";
        private static string _usePrivateMode;


        public TestContext TestContext { get; set; }

        private static TestContext _testContext;

        [ClassInitialize]
        public static void Initialize(TestContext TestContext)
        {
            _testContext = TestContext;

            _username = _testContext.Properties["OnlineUsername"].ToString();
            _password = _testContext.Properties["OnlinePassword"].ToString();
            _xrmUri = new Uri(_testContext.Properties["OnlineUrl"].ToString());
            _testFrameworkUri = new Uri(_testContext.Properties["TestFrameworkUrl"].ToString());
            _resultsDirectory = _testContext.Properties["ResultsDirectory"].ToString();
            _browserType = (BrowserType)Enum.Parse(typeof(BrowserType), _testContext.Properties["BrowserType"].ToString());
            _driversPath = _testContext.Properties["DriversPath"].ToString();
            _usePrivateMode = _testContext.Properties["UsePrivateMode"].ToString();

        }

        [TestCategory("PowerAppsTestFramework")]
        [Priority(1)]
        [TestMethod]
        public void RunTestSuite()
        {
            BrowserOptions options = TestSettings.Options;
            options.BrowserType = _browserType;

            using (var appBrowser = new PowerAppBrowser(options))
            {
                try
                {
                    //Login To PowerApps
                    Debug.WriteLine($"Attempting to authenticate to Maker Portal: {_xrmUri}");

                    for (int retryCount = 0; retryCount < Reference.Login.SignInAttempts; retryCount++)
                    {
                        try
                        {
                            appBrowser.OnlineLogin.Login(_xrmUri, _username.ToSecureString(), _password.ToSecureString());
                            break;
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine($"Exception on Attempt #{retryCount + 1}: {exc}");

                            if (retryCount+1 == Reference.Login.SignInAttempts)
                            {
                                // Login exception occurred, take screenshot
                                _resultsDirectory = TestContext.TestResultsDirectory;
                                string location = $@"{_resultsDirectory}\RunTestSuite-LoginErrorAttempt{retryCount + 1}.jpeg";

                                appBrowser.TakeWindowScreenShot(location, OpenQA.Selenium.ScreenshotImageFormat.Jpeg);
                                _testContext.AddResultFile(location);

                                // Max Sign-In Attempts reached
                                Console.WriteLine($"Login failed after {retryCount + 1} attempts.");
                                throw new InvalidOperationException($"Login failed after {retryCount + 1} attempts. Exception Details: {exc}");
                            }
                            else
                            {
                                // Login exception occurred, take screenshot
                                _resultsDirectory = TestContext.TestResultsDirectory;
                                string location = $@"{_resultsDirectory}\RunTestSuite-LoginErrorAttempt{retryCount+1}.jpeg";

                                appBrowser.TakeWindowScreenShot(location, OpenQA.Selenium.ScreenshotImageFormat.Jpeg);
                                _testContext.AddResultFile(location);

                                //Navigate away and retry
                                appBrowser.Navigate("about:blank");

                                Console.WriteLine($"Login failed after attempt #{retryCount + 1}.");
                                continue;
                            }
                        }
                    }

                    Console.WriteLine("Power Apps Test Framework Execution Starting...");

                    // Initialize TestFrameworok results JSON object
                    JObject testFrameworkResults = new JObject();

                    // Execute TestFramework and return JSON result object
                    testFrameworkResults = appBrowser.TestFramework.ExecuteTestFramework(_testFrameworkUri);

                    // Report Results to DevOps Pipeline                    
                    var testResultCount = appBrowser.TestFramework.ReportResultsToDevOps(testFrameworkResults);
                    
                    if (testResultCount.Item1 > 0 && testResultCount.Item2 > 0)
                    {
                        string message = ("\n" + "Inconclusive Test Result: " + "\n" + $"Test Pass Count: {testResultCount.Item1}" + "\n" + $"Test Fail Count: {testResultCount.Item2}" + "\n" + "Please see the console log for more information.");
                        Assert.Inconclusive(message);
                    }
                    else if (testResultCount.Item2 > 0)
                    {
                        string message = ("\n" + "Test Failed: " + "\n" + $"Test Fail Count: {testResultCount.Item2}" + "\n" + "Please see the console log for more information.");
                        Assert.Fail(message);
                    }
                    else if (testResultCount.Item1 > 0)
                    {
                        var success = true;
                        string message = ("\n" + "Success: " + "\n" + $"Test Pass Count: {testResultCount.Item1}");
                        Assert.IsTrue(success, message);
                    }

                    appBrowser.ThinkTime(5000);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred attempting to run the Power Apps Test Framework: {e}");

                    _resultsDirectory = TestContext.TestResultsDirectory;
                    Console.WriteLine($"Current results directory location: {_resultsDirectory}");
                    string location = $@"{_resultsDirectory}\RunTestSuite-GenericError.jpeg";

                    appBrowser.TakeWindowScreenShot(location, OpenQA.Selenium.ScreenshotImageFormat.Jpeg);
                    _testContext.AddResultFile(location);

                    throw;
                }

                Console.WriteLine("Power Apps Test Framework Execution Completed.");
            }
        }        
    }
}
