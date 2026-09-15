using System;
using System.Globalization;
using Contoso.LegacyBank.Portal.Domain;
using Contoso.LegacyBank.Portal.Presentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Contoso.LegacyBank.Portal.Tests
{
    [TestClass]
    public sealed class PortalRulesTests
    {
        [TestMethod]
        [DataRow("C100001", true)]
        [DataRow("cust-42", true)]
        [DataRow("", false)]
        [DataRow("ABC!", false)]
        public void CustomerNumberValidationHandlesExpectedFormats(string input, bool expected)
        {
            Assert.AreEqual(expected, PortalRules.IsValidCustomerNumber(input));
        }

        [TestMethod]
        public void DateRangeRejectsReversedDates()
        {
            var message = PortalRules.ValidateDateRange(new DateTime(2026, 2, 2), new DateTime(2026, 2, 1));
            StringAssert.Contains(message, "cannot be later");
        }

        [TestMethod]
        public void DateRangeRejectsMoreThanOneYear()
        {
            var message = PortalRules.ValidateDateRange(new DateTime(2024, 1, 1), new DateTime(2025, 1, 3));
            StringAssert.Contains(message, "one year");
        }

        [TestMethod]
        public void NetActivitySumsCreditsAndDebits()
        {
            var transactions = new[]
            {
                new Transaction { Amount = 250.00m },
                new Transaction { Amount = -75.25m },
                new Transaction { Amount = -24.75m }
            };
            Assert.AreEqual(150.00m, PortalRules.CalculateNetActivity(transactions));
        }

        [TestMethod]
        public void MoneyFormattingUsesCurrencyAndTwoDecimals()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                Assert.AreEqual("$1,234.50", PortalRules.FormatMoney(1234.5m));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [TestMethod]
        [DataRow("queued", "Queued for generation")]
        [DataRow("processing", "Generating PDF")]
        [DataRow("completed", "Ready to open")]
        [DataRow("failed", "Generation failed")]
        public void StatementStatusesAreFriendly(string serviceStatus, string expected)
        {
            Assert.AreEqual(expected, PortalRules.DisplayStatementStatus(serviceStatus));
        }

        [TestMethod]
        public void SeedHintsMatchLocalAccountsService()
        {
            CollectionAssert.AreEqual(new[] { "CUST-1001", "CUST-1002" }, PortalRules.SeedCustomerNumbers);
        }

        [TestMethod]
        public void DocumentLocationAllowsLocalPdfAndHttps()
        {
            Uri local;
            Uri remote;

            Assert.IsTrue(PortalRules.TryGetDocumentUri(@"C:\Statements\statement.pdf", out local));
            Assert.IsTrue(local.IsFile);
            Assert.IsTrue(PortalRules.TryGetDocumentUri("https://localhost/statements/statement.pdf", out remote));
            Assert.AreEqual(Uri.UriSchemeHttps, remote.Scheme);
        }

        [TestMethod]
        public void DocumentLocationRejectsUnsafeScheme()
        {
            Uri ignored;
            Assert.IsFalse(PortalRules.TryGetDocumentUri("javascript:alert(1)", out ignored));
        }
    }
}
