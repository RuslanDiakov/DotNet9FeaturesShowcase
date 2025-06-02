using ConsoleApp2;

using System;
using System.Threading;

namespace Playground.Topics
{
    /// <summary>
    /// Демонстрация делегатов.
    /// Делегаты - это типы, которые представляют ссылки на методы с определенной сигнатурой.
    /// Создание делегата:
    /// delegate <Тип возвращаемого значения> <Имя делегата>(<Тип параметров>);
    /// Пример:
    /// delegate int MathOperation(int x, int y);
    /// Эта строка объявляет делегат, который принимает два целых числа и возвращает целое число.
    /// </summary>

    // Delegate type for basic math operations
    public delegate int MathOperation(int x, int y);

    // Delegate type for notifications (return void)
    public delegate void Notify(string message);

    // Base and derived classes for variance demo
    public class Animal
    {
        public virtual string Speak() => "Animal sound";
    }

    public class Dog : Animal
    {
        public override string Speak() => "Woof!";
    }

    public class DelegatesDemo : IDemo
    {
        // -----------------------------------
        // 1) Entry point
        // -----------------------------------
        public void Run()
        {
            Console.WriteLine("=== DelegatesDemo: Начало ===");

            DemoBasicDelegateDeclarationAndInvocation();
            DemoAnonymousMethodAndLambda();
            DemoMulticastDelegate();
            DemoGenericDelegates();
            DemoCovarianceContravariance();
            DemoDelegateAsParameterAndReturn();
            DemoClosures();
            DemoEventsWithDelegates();
            DemoDynamicInvoke();
            DemoAsyncDelegates();

            Console.WriteLine("\n=== DelegatesDemo: Конец ===");
        }

        // -----------------------------------
        // 2) Basic delegate declaration and invocation
        // -----------------------------------
        private void DemoBasicDelegateDeclarationAndInvocation()
        {
            Console.WriteLine("\n-- Basic Delegate Declaration & Invocation --");

            MathOperation add = Add;
            MathOperation multiply = Multiply;

            int a = 5, b = 3;
            Console.WriteLine($"Add({a}, {b}) = {add(a, b)}");
            Console.WriteLine($"Multiply({a}, {b}) = {multiply(a, b)}");
        }

        private int Add(int x, int y)
        {
            return x + y;
        }

        private int Multiply(int x, int y)
        {
            return x * y;
        }

        // -----------------------------------
        // 3) Anonymous methods and lambda expressions
        // -----------------------------------
        private void DemoAnonymousMethodAndLambda()
        {
            Console.WriteLine("\n-- Anonymous Method & Lambda Expression --");

            MathOperation anonymousOp = delegate (int x, int y)
            {
                return x - y;
            };
            Console.WriteLine($"Anonymous subtract: 10 - 4 = {anonymousOp(10, 4)}");

            MathOperation lambdaOp = (x, y) => x / y;
            Console.WriteLine($"Lambda divide: 20 / 5 = {lambdaOp(20, 5)}");
        }

        // -----------------------------------
        // 4) Multicast delegates
        // -----------------------------------
        private void DemoMulticastDelegate()
        {
            Console.WriteLine("\n-- Multicast Delegates --");

            Notify notify1 = Message1;
            Notify notify2 = Message2;

            // Combine into multicast delegate
            Notify multiNotify = notify1 + notify2;
            Console.WriteLine("Invoking multicast delegate:");
            multiNotify("Hello, delegates!");

            // Remove one subscriber
            multiNotify -= notify2;
            Console.WriteLine("After removing second subscriber:");
            multiNotify("Hello again!");
        }

        private void Message1(string msg)
        {
            Console.WriteLine($"  [Message1] {msg}");
        }

        private void Message2(string msg)
        {
            Console.WriteLine($"  [Message2] {msg}");
        }

        // -----------------------------------
        // 5) Generic delegates: Func, Action, Predicate
        // -----------------------------------
        private void DemoGenericDelegates()
        {
            Console.WriteLine("\n-- Generic Delegates (Func, Action, Predicate) --");

            Func<int, int, int> funcAdd = (x, y) => x + y;
            Console.WriteLine($"Func add: 7 + 8 = {funcAdd(7, 8)}");

            Action<string> actionPrint = s => Console.WriteLine($"Action print: {s}");
            actionPrint("Delegates in C#");

            Predicate<int> predicate = x => x % 2 == 0;
            Console.WriteLine($"Predicate: Is 10 even? {predicate(10)}");
            Console.WriteLine($"Predicate: Is 7 even? {predicate(7)}");

            Comparison<string> comparison = (s1, s2) => s1.Length - s2.Length;
            var words = new[] { "apple", "pear", "banana" };
            Array.Sort(words, comparison);
            Console.WriteLine("Comparison sort by length:");
            foreach (var w in words)
                Console.WriteLine($"  {w}");
        }

        // -----------------------------------
        // 6) Covariance and Contravariance
        // -----------------------------------
        private void DemoCovarianceContravariance()
        {
            Console.WriteLine("\n-- Covariance & Contravariance --");

            // Covariance: Method returns Derived (Dog) but assigned to delegate returning Base (Animal)
            Func<Dog> createDog = () => new Dog();
            Func<Animal> createAnimal = createDog; // covariance
            Animal animal = createAnimal();
            Console.WriteLine($"Covariance: animal.Speak() = {animal.Speak()}");

            // Contravariance: Method takes Base (Animal) but assigned to delegate taking Derived (Dog)
            Action<Animal> processAnimal = a => Console.WriteLine($"Processing animal: {a.Speak()}");
            Action<Dog> processDog = processAnimal; // contravariance
            Dog dog = new Dog();
            processDog(dog);
        }

        // -----------------------------------
        // 7) Delegate as parameter and return value
        // -----------------------------------
        private void DemoDelegateAsParameterAndReturn()
        {
            Console.WriteLine("\n-- Delegate as Parameter & Return Value --");

            int x = 6, y = 7;
            int result = ComputeOperation(x, y, (a, b) => a * b);
            Console.WriteLine($"ComputeOperation with lambda (multiply): {x} * {y} = {result}");

            var returnedDelegate = GetOperation("add");
            Console.WriteLine($"Returned delegate add: 15 + 5 = {returnedDelegate(15, 5)}");
        }

        private int ComputeOperation(int a, int b, MathOperation op)
        {
            return op(a, b);
        }

        private MathOperation GetOperation(string opName)
        {
            return opName switch
            {
                "add" => (x, y) => x + y,
                "subtract" => (x, y) => x - y,
                _ => (x, y) => 0
            };
        }

        // -----------------------------------
        // 8) Closures
        // -----------------------------------
        private void DemoClosures()
        {
            Console.WriteLine("\n-- Closures --");

            int counter = 0;
            Func<int> increment = () =>
            {
                counter++;
                return counter;
            };

            Console.WriteLine($"First call: {increment()}");
            Console.WriteLine($"Second call: {increment()}");

            // Capture in loop
            Action[] actions = new Action[3];
            for (int i = 0; i < 3; i++)
            {
                // Demonstrate closure capturing the loop variable
                int loopVar = i;
                actions[i] = () => Console.WriteLine($"  Captured loopVar = {loopVar}");
            }
            Console.WriteLine("Invoking captured actions:");
            foreach (var action in actions)
                action();
        }

        // -----------------------------------
        // 9) Events using delegates
        // -----------------------------------
        private void DemoEventsWithDelegates()
        {
            Console.WriteLine("\n-- Events with Delegates --");

            var publisher = new Publisher();
            var subscriber = new Subscriber(publisher);

            publisher.RaiseEvent("Event 1");
            publisher.RaiseEvent("Event 2");

            // Unsubscribe
            publisher.OnRaise -= subscriber.HandleEvent;
            Console.WriteLine("After unsubscribing:");
            publisher.RaiseEvent("Event 3");
        }

        // Publisher declaring an event
        private class Publisher
        {
            public event Notify OnRaise;

            public void RaiseEvent(string message)
            {
                Console.WriteLine($"Publisher: Raising event with message '{message}'");
                OnRaise?.Invoke(message);
            }
        }

        // Subscriber handling the event
        private class Subscriber
        {
            public Subscriber(Publisher publisher)
            {
                publisher.OnRaise += HandleEvent;
            }

            public void HandleEvent(string message)
            {
                Console.WriteLine($"  [Subscriber] Received: {message}");
            }
        }

        // -----------------------------------
        // 10) DynamicInvoke
        // -----------------------------------
        private void DemoDynamicInvoke()
        {
            Console.WriteLine("\n-- Delegate.DynamicInvoke --");

            MathOperation add = (x, y) => x + y;
            Delegate del = add; // boxing to System.Delegate
            object result = del.DynamicInvoke(10, 20);
            Console.WriteLine($"DynamicInvoke result (10 + 20) = {result}");
        }

        // -----------------------------------
        // 11) Async delegates (BeginInvoke/EndInvoke)
        // -----------------------------------
        private void DemoAsyncDelegates()
        {
            Console.WriteLine("\n-- Async Delegates (BeginInvoke/EndInvoke) --");

            Func<int, int, int> add = (x, y) =>
            {
                Thread.Sleep(500); // simulate work
                return x + y;
            };

            IAsyncResult asyncResult = add.BeginInvoke(8, 9, null, null);
            Console.WriteLine("Doing other work while delegate runs asynchronously...");
            int sum = add.EndInvoke(asyncResult);
            Console.WriteLine($"Async delegate result: 8 + 9 = {sum}");
        }
    }
}
