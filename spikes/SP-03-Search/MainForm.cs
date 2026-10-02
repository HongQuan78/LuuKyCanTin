using System.Diagnostics;
using System.Globalization;
using System.Text;
using SP03Search.Data;
using SP03Search.Search;

namespace SP03Search;

internal sealed class MainForm : Form
{
    private static readonly (string Label, string Term, int IntervalMs)[] UiScenarios =
    {
        ("'nguyen van a' every 60 ms", "nguyen van a", 60),
        ("'dt000123' every 40 ms", "dt000123", 40),
        ("'Nguyễn Văn A' every 80 ms", "Nguyễn Văn A", 80),
        ("'%100' every 60 ms", "%100", 60),
    };

    private readonly SpikeOptions _options;
    private readonly TextBox _searchBox = new();
    private readonly DataGridView _grid = new();
    private readonly Label _status = new();
    private readonly SpikeDbContextFactory _factory;
    private readonly SearchService _searchService;
    private readonly IncrementalSearch _search;

    public MainForm(SpikeOptions options)
    {
        _options = options;
        Text = "SP-03 · Tìm kiếm không dấu trong danh sách đối tượng (spike)";
        ClientSize = new Size(980, 620);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);

        _searchBox.Dock = DockStyle.Top;
        _searchBox.Font = new Font("Segoe UI", 14F);
        _searchBox.PlaceholderText = "Gõ mã số (từ 1 ký tự) hoặc họ tên không dấu (từ 2 ký tự)...";

        _status.Dock = DockStyle.Bottom;
        _status.Height = 30;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.Padding = new Padding(6, 0, 6, 0);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Mã số",
            DataPropertyName = nameof(SearchResult.MaSo),
            Width = 110,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Họ tên",
            DataPropertyName = nameof(SearchResult.HoTen),
            Width = 260,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Năm sinh",
            DataPropertyName = nameof(SearchResult.NamSinh),
            Width = 90,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Buồng giam",
            DataPropertyName = nameof(SearchResult.BuongGiam),
            Width = 140,
        });

        Controls.Add(_grid);
        Controls.Add(_status);
        Controls.Add(_searchBox);

        _factory = new SpikeDbContextFactory(options.DatabaseConnectionString);
        var warmUp = Stopwatch.StartNew();
        _factory.WarmUp();
        WarmUp = warmUp.Elapsed;

        _searchService = new SearchService(_factory);
        _search = new IncrementalSearch(QueryAsync);
        _search.ResultsBound = Bind;
        _search.Failed = (term, ex) => _status.Text = $"Lỗi khi tìm \"{term}\": {ex.Message}";
        _searchBox.TextChanged += (_, _) => _search.OnTextChanged(_searchBox.Text);
        _status.Text = $"Sẵn sàng · {options.DatabaseConnectionString}";
    }

    public event Action<string, int>? Bound;

    public TimeSpan WarmUp { get; }

    public SearchCounters Counters => _search.Counters;

    public string FirstRowText => _grid.Rows.Count > 0
        ? Convert.ToString(_grid.Rows[0].Cells[1].Value, CultureInfo.InvariantCulture) ?? string.Empty
        : string.Empty;

    public void SetSearchText(string text) => _searchBox.Text = text;

    public void ResetCounters() => _search.Counters.Reset();

    public void FocusSearch() => _searchBox.Focus();

    public static int RunUiCheck(SpikeOptions options)
    {
        using var form = new MainForm(options);
        var driver = new UiCheckDriver(form, options);
        form.Shown += async (_, _) => await driver.RunAsync();
        Application.Run(form);
        return driver.Failures;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _search.Dispose();
        base.OnFormClosed(e);
    }

    private async Task<IReadOnlyList<SearchResult>> QueryAsync(string term, CancellationToken ct)
    {
        var outcome = await _searchService.SearchAsync(term, SearchStrategy.Auto, ct);
        return outcome.Rows;
    }

    private void Bind(string term, IReadOnlyList<SearchResult> rows)
    {
        _grid.DataSource = rows.ToList();
        _status.Text =
            $"{rows.Count} kết quả cho \"{term}\" · bắt đầu {_search.Counters.Started}, huỷ {_search.Counters.Cancelled}, xong {_search.Counters.Completed}";
        Bound?.Invoke(term, rows.Count);
    }

    private sealed class UiCheckDriver
    {
        private readonly MainForm _form;
        private readonly SpikeOptions _options;
        private readonly StringBuilder _report = new();

        public UiCheckDriver(MainForm form, SpikeOptions options)
        {
            _form = form;
            _options = options;
        }

        public int Failures { get; private set; }

        public async Task RunAsync()
        {
            try
            {
                var database = new SpikeDatabase(_options);
                var info = await database.GetInfoAsync(CancellationToken.None);
                var rowCount = await database.CountAsync(CancellationToken.None);

                Append("=== SP-03 --ui-check: keystroke → grid filled (WinForms, debounce 300 ms) ===");
                Append($"Run at   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Append($"Machine  : {Environment.MachineName} ({Environment.ProcessorCount} logical CPUs)");
                Append($"SQL      : {info.ServerInfo}");
                Append($"Rows     : {rowCount:N0}");
                Append($"EF model warm-up at form load: {_form.WarmUp.TotalMilliseconds:0} ms (paid once, before the user types)");
                Append(string.Empty);
                Append("| scenario | started | cancelled | completed | failed | rows in grid | last keystroke → grid | first row |");
                Append("|---|---|---|---|---|---|---|---|");

                var totalBinds = 0;
                foreach (var scenario in UiScenarios)
                {
                    var result = await RunScenarioAsync(scenario);
                    totalBinds += result.Completed;
                    var elapsed = result.ElapsedMs >= 0 ? $"{result.ElapsedMs:0} ms" : "timeout";
                    Append(
                        $"| {scenario.Label} | {result.Started} | {result.Cancelled} | {result.Completed} | " +
                        $"{result.Failed} | {result.Rows} | {elapsed} | {result.FirstRow} |");
                    if (result.Completed != 1 || result.Rows < 0 || result.ElapsedMs < 0)
                    {
                        Failures++;
                    }
                }

                Append(string.Empty);
                Append($"Total grid binds during the run: {totalBinds}");

                var report = _report.ToString();
                Directory.CreateDirectory(_options.OutputDirectory);
                var path = Path.Combine(_options.OutputDirectory, $"ui-check-{_options.Rows}.txt");
                await File.WriteAllTextAsync(path, report, CancellationToken.None);
                Console.Write(report);
                Console.WriteLine();
                Console.WriteLine($"UI check report written: {Path.GetFullPath(path)}");
            }
            catch (Exception ex)
            {
                Failures++;
                Console.Error.WriteLine($"ui-check failed: {ex}");
            }
            finally
            {
                _form.Close();
            }
        }

        private async Task<(int Started, int Cancelled, int Completed, int Failed, int Rows, double ElapsedMs, string FirstRow)>
            RunScenarioAsync((string Label, string Term, int IntervalMs) scenario)
        {
            _form.ResetCounters();
            var term = scenario.Term;
            var boundRows = -1;
            var firstRow = string.Empty;
            var lastKeystroke = 0L;
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnBound(string boundTerm, int rows)
            {
                if (!string.Equals(boundTerm, term, StringComparison.Ordinal))
                {
                    return;
                }

                boundRows = rows;
                firstRow = _form.FirstRowText;
                finished.TrySetResult();
            }

            _form.Bound += OnBound;
            try
            {
                for (var length = 1; length <= term.Length; length++)
                {
                    if (length == term.Length)
                    {
                        lastKeystroke = Stopwatch.GetTimestamp();
                    }

                    _form.SetSearchText(term[..length]);
                    if (length < term.Length)
                    {
                        await Task.Delay(scenario.IntervalMs);
                    }
                }

                var winner = await Task.WhenAny(finished.Task, Task.Delay(TimeSpan.FromSeconds(10)));
                var elapsed = winner == finished.Task && lastKeystroke != 0
                    ? Stopwatch.GetElapsedTime(lastKeystroke).TotalMilliseconds
                    : -1;
                return (
                    _form.Counters.Started,
                    _form.Counters.Cancelled,
                    _form.Counters.Completed,
                    _form.Counters.Failed,
                    boundRows,
                    elapsed,
                    firstRow);
            }
            finally
            {
                _form.Bound -= OnBound;
            }
        }

        private void Append(string line) => _report.AppendLine(line);
    }
}
