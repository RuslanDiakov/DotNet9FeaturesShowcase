# CollectionsDemo

**Описание:**  
Класс `CollectionsDemo` демонстрирует ключевые коллекции .NET на уровне Middle/Senior и их особенности:

1. Одномерные, многомерные и зубчатые массивы
2. `List<T>`: динамический массив и принципы работы с `Capacity`/`Count`
3. `LinkedList<T>`: двусвязный список, вставка, удаление, итерация
4. `Queue<T>` и `Stack<T>`: принципы FIFO и LIFO
5. `Dictionary<TKey,TValue>`: хеш-таблица, поиск, обновление, `StringComparer`
6. `SortedDictionary<TKey,TValue>` и `SortedList<TKey,TValue>`: отличие в реализации и производительности
7. `HashSet<T>`: операции над множествами, пользовательский компаратор
8. `ReadOnlyCollection<T>` и `System.Collections.Immutable` (immutable–коллекции)
9. `ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentBag`, `BlockingCollection` (потокобезопасные коллекции)

Каждый метод снабжён демонстрацией и пояснением работы.

---

## 1. Arrays (Массивы)

```csharp
int[] oneDim = new int[5] { 10, 20, 30, 40, 50 };
Console.WriteLine($"Одномерный массив: [{string.Join(", ", oneDim)}]");

// Многомерный массив 2x3
int[,] twoDim = new int[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
// Проход по двумерному:
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

// Зубчатый (jagged) массив: массив массивов разной длины
int[][] jagged = new int[3][];
jagged[0] = new[] { 1, 2 };
jagged[1] = new[] { 3, 4, 5 };
jagged[2] = new[] { 6 };

// ArraySegment<T> — обёртка над частью массива:
var segment = new ArraySegment<int>(oneDim, 1, 3); // [20,30,40]
````

* **Одномерный массив**: фиксированная длина, низкоуровневый доступ по индексу.
* **Многомерный массив**: используется, когда нужна прямоугольная матрица.
* **Зубчатый массив**: позволяет создавать “разреженные” структуры, где каждая строка может иметь свою длину.
* **ArraySegment<T>**: легковесная обёртка над фрагментом существующего массива — выгодна, когда нужно передать в метод часть массива без копирования.

---

## 2. List<T>

```csharp
var list = new List<string>(capacity: 2);
Console.WriteLine($"Изначальная Capacity: {list.Capacity}, Count: {list.Count}");

list.Add("Alpha");
list.Add("Beta");
// Capacity автоматически расширится при добавлении третьего элемента
list.Add("Gamma");

Console.WriteLine($"После добавления: Capacity: {list.Capacity}, Count: {list.Count}");
Console.WriteLine($"Содержимое List: [{string.Join(", ", list)}]");

// Вставка
list.Insert(1, "Inserted"); 
// Удаление
list.Remove("Beta");
list.RemoveAt(0);

// Поиск
bool contains = list.Contains("Gamma");
int idx = list.IndexOf("Gamma");

// Clear + TrimExcess для сброса Count и Capacity
list.Clear();
list.TrimExcess();
```

* **Capacity vs Count:** Capacity — внутренняя длина массива, под которым реализован `List<T>`. Count — фактическое количество элементов. При переполнении Capacity увеличивается (обычно вдвое).
* **Insert/Remove/RemoveAt:** стандартные операции. Важно понимать, что вставка/удаление в середине списка может быть дорогой (O(n)), если список большой.
* **TrimExcess:** уменьшает Capacity до текущего Count, чтобы избежать лишней занимаемой памяти.

---

## 3. LinkedList<T>

```csharp
var linked = new LinkedList<int>();
linked.AddLast(10);
linked.AddLast(20);
linked.AddLast(30);
linked.AddFirst(5);

// Итерация
foreach (var val in linked)
    Console.Write($"{val} "); // 5,10,20,30

// Найти узел и вставить перед ним
var node = linked.Find(30); 
if (node != null)
    linked.AddBefore(node, 25);

// Удаление узла по значению
linked.Remove(20);
```

* **Вставка/удаление в середине** — O(1), если у вас есть `LinkedListNode<T>`.
* **Последовательный доступ** через `First` → `Next` или `Last` → `Previous`.
* Полезен при частых вставках/удалениях, когда не нужен произвольный доступ по индексу.

---

## 4. Queue<T> и Stack<T>

```csharp
// Queue<T> — FIFO
var queue = new Queue<string>();
queue.Enqueue("First");
queue.Enqueue("Second");
queue.Enqueue("Third");
Console.WriteLine($"Peek: {queue.Peek()}"); // "First"
Console.WriteLine($"Dequeue: {queue.Dequeue()}"); // "First"

// Stack<T> — LIFO
var stack = new Stack<string>();
stack.Push("One");
stack.Push("Two");
stack.Push("Three");
Console.WriteLine($"Peek: {stack.Peek()}"); // "Three"
Console.WriteLine($"Pop: {stack.Pop()}");   // "Three"
```

* **Queue<T>**: добавление в конец, удаление из начала; хорошо подходит для очередей задач.
* **Stack<T>**: добавление/удаление с вершины; используется в алгоритмах обратной польской нотации, при обходе графов/деревьев без рекурсии.

---

## 5. Dictionary\<TKey, TValue>

```csharp
var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
{
    ["Apple"] = 5,
    ["Banana"] = 3
};
dict.Add("Cherry", 7);

// Обновление (IgnoreCase)
dict["apple"] = 10; 
Console.WriteLine($"Apple: {dict["Apple"]} (IgnoreCase)");

// TryGetValue
if (dict.TryGetValue("banana", out int cnt))
    Console.WriteLine($"Banana count: {cnt}");

// Перебор
foreach (var kvp in dict)
    Console.WriteLine($"Key={kvp.Key}, Value={kvp.Value}");

// Удаление
dict.Remove("Cherry");
```

* **Хеш-таблица**: среднее время доступа O(1).
* **StringComparer.OrdinalIgnoreCase** позволяет создавать Dictionary без учёта регистра строк-ключей.
* **TryGetValue** — самое быстрое и безопасное получение значения.
* **Итерация** через `foreach (KeyValuePair<TKey,TValue>)`.

---

## 6. SortedDictionary\<TKey, TValue> и SortedList\<TKey, TValue>

```csharp
// SortedDictionary: основан на красно-чёрном дереве
var sortedDict = new SortedDictionary<int, string>();
sortedDict.Add(3, "Three");
sortedDict.Add(1, "One");
sortedDict.Add(2, "Two");
// Итерация по возрастанию ключей
foreach (var kvp in sortedDict)
    Console.Write($"[{kvp.Key}={kvp.Value}] ");

// SortedList: внутри два массива — ключей и значений
var sortedList = new SortedList<int, string>();
sortedList.Add(30, "Thirty");
sortedList.Add(10, "Ten");
sortedList.Add(20, "Twenty");
// Итерация по возрастанию ключей
foreach (var kvp in sortedList)
    Console.Write($"[{kvp.Key}={kvp.Value}] ");
```

* **SortedDictionary**: вставка/удаление O(log n), быстрый поиск, автоматическая сортировка по ключу.
* **SortedList**: быстрая индексация по индексу (O(1)), но вставка/удаление O(n) (из-за сдвига массивов).
* Оба полезны, когда нужно поддерживать упорядоченный набор пар «ключ-значение».

---

## 7. HashSet<T> и операции над множествами

```csharp
var setA = new HashSet<int> { 1, 2, 3, 4 };
var setB = new HashSet<int> { 3, 4, 5, 6 };

// Объединение
var union = new HashSet<int>(setA);
union.UnionWith(setB); // {1,2,3,4,5,6}

// Пересечение
var intersection = new HashSet<int>(setA);
intersection.IntersectWith(setB); // {3,4}

// Разность A \ B
var except = new HashSet<int>(setA);
except.ExceptWith(setB); // {1,2}

// Симметричная разность
var symDiff = new HashSet<int>(setA);
symDiff.SymmetricExceptWith(setB); // {1,2,5,6}

// Пользовательский компаратор (IgnoreCase)
var ciSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "hello", "world", "HELLO"
}; // В результате {"hello","world"}, HELLO дублирует hello
```

* **HashSet<T>**: обеспечивает уникальность элементов, операции над множествами (`UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith`).
* **StringComparer** и другие реализацыи `IEqualityComparer<T>` позволяют контролировать сравнение.

---

## 8. ReadOnlyCollection<T> и Immutable Collections

```csharp
var baseList = new List<int> { 1, 2, 3 };
var readOnly = new ReadOnlyCollection<int>(baseList);
// Несмотря на то, что readOnly — “только для чтения”, 
// если изменить baseList, readOnly отразит эти изменения.
baseList.Add(4);

// ImmutableList<T>
var immutableList = ImmutableList.Create(10, 20, 30);
var newImmutable = immutableList.Add(40);
// immutableList остаётся без изменений: {10,20,30}
// newImmutable содержит {10,20,30,40}

var builder = ImmutableDictionary.CreateBuilder<string, int>();
builder.Add("X", 1);
builder.Add("Y", 2);
var immutableDict = builder.ToImmutable();
```

* **ReadOnlyCollection<T>**: обёртка над изменяемой коллекцией. Запрещает вызов методов `Add/Remove` напрямую, но исходная коллекция может быть изменена.
* **Immutable Collections** (`System.Collections.Immutable`): полностью неизменяемые, поддерживают “функциональные” операции, создают новое состояние при изменении.

  * `ImmutableList<T>`, `ImmutableDictionary<K,V>`, `ImmutableHashSet<T>` и т. д.

---

## 9. Concurrent Collections

```csharp
// ConcurrentDictionary<TKey, TValue>
var concurrentDict = new ConcurrentDictionary<string, int>();
concurrentDict.TryAdd("A", 1);
concurrentDict.TryAdd("B", 2);
// AddOrUpdate: атомарная операция
concurrentDict.AddOrUpdate("A", 0, (_, old) => old + 10); 
concurrentDict.AddOrUpdate("C", 5, (_, old) => old + 10);

// ConcurrentQueue<T>
var concurrentQueue = new ConcurrentQueue<string>();
concurrentQueue.Enqueue("Q1");
concurrentQueue.Enqueue("Q2");
if (concurrentQueue.TryDequeue(out var dq))
    Console.WriteLine($"Dequeued: {dq}");

// ConcurrentBag<T>
var concurrentBag = new ConcurrentBag<int>();
concurrentBag.Add(100);
concurrentBag.Add(200);
while (concurrentBag.TryTake(out var item))
    Console.Write($"{item} ");
Console.WriteLine();

// BlockingCollection<T>: обёртка над IProducerConsumerCollection
using (var blocking = new BlockingCollection<int>(boundedCapacity: 2))
{
    blocking.Add(10);
    blocking.Add(20);
    // blocking.Add(30); // заблокируется, пока кто-то не сделает Take
    var taken = blocking.Take();
    Console.WriteLine($"BlockingCollection.Take(): {taken}");
}
```

* **ConcurrentDictionary**: потокобезопасная хеш-таблица с атомарными операциями `AddOrUpdate`, `GetOrAdd`.
* **ConcurrentQueue / ConcurrentStack / ConcurrentBag**: потокобезопасные версии обычных очереди/стека/“мешка”.
* **BlockingCollection<T>**: обёртка, позволяющая блокировать поток, если коллекция переполнена или пуста, подходит для шаблона producer-consumer.

---

### Использование

В `Program.cs` или общем меню демо-приложения просто вызови:

```csharp
new CollectionsDemo().Run();
```

При запуске последовательно выполнятся все демонстрации, и в консоли можно будет увидеть вывод для каждой коллекции.

---
