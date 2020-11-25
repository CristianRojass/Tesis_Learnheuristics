using Learnheuristics.Components;
using Learnheuristics.Model;
using Learnheuristics.Model.Configurations.Instances.NSGA_III;
using System;
using System.Threading.Tasks;

namespace Consola {

    public class Program {

        public static async Task Main() {

            Console.WriteLine($"Hora de inicio: {DateTime.Now:hh:mm:ss}\n");

            #region Black Hole
            //Punto sobre el cual se centrará la red de hipercubos
            var middle_hyperpoint = new Vector(new float[] { 50, 50 });
            //Longitud de cada dimension para cada hipercubo
            float hypercube_length = 50;
            //Factor por el cual se puede amplificar la longitud de cada dimension de hipercubo
            var factor = 1;
            //Profundidad: Capas de envoltura alrededor del punto central
            var depth = 1;
            //Profundidad interior: Capas de puntos contenidos en el hipercubo
            var inner_depth = 1;
            //Cantidad de generaciones (iteraciones) que realiza la metaheurística de bajo nivel (NSGA-III) en cada evaluación
            var evaluation_duration = 100;
            //Cantidad de épocas (iteraciones) que realiza el algoritmo Black Hole
            var number_of_epochs = 5;
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

            var best_configuration = await Tuner.Run(new NSGA_III_Configuration(middle_hyperpoint), hypercube_length, factor, depth, inner_depth, evaluation_duration, number_of_epochs, iterations_by_round, duration_step_between_rounds, number_of_performances_to_average, verification_duration);
            Console.WriteLine($"\nLa mejor configuración encontrada es: {best_configuration}\n");

            Console.WriteLine($"Hora de termino: {DateTime.Now:hh:mm:ss}");

            return;

        }

    }

}