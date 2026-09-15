using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Contoso.LegacyBank.Portal.Domain;

namespace Contoso.LegacyBank.Portal.Presentation
{
    public static class PortalRules
    {
        public static readonly string[] SeedCustomerNumbers = { "CUST-1001", "CUST-1002" };

        public static bool IsValidCustomerNumber(string customerNumber)
        {
            if (string.IsNullOrWhiteSpace(customerNumber))
            {
                return false;
            }

            var value = customerNumber.Trim();
            return value.Length >= 4 && value.Length <= 20 &&
                   value.All(character => char.IsLetterOrDigit(character) || character == '-');
        }

        public static string ValidateDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
            {
                return "From date cannot be later than the to date.";
            }

            if ((toDate.Date - fromDate.Date).TotalDays > 366)
            {
                return "Select a date range of one year or less.";
            }

            return string.Empty;
        }

        public static decimal CalculateNetActivity(IEnumerable<Transaction> transactions)
        {
            return (transactions ?? Enumerable.Empty<Transaction>()).Sum(transaction => transaction.Amount);
        }

        public static string FormatMoney(decimal amount)
        {
            return amount.ToString("C2", CultureInfo.CurrentCulture);
        }

        public static string DisplayStatementStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return "Not requested";
            }

            switch (status.Trim().ToLowerInvariant())
            {
                case "queued":
                case "pending":
                    return "Queued for generation";
                case "processing":
                case "generating":
                    return "Generating PDF";
                case "completed":
                case "ready":
                    return "Ready to open";
                case "failed":
                case "error":
                    return "Generation failed";
                default:
                    return status.Trim();
            }
        }

        public static bool TryGetDocumentUri(string location, out Uri uri)
        {
            uri = null;
            if (string.IsNullOrWhiteSpace(location))
            {
                return false;
            }

            if (Path.IsPathRooted(location))
            {
                uri = new Uri(Path.GetFullPath(location));
                return uri.IsFile;
            }

            Uri candidate;
            if (!Uri.TryCreate(location, UriKind.Absolute, out candidate) ||
                (candidate.Scheme != Uri.UriSchemeHttp && candidate.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            uri = candidate;
            return true;
        }
    }
}
