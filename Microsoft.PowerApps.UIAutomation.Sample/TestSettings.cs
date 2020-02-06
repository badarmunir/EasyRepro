// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using Microsoft.PowerApps.TestFramework.Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.IO;

namespace Microsoft.PowerApps.TestFramework.Tests
{
    public class TestSettings
    {
        private static readonly string Type = ConfigurationManager.AppSettings["BrowserType"];
        private static readonly string DriversPath = ConfigurationManager.AppSettings["DriversPath"];
        private static readonly bool? UsePrivateMode = Convert.ToBoolean(ConfigurationManager.AppSettings["UsePrivateMode"]);

        public static BrowserOptions Options = new BrowserOptions
        {
            BrowserType = (BrowserType)Enum.Parse(typeof(BrowserType), Type),
            PrivateMode = UsePrivateMode ?? true,
            FireEvents = false,
            Headless = false,
            UserAgent = false,
            DriversPath = Path.IsPathRooted(DriversPath) ? DriversPath : Path.Combine(Directory.GetCurrentDirectory(), DriversPath)
        };
    }
}