using Learnheuristics.Components;
using Learnheuristics.Components.Black_Hole.Model;
using Learnheuristics.Model;
using Learnheuristics.Model.Configurations;
using Learnheuristics.Model.Configurations.Instances.NSGA_II;
using Learnheuristics.Model.Configurations.Instances.NSGA_III;
using Learnheuristics.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consola {

    public class Program {

        public static async Task Main() {

            //U de Mann Whitney utilizando configuración de ejemplo y configuración ajustada.
            var best_configuration = await U_Mann_Whitney.TestConfigurations(new NSGA_III_Configuration(new Vector(new float[] { 92, 100 }), "DTLZ1"), new NSGA_III_Configuration(new Vector(new float[] { 96, 100 }), "DTLZ1"), 50, 450);
            Console.WriteLine(best_configuration != null ? $"La configuración {best_configuration} demostró ser superior estadísticamente." : "No existe suficiente evidencia estadística para afirmar la superioridad de una configuración por sobre la otra.");
            return;

            Console.WriteLine($"Hora de inicio: {DateTime.Now:hh:mm:ss}\n");

            #region Black Hole
            //Punto sobre el cual se centrará la red de hipercubos
            var middle_hyperpoint = new Vector(new float[] { 50, 50 });
            //Longitud de cada dimension para cada hipercubo
            float hypercube_length = 33.3f;
            //Factor por el cual se puede amplificar la longitud de cada dimension de hipercubo
            var factor = 1;
            //Profundidad: Capas de envoltura alrededor del punto central
            var depth = 2;
            //Profundidad interior: Capas de puntos contenidos en el hipercubo
            var inner_depth = 1;
            //Cantidad de generaciones (iteraciones) que realiza la metaheurística de bajo nivel (NSGA-III) en cada evaluación
            var evaluation_duration = 200;
            //Cantidad de épocas (iteraciones) que realiza el algoritmo Black Hole
            var number_of_epochs = 10;
            #endregion

            #region Q-Learning
            //Cantidad de iteraciones por ronda. Cada iteración consiste en seleccionar una acción/configuración (ε-Greedy), evaluarla y recompensarla/castigarla según su resultado.
            var iterations_by_round = 100;
            //Incremento por ronda de la cantidad de generaciones (iteraciones) con las que se evalúa cada acción.
            var duration_step_between_rounds = 250;
            //Cantidad de evaluaciones por semilla aleatoria en cada iteración, las que se promedian y evalúan según su desviación estandar
            var number_of_performances_to_average = 2;
            //Cantidad de generaciones (iteraciones) utilizadas para verificar los resultados obtenidos por el Q-Learning
            var verification_duration = 1000;
            #endregion

            var ProblemName = "DTLZ1";

            var best_configuration_NSGA_II = await Tuner.Run(new NSGA_II_Configuration(middle_hyperpoint, ProblemName), hypercube_length, factor, depth, inner_depth, evaluation_duration, number_of_epochs, iterations_by_round, duration_step_between_rounds, number_of_performances_to_average, verification_duration);
            Console.WriteLine($"\nLa mejor configuración encontrada para el problema {ProblemName} utilizando NSGA-II es: {best_configuration_NSGA_II}\n");

            var best_configuration_NSGA_III = await Tuner.Run(new NSGA_III_Configuration(middle_hyperpoint, ProblemName), hypercube_length, factor, depth, inner_depth, evaluation_duration, number_of_epochs, iterations_by_round, duration_step_between_rounds, number_of_performances_to_average, verification_duration);
            Console.WriteLine($"\nLa mejor configuración encontrada para el problema {ProblemName} utilizando NSGA-III es: {best_configuration_NSGA_III}\n");

            Console.WriteLine($"Hora de termino: {DateTime.Now:hh:mm:ss}");

            return;

        }

        public class U_Mann_Whitney {

            private static DynamicTable Table;

            public static async Task<IConfiguration> TestConfigurations(IConfiguration first_configuration, IConfiguration second_configuration, int number_of_measurements = 30, int evaluation_duration = 400, int seed_limit = 100) {
                if (number_of_measurements > seed_limit)
                    throw new Exception("Error: El número de mediciones no puede superar a la semilla límite.");
                
                var random = new Random();
                var seeds = Enumerable.Range(1, seed_limit).OrderBy(num => random.Next()).Take(number_of_measurements).ToList();
                var u_values = new List<U_Value>();
                Table = new DynamicTable(Console.CursorTop, "U de Mann Whitney", new string[] { "Medición", first_configuration.ToString(), second_configuration.ToString() }, seeds.Select((seed, index) => new string[] { (index + 1).ToString(), "-", "-" }).ToArray());
                Table.Display("Nivel de significancia α: 5%");

                var get_measurements_tasks = new List<Task<List<U_Value>>> {
                    Task.Run(() => GetMeasurements(first_configuration, 1, evaluation_duration, seeds)),
                    Task.Run(() => GetMeasurements(second_configuration, 2, evaluation_duration, seeds))
                };
                await Task.WhenAll(get_measurements_tasks);

                get_measurements_tasks.Select(task => task.Result).ToList().ForEach(measurements => u_values.AddRange(measurements));

                u_values = u_values.OrderBy(u_value => u_value.value).ToList();
                for (int position = 0; position < u_values.Count;)
                    u_values[position].range = ++position;

                var n1 = number_of_measurements;
                var n2 = number_of_measurements;
                var R1 = TotalRange(u_values, first_configuration);
                var R2 = TotalRange(u_values, second_configuration);
                var U1 = Get_U_Statistic(n1, n2, R1);
                var U2 = Get_U_Statistic(n2, n1, R2);
                var U = Math.Min(U1, U2);
                var Z = Math.Round(Math.Abs((U - (float)(n1 * n2) / 2) / Math.Sqrt((float)(n1 * n2 * (n1 + n2 + 1)) / 12)), 2);
                Table.Display($"Estadístico Z: {Z} {( Z > 1.96 ? ">" : "<=" )} 1.96 | α: 5%\n");
                if (Z > 1.96)
                    return R1 <= R2 ? first_configuration : second_configuration;
                return default;
            }

            private static List<U_Value> GetMeasurements(IConfiguration configuration, int table_column, int evaluation_duration, List<int> seeds) {
                var measurements = new List<U_Value>();
                var index = 0;
                seeds.ForEach(seed => {
                    var measurement = configuration.Evaluate(evaluation_duration, seed);
                    measurements.Add(new U_Value(configuration, measurement));
                    Table.Update(measurement.ToString(), index++, table_column);
                });
                return measurements;
            }

            private static int TotalRange(List<U_Value> u_values, IConfiguration configuration) {
                var total_range = u_values.Where(u_value => u_value.configuration.Equals(configuration)).Select(u_value => u_value.range).Sum();
                return total_range;
            }

            private static float Get_U_Statistic(int n_1, int n_2, int total_range) {
                var U = n_1 * n_2 + (float)(n_1 * (n_1 + 1)) / 2 - total_range;
                return U;
            }

                    
        }

        public class U_Value {

            public IConfiguration configuration;        //Referencia a la configuración
            public float value;                         //IGD
            public int range;                           //Posición de la medición en conjunto ordenado de menor a mayor

            public U_Value(IConfiguration configuration, float value) {
                this.configuration = configuration;
                this.value = value;
            }

        }

    }

}