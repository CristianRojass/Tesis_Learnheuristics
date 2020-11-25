using Learnheuristics.Model.Configurations;

namespace Learnheuristics.Components.Q_Learning.Model {

    //Representa el par acción-valor utilizado por Stateless Q-learning para identificar las acciones y sus Q-value acumulados.
    public class Q_Tuple<ConfigurationType> where ConfigurationType : IConfiguration {

        //Action: Para este problema, las acciones son representadas mediante configuraciones de algoritmo, los cuales se someten a un torneo basado en el q-value.
        public ConfigurationType Action { get; set; }

        //Q-value: Mantiene el q-value acumulado a través de las evaluaciones realizadas al seleccionar la <Action> (configuración).
        public float Q_value { get; set; }

        public Q_Tuple(ConfigurationType Action, float Q_value) {
            this.Action = Action;
            this.Q_value = Q_value;
        }

    }

}