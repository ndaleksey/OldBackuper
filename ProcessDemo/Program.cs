using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessDemo
{
    public class Program
    {
        static object _locker = new object();
        static AutoResetEvent _waitHandler = new AutoResetEvent(true);
        static Mutex _mutexObj = new Mutex();

        public delegate int BinaryOp(int data, int time);

        public delegate void Test();

        private static void TaskMethod()
        {
            for (var i = 0; i < 10; ++i)
            {
                Console.WriteLine("В процессе №{0} значение счетчика: {1}", Task.CurrentId, i);
                Thread.Sleep(500);
            }

            Console.WriteLine("Завершение работы процесса №{0}", Task.CurrentId);
        }

        private static int DelegateThread(int data, int time)
        {
            Console.WriteLine("DelegateThread запущен");
            Thread.Sleep(time);
            Console.WriteLine("DelegateThread завершен");

            return ++data;
        }

        public delegate int DisplayHandler();

        public static void Main()
        {
            var handler = new DisplayHandler(Display);

            var resultObj = handler.BeginInvoke(null, null);

            Console.WriteLine("Продолжается работа метода Main");

            var result = handler.EndInvoke(resultObj);

            Console.WriteLine($"Результат равен: {result}");

            Console.ReadKey();
        }

        private static int Display()
        {
            Console.WriteLine("Начинается работа метода Dislay.........");
            var result = 0;

            for (var i = 0; i < 10; i++)
            {
                result += i * i;
            }

            Thread.Sleep(3000);
            Console.WriteLine("Завершение метода Display");

            return result;
        }
    }
}
