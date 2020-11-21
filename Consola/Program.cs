using Black_Hole;
using Learnheuristics.ConfigurationTuner.Configurations;
using Learnheuristics.ConfigurationTuner.Configurations.Instances.NSGA;
using Q_Learning;
using Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Consola {
    public class Program {

        public static async Task Main() {

            Console.WriteLine($"Hora de inicio: {DateTime.Now:hh:mm:ss}\n");

            //Punto sobre el cual se centrará la red de hipercubos
            var middle_hyper_point = new Vector(new float[] { 40, 100 });
            //Punto sobre el cual se centrará la red de hipercubos
            var amount_of_threads = 4;
            //Longitud de cada dimension de cada hipercubo
            float length = 30;
            //Factor por el cual se puede amplificar la longitud de cada dimension de hipercubo
            var factor = 1;
            //Profundidad: Capas de envoltura alrededor del punto central
            var depth = 2;
            //Profundidad interior: Capas de puntos contenidos en el hipercubo
            var inner_depth = 2;

            //Inicia un cronometro para medir el tiempo que tarda en ejecutarse el script
            Stopwatch timer = Stopwatch.StartNew();

            var black_hole = new Hypercubic_Black_Hole<NSGA_III_Configuration>();
            var candidate_solutions = await black_hole.Run(middle_hyper_point, amount_of_threads, length, factor, depth, inner_depth);

            Console.WriteLine($"\nTiempo transcurrido durante HyperCubic Black Hole: {timer.Elapsed:hh\\:mm\\:ss}");

            //var candidate_solutions = new List<NSGA_III_Configuration>() {
            //new NSGA_III_Configuration(new Vector(new float[] { 10, 69 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 6, 59 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 5, 68 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 14, 23 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 13, 27 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 10, 22 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 20, 29 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 15, 15 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 16, 30 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 12, 25 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 17, 24 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 13, 18 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 20, 20 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 21, 17 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 12, 30 })),
            //new NSGA_III_Configuration(new Vector(new float[] { 15, 30 })),
            //};

            Console.WriteLine($"\nSe han encontrado las siguientes {candidate_solutions.Count} soluciones candidatas luego de aplicar HyperCubic Black Hole:");
            candidate_solutions.ForEach(candidate_solution => Console.WriteLine($"\t{candidate_solution}"));
            Console.WriteLine("\nA continuación se determinará cuál de estas soluciones es mejor mediante un torneo asistido por Stateless Q Learning.\n");

            var iterations_by_round = 100;
            var duration_step_between_rounds = 200;
            var amount_of_performance_to_average = 3;

            timer.Restart();
            var best_configuration = Stateless_Q_Learning.Run(candidate_solutions.ToArray(), iterations_by_round, duration_step_between_rounds, amount_of_performance_to_average);
            //Detiene el cronometro que mide el tiempo que tarda en ejecutarse el script
            timer.Stop();
            Console.WriteLine($"\nTiempo transcurrido durante Stateless Q-learning: {timer.Elapsed:hh\\:mm\\:ss}\n");

            //Console.Console.WriteLine($"\nLa mejor configuración encontrada es: {best_configuration}");
            Console.WriteLine($"Hora de termino: {DateTime.Now:hh:mm:ss}\n");

            var number_of_iterations = 10;
            var Q_Table = new Dynamic_Q_Table(System.Console.CursorTop, candidate_solutions.Select(solution => new { configuration = solution as IConfiguration, performance = solution.Evaluate(number_of_iterations) }).ToDictionary(key_value => key_value.configuration, key_value => key_value.performance), "Comprobación de resultado", new string[] { "Configuración", "IGD" });
            Q_Table.Display($"Se ha comprobado con {number_of_iterations} iteraciones.");

            return;

        }

    }
}
