using Microsoft.Dynamics365.UIAutomation.Browser;
using Microsoft.PowerApps.UIAutomation.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
                    Console.WriteLine($"Attempting to execute Test Suite: {_xrmUri}");

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

                    Console.WriteLine("Power Apps Test Framework Test Suite Execution Starting...");

                    appBrowser.TestFramework.ExecuteTestFramework(_testFrameworkUri);

                    appBrowser.ThinkTime(5000);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred attempting to run the Power Apps Test Suite: {e}");

                    _resultsDirectory = TestContext.TestResultsDirectory;
                    Console.WriteLine($"Current results directory location: {_resultsDirectory}");
                    string location = $@"{_resultsDirectory}\RunTestSuite-GenericError.jpeg";

                    appBrowser.TakeWindowScreenShot(location, OpenQA.Selenium.ScreenshotImageFormat.Jpeg);
                    _testContext.AddResultFile(location);

                    throw;
                }

                Console.WriteLine("Power Apps Test Framework Test Suite Execution Completed.");
            }
        }        
    }
}
