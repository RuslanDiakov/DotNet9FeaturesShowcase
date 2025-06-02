# DelegatesDemo

**Описание:**  
Класс `DelegatesDemo` демонстрирует все ключевые аспекты работы с делегатами в C# на уровне Middle/Senior. Он охватывает:

1. Простейшее объявление и вызов делегата  
2. Анонимные методы и лямбда-выражения  
3. Мультикаст-делегаты  
4. Обобщённые делегаты (`Func`, `Action`, `Predicate`, `Comparison`)  
5. Ковариантность и контрвариантность  
6. Использование делегата как параметра и возвращаемого значения  
7. Замыкания (Closures)  
8. События, основанные на делегатах  
9. `DynamicInvoke`  
10. Асинхронные делегаты (`BeginInvoke`/`EndInvoke`)  

Каждый метод выводит в консоль примеры, чтобы показать особенности и подводные камни.

---

## 1. Basic Delegate Declaration & Invocation

```csharp
// Объявление delegate-типов
public delegate int MathOperation(int x, int y);

// Привязка методов
MathOperation add = Add;
MathOperation multiply = Multiply;

int a = 5, b = 3;
Console.WriteLine($"Add({a}, {b}) = {add(a, b)}");        // 8
Console.WriteLine($"Multiply({a}, {b}) = {multiply(a, b)}"); // 15

// Определения методов
private int Add(int x, int y) => x + y;
private int Multiply(int x, int y) => x * y;
````

* **Что важно знать:**

  * Делегат — это тип, представляющий ссылку на метод с определённым сигнатурой (возвращаемый тип + параметры).
  * При присвоении `add = Add;` компилятор автоматически создаёт экземпляр делегата, ссылающийся на статический/инстанс-метод `Add`.
  * Вызов делегата (`add(a, b)`) эквивалентен вызову метода `Add(a, b)`.

---

## 2. Anonymous Methods & Lambda Expressions

```csharp
// Анонимный метод (delegate keyword)
MathOperation anonymousOp = delegate(int x, int y)
{
    return x - y;
};
Console.WriteLine($"Anonymous subtract: 10 - 4 = {anonymousOp(10, 4)}"); // 6

// Лямбда-выражение
MathOperation lambdaOp = (x, y) => x / y;
Console.WriteLine($"Lambda divide: 20 / 5 = {lambdaOp(20, 5)}");         // 4
```

* **Anonymous Method:**

  * Использует ключевое слово `delegate`.
  * Можно описать логику «прямо в месте присваивания».
  * Полезно, когда нужно быстро «вкинуть» кусочек кода без явного метода.
* **Lambda Expression:**

  * Более компактная форма, особенно для одно- или двухстрочных блоков.
  * Позволяет легко передавать функциональную логику в методы, принимающие делегат.

---

## 3. Multicast Delegates

```csharp
public delegate void Notify(string message);

Notify notify1 = Message1;
Notify notify2 = Message2;

// Объединяем в multicast-делегат
Notify multiNotify = notify1 + notify2;
multiNotify("Hello, delegates!");

// Убираем конкретный подписчик
multiNotify -= notify2;
multiNotify("Hello again!");

private void Message1(string msg)
{
    Console.WriteLine($"  [Message1] {msg}");
}

private void Message2(string msg)
{
    Console.WriteLine($"  [Message2] {msg}");
}
```

* **Мультикаст-делегат** — это цепочка делегатов, вызываемых последовательно.
* Операторы `+=` и `-=` добавляют или удаляют методы из списка.
* Если возвращаемый тип у делегата — `void`, то можно безопасно объединять несколько методов.
* Если возвращаемый тип не `void`, то при комбинировании нескольких методов будет возвращено значение крайнего метода в списке.

---

## 4. Generic Delegates: Func, Action, Predicate, Comparison

```csharp
// Func<TResult>, Func<T1, TResult>, Func<T1, T2, TResult> и т. д.
Func<int, int, int> funcAdd = (x, y) => x + y;
Console.WriteLine($"Func add: 7 + 8 = {funcAdd(7, 8)}");

// Action<T> — возвращает void
Action<string> actionPrint = s => Console.WriteLine($"Action print: {s}");
actionPrint("Delegates in C#");

// Predicate<T> — возвращает bool
Predicate<int> predicate = x => x % 2 == 0;
Console.WriteLine($"Is 10 even? {predicate(10)}"); // True
Console.WriteLine($"Is 7 even? {predicate(7)}");   // False

// Comparison<T> — сравнение двух объектов, возвращает int
Comparison<string> comparison = (s1, s2) => s1.Length - s2.Length;
var words = new[] { "apple", "pear", "banana" };
Array.Sort(words, comparison);
Console.WriteLine("Sorted by length:");
foreach (var w in words)
    Console.WriteLine($"  {w}");
```

* **Func<…>** — обобщённый делегат, где последний типовой параметр — возвращаемый тип.

  * `Func<int, int, int>` означает «метод, принимающий два `int` и возвращающий `int`».
* **Action<…>** — обобщённый делегат, возвращает `void`.
* **Predicate<T>** — обобщённый делегат, принимающий один параметр `T` и возвращающий `bool` (удобен в методах вроде `List<T>.Find`).
* **Comparison<T>** — берёт два параметра `T` и возвращает `int` (как `IComparer<T>`).

> Использование **обобщённых делегатов** позволяет избежать объявления собственных типов делегатов во многих случаях и писать более универсальный код.

---

## 5. Covariance & Contravariance

```csharp
// Ковариантность: метод возвращает производный тип (Dog),
// но назначается в делегат, возвращающий базовый тип (Animal).
Func<Dog> createDog = () => new Dog();
Func<Animal> createAnimal = createDog; // Ковариантность
Animal animal = createAnimal();
Console.WriteLine($"Covariance: {animal.Speak()}"); // "Woof!"

// Контрвариантность: метод принимает базовый тип (Animal),
// но назначается в делегат, принимающий производный тип (Dog).
Action<Animal> processAnimal = a => Console.WriteLine($"Processing animal: {a.Speak()}");
Action<Dog> processDog = processAnimal; // Контрвариантность
processDog(new Dog()); // OK
```

* **Ковариантность** (`out`-вариантность) позволяет назначить `Func<Dog>` в переменную `Func<Animal>`, потому что `Dog` является наследником `Animal`.
* **Контрвариантность** (`in`-вариантность) позволяет назначить `Action<Animal>` в переменную `Action<Dog>`, так как метод, принимающий базовый тип, может обрабатывать любые его производные.
* Важно знать подводные камни: нельзя делать ковариантность/контрвариантность для всех сигнатур, но для `Func<…>` (возвращаемый тип) и `Action<…>` (параметры) она поддерживается.

---

## 6. Delegate as Parameter & Return Value

```csharp
public delegate int MathOperation(int x, int y);

// Передаём делегат в метод
private int ComputeOperation(int a, int b, MathOperation op)
{
    return op(a, b);
}

int x = 6, y = 7;
int result = ComputeOperation(x, y, (a, b) => a * b);
Console.WriteLine($"ComputeOperation: {x} * {y} = {result}");

// Возвращаем делегат из метода
private MathOperation GetOperation(string opName)
{
    return opName switch
    {
        "add"      => (x, y) => x + y,
        "subtract" => (x, y) => x - y,
        _          => (x, y) => 0
    };
}

var returnedDelegate = GetOperation("add");
Console.WriteLine($"Returned delegate: 15 + 5 = {returnedDelegate(15, 5)}");
```

* Делегат может выступать в роли **параметра** и/или **возвращаемого значения**.
* Это удобно при написании **высшего порядка функций** (методы, принимающие/возвращающие другие методы).
* Можно легко менять логику, передавая разные реализации в одном интерфейсе (без создания отдельных классов).

---

## 7. Closures

```csharp
// Пример замыкания: лямбда захватывает переменную outerCounter
int counter = 0;
Func<int> increment = () =>
{
    counter++;
    return counter;
};

Console.WriteLine($"First call: {increment()}");  // 1
Console.WriteLine($"Second call: {increment()}"); // 2

// Захват переменных в цикле
Action[] actions = new Action[3];
for (int i = 0; i < 3; i++)
{
    int loopVar = i; // Критично: новая локальная переменная внутри цикла
    actions[i] = () => Console.WriteLine($"  Captured loopVar = {loopVar}");
}

Console.WriteLine("Invoking captured actions:");
foreach (var action in actions)
    action(); 
// Вывод: Captured loopVar = 0, затем 1, затем 2
```

* **Замыкание** (closure) — это лямбда или анонимный метод, который «захватывает» (captures) переменную из внешнего контекста.
* В результате переменная продолжает существовать (в куче) даже после завершения метода, где она была определена.
* При использовании **цикла** важно создавать локальную копию параметра (`int loopVar = i;`), иначе все лямбды захватят одну и ту же переменную `i`.

---

## 8. Events with Delegates

```csharp
// Делегат для события
public delegate void Notify(string message);

private class Publisher
{
    // Объявление события
    public event Notify OnRaise;

    public void RaiseEvent(string message)
    {
        Console.WriteLine($"Publisher: Raising event '{message}'");
        OnRaise?.Invoke(message);
    }
}

private class Subscriber
{
    public Subscriber(Publisher publisher)
    {
        publisher.OnRaise += HandleEvent; // Подписываемся
    }

    public void HandleEvent(string message)
    {
        Console.WriteLine($"  [Subscriber] Received: {message}");
    }
}

// Использование в DemoEventsWithDelegates():
var publisher = new Publisher();
var subscriber = new Subscriber(publisher);

publisher.RaiseEvent("Event 1"); // Subscriber получит уведомление
publisher.RaiseEvent("Event 2");

// Отписка:
publisher.OnRaise -= subscriber.HandleEvent;
Console.WriteLine("After unsubscribing:");
publisher.RaiseEvent("Event 3"); // Subscriber уже не получит
```

* **Событие** (`event`) по сути является обёрткой над `delegate`-полем, позволяющей другим классам **подписываться** (`+=`) или **отписываться** (`-=`), но не присваивать его напрямую.
* Внутри класса-издателя (`Publisher`) событие объявляется как `public event Notify OnRaise;`.
* Подписчики (`Subscriber`) добавляют свои методы-обработчики в список вызова через `publisher.OnRaise += HandleEvent;`.
* Вызов события (`OnRaise?.Invoke(...)`) генерирует все подписанные колбэки.

---

## 9. Delegate.DynamicInvoke

```csharp
MathOperation add = (x, y) => x + y;
Delegate del = add; // Восходящее преобразование к System.Delegate

// Вызов через DynamicInvoke
object result = del.DynamicInvoke(10, 20);
Console.WriteLine($"DynamicInvoke result (10 + 20) = {result}"); // 30
```

* `DynamicInvoke` позволяет вызвать метод, на который указывает `Delegate`, **во время выполнения**, передавая аргументы в виде массива `object[]`.
* **Плюс:** гибкость—можно не знать сигнатуру во время компиляции.
* **Минус:** теряется безопасность типов, и производительность ниже, чем у прямого вызова.

---

## 10. Async Delegates (BeginInvoke / EndInvoke)

```csharp
Func<int, int, int> add = (x, y) =>
{
    Thread.Sleep(500); // эмуляция долгой работы
    return x + y;
};

// Запуск асинхронного выполнения
IAsyncResult asyncResult = add.BeginInvoke(8, 9, null, null);
Console.WriteLine("Doing other work while delegate runs asynchronously...");

// Получение результата
int sum = add.EndInvoke(asyncResult);
Console.WriteLine($"Async delegate result: 8 + 9 = {sum}"); // 17
```

* Раньше, до появления `Task`-базированных API, у каждого делегата автоматически генерировались методы `BeginInvoke` и `EndInvoke`, которые позволяли вызывать его асинхронно.
* `BeginInvoke` запускает метод в пуле потоков, возвращает `IAsyncResult`.
* `EndInvoke` блокирует текущий поток, пока делегат не завершится, и возвращает результат.
* В современных приложениях чаще используют `Task`/`async-await`, но знание `BeginInvoke`/`EndInvoke` всё ещё может встретиться в legacy-кодовой базе.

---

# Итог

Класс `DelegatesDemo` покрывает практически все аспекты, которые понадобятся Senior-разработчику при обсуждении делегатов на собеседовании:

1. **Объявление и вызов пользовательских делегатов**
2. **Anonymous Methods** и **Lambda Expressions**
3. **Multicast Delegates** (цепочки вызовов)
4. **Обобщённые делегаты** (`Func`, `Action`, `Predicate`, `Comparison`)
5. **Ковариантность / Контрвариантность**
6. **Передача делегата как параметр и возвращаемое значение**
7. **Замыкания** (closures) и нюансы захвата переменных
8. **События** (`event`) на основе делегатов
9. **DynamicInvoke** — вызов делегата «в рантайме»
10. **Async Delegates** (устаревший, но всё ещё встречающийся паттерн)

