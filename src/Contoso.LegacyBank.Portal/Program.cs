using System;
using System.Windows.Forms;
using Contoso.LegacyBank.Portal.Infrastructure;
using Contoso.LegacyBank.Portal.Services;
using Contoso.LegacyBank.Portal.UI;

namespace Contoso.LegacyBank.Portal
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var settings = PortalConfiguration.Load();
            IAccountGateway accounts = new AccountServiceGateway(settings.AccountServiceUrl);
            IStatementGateway statements = new StatementApiClient(settings.StatementApiUrl);
            Application.Run(new MainForm(accounts, statements));
        }
    }
}
