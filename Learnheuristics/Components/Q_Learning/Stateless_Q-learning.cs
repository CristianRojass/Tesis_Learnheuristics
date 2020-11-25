using Learnheuristics.Components.Q_Learning.Model;
using Learnheuristics.Components.Q_Learning.Services;
using Learnheuristics.Model.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Learnheuristics.Components.Q_Learning {

    public class Stateless_Q_Learning {

        //Pone en ejecución el algoritmo que selecciona la mejor configuración de un conjunto de ellas.
        //El algoritmo realiza un torneo entre las N configuraciones candidatas, con log_2(N) rondas
        //en las que se evalúan y seleccionan la mitad de las configuraciones aún en competencia,
        //reduciendo el conjunto a la mitad de forma recursiva hasta obtener una única configuración.
        public static ConfigurationType Run<ConfigurationType>(ConfigurationType[] configurations, int iterations_by_round = 50, int duration_step_between_rounds = 250, int number_of_performances_to_average = 5, int seed_limit = 100) where ConfigurationType : IConfiguration {

            #region Funciones estáticas locales
            //Selecciona la mejor acción (configuración) según la posición del máximo Q_value del vector Q.
            static Q_Tuple<ConfigurationType> ChooseBestAction(Q_Tuple<ConfigurationType>[] Action_Value) {
                var best_action = Action_Value.Aggregate((first_action_value, second_action_value) => first_action_value.Q_value >= second_action_value.Q_value ? first_action_value : second_action_value);
                return best_action;
            }

            //Selecciona una acción (configuración) siguiendo la política ε-Greedy.
            static Q_Tuple<ConfigurationType> ChooseAnAction(Q_Tuple<ConfigurationType>[] Actions_Values, float epsilon = 0.1f) {
                var random = new Random();
                if (random.NextDouble() < epsilon)
                    return Actions_Values[random.Next() % Actions_Values.Length];
                else
                    return ChooseBestAction(Actions_Values);
            }

            //Evalúa la acción (configuración) seleccionada.
            static float EvaluateAction(ConfigurationType selected_action, int max_iterations = 250, int seed = 1) {
                var IGD = selected_action.Evaluate(max_iterations, seed);
                return IGD;
            }

            //Obtiene la recompensa según si el IGD (performance_metric) mejora o no el actual mejor rendimiento.
            static float GetReward(float IGD, float standard_deviation) {
                var reward = 1 - IGD;
                if (standard_deviation > 0)
                    reward += 0.5f - standard_deviation;
                return reward;
            }

            //Stateless Q learning variation: Q(a) =  Q(a) + α(R(a) - Q(a)) donde α es la tasa de aprendizaje (0 < α < 1), Q(a) es el valor Q de la acción a, R(a) es la recompensa actual de la acción a.
            static float Update_Q_Value(float q_value, float reward, float alpha = 0.9f) {
                q_value += alpha * (reward - q_value);
                return q_value;
            }

            //Selecciona las int(N/2) mejores acciones (configuraciones) según el vector Q.
            static void ChooseBetterActions(ref Q_Tuple<ConfigurationType>[] Actions_Values) {
                var number_of_selected = Actions_Values.Length / 2;
                Actions_Values = Actions_Values.OrderByDescending(action_value => action_value.Q_value).Take(number_of_selected).ToArray();
            }
            #endregion

            #region Validación de parámetros
            if (configurations == null || !configurations.Any())
                throw new ArgumentNullException("El parámetro <configurations> no puede ser nulo o estar vacío.");
            else if (configurations.Distinct().Count() < configurations.Count())
                throw new ArgumentException("El parámetro <configurations> no puede tener elementos duplicados.");
            if (iterations_by_round < 1)
                throw new ArgumentOutOfRangeException("El parámetro <iterations_by_round> no puede ser menor a 1.");
            else if (iterations_by_round < configurations.Length)
                throw new ArgumentException("El parámetro <iterations_by_round> no puede ser menor que la cantidad de configuraciones disponibles.");
            if (duration_step_between_rounds < 1)
                throw new ArgumentOutOfRangeException("El parámetro <duration_step_between_rounds> no puede ser menor a 1.");
            if (number_of_performances_to_average < 1)
                throw new ArgumentOutOfRangeException("El parámetro <amount_of_performance_to_average> no puede ser menor a 1.");
            if (seed_limit < number_of_performances_to_average)
                throw new ArgumentException("El parámetro <seed_limit> no puede ser menor que la cantidad de evaluaciones a promediar.");
            #endregion

            #region Declaración e inicialización de variables
            var Actions_Values = configurations.Select(configuration => new Q_Tuple<ConfigurationType>(configuration, 0)).ToArray();
            var Q_Table = new Dynamic_Q_Table(Console.CursorTop, new Dictionary<IConfiguration, float>());
            Q_Tuple<ConfigurationType> selected_action, best_action;
            float alpha_step, epsilon_step;
            alpha_step = epsilon_step = 1.0f / iterations_by_round;
            int[] seeds;
            List<float> performance = new List<float>();
            int duration_by_round;
            float average_IGD, standard_deviation, reward;
            var random = new Random();
            #endregion

            #region Torneo
            for (int round = 1; Actions_Values.Length > 1; round++) {
                Q_Table.Replace(Actions_Values.Select(action_value => new { configuration = action_value.Action as IConfiguration, q_value = action_value.Q_value }).ToDictionary(key_value => key_value.configuration, key_value => key_value.q_value), $"Round {round} / Iteración 0");
                duration_by_round = round * duration_step_between_rounds;
                for (int iteration = 0; iteration < iterations_by_round; iteration++) {
                    selected_action = ChooseAnAction(Actions_Values, 1 - 0.9f * (epsilon_step * iteration));
                    seeds = Enumerable.Range(1, seed_limit).OrderBy(num => random.Next()).Take(number_of_performances_to_average).ToArray();
                    performance.Clear();
                    foreach (var seed in seeds)
                        performance.Add(EvaluateAction(selected_action.Action, duration_by_round, seed));
                    average_IGD = selected_action.Action.Performance = performance.Average();
                    standard_deviation = (float)Math.Sqrt(performance.Select(IGD => Math.Pow(IGD - average_IGD, 2)).Sum() / performance.Count);
                    reward = GetReward(average_IGD, standard_deviation);
                    selected_action.Q_value = Update_Q_Value(selected_action.Q_value, reward, 1 - 0.9f * (alpha_step * iteration));   //A Hybrid Q-Learning Sine-Cosine-based Strategy for Addressing the Combinatorial Test Suite Minimization Problem (https://arxiv.org/pdf/1805.00873.pdf)
                    Q_Table.Update(selected_action.Action, selected_action.Q_value, $"Round {round} / Iteración {iteration + 1}");
                }
                ChooseBetterActions(ref Actions_Values);
            }
            #endregion

            //Selecciona la acción (configuración) ganadora.
            best_action = ChooseBestAction(Actions_Values);
            Q_Table.Replace(new Dictionary<IConfiguration, float> { { best_action.Action, best_action.Q_value } }, "Configuración ganadora");

            return best_action.Action;

        }

    }

}