using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Windows.Forms;
using Contoso.LegacyBank.Portal.Domain;
using Contoso.LegacyBank.Portal.Presentation;
using Contoso.LegacyBank.Portal.Services;

namespace Contoso.LegacyBank.Portal.UI
{
    public sealed class MainForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(14, 42, 79);
        private static readonly Color Blue = Color.FromArgb(25, 92, 160);
        private static readonly Color PaleBlue = Color.FromArgb(235, 243, 251);
        private readonly IAccountGateway _accounts;
        private readonly IStatementGateway _statements;

        private ComboBox _role;
        private ComboBox _login;
        private TextBox _customerNumber;
        private Button _search;
        private Label _nameValue;
        private Label _numberValue;
        private Label _emailValue;
        private Label _createdValue;
        private DataGridView _accountsGrid;
        private DataGridView _transactionsGrid;
        private DateTimePicker _fromDate;
        private DateTimePicker _toDate;
        private Label _netActivity;
        private Button _requestStatement;
        private Button _refreshStatement;
        private Button _openPdf;
        private Label _statementStatus;
        private Label _statementId;
        private Label _status;
        private Customer _customer;
        private Account _selectedAccount;
        private StatementResult _currentStatement;

        public MainForm(IAccountGateway accounts, IStatementGateway statements)
        {
            _accounts = accounts ?? throw new ArgumentNullException("accounts");
            _statements = statements ?? throw new ArgumentNullException("statements");
            InitializeForm();
            BuildLayout();
            ConfigureEvents();
            ApplyLoggedInState(false);
        }

        private void InitializeForm()
        {
            Text = "Contoso Bank | Customer Service Portal";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1120, 720);
            Size = new Size(1280, 820);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            Controls.Add(root);

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildSearchBar(), 0, 1);
            root.Controls.Add(BuildWorkspace(), 0, 2);
            root.Controls.Add(BuildStatusBar(), 0, 3);
        }

        private Control BuildHeader()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Navy,
                ColumnCount = 3,
                Padding = new Padding(22, 10, 22, 10)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));

            var brand = new Label
            {
                AutoSize = true,
                Text = "CONTOSO BANK\nCustomer Service Portal",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 15F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(brand, 0, 0);
            header.Controls.Add(LabeledCombo("Demo role", out _role,
                new[] { "Customer Service", "Teller", "Branch Manager" }), 1, 0);
            header.Controls.Add(LabeledCombo("Demo login", out _login,
                new[] { "Alex Morgan", "Jordan Lee", "Taylor Brooks" }), 2, 0);
            return header;
        }

        private Control BuildSearchBar()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = PaleBlue,
                ColumnCount = 4,
                Padding = new Padding(22, 14, 22, 12)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

            panel.Controls.Add(new Label
            {
                Text = "Customer number",
                Font = new Font("Segoe UI Semibold", 10F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            _customerNumber = new TextBox
            {
                Dock = DockStyle.Fill,
                CharacterCasing = CharacterCasing.Upper,
                Font = new Font("Segoe UI", 11F)
            };
            panel.Controls.Add(_customerNumber, 1, 0);
            _search = PrimaryButton("Search");
            panel.Controls.Add(_search, 2, 0);
            panel.Controls.Add(new Label
            {
                Text = "Demo customers: " + string.Join(", ", PortalRules.SeedCustomerNumbers),
                ForeColor = Color.FromArgb(70, 84, 103),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            }, 3, 0);
            return panel;
        }

        private Control BuildWorkspace()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 5) };
            tabs.TabPages.Add(BuildCustomerTab());
            tabs.TabPages.Add(BuildTransactionsTab());
            tabs.TabPages.Add(BuildStatementsTab());
            return tabs;
        }

        private TabPage BuildCustomerTab()
        {
            var tab = NewTab("Customer & Accounts");
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 165F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tab.Controls.Add(layout);

            layout.Controls.Add(SectionTitle("Customer details"), 0, 0);
            var details = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            AddDetail(details, 0, "Name", out _nameValue);
            AddDetail(details, 1, "Customer #", out _numberValue);
            AddDetail(details, 2, "Email", out _emailValue);
            AddDetail(details, 3, "Customer since", out _createdValue);
            layout.Controls.Add(details, 0, 1);

            var accountPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            accountPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            accountPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            accountPanel.Controls.Add(SectionTitle("Accounts (select an account to view activity)"), 0, 0);
            _accountsGrid = CreateGrid();
            accountPanel.Controls.Add(_accountsGrid, 0, 1);
            layout.Controls.Add(accountPanel, 0, 2);
            return tab;
        }

        private TabPage BuildTransactionsTab()
        {
            var tab = NewTab("Transaction History");
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(16)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tab.Controls.Add(layout);

            var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            filters.Controls.Add(FilterLabel("From"));
            _fromDate = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
            filters.Controls.Add(_fromDate);
            filters.Controls.Add(FilterLabel("To"));
            _toDate = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            filters.Controls.Add(_toDate);
            var load = PrimaryButton("Load transactions");
            load.Width = 150;
            load.Click += delegate { LoadTransactions(); };
            filters.Controls.Add(load);
            layout.Controls.Add(filters, 0, 0);

            _transactionsGrid = CreateGrid();
            layout.Controls.Add(_transactionsGrid, 0, 1);
            _netActivity = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI Semibold", 10F),
                Text = "Net activity: —"
            };
            layout.Controls.Add(_netActivity, 0, 2);
            return tab;
        }

        private TabPage BuildStatementsTab()
        {
            var tab = NewTab("Statements");
            var content = new TableLayoutPanel
            {
                Width = 720,
                Height = 300,
                Location = new Point(30, 30),
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(18),
                BackColor = PaleBlue
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            content.Controls.Add(SectionTitle("Statement request"), 0, 0);
            content.SetColumnSpan(content.GetControlFromPosition(0, 0), 2);
            content.Controls.Add(DetailCaption("Selected account"), 0, 1);
            var accountHint = DetailValue();
            accountHint.Name = "StatementAccountHint";
            accountHint.Text = "Choose an account on the Customer & Accounts tab.";
            content.Controls.Add(accountHint, 1, 1);
            content.Controls.Add(DetailCaption("Statement ID"), 0, 2);
            _statementId = DetailValue();
            content.Controls.Add(_statementId, 1, 2);
            content.Controls.Add(DetailCaption("Status"), 0, 3);
            _statementStatus = DetailValue();
            _statementStatus.Text = PortalRules.DisplayStatementStatus(null);
            content.Controls.Add(_statementStatus, 1, 3);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _requestStatement = PrimaryButton("Request statement");
            _requestStatement.Width = 150;
            _refreshStatement = SecondaryButton("Refresh status");
            _refreshStatement.Width = 125;
            _openPdf = SecondaryButton("Open PDF");
            _openPdf.Width = 100;
            actions.Controls.Add(_requestStatement);
            actions.Controls.Add(_refreshStatement);
            actions.Controls.Add(_openPdf);
            content.Controls.Add(actions, 1, 4);
            tab.Controls.Add(content);
            return tab;
        }

        private Control BuildStatusBar()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(242, 242, 242) };
            _status = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Select a demo login to begin.",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                ForeColor = Color.FromArgb(75, 75, 75)
            };
            panel.Controls.Add(_status);
            return panel;
        }

        private void ConfigureEvents()
        {
            _role.SelectedIndex = 0;
            _login.SelectedIndex = -1;
            _login.SelectedIndexChanged += delegate
            {
                ApplyLoggedInState(_login.SelectedIndex >= 0);
                _status.Text = _login.SelectedIndex >= 0
                    ? "Signed in as " + _login.SelectedItem + " (" + _role.SelectedItem + ")."
                    : "Select a demo login to begin.";
            };
            _search.Click += delegate { SearchCustomer(); };
            _customerNumber.KeyDown += delegate(object sender, KeyEventArgs args)
            {
                if (args.KeyCode == Keys.Enter)
                {
                    SearchCustomer();
                    args.SuppressKeyPress = true;
                }
            };
            _accountsGrid.SelectionChanged += delegate { SelectAccount(); };
            _requestStatement.Click += delegate { RequestStatement(); };
            _refreshStatement.Click += delegate { RefreshStatement(); };
            _openPdf.Click += delegate { OpenStatementPdf(); };
        }

        private void SearchCustomer()
        {
            var customerNumber = _customerNumber.Text.Trim();
            if (!PortalRules.IsValidCustomerNumber(customerNumber))
            {
                ShowValidation("Enter a valid customer number (4–20 letters, numbers, or hyphens).");
                return;
            }

            RunServiceCall("Searching for customer " + customerNumber + "…", delegate
            {
                var customer = _accounts.GetCustomer(customerNumber);
                if (customer == null)
                {
                    ClearCustomer();
                    MessageBox.Show(this, "No customer was found for " + customerNumber + ".", "Customer not found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var accounts = _accounts.GetAccounts(customerNumber);
                _customer = customer;
                _nameValue.Text = customer.FullName;
                _numberValue.Text = customer.CustomerNumber;
                _emailValue.Text = customer.Email;
                _createdValue.Text = customer.CreatedUtc.ToLocalTime().ToString("d");
                _accountsGrid.DataSource = new BindingList<Account>(accounts.ToList());
                FormatAccountGrid();
                _status.Text = string.Format("Loaded {0} account(s) for {1}.", accounts.Length, customer.FullName);
                if (accounts.Length == 0)
                {
                    _selectedAccount = null;
                    ApplyAccountState();
                }
            });
        }

        private void SelectAccount()
        {
            _selectedAccount = _accountsGrid.CurrentRow == null
                ? null
                : _accountsGrid.CurrentRow.DataBoundItem as Account;
            _currentStatement = null;
            ApplyAccountState();
        }

        private void LoadTransactions()
        {
            if (_selectedAccount == null)
            {
                ShowValidation("Select an account before loading transactions.");
                return;
            }

            var dateError = PortalRules.ValidateDateRange(_fromDate.Value, _toDate.Value);
            if (!string.IsNullOrEmpty(dateError))
            {
                ShowValidation(dateError);
                return;
            }

            RunServiceCall("Loading transaction history…", delegate
            {
                var transactions = _accounts.GetTransactions(
                    _selectedAccount.AccountNumber, _fromDate.Value.Date, _toDate.Value.Date);
                _transactionsGrid.DataSource = new BindingList<Transaction>(transactions.ToList());
                FormatTransactionGrid();
                _netActivity.Text = "Net activity: " + PortalRules.FormatMoney(PortalRules.CalculateNetActivity(transactions));
                _status.Text = string.Format("Loaded {0} transaction(s) for account {1}.",
                    transactions.Length, _selectedAccount.AccountNumber);
            });
        }

        private void RequestStatement()
        {
            if (_customer == null || _selectedAccount == null)
            {
                ShowValidation("Search for a customer and select an account before requesting a statement.");
                return;
            }

            var dateError = PortalRules.ValidateDateRange(_fromDate.Value, _toDate.Value);
            if (!string.IsNullOrEmpty(dateError))
            {
                ShowValidation(dateError);
                return;
            }

            RunServiceCall("Submitting statement request…", delegate
            {
                _currentStatement = _statements.RequestStatement(new StatementRequest
                {
                    CustomerNumber = _customer.CustomerNumber,
                    AccountNumber = _selectedAccount.AccountNumber,
                    FromDate = _fromDate.Value.Date,
                    ToDate = _toDate.Value.Date
                });
                RenderStatement();
                _status.Text = "Statement request submitted.";
            });
        }

        private void RefreshStatement()
        {
            if (_currentStatement == null || string.IsNullOrWhiteSpace(_currentStatement.StatementId))
            {
                ShowValidation("Request a statement before refreshing its status.");
                return;
            }

            RunServiceCall("Refreshing statement status…", delegate
            {
                _currentStatement = _statements.GetStatement(_currentStatement.StatementId);
                RenderStatement();
                _status.Text = "Statement status refreshed.";
            });
        }

        private void OpenStatementPdf()
        {
            if (_currentStatement == null || string.IsNullOrWhiteSpace(_currentStatement.PdfUrl))
            {
                ShowValidation("The PDF is not available yet. Refresh the statement status.");
                return;
            }

            Uri pdfUri;
            if (!PortalRules.TryGetDocumentUri(_currentStatement.PdfUrl, out pdfUri))
            {
                ShowValidation("The statement service returned an invalid PDF location.");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(pdfUri.AbsoluteUri) { UseShellExecute = true });
                _status.Text = "Opened statement PDF.";
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Windows could not open the statement PDF.\n\n" + exception.Message,
                    "Unable to open PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunServiceCall(string busyMessage, Action operation)
        {
            SetBusy(true, busyMessage);
            try
            {
                operation();
            }
            catch (StatementApiException exception)
            {
                _statementStatus.Text = "Generation failed";
                MessageBox.Show(this, exception.Message, "Statement generation failure",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FaultException<AccountFault> exception)
            {
                _status.Text = "The request was rejected.";
                ShowValidation(exception.Detail == null ? exception.Message : exception.Detail.Message);
            }
            catch (CommunicationException exception)
            {
                ShowUnavailable("accounts", exception.Message);
            }
            catch (TimeoutException exception)
            {
                ShowUnavailable("requested", exception.Message);
            }
            catch (HttpRequestException exception)
            {
                ShowUnavailable("statements", exception.Message);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "The operation could not be completed.\n\n" + exception.Message,
                    "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, _status.Text);
            }
        }

        private void RenderStatement()
        {
            _statementId.Text = _currentStatement == null ? "—" : _currentStatement.StatementId ?? "—";
            _statementStatus.Text = PortalRules.DisplayStatementStatus(_currentStatement == null ? null : _currentStatement.Status);
            _openPdf.Enabled = _currentStatement != null && !string.IsNullOrWhiteSpace(_currentStatement.PdfUrl);
            _refreshStatement.Enabled = _currentStatement != null && !string.IsNullOrWhiteSpace(_currentStatement.StatementId);
            if (_currentStatement != null && !string.IsNullOrWhiteSpace(_currentStatement.Message) &&
                string.Equals(_currentStatement.Status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, _currentStatement.Message, "Statement generation failure",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyLoggedInState(bool enabled)
        {
            _customerNumber.Enabled = enabled;
            _search.Enabled = enabled;
            if (!enabled)
            {
                ClearCustomer();
            }
        }

        private void ApplyAccountState()
        {
            _requestStatement.Enabled = _selectedAccount != null;
            _refreshStatement.Enabled = false;
            _openPdf.Enabled = false;
            _statementId.Text = "—";
            _statementStatus.Text = PortalRules.DisplayStatementStatus(null);
            var hint = FindControl<Label>(this, "StatementAccountHint");
            if (hint != null)
            {
                hint.Text = _selectedAccount == null
                    ? "Choose an account on the Customer & Accounts tab."
                    : _selectedAccount.AccountNumber + " · " + _selectedAccount.AccountType;
            }
        }

        private void ClearCustomer()
        {
            _customer = null;
            _selectedAccount = null;
            _currentStatement = null;
            foreach (var value in new[] { _nameValue, _numberValue, _emailValue, _createdValue })
            {
                if (value != null)
                {
                    value.Text = "—";
                }
            }
            if (_accountsGrid != null) _accountsGrid.DataSource = null;
            if (_transactionsGrid != null) _transactionsGrid.DataSource = null;
            if (_netActivity != null) _netActivity.Text = "Net activity: —";
            if (_requestStatement != null) ApplyAccountState();
        }

        private void SetBusy(bool busy, string message)
        {
            UseWaitCursor = busy;
            _status.Text = message;
            _search.Enabled = !busy && _login.SelectedIndex >= 0;
            _requestStatement.Enabled = !busy && _selectedAccount != null;
            _refreshStatement.Enabled = !busy && _currentStatement != null &&
                                        !string.IsNullOrWhiteSpace(_currentStatement.StatementId);
            Application.DoEvents();
        }

        private void ShowUnavailable(string serviceName, string details)
        {
            _status.Text = "Service unavailable.";
            MessageBox.Show(this,
                "The " + serviceName + " service is unavailable. Confirm the local service is running and try again.\n\n" +
                details, "Service unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowValidation(string message)
        {
            MessageBox.Show(this, message, "Check your entry", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FormatAccountGrid()
        {
            ConfigureColumn(_accountsGrid, "AccountNumber", "Account number", null);
            ConfigureColumn(_accountsGrid, "AccountType", "Type", null);
            ConfigureColumn(_accountsGrid, "CurrentBalance", "Balance", "C2");
            ConfigureColumn(_accountsGrid, "Status", "Status", null);
            ConfigureColumn(_accountsGrid, "CurrencyCode", "Currency", null);
            ConfigureColumn(_accountsGrid, "OpenedUtc", "Opened", "d");
            HideColumn(_accountsGrid, "AvailableBalance");
        }

        private void FormatTransactionGrid()
        {
            ConfigureColumn(_transactionsGrid, "TransactionId", "Transaction ID", null);
            ConfigureColumn(_transactionsGrid, "PostedDate", "Posted", "d");
            ConfigureColumn(_transactionsGrid, "Description", "Description", null);
            ConfigureColumn(_transactionsGrid, "Amount", "Amount", "C2");
            ConfigureColumn(_transactionsGrid, "TransactionType", "Type", null);
            HideColumn(_transactionsGrid, "RunningBalance");
        }

        private static void ConfigureColumn(DataGridView grid, string name, string header, string format)
        {
            var column = grid.Columns[name];
            if (column == null) return;
            column.HeaderText = header;
            if (!string.IsNullOrEmpty(format)) column.DefaultCellStyle.Format = format;
            if (name == "Description") column.FillWeight = 180F;
        }

        private static void HideColumn(DataGridView grid, string name)
        {
            var column = grid.Columns[name];
            if (column != null) column.Visible = false;
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };
        }

        private static TabPage NewTab(string text)
        {
            return new TabPage(text) { BackColor = Color.White, Padding = new Padding(4) };
        }

        private static Label SectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 12F),
                ForeColor = Navy,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Label FilterLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(8, 8, 4, 0),
                Font = new Font("Segoe UI Semibold", 9F)
            };
        }

        private static Control LabeledCombo(string caption, out ComboBox combo, IEnumerable<string> items)
        {
            var holder = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Margin = new Padding(6, 0, 6, 0) };
            holder.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            holder.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            holder.Controls.Add(new Label { Text = caption, ForeColor = Color.White, Dock = DockStyle.Fill }, 0, 0);
            combo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            combo.Items.AddRange(items.Cast<object>().ToArray());
            holder.Controls.Add(combo, 0, 1);
            return holder;
        }

        private static void AddDetail(TableLayoutPanel panel, int index, string caption, out Label value)
        {
            var row = index / 2;
            var column = (index % 2) * 2;
            panel.Controls.Add(DetailCaption(caption), column, row);
            value = DetailValue();
            panel.Controls.Add(value, column + 1, row);
        }

        private static Label DetailCaption(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Color.FromArgb(65, 77, 92),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Label DetailValue()
        {
            return new Label
            {
                Text = "—",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
        }

        private static Button PrimaryButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                BackColor = Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 6, 0)
            };
        }

        private static Button SecondaryButton(string text)
        {
            return new Button
            {
                Text = text,
                Height = 30,
                BackColor = Color.White,
                ForeColor = Navy,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 6, 0)
            };
        }

        private static T FindControl<T>(Control root, string name) where T : Control
        {
            if (root.Name == name) return root as T;
            foreach (Control child in root.Controls)
            {
                var match = FindControl<T>(child, name);
                if (match != null) return match;
            }
            return null;
        }
    }
}
