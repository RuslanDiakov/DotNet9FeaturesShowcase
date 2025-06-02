# AsyncAwaitDemo

**Описание:**  
Класс `AsyncAwaitDemo` демонстрирует все продвинутые аспекты асинхронного программирования в C# на уровне Middle/Senior. В нём разбираются:

1. Создание `Task`, `Task.Run` и `await`
2. Параллельные асинхронные операции: `Task.WhenAll`, `Task.WhenAny`
3. Использование `ConfigureAwait(false)` и взаимодействие с `SynchronizationContext`
4. Отмена операций через `CancellationToken`
5. Обработка исключений в асинхронных методах
6. Разница между `ValueTask` и `Task`
7. Асинхронные потоки (`IAsyncEnumerable<T>`)
8. `TaskCompletionSource` для интеграции callback-паттернов
9. Опасности `async void`
10. Асинхронное освобождение ресурсов (`IAsyncDisposable`)

Каждый раздел проиллюстрирован отдельным методом.

---

## Содержание

1. [Task Creation & Await](#1-task-creation--await)  
2. [Parallel Async (WhenAll, WhenAny)](#2-parallel-async-whenall-whenany)  
3. [ConfigureAwait Usage](#3-configureawait-usage)  
4. [CancellationToken Usage](#4-cancellationtoken-usage)  
5. [Exception Handling in Async](#5-exception-handling-in-async)  
6. [ValueTask vs Task](#6-valuetask-vs-task)  
7. [Async Streams (IAsyncEnumerable)](#7-async-streams-iasyncenumerable)  
8. [TaskCompletionSource Demo](#8-taskcompletionsource-demo)  
9. [Async Void Pitfall](#9-async-void-pitfall)  
10. [Async Disposable (IAsyncDisposable)](#10-async-disposable-iasyncdisposable)  

---

## 1. Task Creation & Await

```csharp
private async Task DemoTaskCreationAndAwait()
{
    // Пример асинхронного метода, возвращающего Task<int>
    async Task<int> ComputeSumAsync(int a, int b)
    {
        await Task.Delay(100).ConfigureAwait(false);
        return a + b;
    }

    int sumResult = await ComputeSumAsync(5, 7);
    Console.WriteLine($"ComputeSumAsync(5,7) = {sumResult}");

    // Запуск CPU-bound работы через Task.Run
    int factorialResult = await Task.Run(() =>
    {
        int f = 1;
        for (int i = 1; i <= 7; i++)
            f *= i;
        return f;
    });
    Console.WriteLine($"Factorial of 7 (Task.Run) = {factorialResult}");

    // Обёртка синхронного результата в Task через Task.FromResult
    Task<string> wrapped = Task.FromResult("От Task.FromResult()");
    Console.WriteLine($"Wrapped string: {await wrapped}");
}
````

* **`async Task<T>`**: позволяет использовать `await` внутри метода и возвращать результат.
* **`Task.Run`**: нужен для выполнения CPU-bound работы на пуле потоков, чтобы не блокировать вызывающий контекст.
* **`Task.FromResult`**: используется, когда результат уже есть, и вы не хотите создавать новый поток.

---

## 2. Parallel Async (WhenAll, WhenAny)

```csharp
private async Task DemoParallelAsync()
{
    Task<int> taskA = SimulateWorkAsync("A", 300);
    Task<int> taskB = SimulateWorkAsync("B", 200);
    Task<int> taskC = SimulateWorkAsync("C", 100);

    // Task.WhenAll – дождаться всех задач одновременно
    Console.WriteLine("Ожидаю Task.WhenAll...");
    int[] resultsAll = await Task.WhenAll(taskA, taskB, taskC);
    Console.WriteLine($"Результаты WhenAll: {string.Join(", ", resultsAll)}");

    // Task.WhenAny – дождаться первой завершившейся задачи
    Task<int> taskD = SimulateWorkAsync("D", 150);
    Task<int> taskE = SimulateWorkAsync("E", 250);
    Console.WriteLine("Ожидаю Task.WhenAny...");
    Task<int> first = await Task.WhenAny(taskD, taskE);
    Console.WriteLine($"Первой завершилась Task {first.Result}");

    static async Task<int> SimulateWorkAsync(string name, int delayMs)
    {
        await Task.Delay(delayMs).ConfigureAwait(false);
        Console.WriteLine($"Task {name} завершена после {delayMs}ms");
        return delayMs;
    }
}
```

* **`Task.WhenAll`**: объединяет несколько `Task<T>` в один, который завершается, когда все входные задачи завершены. Возвращает массив результатов.
* **`Task.WhenAny`**: возвращает задачу, которая завершилась первой. Полезно для таймаутов или “первого пришедшего” ответа.

---

## 3. ConfigureAwait Usage

```csharp
private async Task DemoConfigureAwaitUsage()
{
    Console.WriteLine($"До await: Thread ID = {Thread.CurrentThread.ManagedThreadId}");
    await Task.Delay(100).ConfigureAwait(false);
    Console.WriteLine($"После await с ConfigureAwait(false): Thread ID = {Thread.CurrentThread.ManagedThreadId}");
}
```

* **`ConfigureAwait(false)`** отключает попытку восстановить `SynchronizationContext` при продолжении.
* В консольных приложениях `SynchronizationContext` равен `null`, но в UI (WinForms/WPF) или ASP.NET Core этот код предотвращает “deadlock” и улучшает производительность.

---

## 4. CancellationToken Usage

```csharp
private async Task DemoCancellation()
{
    using CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken token = cts.Token;

    Task longRunning = Task.Run(async () =>
    {
        Console.WriteLine("LongRunningTask: Старт");
        for (int i = 0; i < 10; i++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(200, token).ConfigureAwait(false);
            Console.WriteLine($"LongRunningTask: итерация {i}");
        }
        Console.WriteLine("LongRunningTask: Завершена");
    }, token);

    // Отмена через 500ms
    _ = Task.Run(async () =>
    {
        await Task.Delay(500).ConfigureAwait(false);
        Console.WriteLine("Запрашиваю отмену...");
        cts.Cancel();
    });

    try
    {
        await longRunning.ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("LongRunningTask: Отменена!");
    }
}
```

* **`CancellationTokenSource`** выдаёт токен `CancellationToken`, который передаётся в асинхронные методы.
* **`token.ThrowIfCancellationRequested()`** и **`Task.Delay(..., token)`** позволяют корректно завершать таск при отмене.
* При вызове `cts.Cancel()` внутри таска выбрасывается `OperationCanceledException`, который нужно отлавливать.

---

## 5. Exception Handling in Async

```csharp
private async Task DemoExceptionHandling()
{
    async Task ThrowInAsync()
    {
        await Task.Delay(100).ConfigureAwait(false);
        throw new InvalidOperationException("Ошибка из async-метода");
    }

    try
    {
        await ThrowInAsync().ConfigureAwait(false);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Поймано исключение: {ex.Message}");
    }

    // Незаметное исключение (unobserved)
    Task unobserved = ThrowInAsync();
    await Task.Delay(200).ConfigureAwait(false);
    Console.WriteLine("Unobserved Task exception может быть пойман в TaskScheduler.UnobservedTaskException");
}
```

* Исключения в `async Task` или `async Task<T>` “пробрасываются” при `await`.
* Если задача завершилась с ошибкой, но на неё не вызвали `await` или не подписались на `Task.ContinueWith`, это приводит к “unobserved exception”.
* Можно подписаться на `TaskScheduler.UnobservedTaskException`, чтобы отлавливать такие случаи.

---

## 6. ValueTask vs Task

```csharp
private async ValueTask DemoValueTask()
{
    ValueTask<int> FastPathAsync(bool quick)
    {
        if (quick)
            return new ValueTask<int>(42);
        else
            return SlowPathAsync();

        static async Task<int> SlowPathAsync()
        {
            await Task.Delay(100).ConfigureAwait(false);
            return 100;
        }
    }

    int fastResult = await FastPathAsync(true);
    Console.WriteLine($"FastPathAsync(true): {fastResult}");

    int slowResult = await FastPathAsync(false);
    Console.WriteLine($"FastPathAsync(false): {slowResult}");
}
```

* **`ValueTask<T>`** оптимизирует сценарии, когда результат может быть готов синхронно — тогда не создаётся лишний объект `Task<T>`.
* Если же требуется асинхронная работа, `ValueTask<T>` можно конвертировать в `Task<T>`.
* **Важно**: нельзя многократно `await`ить один и тот же `ValueTask<T>` без преобразования в `Task<T>`, иначе будет исключение.

---

## 7. Async Streams (IAsyncEnumerable)

```csharp
private async IAsyncEnumerable<int> GenerateSequenceAsync(int count, int delayMs, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
{
    for (int i = 0; i < count; i++)
    {
        ct.ThrowIfCancellationRequested();
        await Task.Delay(delayMs, ct).ConfigureAwait(false);
        yield return i;
    }
}

private async Task DemoAsyncEnumerable()
{
    using CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken token = cts.Token;

    // Отменяем после 350ms
    _ = Task.Run(async () =>
    {
        await Task.Delay(350).ConfigureAwait(false);
        Console.WriteLine("AsyncEnumerable: Запрашиваю отмену...");
        cts.Cancel();
    });

    try
    {
        await foreach (int item in GenerateSequenceAsync(10, 150, token))
        {
            Console.WriteLine($"Получил элемент: {item}");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("AsyncEnumerable: Итого отменён!");
    }
}
```

* **`IAsyncEnumerable<T>`** позволяет возвращать последовательность асинхронно (async streams).
* Используется `await foreach` для последовательного получения элементов.
* Аргумент `[EnumeratorCancellation]` позволяет передавать `CancellationToken` в итератор.

---

## 8. TaskCompletionSource Demo

```csharp
private async Task DemoTaskCompletionSource()
{
    Task<string> WrappedCallbackAsync()
    {
        var tcs = new TaskCompletionSource<string>();
        // Имитируем внешний callback:
        _ = Task.Run(async () =>
        {
            await Task.Delay(200).ConfigureAwait(false);
            tcs.SetResult("Результат от callback-ориентации");
        });
        return tcs.Task;
    }

    string callbackResult = await WrappedCallbackAsync();
    Console.WriteLine($"WrappedCallbackAsync() вернул: {callbackResult}");
}
```

* **`TaskCompletionSource<TResult>`** позволяет вручную создавать `Task<TResult>`, который завершится при вызове `SetResult`, `SetException` или `SetCanceled`.
* Полезно для интеграции устаревших API с callback-подходом в современный `async/await`.

---

## 9. Async Void Pitfall

```csharp
private void DemoAsyncVoidPitfall()
{
    async void FireAndForget()
    {
        await Task.Delay(100).ConfigureAwait(false);
        throw new Exception("Ошибка в async void методе");
    }

    try
    {
        FireAndForget();
        Console.WriteLine("FireAndForget вызван");
        Task.Delay(200).Wait();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Никогда не попадёт сюда: {ex.Message}");
    }

    Console.WriteLine("Исключение из async void не поймано здесь, нужно подписываться на UnhandledException.");
}
```

* **`async void`** используют только для обработчиков событий.
* Такой метод нельзя `await`, и исключения из него падают в `SynchronizationContext.UnhandledException` (или `AppDomain.UnhandledException`).
* Нельзя использовать `async void` для “fire-and-forget” без дополнительной обработки ошибок.

---

## 10. Async Disposable (IAsyncDisposable)

```csharp
private async Task DemoAsyncDisposable()
{
    await using (var resource = new AsyncResource())
    {
        await resource.UseAsync().ConfigureAwait(false);
    }
}

private class AsyncResource : IAsyncDisposable
{
    public AsyncResource()
    {
        Console.WriteLine("AsyncResource: Конструктор – ресурс создан");
    }

    public async Task UseAsync()
    {
        Console.WriteLine("AsyncResource: Начинаем использовать ресурс...");
        await Task.Delay(100).ConfigureAwait(false);
        Console.WriteLine("AsyncResource: Использование завершено");
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("AsyncResource: Начинаем асинхронную очистку...");
        await Task.Delay(100).ConfigureAwait(false);
        Console.WriteLine("AsyncResource: Очистка завершена");
    }
}
```

* **`IAsyncDisposable`** вводит метод `ValueTask DisposeAsync()`, который вызывается при использовании `await using`.
* Полезно для асинхронного закрытия или очистки ресурсов (сетевые потоки, подключение к БД и т.д.).

---

## Как использовать

1. В `Program.cs` достаточно вызвать:

   ```csharp
   new AsyncAwaitDemo().Run();
   ```

2. При запуске консоли вы увидите пошаговую демонстрацию всех перечисленных техник.

---
