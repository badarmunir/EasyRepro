using Microsoft.Dynamics365.UIAutomation.Browser;
using Microsoft.PowerApps.UIAutomation.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Security;

namespace Microsoft.PowerApps.UIAutomation.Sample.TestFramework
{
    [TestClass]
    public class RunTestFrameworkWithAppConfig
    {

        private readonly SecureString _username = ConfigurationManager.AppSettings["OnlineUsername"].ToSecureString();
        private readonly SecureString _password = ConfigurationManager.AppSettings["OnlinePassword"].ToSecureString();
        private readonly Uri _xrmUri = new Uri(ConfigurationManager.AppSettings["OnlineUrl"]);
        private readonly Uri _testFrameworkUri = new Uri(ConfigurationManager.AppSettings["TestFrameworkUrl"]);
        private static readonly string Type = ConfigurationManager.AppSettings["BrowserType"];
        private static readonly string DriversPath = ConfigurationManager.AppSettings["DriversPath"];
        private static readonly bool? UsePrivateMode = Convert.ToBoolean(ConfigurationManager.AppSettings["UsePrivateMode"]);


        [TestCategory("PowerAppsTestFramework-AppConfig")]
        [Priority(1)]
        [TestMethod]
        public void RunTestFramework()
        {
            BrowserOptions options = TestSettings.Options;
            options.BrowserType = (BrowserType)Enum.Parse(typeof(BrowserType), Type);

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
                            appBrowser.OnlineLogin.Login(_xrmUri, _username, _password);
                            break;
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine($"Exception on Attempt #{retryCount + 1}: {exc}");

                            if (retryCount+1 == Reference.Login.SignInAttempts)
                            {
                                Console.WriteLine($"Login failed after {retryCount + 1} attempts.");
                                throw new InvalidOperationException($"Login failed after {retryCount + 1} attempts. Exception Details: {exc}");
                            }
                            else
                            {
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

                    throw;
                }

                Console.WriteLine("Power Apps Test Framework Test Suite Execution Completed.");
            }
        }        
    }
}
