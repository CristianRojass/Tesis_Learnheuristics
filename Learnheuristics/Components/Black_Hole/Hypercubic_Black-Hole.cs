using Learnheuristics.Components.Black_Hole.Model;
using Learnheuristics.Model;
using Learnheuristics.Model.Configurations;
using Learnheuristics.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Learnheuristics.Components.Black_Hole {

    //Componente que realiza la segmentación del hiperespacio de forma hipercúbica alrededor de un punto central para luego procesar concurrentemente cada cluster mediante Black Hole.
    public class Hypercubic_Black_Hole {

        //Tabla dinámica utilizada para mostrar el progreso (en épocas) de cada black hole.
        private static DynamicTable Table;

        //Genera una red hipercúbica alrededor de la configuración vectorizada central, para luego aplicar el black hole concurrentemente en cada hipercubo.
        public static async Task<List<ConfigurationType>> Run<ConfigurationType>(ConfigurationType middle_configuration, float length, int factor, int depth, int inner_depth, int evaluation_duration, int number_of_epochs) where ConfigurationType : IConfiguration {
            if (middle_configuration == null)
                throw new ArgumentNullException("<middle_configuration> no puede ser nulo.");

            var path_to_folder_container = "C:\\Users\\Trifenix\\Desktop\\HyperSpace";
            if (Directory.Exists(path_to_folder_container))
                Directory.Delete(path_to_folder_container, true);

            var HyperSpace = HyperCube.CreateHyperCubeNetwork(middle_configuration.Vectorized_Configuration, length, factor, depth, inner_depth);
            Console.WriteLine($"A continuación se procesarán los {HyperSpace.Count} hipercubos concurrentemente.\n");
            Table = new DynamicTable(Console.CursorTop, "Progreso", new string[] { "HiperCubo", "Época" }, HyperSpace.Select((hypercube, index) => new string[] { (index + 1).ToString(), "0" }).ToArray(), $"Época final: {number_of_epochs}");
            Table.Display();

            var black_holes_hypercubes_task = new List<Task<ConfigurationType>>();
            for (int i = 0; i < HyperSpace.Count; i++) {
                var index = i;
                var hypercube = HyperSpace[index];
                var task = Task.Run(() => ProcessHyperCube<ConfigurationType>(hypercube, $"HyperCube_{++index}", evaluation_duration, number_of_epochs));
                black_holes_hypercubes_task.Add(task);
            }
            // Esperando por las tareas (Un black hole por hipercubo).
            await Task.WhenAll(black_holes_hypercubes_task);

            var candidate_solutions = new List<ConfigurationType>();
            black_holes_hypercubes_task.Select(task => task.Result).Where(result => result != null).ToList().ForEach(candidate_solution => candidate_solutions.Add(candidate_solution));
            return candidate_solutions;
        }

        //Realiza el ciclo principal del black hole aplicado a un hipercubo (cluster).
        private static ConfigurationType ProcessHyperCube<ConfigurationType>(HyperCube hypercube, string hypercube_name, int evaluation_duration, int number_of_epochs, bool open_images = false) where ConfigurationType : IConfiguration {

            #region Funciones estáticas locales
            /*x(t + 1) = x(t) + rand([0,1]) * (x(BH) - x(t))
              Siguiente posición = posición actual de la estrella + rand([0,1]) * (posición actual del agujero negro - posicion actual de la estrella)*/
            //Calcula la nueva posición de la estrella luego de haber sido atraida por el agujero negro.
            static Vector CalculateNewPosition(Vector star, Vector black_hole) {
                var newPosition = Vector.Add(star, Vector.Scale(Vector.Subtract(black_hole, star), (float)new Random().NextDouble()));
                return newPosition;
            }

            /* El algoritmo BH considera que un mayor valor es un mejor performance, lo que incide en el radio del agujero negro,
               sin embargo, para esta evaluación en particular estoy utilizando la IGD, que mejora a medida que su valor decrece.
                            R (radio de agujero negro) = fitness agujero negro / Σ fitness estrellas
               Debido a lo anterior, el radio calculado tiende a cero, retrasando la absorción de estrellas cercanas al agujero negro
               y la posterior generación de nuevas estrellas.*/
            //Calcula el radio del horizonte de eventos.
            static float CalculateEventHorizon(List<ConfigurationType> stars, ConfigurationType black_hole) {
                var sum = stars.Where(star => !star.Equals(black_hole)).Select(star => star.Performance).Sum();
                var radius = sum > 0 ? black_hole.Performance / sum : 0;
                return radius;
            }
            #endregion

            var stars = hypercube.inner_hyper_points;
            var configurations = stars.Select(star => (ConfigurationType)Activator.CreateInstance(typeof(ConfigurationType), star)).ToList();
            List<ConfigurationType> feasible_configurations = configurations.Where(configuration => configuration.IsFeasible()).ToList();
            var table_row = Convert.ToInt32(hypercube_name.Split("_")[1]) - 1;
            bool IsFeasible = hypercube.corner_hyper_points.Select(corner_hyper_point => (ConfigurationType)Activator.CreateInstance(typeof(ConfigurationType), corner_hyper_point)).Where(configuration => configuration.IsFeasible()).Any() && feasible_configurations.Any();
            if (!IsFeasible) {
                Table.Update("Inviable", table_row, 1);
                return default;
            }

            feasible_configurations.ForEach(configuration => configuration.TransformByDomain());
            var arguments = new Dictionary<string, object>() {
                { "hypercube_name", hypercube_name },
                { "epoch", 0},
                { "open_image", open_images }
            };
            Vector.PlotVectors(feasible_configurations.Select(configuration => configuration.Vectorized_Configuration).ToList(), arguments);

            ConfigurationType black_hole = default;
            float eventHorizonRadius;
            (float?, float?)[] min_max_constraints = hypercube.corner_hyper_points.First().coordinates.Zip(hypercube.corner_hyper_points.Last().coordinates, (min_value, max_value) => ((float?)min_value, (float?)max_value)).ToArray();

            #region Ciclo principal del Black Hole
            for (int epoch = 1; epoch <= number_of_epochs; epoch++) {
                feasible_configurations.ForEach(configuration => configuration.Evaluate(evaluation_duration));
                black_hole = feasible_configurations.Aggregate((first_configuration, second_configuration) => first_configuration.Performance < second_configuration.Performance ? first_configuration : second_configuration);
                configurations.ForEach(configuration => configuration.Transform(CalculateNewPosition(configuration.Vectorized_Configuration, black_hole.Vectorized_Configuration)));
                eventHorizonRadius = Math.Max(CalculateEventHorizon(feasible_configurations, black_hole), 1);
                configurations.Where(configuration => !configuration.Equals(black_hole) && Vector.Module(Vector.Subtract(black_hole.Vectorized_Configuration, configuration.Vectorized_Configuration)) < eventHorizonRadius).ToList().ForEach(configuration => configuration.RegenerateSubjectConstraints(min_max_constraints));
                feasible_configurations = configurations.Where(configuration => configuration.IsFeasible()).ToList();
                feasible_configurations.ForEach(configuration => configuration.TransformByDomain());
                configurations.Where(configuration => configuration.CountdownToRepair == 0).ToList().ForEach(configuration => configuration.Repair(min_max_constraints));
                arguments["black_hole"] = black_hole.Vectorized_Configuration.ToString();
                arguments["epoch"] = epoch;
                Vector.PlotVectors(feasible_configurations.Select(configuration => configuration.Vectorized_Configuration).ToList(), arguments);
                Table.Update(epoch.ToString(), table_row, 1);
            }
            #endregion

            return black_hole;
        }

    }

}