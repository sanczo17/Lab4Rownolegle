﻿using System.Diagnostics;

class Program
{
    private static double IntegralSum = 0;
    private static object LockObject = new object();

    static void Main()
    {
        Console.WriteLine("Podaj przedział całkowania (a):");
        double a = double.Parse(Console.ReadLine());
        Console.WriteLine("Podaj przedział całkowania (b):");
        double b = double.Parse(Console.ReadLine());

        Console.WriteLine("Podaj liczbę trapezów:");
        int n = int.Parse(Console.ReadLine());

        Console.WriteLine("Podaj nazwę pliku do zapisu wykresu (np. wykres.png):");
        string fileName = Console.ReadLine();

        double h = (b - a) / n;
        int numberOfThreads = Environment.ProcessorCount;

        Stopwatch stopwatch = Stopwatch.StartNew();
        Parallel.For(0, numberOfThreads, i =>
        {
            double localSum = 0;
            double localStart = a + i * (b - a) / numberOfThreads;
            double localEnd = a + (i + 1) * (b - a) / numberOfThreads;

            for (double x = localStart; x < localEnd; x += h)
            {
                localSum += (Function(x) + Function(x + h)) * h / 2;
            }

            lock (LockObject)
            {
                IntegralSum += localSum;
            }
        });

        stopwatch.Stop();

        Console.WriteLine($"Wynik całkowania: {IntegralSum}");
        Console.WriteLine($"Czas obliczeń: {stopwatch.ElapsedMilliseconds} ms");

        PlotFunction(a, b, fileName);

        Console.WriteLine("Naciśnij dowolny klawisz, aby zakończyć...");
        Console.ReadKey();
    }

    static double Function(double x)
    {
        return Math.Pow(2, x);
    }

    static void PlotFunction(double a, double b, string fileName)
    {
        var plt = new ScottPlot.Plot();
        int points = 1000;
        double[] xs = new double[points];
        double[] ys = new double[points];

        double step = (b - a) / (points - 1);
        for (int i = 0; i < points; i++)
        {
            xs[i] = a + i * step;
            ys[i] = Function(xs[i]);
        }

        plt.Add.Scatter(xs, ys);
        plt.Title("Wykres funkcji");
        plt.XLabel("x");
        plt.YLabel("f(x)");

        bool success = false;
        while (!success)
        {
            try
            {
                plt.SavePng(fileName, 800, 600);
                Console.WriteLine($"Wykres zapisano do pliku {fileName}");
                success = true;
            }
            catch (IOException)
            {
                Console.WriteLine("Błąd zapisu pliku. Podaj inną nazwę pliku:");
                fileName = Console.ReadLine();
            }
        }
        
        try
        {
            Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Nie udało się otworzyć pliku: {ex.Message}");
        }
    }
}
