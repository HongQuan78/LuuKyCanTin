namespace SP03Search.Search;

internal sealed class SearchCounters
{
    private int _started;
    private int _cancelled;
    private int _completed;
    private int _failed;

    public int Started => Volatile.Read(ref _started);

    public int Cancelled => Volatile.Read(ref _cancelled);

    public int Completed => Volatile.Read(ref _completed);

    public int Failed => Volatile.Read(ref _failed);

    public void Start() => Interlocked.Increment(ref _started);

    public void Cancel() => Interlocked.Increment(ref _cancelled);

    public void Complete() => Interlocked.Increment(ref _completed);

    public void Fail() => Interlocked.Increment(ref _failed);

    public void Reset()
    {
        Interlocked.Exchange(ref _started, 0);
        Interlocked.Exchange(ref _cancelled, 0);
        Interlocked.Exchange(ref _completed, 0);
        Interlocked.Exchange(ref _failed, 0);
    }
}

/// <summary>
/// The debounce pattern Story 3.2 must reuse: on every TextChanged the previous query is
/// cancelled, a new 300 ms delay starts, and only the run whose token is still current binds
/// its rows. Awaiting <c>Task.Delay(..., ct)</c> and <c>ToListAsync(ct)</c> passes the token
/// down to <c>SqlCommand</c>, so the server command is cancelled too.
/// </summary>
internal sealed class IncrementalSearch : IDisposable
{
    private readonly object _gate = new();
    private readonly Func<string, CancellationToken, Task<IReadOnlyList<SearchResult>>> _queryAsync;
    private CancellationTokenSource? _pending;
    private long _version;
    private Task _current = Task.CompletedTask;
    private bool _disposed;

    public IncrementalSearch(Func<string, CancellationToken, Task<IReadOnlyList<SearchResult>>> queryAsync)
        => _queryAsync = queryAsync;

    public TimeSpan Debounce { get; init; } = TimeSpan.FromMilliseconds(300);

    public Action<string, IReadOnlyList<SearchResult>>? ResultsBound { get; set; }

    public Action<string, Exception>? Failed { get; set; }

    public SearchCounters Counters { get; } = new();

    public void OnTextChanged(string term)
    {
        CancellationTokenSource cts;
        long version;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _pending?.Cancel();
            cts = _pending = new CancellationTokenSource();
            version = ++_version;
        }

        Counters.Start();
        _current = RunAsync(term, version, cts);
    }

    public Task WaitForIdleAsync()
    {
        lock (_gate)
        {
            return _current;
        }
    }

    public void CancelPending()
    {
        lock (_gate)
        {
            _pending?.Cancel();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pending?.Cancel();
        }
    }

    private async Task RunAsync(string term, long version, CancellationTokenSource cts)
    {
        try
        {
            await Task.Delay(Debounce, cts.Token).ConfigureAwait(true);
            var rows = await _queryAsync(term, cts.Token).ConfigureAwait(true);
            if (version != Volatile.Read(ref _version))
            {
                Counters.Cancel();
                return;
            }

            cts.Token.ThrowIfCancellationRequested();
            Counters.Complete();
            ResultsBound?.Invoke(term, rows);
        }
        catch (OperationCanceledException)
        {
            Counters.Cancel();
        }
        catch (Exception ex)
        {
            Counters.Fail();
            Failed?.Invoke(term, ex);
        }
        finally
        {
            lock (_gate)
            {
                if (ReferenceEquals(_pending, cts))
                {
                    _pending = null;
                }
            }

            cts.Dispose();
        }
    }
}
