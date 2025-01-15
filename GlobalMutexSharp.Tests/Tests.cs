namespace GlobalMutexSharp.Tests;

public class Tests {
    [Fact]
    public void BasicTest() {
        bool Success = false;

        using GlobalMutex Mutex = new("GlobalMutex BasicTest");

        using (Mutex.Acquire(TimeSpan.FromSeconds(5))) {
            Success = true;
        }

        Assert.True(Success);
    }
    [Fact]
    public void ReentrancyTest() {
        bool Success = false;

        using GlobalMutex Mutex = new("GlobalMutex ReentrancyTest");

        using (Mutex.Acquire(TimeSpan.FromSeconds(5))) {
            using (Mutex.Acquire(TimeSpan.FromSeconds(5))) {
                using (Mutex.Acquire(TimeSpan.FromSeconds(5))) {
                    Success = true;
                }
            }
        }

        Assert.True(Success);
    }
    [Fact]
    public void MultiProcessSimulationTest() {
        int Success = 0;

        Thread ThreadA = new(() => {
            try {
                using GlobalMutex Mutex = new("GlobalMutex MultiProcessSimulationTest");

                using (Mutex.Acquire(TimeSpan.FromSeconds(5))) {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Success++;
                }
            }
            catch { }
        });
        Thread ThreadB = new(() => {
            try {
                using GlobalMutex Mutex = new("GlobalMutex MultiProcessSimulationTest");

                using (Mutex.Acquire(TimeSpan.FromSeconds(5))) {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Success++;
                }
            }
            catch { }
        });

        ThreadA.Start();
        ThreadB.Start();

        ThreadA.Join();
        ThreadB.Join();

        Assert.Equal(2, Success);
    }
    [Fact]
    public void MultiProcessTimeoutSimulationTest() {
        int Success = 0;

        Thread ThreadA = new(() => {
            try {
                using GlobalMutex Mutex = new("GlobalMutex MultiProcessTimeoutSimulationTest");

                using (Mutex.Acquire(TimeSpan.FromSeconds(1))) {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    Success++;
                }
            }
            catch { }
        });
        Thread ThreadB = new(() => {
            try {
                using GlobalMutex Mutex = new("GlobalMutex MultiProcessTimeoutSimulationTest");

                using (Mutex.Acquire(TimeSpan.FromSeconds(1))) {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    Success++;
                }
            }
            catch { }
        });

        ThreadA.Start();
        ThreadB.Start();

        ThreadA.Join();
        ThreadB.Join();

        Assert.Equal(1, Success);
    }
}