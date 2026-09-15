using System;
using Contoso.LegacyBank.Portal.Domain;

namespace Contoso.LegacyBank.Portal.Services
{
    public interface IAccountGateway
    {
        Customer GetCustomer(string customerNumber);
        Account[] GetAccounts(string customerNumber);
        Transaction[] GetTransactions(string accountNumber, DateTime fromDate, DateTime toDate);
    }

    public interface IStatementGateway
    {
        StatementResult RequestStatement(StatementRequest request);
        StatementResult GetStatement(string statementId);
    }
}
