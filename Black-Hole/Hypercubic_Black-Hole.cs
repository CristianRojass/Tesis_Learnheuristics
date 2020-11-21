using Black_Hole.HyperSpace;
using Learnheuristics.ConfigurationTuner.Configurations;
using Services;
using Services.DynamicTable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Black_Hole {

    public class Hypercubic_Black_Hole<ConfigurationType> where ConfigurationType : IConfiguration {

        public List<HyperCube> HyperSpace;
        public DynamicTable Table;

        public async Task<List<ConfigurationType>> Run(Vector middle_hyper_point, int amount_of_threads = 2, float length = 50, int factor = 1, int depth = 2, int inner_depth = 2) {

            if (middle_hyper_point == null)
                throw new ArgumentNullException("<middle_hyper_point> no puede ser nulo.");
            HyperSpace = BigBang(middle_hyper_point, length, factor, depth, inner_depth);

            List<ConfigurationType> candidate_solutions = new List<ConfigurationType>();

            var path_to_folder_container = "C:\\Users\\Trifenix\\Desktop\\HyperSpace";
            if (Directory.Exists(path_to_folder_container))
                Directory.Delete(path_to_folder_container, true);

            var black_holes_hypercubes_task = new List<Task<ConfigurationType>>();

            Console.WriteLine($"A continuación se procesarán los {HyperSpace.Count} hipercubos concurrentemente.");

            for (int i = 0; i < HyperSpace.Count; i++) {
                var index = i;
                var hypercube = HyperSpace[index];
                var task = Task.Run(() => ProcessHyperCube($"HyperCube_{index}", hypercube));
                black_holes_hypercubes_task.Add(task);
                // Comprueba si hemos alcanzado el número especificado de subprocesos/tareas.
                if (black_holes_hypercubes_task.Count >= amount_of_threads) {
                    Task<ConfigurationType> firstTaskFinished = await Task.WhenAny(black_holes_hypercubes_task);
                    var candidate_solution = firstTaskFinished.Result;
                    if (candidate_solution != null) {
                        Console.WriteLine($"{candidate_solution.Vectorized_Configuration}: {candidate_solution.Performance}");
                        candidate_solutions.Add(candidate_solution);
                    }
                    black_holes_hypercubes_task.Remove(firstTaskFinished);
                }
            }
            // Esperando por las tareas restantes.
            await Task.WhenAll(black_holes_hypercubes_task);
            black_holes_hypercubes_task.Select(task => task.Result).Where(result => result != null).ToList().ForEach(candidate_solution => {
                Console.WriteLine($"{candidate_solution.Vectorized_Configuration}: {candidate_solution.Performance}");
                candidate_solutions.Add(candidate_solution);
            });
            return candidate_solutions;
        }

        private List<HyperCube> BigBang(Vector middle_hyper_point, float length, int factor, int depth, int inner_depth) => HyperCube.CreateHyperCubeNetwork(middle_hyper_point, length, factor, depth, inner_depth);

        private ConfigurationType ProcessHyperCube(string hypercube_name, HyperCube hypercube, int number_of_epochs = 10, bool open_images = false) {
            bool IsFeasible = hypercube.corner_hyper_points.Union(hypercube.inner_hyper_points).Select(hyper_point => (ConfigurationType)Activator.CreateInstance(typeof(ConfigurationType), hyper_point)).Where(configuration => configuration.IsFeasible()).Any();
            if (!IsFeasible)
                return default;
            (float?, float?)[] min_max_constraints = hypercube.corner_hyper_points.First().coordinates.Zip(hypercube.corner_hyper_points.Last().coordinates, (min_value, max_value) => ((float?)min_value, (float?)max_value)).ToArray();
            var stars = hypercube.inner_hyper_points;
            var configurations = stars.Select(star => (ConfigurationType)Activator.CreateInstance(typeof(ConfigurationType), star)).ToList();
            List<ConfigurationType> feasible_configurations;
            ConfigurationType black_hole = default;
            float eventHorizonRadius;
            var arguments = new Dictionary<string, object>() {
                { "hypercube_name", hypercube_name },
                { "open_image", open_images }
            };
            for (int epoch = 0; epoch < number_of_epochs; epoch++) {
                feasible_configurations = configurations.Where(configuration => configuration.IsFeasible()).ToList();
                arguments["epoch"] = epoch;
                Vector.PlotVectors(feasible_configurations.Select(configuration => configuration.Vectorized_Configuration).ToList(), arguments);
                configurations.Where(configuration => configuration.CountdownToRepair == 0).ToList().ForEach(configuration => configuration.Repair(min_max_constraints));
                feasible_configurations.ForEach(configuration => configuration.Evaluate(200));
                black_hole = feasible_configurations.Aggregate((first_configuration, second_configuration) => first_configuration.Performance < second_configuration.Performance ? first_configuration : second_configuration);
                arguments["black_hole"] = black_hole.Vectorized_Configuration.ToString();
                configurations.ForEach(configuration => configuration.Transform(CalculateNewPosition(configuration.Vectorized_Configuration, black_hole.Vectorized_Configuration)));
                eventHorizonRadius = CalculateEventHorizon(feasible_configurations, black_hole);
                configurations.Where(configuration => !configuration.Equals(black_hole) && Vector.Module(Vector.Subtract(black_hole.Vectorized_Configuration, configuration.Vectorized_Configuration)) < 1).ToList().ForEach(configuration => configuration.RegenerateSubjectConstraints(min_max_constraints));
                feasible_configurations.ForEach(configuration => configuration.TransformByDomain());
            }
            arguments["epoch"] = number_of_epochs;
            Vector.PlotVectors(configurations.Where(configuration => configuration.IsFeasible()).Select(configuration => configuration.Vectorized_Configuration).ToList(), arguments);
            return black_hole;
        }

        //x(t + 1) = x(t) + rand([0,1]) * (x(BH) - x(t))
        //Siguiente posición = posición actual de la estrella + rand([0,1]) * (posición actual del agujero negro - posicion actual de la estrella)
        private Vector CalculateNewPosition(Vector star, Vector black_hole) {
            var newPosition = Vector.Add(star, Vector.Scale(Vector.Subtract(black_hole, star), (float)new Random().NextDouble()));
            return newPosition;
        }

        //TODO: Conversar acerca de la fórmula
        // El algoritmo BH considera que un mayor valor es un mejor performance, lo que incide en el radio del agujero negro,
        // sin embargo, para esta evaluación en particular estoy utilizando la IGD, que mejora a medida que su valor decrece.
        // R (radio de agujero negro) = fitness agujero negro / Σ fitness estrellas
        private float CalculateEventHorizon(List<ConfigurationType> stars, ConfigurationType black_hole) {
            var sum = stars.Where(star => !star.Equals(black_hole)).Select(star => star.Performance).Sum();
            var radius = sum > 0 ? black_hole.Performance / sum : 0;
            return radius;
        }

    }

}