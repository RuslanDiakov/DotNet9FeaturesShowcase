using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.Topics
{
    public class VariablesDemo : IDemo
    {
        // -----------------------------------
        // 1) Constant, readonly, static fields
        // -----------------------------------

        // const – компиляторно «вшитое» значение, нельзя изменить:
        private const double PiConst = 3.14159265358979323846;

        // readonly – можно инициализировать в месте объявления или в конструкторе, после – нельзя менять:
        private readonly int _readonlyField = 10;

        // static readonly – аналог readonly, но для статических полей:
        private static readonly string StaticReadonlyString = "Hello, Static Readonly";

        // instance field – поле экземпляра:
        private int _instanceField = 5;

        // static field – общее поле для всех экземпляров:
        private static int _staticField = 100;


        // -----------------------------------
        // 2) IDemo.Run() – входная точка демонстрации
        // -----------------------------------
        public void Run()
        {
            Console.WriteLine("=== VariablesDemo: Начало ===");

            DemoPrimitiveTypes();
            DemoNullableAndDefault();
            DemoConstantReadonlyStatic();
            DemoTypeInferenceAndDynamic();
            DemoTuplesAndDeconstruction();
            DemoRefOutInParameters();
            DemoObjectBoxingUnboxing();
            DemoDiscardPattern();
            DemoSpanAndStackalloc();

            Console.WriteLine("\n=== VariablesDemo: Конец ===");
        }


        // -----------------------------------
        // 3) Демонстрация всех примитивных типов
        // -----------------------------------
        private void DemoPrimitiveTypes()
        {
            Console.WriteLine("\n-- Primitive Types --");

            sbyte sb = -100;                       // 8-bit signed
            byte b = 200;                        // 8-bit unsigned
            short s = -30_000;                    // 16-bit signed
            ushort us = 60_000;                     // 16-bit unsigned
            int i = 123_456;                    // 32-bit signed
            uint ui = 4_000_000_000;              // 32-bit unsigned
            long l = -9_000_000_000_000_000_000;  // 64-bit signed
            ulong ul = 18_000_000_000_000_000_000;  // 64-bit unsigned
            float f = 3.14F;                      // 32-bit floating
            double d = 2.718281828;                // 64-bit floating
            decimal dec = 79_228_162_514_264_337_593_543_950_335M; // 128-bit high-precision
            bool boolean = true;                   // true/false
            char ch = 'Z';                        // 16-bit Unicode character

            Console.WriteLine($"sbyte:   {sb}");
            Console.WriteLine($"byte:    {b}");
            Console.WriteLine($"short:   {s}");
            Console.WriteLine($"ushort:  {us}");
            Console.WriteLine($"int:     {i}");
            Console.WriteLine($"uint:    {ui}");
            Console.WriteLine($"long:    {l}");
            Console.WriteLine($"ulong:   {ul}");
            Console.WriteLine($"float:   {f}");
            Console.WriteLine($"double:  {d}");
            Console.WriteLine($"decimal: {dec}");
            Console.WriteLine($"bool:    {boolean}");
            Console.WriteLine($"char:    {ch}");
        }


        // -----------------------------------
        // 4) Nullable<T> и default(T)
        // -----------------------------------
        private void DemoNullableAndDefault()
        {
            Console.WriteLine("\n-- Nullable Types & default --");

            int? nullableInt = null;
            Console.WriteLine($"nullableInt.HasValue: {nullableInt.HasValue}");

            nullableInt = 42;
            Console.WriteLine($"nullableInt.Value:    {nullableInt.Value}");

            // null-coalescing operator: если null, используем default(int) == 0
            int valueOrDefault = nullableInt ?? default;
            Console.WriteLine($"valueOrDefault:       {valueOrDefault}");

            // default для структуры DateTime – это DateTime.MinValue
            DateTime defaultDate = default;
            Console.WriteLine($"default(DateTime):    {defaultDate:O}");
        }


        // -----------------------------------
        // 5) const, readonly, static readonly, instance/static fields
        // -----------------------------------
        private void DemoConstantReadonlyStatic()
        {
            Console.WriteLine("\n-- Constant, Readonly & Static Fields --");

            Console.WriteLine($"const PiConst:                {PiConst}");

            Console.WriteLine($"readonly field (_readonlyField): {_readonlyField}");

            Console.WriteLine($"static readonly string:      {StaticReadonlyString}");

            Console.WriteLine($"instance field before:       {_instanceField}");
            _instanceField = 20;  // instanceField можно менять, но readonly нельзя
            Console.WriteLine($"instance field after:        {_instanceField}");

            Console.WriteLine($"static field before:         {_staticField}");
            _staticField = 200;  // static field можно менять
            Console.WriteLine($"static field after:          {_staticField}");
        }


        // -----------------------------------
        // 6) var, implicit typing & dynamic
        // -----------------------------------
        private void DemoTypeInferenceAndDynamic()
        {
            Console.WriteLine("\n-- var (Implicit typing) --");

            var implicitInt = 123;                       // тип int
            var implicitString = "Implicit typing example"; // тип string
            var implicitArray = new[] { 1, 2, 3, 4 };      // тип int[]
            var implicitList = new System.Collections.Generic.List<string> { "a", "b", "c" };
            var implicitDateTime = DateTime.UtcNow;           // тип DateTime

            Console.WriteLine($"implicitInt   ({implicitInt.GetType().Name}): {implicitInt}");
            Console.WriteLine($"implicitString({implicitString.GetType().Name}): {implicitString}");
            Console.WriteLine($"implicitArray ({implicitArray.GetType().Name} Length={implicitArray.Length})");
            Console.WriteLine($"implicitList  ({implicitList.GetType().Name} Count={implicitList.Count})");
            Console.WriteLine($"implicitDateTime({implicitDateTime.GetType().Name}): {implicitDateTime:O}");

            Console.WriteLine("\n-- dynamic (Run-time binding) --");

            dynamic dyn = "This is a string at runtime";
            Console.WriteLine($"dyn as string: {dyn} (Type: {dyn.GetType().Name})");
            dyn = 654321;
            Console.WriteLine($"dyn as int:    {dyn} (Type: {dyn.GetType().Name})");

            // При использовании dynamic, ошибки типов будут только на этапе выполнения
        }


        // -----------------------------------
        // 7) Tuples & Deconstruction
        // -----------------------------------
        private void DemoTuplesAndDeconstruction()
        {
            Console.WriteLine("\n-- Tuples & Deconstruction --");

            // Именованный кортеж
            (int X, int Y) namedPoint = (10, 20);
            Console.WriteLine($"Named tuple: ({namedPoint.X}, {namedPoint.Y})");

            // Неименованный кортеж
            var unnamedTuple = ("Alice", 30);
            Console.WriteLine($"Unnamed tuple: (\"{unnamedTuple.Item1}\", {unnamedTuple.Item2})");

            // Деконструкция (распаковка)
            var (name, age) = unnamedTuple;
            Console.WriteLine($"Deconstructed: name = {name}, age = {age}");

            // Кортеж можно вернуть из метода, передать как аргумент и т.д.
            var result = GetMinMax((5, 100));
            Console.WriteLine($"GetMinMax из (5,100): min = {result.min}, max = {result.max}");
        }

        private static (int min, int max) GetMinMax((int a, int b) input)
        {
            return input.a < input.b
                ? (input.a, input.b)
                : (input.b, input.a);
        }


        // -----------------------------------
        // 8) ref, out и in параметры
        // -----------------------------------
        private void DemoRefOutInParameters()
        {
            Console.WriteLine("\n-- ref, out & in parameters --");

            int refValue = 5;
            Console.WriteLine($"Before RefMethod(refValue): {refValue}");
            RefMethod(ref refValue);
            Console.WriteLine($"After  RefMethod(refValue): {refValue}");

            Console.WriteLine("\nCalling OutMethod(out outValue):");
            OutMethod(out int outValue);
            Console.WriteLine($"OutMethod returned: {outValue}");

            Console.WriteLine("\nCalling InMethod(in inValue):");
            int inValue = 100;
            InMethod(in inValue);
        }

        // ref: передаёт переменную по ссылке, в методе можно изменить значение вызывающей переменной
        private void RefMethod(ref int number)
        {
            number += 10;
            Console.WriteLine($"  [Inside RefMethod] number = {number}");
        }

        // out: передаётся по ссылке, но до вызова не инициализировано, метод должен присвоить значение
        private void OutMethod(out int result)
        {
            result = 99;
            Console.WriteLine($"  [Inside OutMethod] result = {result}");
        }

        // in: передача по ссылке для «readonly» доступа, нельзя переназначить в методе
        private void InMethod(in int number)
        {
            Console.WriteLine($"  [Inside InMethod] number (read-only) = {number}");
            // number = 5; // Ошибка: нельзя изменить in-параметр
        }


        // -----------------------------------
        // 9) object, boxing & unboxing
        // -----------------------------------
        private void DemoObjectBoxingUnboxing()
        {
            Console.WriteLine("\n-- object type & Boxing/Unboxing --");

            int primitiveInt = 1234;
            object boxedObj = primitiveInt; // boxing: значение помещается в object (heap)
            Console.WriteLine($"boxedObj (boxed int): {boxedObj} (Type: {boxedObj.GetType().Name})");

            int unboxedInt = (int)boxedObj; // unboxing: приводим object обратно к int
            Console.WriteLine($"unboxedInt: {unboxedInt} (Type: {unboxedInt.GetType().Name})");
        }


        // -----------------------------------
        // 10) Discard-паттерн (_)
        // -----------------------------------
        private void DemoDiscardPattern()
        {
            Console.WriteLine("\n-- Discard Pattern '_' --");

            // Если нам нужны только некоторые элементы кортежа, остальные пропускаем:
            var triple = (1, 2, 3);
            var (_, second, _) = triple;
            Console.WriteLine($"Из кортежа (1,2,3) извлёк только second = {second}");

            // Можно использовать discard для игнорирования out-параметра:
            if (int.TryParse("999", out _))
            {
                Console.WriteLine("Parse succeeded, но нам не нужно значение.");
            }
        }


        // -----------------------------------
        // 11) Span<T> & stackalloc (unsafe-подобные примитивы на стеке)
        // -----------------------------------
        private void DemoSpanAndStackalloc()
        {
            Console.WriteLine("\n-- Span<T> & stackalloc --");

            // stackalloc выделяет память на стеке под массив из 3 int'ов
            Span<int> span = stackalloc int[3] { 10, 20, 30 };
            for (int idx = 0; idx < span.Length; idx++)
            {
                Console.WriteLine($"span[{idx}] = {span[idx]}");
            }

            // Span<T> позволяет работать с памятью без аллока в heap и без GC-накладных расходов
        }
    }

}
