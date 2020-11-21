using Learnheuristics.ConfigurationTuner.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Q_Learning {
    public class Stateless_Q_Learning {

        public static IConfiguration[] Actions { get; set; }

        public static float[] Q_values { get; set; }

        //Pone en ejecución el algoritmo que selecciona la mejor configuración de un conjunto de ellas.
        //El algoritmo realiza un torneo entre las N configuraciones candidatas, con log_2(N) rondas
        //en las que se evalúan y seleccionan la mitad de las configuraciones aún en competencia,
        //reduciendo el conjunto a la mitad de forma recursiva hasta obtener una única configuración.
        public static IConfiguration Run(IConfiguration[] configurations, int iterations_by_round = 50, int duration_step_between_rounds = 250, int amount_of_performance_to_average = 5, int seed_limit = 100) {

            configurations = configurations.Distinct().ToArray();

            //Validación de parámetros.
            if (configurations == null || !configurations.Any())
                throw new ArgumentNullException("El parámetro <configurations> no puede ser nulo o estar vacío.");
            if (iterations_by_round < 1)
                throw new ArgumentOutOfRangeException("El parámetro <iterations_by_round> no puede ser menor a 1.");
            else if (iterations_by_round < configurations.Length)
                throw new ArgumentException("El parámetro <iterations_by_round> no puede ser menor que la cantidad de configuraciones disponibles.");
            if (duration_step_between_rounds < 1)
                throw new ArgumentOutOfRangeException("El parámetro <duration_step_between_rounds> no puede ser menor a 1.");
            if (amount_of_performance_to_average < 1)
                throw new ArgumentOutOfRangeException("El parámetro <amount_of_performance_to_average> no puede ser menor a 1.");
            if (seed_limit < amount_of_performance_to_average)
                throw new ArgumentException("El parámetro <seed_limit> no puede ser menor que la cantidad de evaluaciones a promediar.");

            //Declaración e inicialización de variables.
            Actions = configurations;
            Q_values = new float[Actions.Length];
            var Q_Table = new Dynamic_Q_Table(Console.CursorTop, new Dictionary<IConfiguration, float>());
            IConfiguration selected_action;
            float alpha_step, epsilon_step;
            alpha_step = epsilon_step = 1.0f / iterations_by_round;
            int[] seeds;
            List<float> performance = new List<float>();
            int duration_by_round;
            float average_IGD;
            float reward;
            int q_value_index;
            var random = new Random();

            //Torneo
            for (int round = 1; Actions.Length > 1; round++) {
                Q_Table.ReplaceTable(Actions.Select((configuration, index) => new { configuration, q_value = Q_values[index] }).ToDictionary(key_value => key_value.configuration, key_value => key_value.q_value), $"Round {round} / Iteración 0");
                duration_by_round = round * duration_step_between_rounds;
                for (int iteration = 0; iteration < iterations_by_round; iteration++) {
                    selected_action = ChooseAnAction(1 - 0.9f * (epsilon_step * iteration));
                    seeds = Enumerable.Range(1, seed_limit).OrderBy(num => random.Next()).Take(amount_of_performance_to_average).ToArray();
                    performance.Clear();
                    foreach (var seed in seeds)
                        performance.Add(EvaluateAction(selected_action, duration_by_round, seed));
                    average_IGD = selected_action.Performance = performance.Average();
                    var standard_deviation = (float)Math.Sqrt(performance.Select(IGD => Math.Pow(IGD - average_IGD, 2)).Sum() / performance.Count);
                    reward = GetReward(average_IGD, standard_deviation);
                    q_value_index = Actions.ToList().IndexOf(selected_action);
                    Update_Q_Value(ref Q_values[q_value_index], reward, 1 - 0.9f * (alpha_step * iteration));   //A Hybrid Q-Learning Sine-Cosine-based Strategy for Addressing the Combinatorial Test Suite Minimization Problem (https://arxiv.org/pdf/1805.00873.pdf)
                    Q_Table.UpdateTable(selected_action, Q_values[q_value_index], $"Round {round} / Iteración {iteration + 1}");
                }
                ChooseBetterActions();
            }

            Q_Table.ReplaceTable(Actions.Select((configuration, index) => new { configuration, q_value = Q_values[index] }).ToDictionary(key_value => key_value.configuration, key_value => key_value.q_value), "Configuración ganadora");

            //Selecciona la acción (configuración) ganadora.
            var best_action = ChooseBestAction();
            return best_action;

        }

        //Selecciona la mejor acción (configuración) según la posición del máximo Q_value del vector Q.
        private static IConfiguration ChooseBestAction() {
            float maxValue = Q_values.Max();
            int maxIndex = Q_values.ToList().IndexOf(maxValue);
            return Actions[maxIndex];
        }

        //Selecciona una acción (configuración) siguiendo la política ε-Greedy.
        private static IConfiguration ChooseAnAction(float epsilon = 0.1f) {
            var random = new Random();
            if (random.NextDouble() < epsilon)
                return Actions[random.Next() % Actions.Length];
            else
                return ChooseBestAction();
        }

        //Evalúa la acción (configuración) seleccionada.
        private static float EvaluateAction(IConfiguration selected_action, int max_iterations = 250, int seed = 1) {
            var IGD = selected_action.Evaluate(max_iterations, seed);
            return IGD;
        }

        //Obtiene la recompensa según si el IGD (performance_metric) mejora o no el actual mejor rendimiento.
        private static float GetReward(float IGD, float standard_deviation) {
            var reward = 1 - IGD;
            if (standard_deviation > 0)
                reward += 0.5f - standard_deviation;
            return reward;
        }

        //Stateless Q learning variation: Q(a) =  Q(a) + α(R(a) - Q(a)) donde α es la tasa de aprendizaje (0 < α < 1), Q(a) es el valor Q de la acción a, R(a) es la recompensa actual de la acción a.
        private static void Update_Q_Value(ref float q_value, float reward, float alpha = 0.9f) {
            q_value += alpha * (reward - q_value);
        }

        //Selecciona las int(N/2) mejores acciones (configuraciones) según el vector Q.
        private static void ChooseBetterActions() {
            var q_values = Q_values.ToList();
            q_values.Sort();
            q_values.Reverse();
            var remove_from = Actions.Length / 2;
            q_values.RemoveRange(remove_from, Actions.Length - remove_from);
            var new_tuples = q_values.Select(q_value => {
                var q_value_index = Q_values.ToList().IndexOf(q_value);
                return new { action = Actions[q_value_index], q_value = Q_values[q_value_index] };
            }).ToList();
            Actions = new_tuples.Select(tuple => tuple.action).ToArray();
            Q_values = new_tuples.Select(tuple => tuple.q_value).ToArray();
        }

    }

}