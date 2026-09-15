using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Contoso.LegacyBank.Portal.Domain;

namespace Contoso.LegacyBank.Portal.Services
{
    // Generated-style WCF contract kept in source so the legacy build is reproducible without Connected Services.
    internal static class AccountContract
    {
        public const string Namespace = "urn:contoso:legacy-bank:accounts:v1";
    }

    [ServiceContract(Namespace = AccountContract.Namespace)]
    public interface IAccountService
    {
        [OperationContract]
        [FaultContract(typeof(AccountFault))]
        CustomerDto GetCustomer(string customerNumber);

        [OperationContract]
        [FaultContract(typeof(AccountFault))]
        IList<AccountDto> GetAccounts(string customerNumber);

        [OperationContract]
        [FaultContract(typeof(AccountFault))]
        IList<TransactionDto> GetTransactions(string accountNumber, DateTime fromDate, DateTime toDate);
    }

    public sealed class AccountServiceClient : ClientBase<IAccountService>, IAccountService
    {
        public AccountServiceClient(Binding binding, EndpointAddress endpointAddress)
            : base(binding, endpointAddress)
        {
        }

        public CustomerDto GetCustomer(string customerNumber)
        {
            return Channel.GetCustomer(customerNumber);
        }

        public IList<AccountDto> GetAccounts(string customerNumber)
        {
            return Channel.GetAccounts(customerNumber);
        }

        public IList<TransactionDto> GetTransactions(string accountNumber, DateTime fromDate, DateTime toDate)
        {
            return Channel.GetTransactions(accountNumber, fromDate, toDate);
        }
    }

    public sealed class AccountServiceGateway : IAccountGateway
    {
        private readonly Uri _endpoint;

        public AccountServiceGateway(Uri endpoint)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException("endpoint");
        }

        public Customer GetCustomer(string customerNumber)
        {
            var dto = Execute(client => client.GetCustomer(customerNumber));
            return dto == null
                ? null
                : new Customer
                {
                    CustomerNumber = dto.CustomerNumber,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    CreatedUtc = dto.CreatedUtc
                };
        }

        public Account[] GetAccounts(string customerNumber)
        {
            var results = Execute(client => client.GetAccounts(customerNumber)) ?? new List<AccountDto>();
            return results.Select(dto => new Account
            {
                AccountNumber = dto.AccountNumber,
                AccountType = dto.AccountType,
                CurrentBalance = dto.Balance,
                AvailableBalance = dto.Balance,
                CurrencyCode = dto.CurrencyCode,
                OpenedUtc = dto.OpenedUtc,
                Status = dto.IsActive ? "Active" : "Closed"
            }).ToArray();
        }

        public Transaction[] GetTransactions(string accountNumber, DateTime fromDate, DateTime toDate)
        {
            var results = Execute(client => client.GetTransactions(accountNumber, fromDate, toDate)) ??
                          new List<TransactionDto>();
            return results.Select(dto => new Transaction
            {
                TransactionId = dto.ExternalId,
                PostedDate = dto.PostedUtc,
                Description = dto.Description,
                Amount = dto.Amount,
                TransactionType = dto.TransactionType
            }).ToArray();
        }

        private T Execute<T>(Func<AccountServiceClient, T> operation)
        {
            var binding = new BasicHttpBinding
            {
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                SendTimeout = TimeSpan.FromSeconds(20),
                ReceiveTimeout = TimeSpan.FromSeconds(20),
                MaxReceivedMessageSize = 1024 * 1024
            };
            var client = new AccountServiceClient(binding, new EndpointAddress(_endpoint));

            try
            {
                var result = operation(client);
                client.Close();
                return result;
            }
            catch
            {
                client.Abort();
                throw;
            }
        }
    }

    [DataContract(Namespace = AccountContract.Namespace)]
    public sealed class CustomerDto
    {
        [DataMember(Order = 1)] public string CustomerNumber { get; set; }
        [DataMember(Order = 2)] public string FirstName { get; set; }
        [DataMember(Order = 3)] public string LastName { get; set; }
        [DataMember(Order = 4)] public string Email { get; set; }
        [DataMember(Order = 5)] public DateTime CreatedUtc { get; set; }
    }

    [DataContract(Namespace = AccountContract.Namespace)]
    public sealed class AccountDto
    {
        [DataMember(Order = 1)] public string AccountNumber { get; set; }
        [DataMember(Order = 2)] public string CustomerNumber { get; set; }
        [DataMember(Order = 3)] public string AccountType { get; set; }
        [DataMember(Order = 4)] public string CurrencyCode { get; set; }
        [DataMember(Order = 5)] public decimal Balance { get; set; }
        [DataMember(Order = 6)] public DateTime OpenedUtc { get; set; }
        [DataMember(Order = 7)] public bool IsActive { get; set; }
    }

    [DataContract(Namespace = AccountContract.Namespace)]
    public sealed class TransactionDto
    {
        [DataMember(Order = 1)] public string ExternalId { get; set; }
        [DataMember(Order = 2)] public string AccountNumber { get; set; }
        [DataMember(Order = 3)] public DateTime PostedUtc { get; set; }
        [DataMember(Order = 4)] public decimal Amount { get; set; }
        [DataMember(Order = 5)] public string Description { get; set; }
        [DataMember(Order = 6)] public string TransactionType { get; set; }
    }

    [DataContract(Namespace = AccountContract.Namespace)]
    public sealed class AccountFault
    {
        [DataMember(Order = 1)] public string Code { get; set; }
        [DataMember(Order = 2)] public string Message { get; set; }
        [DataMember(Order = 3, EmitDefaultValue = false)] public string Field { get; set; }
    }
}
