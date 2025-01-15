namespace GlobalMutexSharp;

/// <summary>
/// A re-entrant named mutex that can lock across processes.
/// </summary>
public sealed class GlobalMutex : IDisposable {
    private readonly Mutex Mutex;
    private readonly Lock Lock = new();
    private int Depth = 0;

    /// <summary>
    /// The current entrancy depth.
    /// </summary>
    public int CurrentDepth => Depth;
    /// <summary>
    /// Whether the mutex is currently acquired.
    /// </summary>
    public bool IsAcquired => Depth > 0;

    /// <summary>
    /// Constructs a global mutex with the given name.
    /// </summary>
    /// <param name="Name">The mutex name, which will be automatically escaped and formatted.</param>
    public GlobalMutex(string Name) {
        Mutex = new Mutex(initiallyOwned: false, $"Global\\{Uri.EscapeDataString(Name)}");
    }

    /// <summary>
    /// Releases the mutex and frees all resources used.
    /// </summary>
    public void Dispose() {
        Depth = 0;
        Mutex.Dispose();
    }
    /// <summary>
    /// Enters the mutex, blocking up to the given <paramref name="Timeout"/> until it is acquired.
    /// </summary>
    /// <returns>
    /// A <see cref="DisposeExiter"/> that will exit the mutex when disposed.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    public DisposeExiter Acquire(TimeSpan Timeout) {
        if (!TryEnter(Timeout)) {
            throw new TimeoutException("Timeout acquiring global mutex.");
        }
        return new DisposeExiter(this);
    }
    /// <inheritdoc cref="Acquire(TimeSpan)"/>
    public Task<DisposeExiter> AcquireAsync(TimeSpan Timeout) {
        return Task.Run(() => Acquire(Timeout));
    }
    /// <summary>
    /// Tries to enter the mutex, blocking up to the given <paramref name="Timeout"/> until it is acquired.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the mutex was acquired, <see langword="false"/> otherwise.
    /// </returns>
    /// <param name="Exiter">A <see cref="DisposeExiter"/> that will exit the mutex when disposed.</param>
    public bool TryAcquire(TimeSpan Timeout, out DisposeExiter Exiter) {
        if (TryEnter(Timeout)) {
            Exiter = new DisposeExiter(this);
            return true;
        }
        Exiter = default;
        return false;
    }

    private bool TryEnter(TimeSpan Timeout) {
        lock (Lock) {
            // Re-enter mutex if already acquired
            if (Depth > 0) {
                Depth++;
                return true;
            }

            try {
                // Try to acquire mutex
                if (Mutex.WaitOne(Timeout, exitContext: false)) {
                    Depth++;
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (AbandonedMutexException) {
                // The mutex was abandoned in another process, but it still gets acquired
                return true;
            }
        }
    }
    private bool TryExit() {
        lock (Lock) {
            // Fail if mutex is not acquired
            if (Depth <= 0) {
                return false;
            }

            // Exit mutex
            Depth--;

            // Release mutex if fully exited
            if (Depth <= 0) {
                Mutex.ReleaseMutex();
            }
            return true;
        }
    }

    /// <summary>
    /// An object that exits a global mutex when disposed.
    /// </summary>
    public readonly struct DisposeExiter : IDisposable {
        /// <summary>
        /// The global mutex to exit when disposed.
        /// </summary>
        public GlobalMutex Mutex { get; }

        internal DisposeExiter(GlobalMutex Mutex) {
            this.Mutex = Mutex;
        }

        /// <summary>
        /// Exits the global mutex.
        /// </summary>
        public void Dispose() {
            Mutex.TryExit();
        }
    }
}