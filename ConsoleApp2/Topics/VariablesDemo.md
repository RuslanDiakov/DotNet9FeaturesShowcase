# VariablesDemo

**Описание:**  
Класс `VariablesDemo` демонстрирует все ключевые возможности работы с переменными и типами в C# на уровне Middle/Senior. Он приведёт примеры:

1. Примитивные типы (целые, с плавающей точкой, `bool`, `char`)
2. Nullable-типы (`T?`) и `default(T)`
3. Константы (`const`), `readonly`-поля и `static readonly`-поля
4. Экземплярные и статические поля
5. Инференция типов (`var`) и `dynamic`
6. Кортежи (Tuples) и деконструкция
7. Параметры `ref`, `out`, `in`
8. `object`-тип, боксинг/анбоксинг
9. Discard (`_`)
10. Span<T> и `stackalloc`

Каждый метод помечен соответствующим заголовком и выводит в консоль демонстрацию работы.

---

## 1. Constant, Readonly, Static Fields

```csharp
private const double PiConst = 3.14159265358979323846;
private readonly int _readonlyField = 10;
private static readonly string StaticReadonlyString = "Hello, Static Readonly";
private int _instanceField = 5;
private static int _staticField = 100;
````

* `const`: компилятор вставляет значение напрямую в IL-код. Значение неизменяемо, может быть только примитивным или `string`.
* `readonly`: поле может быть инициализировано либо при объявлении, либо в конструкторе; далее им нельзя присвоить новое значение.
* `static readonly`: то же самое, но для статических полей класса.
* `_instanceField`: поле экземпляра, хранится отдельно для каждого объекта.
* `_staticField`: статическое поле, общее для всех экземпляров класса.

---

## 2. Primitive Types

```csharp
sbyte   sb  = -100;                    // 8-bit signed
byte    b   = 200;                     // 8-bit unsigned
short   s   = -30_000;                 // 16-bit signed
ushort  us  = 60_000;                  // 16-bit unsigned
int     i   = 123_456;                 // 32-bit signed
uint    ui  = 4_000_000_000;           // 32-bit unsigned
long    l   = -9_000_000_000_000_000_000; // 64-bit signed
ulong   ul  = 18_000_000_000_000_000_000; // 64-bit unsigned
float   f   = 3.14F;                   // 32-bit floating
double  d   = 2.718281828;             // 64-bit floating
decimal dec = 79_228_162_514_264_337_593_543_950_335M; // 128-bit, финансы
bool    boolean = true;                // true/false
char    ch  = 'Z';                     // 16-bit Unicode character
```

* **Целые** (`sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`): отличаются размером (от 8 до 64 бит) и знаком.
* **С плавающей точкой** (`float`, `double`, `decimal`):

  * `float` (32-бит)
  * `double` (64-бит)
  * `decimal` (128-бит, высокоточный, чтобы избежать потерь при финансовых вычислениях).
* **bool**: хранит только `true` или `false`.
* **char**: 16-битный, хранит единственный символ Unicode.

---

## 3. Nullable Types & default(T)

```csharp
int?     nullableInt     = null;
Console.WriteLine(nullableInt.HasValue); // false
nullableInt = 42;
Console.WriteLine(nullableInt.Value);    // 42

int valueOrDefault = nullableInt ?? default; // если nullableInt == null, вернётся default(int) == 0

DateTime defaultDate = default; 
Console.WriteLine(defaultDate); // 0001-01-01T00:00:00.0000000 (DateTime.MinValue)
```

* `T?` (или `Nullable<T>`) позволяет примитивному типу хранить `null`.
* Проверка на наличие значения: `nullableInt.HasValue`.
* Получение через `nullableInt.Value` (или `nullableInt.GetValueOrDefault()`).
* `default(T)` возвращает «значение по умолчанию»: для чисел — 0, для `bool` — `false`, для `DateTime` — `DateTime.MinValue` и т. д.

---

## 4. var (Implicit Typing) и dynamic

```csharp
var implicitInt      = 123;                       // компилятором выведется int
var implicitString   = "Implicit typing example"; // string
var implicitArray    = new[] { 1, 2, 3, 4 };      // int[]
var implicitList     = new List<string> { "a", "b", "c" };
var implicitDateTime = DateTime.UtcNow;

Console.WriteLine(implicitInt.GetType().Name);   // "Int32"
Console.WriteLine(implicitArray.GetType().Name); // "Int32[]"
```

* При `var` компилятор выводит конкретный тип на этапе компиляции — после этого переменная остаётся строго типизированной.
* `dynamic` отключает проверку типов во время компиляции: все операции проверяются лишь при запуске. Пример:

```csharp
dynamic dyn = "First as string";
Console.WriteLine($"{dyn} (Type: {dyn.GetType().Name})");
dyn = 12345;
Console.WriteLine($"{dyn} (Type: {dyn.GetType().Name})");
```

> **Важно:** при неправильном приведении типов ошибки выпадут только во время выполнения (runtime).

---

## 5. Tuples & Deconstruction

```csharp
(int X, int Y) namedPoint = (10, 20);
Console.WriteLine($"Named tuple: ({namedPoint.X}, {namedPoint.Y})");

var unnamedTuple = ("Alice", 30);
Console.WriteLine($"Unnamed tuple: (\"{unnamedTuple.Item1}\", {unnamedTuple.Item2})");

// Деконструкция:
var (name, age) = unnamedTuple;
Console.WriteLine($"Deconstructed: name = {name}, age = {age}");

var result = GetMinMax((5, 100));
Console.WriteLine($"GetMinMax from (5,100): min = {result.min}, max = {result.max}");
```

* Кортежи позволяют возвращать/передавать несколько значений без создания отдельного класса или структуры.
* Можно давать имена элементам `(X, Y)` или обращаться по `Item1`, `Item2`.
* Деконструкция (`var (a, b) = tuple;`) упрощает получение значений.
* Метод может возвращать именованный кортеж:

```csharp
private static (int min, int max) GetMinMax((int a, int b) input)
{
    return input.a < input.b
        ? (input.a, input.b)
        : (input.b, input.a);
}
```

---

## 6. ref, out и in параметры

```csharp
int refValue = 5;
Console.WriteLine($"Before RefMethod: {refValue}");
RefMethod(ref refValue);
Console.WriteLine($"After RefMethod:  {refValue}");

OutMethod(out int outValue);
Console.WriteLine($"OutMethod returned: {outValue}");

int inValue = 100;
InMethod(in inValue);
```

* `ref`: передача по ссылке, значение должно быть инициализировано до вызова.

  * Метод `RefMethod(ref int number)` может изменить исходную переменную.
* `out`: передача по ссылке, до вызова переменная не инициализируется. Метод обязан присвоить значение.

  * `OutMethod(out int result)` присваивает `result`, и после вызова переменная содержит это значение.
* `in`: передача “по ссылке” в режиме «только чтение». Полезно для performance-сценариев (большие структуры), т. к. нет копирования, но метод не может переопределить параметр.

```csharp
private void RefMethod(ref int number)
{
    number += 10; 
    Console.WriteLine($"  [Inside RefMethod] number = {number}");
}

private void OutMethod(out int result)
{
    result = 99;
    Console.WriteLine($"  [Inside OutMethod] result = {result}");
}

private void InMethod(in int number)
{
    Console.WriteLine($"  [Inside InMethod] number (readonly) = {number}");
    // number = 5; // Ошибка: изменить нельзя
}
```

---

## 7. object, Boxing & Unboxing

```csharp
int primitiveInt = 1234;
object boxedObj = primitiveInt;      // boxing: значение типа-значения кладётся в heap
Console.WriteLine($"boxedObj: {boxedObj} (Type: {boxedObj.GetType().Name})");

int unboxedInt = (int)boxedObj;      // unboxing: явное приведение к исходному типу
Console.WriteLine($"unboxedInt: {unboxedInt} (Type: {unboxedInt.GetType().Name})");
```

* Любой тип-значение (value type) при присвоении переменной типа `object` упаковывается (boxing) в объект на heap.
* При приведении `object` обратно к исходному типу происходит распаковка (unboxing).

> **Совет:** чтобы избежать лишнего boxing/unboxing, избегай использовать `object` там, где нужен value type.

---

## 8. Discard Pattern (`_`)

```csharp
var triple = (1, 2, 3);
var (_, second, _) = triple;
Console.WriteLine($"Из кортежа (1,2,3) извлечён только second = {second}");

if (int.TryParse("999", out _))
{
    Console.WriteLine("Parse succeeded, но нам не нужно само число.");
}
```

* `_` (discard) позволяет явно игнорировать ненужные значения:

  * При деконструкции кортежа.
  * При использовании `out`-параметра, если само значение не нужно.

---

## 9. Span<T> & stackalloc

```csharp
Span<int> span = stackalloc int[3] { 10, 20, 30 };
for (int idx = 0; idx < span.Length; idx++)
{
    Console.WriteLine($"span[{idx}] = {span[idx]}");
}
```

* `stackalloc` выделяет «блок» памяти на стеке, а `Span<T>` позволяет безопасно обрабатывать этот буфер без аллока в managed heap.
* Полезно для временных небольших массивов, когда нужна максимальная производительность и минимум сборок мусора.

> **Важно:** `Span<T>` работает только «локально», его нельзя хранить в полях класса, только как локальную переменную или параметр метода.

---

## Как использовать

1. В `Program.cs` (или в общем меню демо-приложения) достаточно вызвать:

   ```csharp
   new VariablesDemo().Run();
   ```

2. При запуске консоли вы увидите, как последовательно выводятся все примеры для каждой темы.

---

**Этот демо-класс позволяет изучить и отработать на практике все нюансы работы с переменными в C# (уровень Middle/Senior).**\`\`\`
