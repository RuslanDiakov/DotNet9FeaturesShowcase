using ConsoleApp2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Playground.Topics
{
    public class CollectionsDemo : IDemo
    {
        public void Run()
        {
            Console.WriteLine("=== CollectionsDemo: Начало ===\n");

            DemoArrays();
            DemoList();
            DemoLinkedList();
            DemoQueueAndStack();
            DemoDictionary();
            DemoSortedDictionaryAndSortedList();
            DemoHashSet();
            DemoReadOnlyAndImmutable();
            DemoConcurrentCollections();

            Console.WriteLine("\n=== CollectionsDemo: Конец ===");
        }


        // -----------------------------------
        // 1) Массивы: одномерные, многомерные, зубчатые (jagged)
        // -----------------------------------
        private void DemoArrays()
        {
            Console.WriteLine("-- Arrays --");

            // Одномерный массив
            int[] oneDim = new int[5] { 10, 20, 30, 40, 50 };
            Console.WriteLine($"Одномерный массив: [{string.Join(", ", oneDim)}]");

            // Многомерный массив 2x3
            int[,] twoDim = new int[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
            Console.Write("Многомерный (2x3): ");
            for (int i = 0; i < twoDim.GetLength(0); i++)
            {
                Console.Write("[");
                for (int j = 0; j < twoDim.GetLength(1); j++)
                {
                    Console.Write(twoDim[i, j] + (j < twoDim.GetLength(1) - 1 ? ", " : ""));
                }
                Console.Write("]");
            }
            Console.WriteLine();

            // Зубчатый (jagged) массив: массив массивов
            int[][] jagged = new int[3][];
            jagged[0] = new[] { 1, 2 };
            jagged[1] = new[] { 3, 4, 5 };
            jagged[2] = new[] { 6 };
            Console.Write("Зубчатый массив: ");
            for (int i = 0; i < jagged.Length; i++)
            {
                Console.Write($"[{string.Join(", ", jagged[i])}] ");
            }
            Console.WriteLine();

            // ArraySegment<T>
            var segment = new ArraySegment<int>(oneDim, 1, 3); // элемент 1, длина 3 => [20,30,40]
            Console.WriteLine($"ArraySegment из oneDim (start=1, length=3): [{string.Join(", ", segment)}]");
        }


        // -----------------------------------
        // 2) List<T>: динамический массив
        // -----------------------------------
        private void DemoList()
        {
            Console.WriteLine("\n-- List<T> --");

            var list = new List<string>(capacity: 2);
            Console.WriteLine($"Изначальная емкость List: {list.Capacity}, Count: {list.Count}");

            list.Add("Alpha");
            list.Add("Beta");
            Console.WriteLine($"After adding 2 items => Capacity: {list.Capacity}, Count: {list.Count}");

            list.Add("Gamma"); // Capacity увеличится автоматически (обычно вдвое)
            Console.WriteLine($"After adding 3rd item => Capacity: {list.Capacity}, Count: {list.Count}");
            Console.WriteLine($"Содержимое List: [{string.Join(", ", list)}]");

            // Вставка, удаление, поиск
            list.Insert(1, "Inserted");      // ["Alpha", "Inserted", "Beta", "Gamma"]
            Console.WriteLine($"После Insert(1): [{string.Join(", ", list)}]");

            list.Remove("Beta");             // удаляет первое вхождение "Beta"
            Console.WriteLine($"После Remove(\"Beta\"): [{string.Join(", ", list)}]");

            list.RemoveAt(0);                // удаляет элемент по индексу 0
            Console.WriteLine($"После RemoveAt(0): [{string.Join(", ", list)}]");

            Console.WriteLine($"Contains(\"Gamma\"): {list.Contains("Gamma")}");
            int idx = list.IndexOf("Gamma");
            Console.WriteLine($"IndexOf(\"Gamma\"): {idx}");

            // Clear и TrimExcess
            list.Clear();
            Console.WriteLine($"После Clear => Count: {list.Count}, Capacity (до TrimExcess): {list.Capacity}");
            list.TrimExcess();
            Console.WriteLine($"После TrimExcess => Capacity: {list.Capacity}");
        }


        // -----------------------------------
        // 3) LinkedList<T>: двусвязный список
        // -----------------------------------
        private void DemoLinkedList()
        {
            Console.WriteLine("\n-- LinkedList<T> --");

            var linked = new LinkedList<int>();
            linked.AddLast(10);
            linked.AddLast(20);
            linked.AddLast(30);
            Console.WriteLine("Добавили 10, 20, 30");

            // Добавление в начало
            linked.AddFirst(5);
            Console.WriteLine("Добавили 5 в начало");

            Console.Write("LinkedList сейчас: ");
            foreach (var val in linked)
                Console.Write($"{val} ");
            Console.WriteLine();

            // Удаление узла
            linked.Remove(20);
            Console.WriteLine("Удалили узел со значением 20");

            // Вставка перед указанным узлом
            var node = linked.Find(30);
            if (node != null)
                linked.AddBefore(node, 25);
            Console.WriteLine("Вставили 25 перед 30");

            Console.Write("После всех операций: ");
            for (var cur = linked.First; cur != null; cur = cur.Next)
                Console.Write($"{cur.Value} ");
            Console.WriteLine();
        }


        // -----------------------------------
        // 4) Queue<T> и Stack<T>
        // -----------------------------------
        private void DemoQueueAndStack()
        {
            Console.WriteLine("\n-- Queue<T> & Stack<T> --");

            // Queue<T> - FIFO
            var queue = new Queue<string>();
            queue.Enqueue("First");
            queue.Enqueue("Second");
            queue.Enqueue("Third");
            Console.WriteLine($"Queue.Peek(): {queue.Peek()} (не удаляет элемент)");
            Console.WriteLine($"Dequeued: {queue.Dequeue()}");
            Console.WriteLine($"Оставшиеся в очереди: [{string.Join(", ", queue)}]");

            // Stack<T> - LIFO
            var stack = new Stack<string>();
            stack.Push("One");
            stack.Push("Two");
            stack.Push("Three");
            Console.WriteLine($"Stack.Peek(): {stack.Peek()} (показывает верхний элемент)");
            Console.WriteLine($"Popped: {stack.Pop()}");
            Console.WriteLine($"Оставшиеся в стеке: [{string.Join(", ", stack)}]");
        }


        // -----------------------------------
        // 5) Dictionary<TKey, TValue>
        // -----------------------------------
        private void DemoDictionary()
        {
            Console.WriteLine("\n-- Dictionary<TKey, TValue> --");

            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Apple"] = 5,
                ["Banana"] = 3
            };
            dict.Add("Cherry", 7);
            Console.WriteLine($"Изначальный Dictionary: Apple=5, Banana=3, Cherry=7");

            // Обновление
            dict["apple"] = 10; // благодаря IgnoreCase ключ "Apple"/"apple" считается одним
            Console.WriteLine($"После обновления apple => 10 (IgnoreCase): Apple={dict["Apple"]}");

            // TryGetValue
            if (dict.TryGetValue("banana", out int bananaCount))
                Console.WriteLine($"TryGetValue(\"banana\"): {bananaCount}");

            // Перебор пар ключ-значение
            Console.Write("Все элементы: ");
            foreach (var kvp in dict)
                Console.Write($"[{kvp.Key}={kvp.Value}] ");
            Console.WriteLine();

            // Удаление
            dict.Remove("Cherry");
            Console.WriteLine("После Remove(\"Cherry\"): " + (dict.ContainsKey("Cherry") ? "Cherry существует" : "Cherry удалён"));
        }


        // -----------------------------------
        // 6) SortedDictionary<TKey, TValue> и SortedList<TKey, TValue>
        // -----------------------------------
        private void DemoSortedDictionaryAndSortedList()
        {
            Console.WriteLine("\n-- SortedDictionary & SortedList --");

            // SortedDictionary: красно-чёрное дерево (автосортировка по ключу)
            var sortedDict = new SortedDictionary<int, string>();
            sortedDict.Add(3, "Three");
            sortedDict.Add(1, "One");
            sortedDict.Add(2, "Two");
            Console.Write("SortedDictionary (по возрастанию ключей): ");
            foreach (var kvp in sortedDict)
                Console.Write($"[{kvp.Key}={kvp.Value}] ");
            Console.WriteLine();

            // SortedList: внутри массив ключей и значений; быстрая индексация, но дороже вставка/удаление
            var sortedList = new SortedList<int, string>();
            sortedList.Add(30, "Thirty");
            sortedList.Add(10, "Ten");
            sortedList.Add(20, "Twenty");
            Console.Write("SortedList (по возрастанию ключей): ");
            foreach (var kvp in sortedList)
                Console.Write($"[{kvp.Key}={kvp.Value}] ");
            Console.WriteLine();
        }


        // -----------------------------------
        // 7) HashSet<T> и операции над множествами
        // -----------------------------------
        private void DemoHashSet()
        {
            Console.WriteLine("\n-- HashSet<T> & Set Operations --");

            var setA = new HashSet<int> { 1, 2, 3, 4 };
            var setB = new HashSet<int> { 3, 4, 5, 6 };

            Console.WriteLine($"setA: [{string.Join(", ", setA)}]");
            Console.WriteLine($"setB: [{string.Join(", ", setB)}]");

            // Union: объединение
            var union = new HashSet<int>(setA);
            union.UnionWith(setB);
            Console.WriteLine($"UnionWith: [{string.Join(", ", union)}]");

            // Intersection: пересечение
            var intersection = new HashSet<int>(setA);
            intersection.IntersectWith(setB);
            Console.WriteLine($"IntersectWith: [{string.Join(", ", intersection)}]");

            // Except: разность A\B
            var except = new HashSet<int>(setA);
            except.ExceptWith(setB);
            Console.WriteLine($"ExceptWith(A \\ B): [{string.Join(", ", except)}]");

            // SymmetricExcept: симметричная разность
            var symDiff = new HashSet<int>(setA);
            symDiff.SymmetricExceptWith(setB);
            Console.WriteLine($"SymmetricExceptWith: [{string.Join(", ", symDiff)}]");

            // Пользовательский компаратор
            var ciSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hello", "world", "HELLO"
        };
            Console.WriteLine($"HashSet<string> IgnoreCase: [{string.Join(", ", ciSet)}] (без дублирования)");
        }


        // -----------------------------------
        // 8) ReadOnlyCollection<T> и Immutable Collections
        // -----------------------------------
        private void DemoReadOnlyAndImmutable()
        {
            Console.WriteLine("\n-- ReadOnlyCollection & Immutable Collections --");

            // ReadOnlyCollection<T>: обёртка над List<T> или массивом
            var baseList = new List<int> { 1, 2, 3 };
            var readOnly = new ReadOnlyCollection<int>(baseList);
            Console.WriteLine($"ReadOnlyCollection из List<int>: [{string.Join(", ", readOnly)}]");

            // Попытка изменить выдаст ошибку времени выполнения, потому что Add/Remove не доступны
            baseList.Add(4);
            Console.WriteLine($"После модификации baseList: [{string.Join(", ", readOnly)}] (ReadOnly меняется, т.к. это обёртка)");

            // Чтобы полностью предотвратить изменение, используют Immutable Collections
            var immutableList = ImmutableList.Create(10, 20, 30);
            Console.WriteLine($"ImmutableList initial: [{string.Join(", ", immutableList)}]");
            var newImmutable = immutableList.Add(40);
            Console.WriteLine($"After Add(40): [{string.Join(", ", newImmutable)}] (оригинал не меняется: [{string.Join(", ", immutableList)}])");

            var immutableDict = ImmutableDictionary.CreateBuilder<string, int>();
            immutableDict.Add("X", 1);
            immutableDict.Add("Y", 2);
            var builtDict = immutableDict.ToImmutable();
            Console.Write("ImmutableDictionary: ");
            foreach (var kvp in builtDict)
                Console.Write($"[{kvp.Key}={kvp.Value}] ");
            Console.WriteLine();
        }


        // -----------------------------------
        // 9) Concurrent Collections
        // -----------------------------------
        private void DemoConcurrentCollections()
        {
            Console.WriteLine("\n-- Concurrent Collections --");

            // ConcurrentDictionary<TKey, TValue>
            var concurrentDict = new ConcurrentDictionary<string, int>();
            concurrentDict.TryAdd("A", 1);
            concurrentDict.TryAdd("B", 2);
            Console.WriteLine($"ConcurrentDictionary initial: [{string.Join(", ", concurrentDict)}]");

            // AddOrUpdate: атомарная операция
            concurrentDict.AddOrUpdate("A", 0, (_, old) => old + 10);
            concurrentDict.AddOrUpdate("C", 5, (_, old) => old + 10);
            Console.WriteLine($"After AddOrUpdate: [{string.Join(", ", concurrentDict)}]");

            // ConcurrentQueue<T>
            var concurrentQueue = new ConcurrentQueue<string>();
            concurrentQueue.Enqueue("Q1");
            concurrentQueue.Enqueue("Q2");
            concurrentQueue.Enqueue("Q3");
            Console.WriteLine($"ConcurrentQueue initial: [{string.Join(", ", concurrentQueue)}]");

            if (concurrentQueue.TryDequeue(out var dequeued))
                Console.WriteLine($"Dequeued (ConcurrentQueue): {dequeued}");
            Console.WriteLine($"After Dequeue: [{string.Join(", ", concurrentQueue)}]");

            // ConcurrentBag<T>
            var concurrentBag = new ConcurrentBag<int>();
            concurrentBag.Add(100);
            concurrentBag.Add(200);
            concurrentBag.Add(300);
            Console.Write("ConcurrentBag items: ");
            while (concurrentBag.TryTake(out var item))
                Console.Write($"{item} ");
            Console.WriteLine();

            // BlockingCollection<T> (обёртка над IProducerConsumerCollection)
            using (var blocking = new BlockingCollection<int>(boundedCapacity: 2))
            {
                // TryAdd / TryTake с блокировкой, полезно для producer-consumer
                blocking.Add(10);
                blocking.Add(20);
                Console.WriteLine("BlockingCollection: добавили 10, 20");
                // Следующий Add заблокирует, пока не вызовут Take в другом потоке
                // Demonstration: берем элемент
                var taken = blocking.Take();
                Console.WriteLine($"BlockingCollection.Take(): {taken}");
            }
        }
    }
}
