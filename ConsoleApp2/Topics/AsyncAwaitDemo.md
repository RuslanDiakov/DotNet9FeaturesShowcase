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
## 11. Async File I/O

```csharp
private async Task DemoFileIOAsync()
{
    Console.WriteLine("\n-- 11) Async File I/O --");

    string filePath = Path.Combine(Environment.CurrentDirectory, "demo_async_io.txt");

    // 11.1: Асинхронная запись текста полностью
    string contentToWrite = "Это демонстрация асинхронной работы с файлами.\nTimestamp: " + DateTime.UtcNow.ToString("O");
    await File.WriteAllTextAsync(filePath, contentToWrite).ConfigureAwait(false);
    Console.WriteLine($"Записали текст в файл: {filePath}");

    // 11.2: Асинхронное чтение всего файла
    string readAll = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
    Console.WriteLine("Прочитано содержимое файла:");
    Console.WriteLine(readAll);

    // 11.3: Асинхронные FileStream чтение/запись
    string streamFile = Path.Combine(Environment.CurrentDirectory, "demo_async_stream.bin");
    byte[] data = new byte[1024];
    new Random().NextBytes(data);

    await using (var fsWrite = new FileStream(streamFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
    {
        await fsWrite.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
        Console.WriteLine($"Записали {data.Length} байт в {streamFile}");
    }

    await using (var fsRead = new FileStream(streamFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
    {
        byte[] buffer = new byte[data.Length];
        int bytesRead = await fsRead.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        Console.WriteLine($"Прочитали {bytesRead} байт из {streamFile}");
    }

    // 11.4: Асинхронная обработка больших файлов с отменой
    using CancellationTokenSource cts = new CancellationTokenSource();
    var largeFile = Path.Combine(Environment.CurrentDirectory, "large_demo.txt");
    // Сгенерируем файл, если его нет
    if (!File.Exists(largeFile))
    {
        using (var w = new StreamWriter(largeFile))
        {
            for (int i = 0; i < 100_000; i++)
                await w.WriteLineAsync($"Строка номер {i}").ConfigureAwait(false);
        }
    }

    Console.WriteLine("Начинаем асинхронное чтение большого файла...");
    _ = Task.Run(async () =>
    {
        await Task.Delay(200).ConfigureAwait(false);
        Console.WriteLine("Запрашиваю отмену чтения большого файла...");
        cts.Cancel();
    });

    try
    {
        await using (var reader = new StreamReader(largeFile))
        {
            string? line;
            int counter = 0;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                cts.Token.ThrowIfCancellationRequested();
                if (++counter % 20000 == 0)
                    Console.WriteLine($"Прочитано {counter} строк...");
            }
            Console.WriteLine("Чтение большого файла завершено успешно.");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Чтение большого файла отменено пользователем.");
    }
}
```

В этом разделе:

* **File.WriteAllTextAsync / File.ReadAllTextAsync** — простая запись и чтение всего текста.
* **FileStream с флагом useAsync: true** — потоковое чтение/запись бинарных данных.
* **Асинхронная обработка большого файла** с проверкой `CancellationToken`:

  * Генерация файла, если отсутствует;
  * Чтение построчно через `ReadLineAsync`;
  * Отмена чтения при запросе `cts.Cancel()`.

---

## 12. Async Database Access (SQLite)

```csharp
private async Task DemoDatabaseAsync()
{
    Console.WriteLine("\n-- 12) Async Database Access (SQLite) --");

    string dbPath = Path.Combine(Environment.CurrentDirectory, "async_demo.db");
    string connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadWriteCreate
    }.ToString();

    // 12.1: Создание БД и таблицы, если ещё нет
    await using (var connection = new SqliteConnection(connectionString))
    {
        await connection.OpenAsync().ConfigureAwait(false);

        string createTableSql = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );";
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = createTableSql;
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // 12.2: Вставка нескольких записей асинхронно с транзакцией
        await using (var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false))
        {
            for (int i = 1; i <= 5; i++)
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Users (Name, CreatedAt) VALUES ($name, $createdAt);";
                insertCmd.Parameters.AddWithValue("$name", $"User_{i}");
                insertCmd.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
                insertCmd.Transaction = transaction;

                await insertCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                Console.WriteLine($"Inserted User_{i}");
            }

            await transaction.CommitAsync().ConfigureAwait(false);
        }

        // 12.3: Асинхронное чтение данных
        Console.WriteLine("Чтение данных из таблицы Users:");
        string selectSql = "SELECT Id, Name, CreatedAt FROM Users;";
        await using (var selectCmd = connection.CreateCommand())
        {
            selectCmd.CommandText = selectSql;
            await using (var reader = await selectCmd.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    long id = reader.GetInt64(0);
                    string name = reader.GetString(1);
                    string createdAt = reader.GetString(2);
                    Console.WriteLine($"Id={id}, Name={name}, CreatedAt={createdAt}");
                }
            }
        }

        await connection.CloseAsync().ConfigureAwait(false);
    }
}
```

В этом разделе:

1. **Настройка SQLite-подключения** через `SqliteConnectionStringBuilder`.
2. **Создание таблицы** при первом запуске (IF NOT EXISTS).
3. **Транзакционная вставка нескольких записей** с параметризованными SQL-командами (`$name`, `$createdAt`).
4. **Асинхронное чтение** через `ExecuteReaderAsync` и `ReadAsync`.

> **Важно:**
>
> * Для работы этого кода нужно добавить NuGet-пакет `Microsoft.Data.Sqlite`.
> * Поддерживаются все основные операции: создание схемы, вставка, выборка.

---

## 13. Fundamental Concepts

### 13.1 Что такое `Task` и зачем он нужен
- `Task` (или `Task<TResult>`) — это представление асинхронной операции, которая в будущем вернёт результат (`TResult`) или завершится без результата (`Task`).  
- Позволяет:
  - Параллельно выполнять несколько операций без блокировки основного потока.  
  - Отслеживать состояние операции (завершена, ожидает, отменена, произошла ошибка).  
  - Использовать методы-расширения (`ContinueWith`, `WhenAll`, `WhenAny`) для объединения или обработки нескольких задач.  
- Без `Task` невозможно написать неблокирующий код: любые долгие операции (I/O, работа с диском, сетью, базами данных) будут висеть в одном потоке, блокируя интерфейс или снижая пропускную способность сервера.

---

### 13.2 Что такое `async` и зачем мы его пишем
- Ключевое слово `async` перед объявлением метода (или лямбды) указывает компилятору:
  - Внутри метода могут использоваться выражения `await`.  
  - Метод будет автоматически преобразован в цепочку состояний с внутренним `Task`/`StateMachine`.  
- Методы с `async`:
  - Возвращают `Task`, `Task<TResult>` или `ValueTask<TResult>`.  
  - При вызове не начинают выполнение в фоне без `await` (метод стартует синхронно до первого `await`), но дают возможность «расколоть» тело метода на асинхронные части.  
- Без `async` нельзя было бы «декларировать» асинхронность на уровне синтаксиса, и всякая асинхронная работа потребовала бы ручной работы с колбэками или `ContinueWith`, что усложняет код и затрудняет отладку.

---

### 13.3 Что такое `await` и зачем мы его пишем
- `await` — оператор, который «приостанавливает» выполнение текущего метода до завершения указанного `Task`/`ValueTask`.  
- При встрече `await` компилятор:
  1. Регистрирует продолжение (continuation) после завершения `Task`.  
  2. Возвращает управление вызывающему коду (или освобождает поток), не блокируя его.  
- После завершения асинхронной операции компилятор «возвращает» управление внутрь метода сразу после `await`, восстанавливая контекст выполнения (SynchronizationContext), если не указан `ConfigureAwait(false)`.  
- Без `await` мы были бы вынуждены использовать `Task.ContinueWith`, явные колбэки или `.Result`/`.Wait()`, что потенциально приводит к дедлокам и снижает читаемость.

---

### 13.4 Что такое `using` и зачем мы его пишем
- `using` (или `await using`) — синтаксический сахар для автоматического вызова `IDisposable.Dispose()` (или `IAsyncDisposable.DisposeAsync()`) когда объект выходит за пределы области видимости.  
- Гарантирует:
  - Корректное освобождение ресурсов (файловые дескрипторы, сетевые соединения, транзакции и т. д.).  
  - В случае `await using` — асинхронное освобождение (например, асинхронное закрытие потока или соединения), что позволяет не блокировать поток во время очистки.  
- Без `using` мы всегда должны вручную писать `try/finally` и вызывать `Dispose()` (или `DisposeAsync()`), что усложняет код и повышает риск утечек ресурсов.

---

### 13.5 Зачем использовать `async/await` при работе с файлами и БД
- **Неблокирующий ввод-вывод (I/O)**  
  — Операции с диском и БД обычно гораздо медленнее, чем работа CPU: чтение/запись, сетевое общение, диск.  
  — Если использовать синхронные методы (`ReadAllText`, `ExecuteReader` и т. д.), поток заблокируется, пока операция не завершится:  
    - В консольном приложении это «зависнет» весь процесс.  
    - В веб-приложении (ASP.NET) поток из пула будет заблокирован, снижая пропускную способность и конкурентность.  
- **Масштабируемость**  
  — `async/await` позволяет возвращать поток в пул до завершения I/O. После окончания операции поток может обслуживать другие запросы.  
  — В ряде сценариев (сервисы с высокой нагрузкой) это критично для обеспечения высокой производительности и отказоустойчивости.  
- **Обработка ошибок и отмена**  
  — Через `CancellationToken` можно корректно прервать долгую операцию (чтение большого файла, длительный запрос к БД) без «рывков» или утечек.  
  — Асинхронные операции внутри `try/catch` корректно пробрасывают исключения, которые можно отловить и обработать.  
- **Асинхронное освобождение ресурсов**  
  — При большом числе одновременных операций гарантировано правильное закрытие файловых потоков и транзакций с минимальным влиянием на производительность.  

---

### 13.6 Зачем ещё использовать `ConfigureAwait(false)`
- В консольных приложениях `SynchronizationContext` отсутствует, но в GUI- и ASP.NET-приложениях:
  - По умолчанию после `await` код пытается вернуться в исходный контекст (UI-поток или контекст запроса).  
  - Иногда это не нужно и даже вредно (deadlock в ASP.NET при `.Result/.Wait()`, избыточная перерисовка UI).  
- `ConfigureAwait(false)` говорит компилятору «не нужно возвращаться в исходный контекст», таким образом:
  - Снижается накладная стоимость переключения контекста.  
  - Уменьшается риск дедлоков при смешанном использовании синхронных и асинхронных вызовов.

---

### 13.7 Коротко о `IAsyncEnumerable` и `await foreach`
- `IAsyncEnumerable<T>` — позволяет возвращать последовательность элементов асинхронно (streaming).  
  - Полезно при работе с большими наборами данных (файлы, результаты БД-запросов).  
  - Каждый `yield return` ждёт завершения асинхронной операции (например, считывания следующей записи).  
- `await foreach` автоматизирует асинхронную итерацию:  
  - Приостанавливает цикл до готовности следующего элемента.  
  - Позволяет комбинировать с `CancellationToken` (через атрибут `[EnumeratorCancellation]`).  

---

### 13.8 Итог: почему всё это важно для Middle/Senior .NET Developer
- Понимание того, что такое `Task`, `async`, `await`, `using` — фундамент для построения производительного, отказоустойчивого и масштабируемого кода.  
- Реальные проекты (API-сервисы, десктопные приложения, фоновые службы) неизбежно сталкиваются с долгими I/O-операциями (файлы, сетевые запросы, базы данных).  
- Правильное применение `async/await`:
  - Повышает отзывчивость приложений (не блокирует UI).  
  - Увеличивает количество одновременных запросов в веб-сервисах.  
  - Обеспечивает корректное управление ресурсами (файловые дескрипторы, соединения).  
  - Упрощает поддержку и тестирование: асинхронный код легче «отлаживать» и «обрабатывать» ошибки.  

---

## Как использовать

В `Program.cs` достаточно вызвать:

   ```csharp
   new AsyncAwaitDemo().Run();
   ```

При запуске:

1. Вы последовательно увидите демонстрацию каждой техники async/await.
2. Будут созданы файлы `demo_async_io.txt`, `demo_async_stream.bin` и `large_demo.txt` (если их нет).
3. Будет создана SQLite БД `async_demo.db` и таблица `Users` (если её нет), после чего вставятся записи и выведутся на консоль.
---
