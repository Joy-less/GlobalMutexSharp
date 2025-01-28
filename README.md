# GlobalMutexSharp

[![NuGet](https://img.shields.io/nuget/v/GlobalMutexSharp.svg)](https://www.nuget.org/packages/GlobalMutexSharp)

An easy-to-use, re-entrant named system mutex in C#, allowing you to lock across processes.


## Example

```cs
using GlobalMutex Mutex = new("my mutex name");

using (Mutex.Acquire(TimeSpan.FromSeconds(5))) {
    Console.WriteLine("^-^");
}
```
