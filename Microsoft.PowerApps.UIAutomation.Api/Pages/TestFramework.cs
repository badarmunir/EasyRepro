// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Dynamics365.UIAutomation.Browser;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.PowerApps.UIAutomation.Api
{

    /// <summary>
    ///  Test Framework methods.
    ///  </summary>
    public class TestFramework
        : AppPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFramework"/> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        public TestFramework(InteractiveBrowser browser)
            : base(browser)
        {
        }

        public BrowserCommandResult<JObject> ExecuteTestFramework(Uri uri)
        {
            return this.Execute(GetOptions("Execute Test Framework"), driver =>
            {
                // Navigate to TestSuite or TestCase URL
                InitiateTest(driver, uri);

                // Check for existence of permissions dialog (1st test load for user)
                CheckForPermissionDialog(driver);

                // Wait for test completion and collect results
                JObject testResults = WaitForTestResults(driver);

                return testResults;
            });
        }

        public class TestSuiteResult
        {
            public string TestSuiteId { get; set; }
            public string TestSuiteName { get; set; }
            public string TestSuiteDescription { get; set; }
            public long StartTime { get; set; }
            public long EndTime { get; set; }
            public int TestsPassed { get; set; }
            public int TestsFailed { get; set; }
        }

        internal void CheckForPermissionDialog(IWebDriver driver)
        {
            // Switch to default content
            driver.SwitchTo().DefaultContent();

            var dialogButtons = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.TestFramework.PermissionDialogButtons]), new TimeSpan(0, 0, 5));

            if (dialogButtons != null)
            {
                // Should be two buttons (Allow, Don't Allow)
                var buttons = dialogButtons.FindElements(By.TagName("button"));

                foreach (var b in buttons)
                {
                    if (b.Text.Equals("Allow"))
                    {
                        b.Hover(driver, true);
                        b.Click(true);
                        b.SendKeys(Keys.Enter);
                        driver.WaitForPageToLoad();
                    }
                }
            }
        }

        internal JObject WaitForTestResults(IWebDriver driver)
        {
            JObject jsonResultString = new JObject();
            jsonResultString = driver.WaitForTestResults();

            return jsonResultString;

            /*
            // Switch to app frame
            driver.SwitchTo().Frame("fullscreen-app-host");

            // Define for current state of TestExecution
            int testExecutionState = 0;


            do
            {
                driver.WaitForTransaction();
                jsonResultString = driver.GetJsonObject("AppMagic.TestStudio.GetTestExecutionInfo()");
                testExecutionState = (int)jsonResultString.GetValue("ExecutionState");
            }
            while (testExecutionState == 0 || testExecutionState == 1);

            return jsonResultString;
            */
        }

        internal void InitiateTest(IWebDriver driver, Uri uri)
        {
            driver.Navigate().GoToUrl(uri);

            // Wait for page to load
            driver.WaitForPageToLoad();
        }

        public Tuple<int, int> ReportResultsToDevOps(JObject jObject)
        {
            var testExecutionMode = (int)jObject.GetValue("ExecutionMode");

            int passCount = 0;
            int failCount = 0;


            if (testExecutionMode == 0)
            {
                // TestCase
            }
            else if (testExecutionMode == 1)
            {
                // Put JSON result objects into a TestSuiteResult
                var testSuiteResults = jObject["TestSuiteResult"]?.ToObject<TestSuiteResult>();

                passCount = testSuiteResults.TestsPassed;
                failCount = testSuiteResults.TestsFailed;

                // Calculate Total Execution Time
                int testSuiteElapsedMs = (int)(testSuiteResults.EndTime - testSuiteResults.StartTime);
                TimeSpan testSuiteElapsedTime = new TimeSpan(0,0,0,0, testSuiteElapsedMs);
            
                // Output results to Console
                Console.WriteLine($"TestSuite Name: {testSuiteResults.TestSuiteName} with ID {testSuiteResults.TestSuiteId}");
                Console.WriteLine($"TestSuite Description: {testSuiteResults.TestSuiteDescription}");
                Console.WriteLine($"Tests Passed: {testSuiteResults.TestsPassed}");
                Console.WriteLine($"Tests Failed: {testSuiteResults.TestsPassed}");
                Console.WriteLine($"TestSuite execution time: {testSuiteElapsedTime}");

            }

            var countPassFailResult = Tuple.Create(passCount, failCount);
            return countPassFailResult;
        }
    }
}