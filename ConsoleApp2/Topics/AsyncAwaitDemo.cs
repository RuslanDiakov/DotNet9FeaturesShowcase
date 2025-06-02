using ConsoleApp2;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite; // Требуется установить пакет Microsoft.Data.Sqlite


namespace Playground.Topics
{
    public class AsyncAwaitDemo : IDemo
    {
        public async void Run()
        {
            Console.WriteLine("\n=== AsyncAwaitDemo: Начало ===");

            await DemoTaskCreationAndAwait();
            await DemoParallelAsync();
            await DemoConfigureAwaitUsage();
            await DemoCancellation();
            await DemoExceptionHandling();
            await DemoValueTask();
            await DemoAsyncEnumerable();
            await DemoTaskCompletionSource();
            DemoAsyncVoidPitfall();
            await DemoAsyncDisposable();
            await DemoFileIOAsync();
            await DemoDatabaseAsync();

            Console.WriteLine("=== AsyncAwaitDemo: Конец ===\n");
        }

        // -----------------------------
        // 1) Создание Task и Await
        // -----------------------------
        private async Task DemoTaskCreationAndAwait()
        {
            Console.WriteLine("\n-- 1) Task Creation & Await --");

            // 1.1: Асинхронный метод, возвращающий Task<int>
            async Task<int> ComputeSumAsync(int a, int b)
            {
                await Task.Delay(100).ConfigureAwait(false);
                return a + b;
            }

            // 1.2: Вызов асинхронного метода и получение результата
            int sumResult = await ComputeSumAsync(5, 7);
            Console.WriteLine($"ComputeSumAsync(5,7) = {sumResult}");

            // 1.3: Task.Run для запуска CPU-bound работы на пуле потоков
            int factorialResult = await Task.Run(() =>
            {
                int f = 1;
                for (int i = 1; i <= 7; i++)
                    f *= i;
                return f;
            });
            Console.WriteLine($"Factorial of 7 (Task.Run) = {factorialResult}");

            // 1.4: Синхронный метод, заточенный под Task (Task.FromResult)
            Task<string> wrapped = Task.FromResult("От Task.FromResult()");
            Console.WriteLine($"Wrapped string: {await wrapped}");
        }

        // -----------------------------
        // 2) Параллельные асинхронные операции
        // -----------------------------
        private async Task DemoParallelAsync()
        {
            Console.WriteLine("\n-- 2) Parallel Async (WhenAll, WhenAny) --");

            // 2.1: Создание списка задач
            Task<int> taskA = SimulateWorkAsync("A", 300);
            Task<int> taskB = SimulateWorkAsync("B", 200);
            Task<int> taskC = SimulateWorkAsync("C", 100);

            // 2.2: Одновременный запуск и ожидание всех
            Console.WriteLine("Ожидаю Task.WhenAll...");
            int[] resultsAll = await Task.WhenAll(taskA, taskB, taskC);
            Console.WriteLine($"Результаты WhenAll: {string.Join(", ", resultsAll)}");

            // 2.3: Когда первая задача завершится (WhenAny)
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

        // -----------------------------
        // 3) ConfigureAwait и SynchronizationContext
        // -----------------------------
        private async Task DemoConfigureAwaitUsage()
        {
            Console.WriteLine("\n-- 3) ConfigureAwait(false) Usage --");

            // В консольных приложениях SynchronizationContext = null, но в UI/ASP.NET важно.
            Console.WriteLine($"До await: Thread ID = {Thread.CurrentThread.ManagedThreadId}");

            await Task.Delay(100).ConfigureAwait(false);

            Console.WriteLine($"После await с ConfigureAwait(false): Thread ID = {Thread.CurrentThread.ManagedThreadId}");

            // Демонстрация: если убрать ConfigureAwait(false), то в UI-контексте вернётся тот же поток, 
            // а с false — может быть возвращено на другой поток пула.
        }

        // -----------------------------
        // 4) CancellationToken и отмена
        // -----------------------------
        private async Task DemoCancellation()
        {
            Console.WriteLine("\n-- 4) CancellationToken Usage --");

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

            // Отмена после 500ms
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

        // -----------------------------
        // 5) Обработка исключений в async
        // -----------------------------
        private async Task DemoExceptionHandling()
        {
            Console.WriteLine("\n-- 5) Exception Handling in Async --");

            // 5.1: Исключение в асинхронном методе
            async Task ThrowInAsync()
            {
                await Task.Delay(100).ConfigureAwait(false);
                throw new InvalidOperationException("Ошибка из async-метода");
            }

            // Корректное ожидание с try-catch
            try
            {
                await ThrowInAsync().ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Поймано исключение: {ex.Message}");
            }

            // 5.2: Ошибка в Task без await (unobserved exception)
            Task unobserved = ThrowInAsync();
            // Специально не ожидаем — с версии .NET, когда сборщик мусора вызовет UnobservedTaskException, 
            // можно подписаться на AppDomain.CurrentDomain.UnhandledException или TaskScheduler.UnobservedTaskException.
            await Task.Delay(200).ConfigureAwait(false);
            Console.WriteLine("Unobserved Task exception может быть пойман в TaskScheduler.UnobservedTaskException");
        }

        // -----------------------------
        // 6) ValueTask vs Task
        // -----------------------------
        private async ValueTask DemoValueTask()
        {
            Console.WriteLine("\n-- 6) ValueTask vs Task --");

            ValueTask<int> FastPathAsync(bool quick)
            {
                if (quick)
                    return new ValueTask<int>(42);
                else
                    return new ValueTask<int>(SlowPathAsync());

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

        // -----------------------------
        // 7) IAsyncEnumerable (Asynchronous Streams)
        // -----------------------------
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
            Console.WriteLine("\n-- 7) Async Streams (IAsyncEnumerable) --");

            using CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // Запускаем поток, который отменит после задержки 350ms
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

        // -----------------------------
        // 8) TaskCompletionSource для обёртывания callback-оригиналов
        // -----------------------------
        private async Task DemoTaskCompletionSource()
        {
            Console.WriteLine("\n-- 8) TaskCompletionSource Demo --");

            Task<string> WrappedCallbackAsync()
            {
                var tcs = new TaskCompletionSource<string>();
                // Имитируем внешний callback, который вызовется через delay
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

        // -----------------------------
        // 9) Async void – опасности и примеры
        // -----------------------------
        private void DemoAsyncVoidPitfall()
        {
            Console.WriteLine("\n-- 9) Async Void Pitfall --");

            // async void нельзя ожидать, исключения пойдут в SynchronizationContext.UnhandledException
            async void FireAndForget()
            {
                await Task.Delay(100).ConfigureAwait(false);
                throw new Exception("Ошибка в async void методе");
            }

            try
            {
                FireAndForget();
                Console.WriteLine("FireAndForget вызван");
                // Делаем небольшую задержку, чтобы увидеть исключение
                Task.Delay(200).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Никогда не попадёт сюда: {ex.Message}");
            }

            Console.WriteLine("Исключение из async void не поймано здесь, нужно подписываться на UnhandledException.");
        }

        // -----------------------------
        // 10) Async Disposable (IAsyncDisposable)
        // -----------------------------
        private async Task DemoAsyncDisposable()
        {
            Console.WriteLine("\n-- 10) Async Disposable (IAsyncDisposable) --");

            await using (var resource = new AsyncResource())
            {
                await resource.UseAsync().ConfigureAwait(false);
            }
        }

        // Вспомогательный класс для демонстрации IAsyncDisposable
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

        // -----------------------------
        // 11) Async File I/O
        // -----------------------------
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
                using (var reader = new StreamReader(largeFile))
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

        // -----------------------------
        // 12) Async Database Access (SQLite)
        // -----------------------------
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
                        insertCmd.Transaction = (SqliteTransaction?)transaction;

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
    }
}
