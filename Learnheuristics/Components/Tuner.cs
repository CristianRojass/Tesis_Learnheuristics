using Learnheuristics.Components.Black_Hole;
using Learnheuristics.Components.Q_Learning;
using Learnheuristics.Model.Configurations;
using Learnheuristics.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Learnheuristics.Components {

    //Componente de ajuste automático de parámetros.
    public class Tuner {

        //Segmenta el hiperespacio alrededor de un punto central de forma hipercúbica para luego procesar concurrentemente cada cluster mediante Black Hole,
        //con lo que se obtiene una configuración candidata por cada hipercubo. Finalmente se realiza un torneo asistido por Stateless Q-learning para determinar
        //cuál de las configuraciones candidatas es mejor considerando los resultados y desviaciones estandar obtenidas tras una serie de evaluaciones por ronda.
        public static async Task<ConfigurationType> Run<ConfigurationType>(ConfigurationType middle_configuration, float hypercube_length, int factor = 1, int depth = 1, int inner_depth = 1, int evaluation_duration = 200, int number_of_epochs = 10, int iterations_by_round = 100, int duration_step_between_rounds = 250, int number_of_performances_to_average = 2, int verification_duration = 1000) where ConfigurationType : IConfiguration {
            var candidate_solutions = await Run_Hypercubic_Black_Hole(middle_configuration, hypercube_length, factor, depth, inner_depth, evaluation_duration, number_of_epochs);
            Console.WriteLine($"\nSe han encontrado las siguientes {candidate_solutions.Count} soluciones candidatas luego de aplicar HyperCubic Black Hole:");
            for (int index = 0; index < candidate_solutions.Count; index++)
                Console.WriteLine($"{index + 1}. {candidate_solutions[index]}");
            Console.WriteLine("\nA continuación se determinará cuál de estas soluciones es mejor mediante un torneo asistido por Stateless Q Learning.\n");
            var best_configuration = Run_Stateless_Q_Learning(candidate_solutions.ToArray(), iterations_by_round, duration_step_between_rounds, number_of_performances_to_average);
            var Table = new DynamicTable(Console.CursorTop, "Comprobación de resultado", new string[] { "Configuración", "IGD" }, candidate_solutions.Select(solution => new string[] { solution.ToString(), "─" }).ToArray());
            Table.Display($"Generación final: {verification_duration}");
            for (int index = 0; index < candidate_solutions.Count; index++)
                Table.Update(decimal.Parse(candidate_solutions[index].Evaluate(verification_duration).ToString(), NumberStyles.Any, CultureInfo.InvariantCulture).ToString(), index, 1);
            return best_configuration;
        }

        //Genera una red hipercúbica alrededor de la configuración vectorizada central, para luego aplicar el Black Hole concurrentemente en cada hipercubo.
        public static async Task<List<ConfigurationType>> Run_Hypercubic_Black_Hole<ConfigurationType>(ConfigurationType middle_configuration, float length, int factor, int depth, int inner_depth, int evaluation_duration, int number_of_epochs) where ConfigurationType : IConfiguration {
            Stopwatch timer = Stopwatch.StartNew();
            var candidate_solutions = await Hypercubic_Black_Hole.Run(middle_configuration, length, factor, depth, inner_depth, evaluation_duration, number_of_epochs);
            timer.Stop();
            Console.WriteLine($"\nTiempo transcurrido durante HyperCubic Black Hole: {timer.Elapsed:hh\\:mm\\:ss}");
            return candidate_solutions;
        }

        //Selecciona la mejor configuración de un conjunto de ellas:
        //El algoritmo realiza un torneo entre las N configuraciones candidatas, con log_2(N) rondas
        //en las que se evalúan y seleccionan la mitad de las configuraciones aún en competencia según su Q-value,
        //reduciendo el conjunto a la mitad de forma recursiva en cada ronda hasta obtener una única configuración.
        public static ConfigurationType Run_Stateless_Q_Learning<ConfigurationType>(ConfigurationType[] candidate_solutions, int iterations_by_round, int duration_step_between_rounds, int number_of_performances_to_average) where ConfigurationType : IConfiguration {
            Stopwatch timer = Stopwatch.StartNew();
            var best_solution = Stateless_Q_Learning.Run(candidate_solutions, iterations_by_round, duration_step_between_rounds, number_of_performances_to_average);
            timer.Stop();
            Console.WriteLine($"\nTiempo transcurrido durante Stateless Q-learning: {timer.Elapsed:hh\\:mm\\:ss}\n");
            return best_solution;
        }

    }

}