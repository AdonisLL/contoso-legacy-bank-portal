using System;
using System.Runtime.Serialization;

namespace Contoso.LegacyBank.Portal.Domain
{
    [DataContract]
    public sealed class Customer
    {
        [DataMember(Order = 1)] public string CustomerNumber { get; set; }
        [DataMember(Order = 2)] public string FirstName { get; set; }
        [DataMember(Order = 3)] public string LastName { get; set; }
        [DataMember(Order = 4)] public string Email { get; set; }
        [DataMember(Order = 5)] public string Phone { get; set; }
        [DataMember(Order = 6)] public string AddressLine1 { get; set; }
        [DataMember(Order = 7)] public string City { get; set; }
        [DataMember(Order = 8)] public string State { get; set; }
        [DataMember(Order = 9)] public string PostalCode { get; set; }
        [DataMember(Order = 10)] public DateTime CreatedUtc { get; set; }

        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName).Trim(); }
        }

        public string MailingAddress
        {
            get { return string.Format("{0}, {1}, {2} {3}", AddressLine1, City, State, PostalCode).Trim(' ', ','); }
        }
    }

    [DataContract]
    public sealed class Account
    {
        [DataMember(Order = 1)] public string AccountNumber { get; set; }
        [DataMember(Order = 2)] public string AccountType { get; set; }
        [DataMember(Order = 3)] public decimal CurrentBalance { get; set; }
        [DataMember(Order = 4)] public decimal AvailableBalance { get; set; }
        [DataMember(Order = 5)] public string Status { get; set; }
        [DataMember(Order = 6)] public string CurrencyCode { get; set; }
        [DataMember(Order = 7)] public DateTime OpenedUtc { get; set; }
    }

    [DataContract]
    public sealed class Transaction
    {
        [DataMember(Order = 1)] public string TransactionId { get; set; }
        [DataMember(Order = 2)] public DateTime PostedDate { get; set; }
        [DataMember(Order = 3)] public string Description { get; set; }
        [DataMember(Order = 4)] public decimal Amount { get; set; }
        [DataMember(Order = 5)] public decimal RunningBalance { get; set; }
        [DataMember(Order = 6)] public string TransactionType { get; set; }
    }

    public sealed class StatementRequest
    {
        public string CustomerNumber { get; set; }
        public string AccountNumber { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public sealed class StatementResult
    {
        public Guid JobId { get; set; }
        public string Status { get; set; }
        public string StatusUrl { get; set; }
        public string PdfPath { get; set; }
        public string Error { get; set; }
        public string Detail { get; set; }
        public DateTime? UpdatedUtc { get; set; }

        public string StatementId { get { return JobId == Guid.Empty ? null : JobId.ToString(); } }
        public string PdfUrl { get { return PdfPath; } }
        public string Message { get { return string.IsNullOrWhiteSpace(Detail) ? Error : Detail; } }
    }
}
