using Learnheuristics.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Learnheuristics.Model.Configurations.Instances.NSGA_II {

    //Representa una instancia de configuración.
    public class NSGA_II_Configuration : Configuration<NSGA_II_Parameters>, IConfiguration {

        //Asigna restricciones de forma estática a la clase, ya que estas restricciones se comparten entre todas las instancias de la configuración.
        static NSGA_II_Configuration() {
            var number_of_parameters = Enum.GetNames(typeof(NSGA_II_Parameters)).Length;
            Parameters = new Parameter[number_of_parameters];
            Parameters[(int)NSGA_II_Parameters.pop_size] = new Parameter {
                min_value = 1,
                max_value = 100,
                TransformByDomain = (value) => Convert.ToInt32(Math.Round(value))
            };
            Parameters[(int)NSGA_II_Parameters.crossover_probability] = new Parameter {
                min_value = 0,
                max_value = 100,
                TransformByDomain = (value) => Convert.ToInt32(Math.Round(value))
            };
        }

        public NSGA_II_Configuration(Vector Vectorized_Configuration, string ProblemName) {
            if (Vectorized_Configuration.DimensionalSize != Parameters.Length)
                throw new ArgumentException($"La dimensión del vector {Vectorized_Configuration} no coincide con la cantidad de parámetros registrados en {GetType().Name}.");
            if(string.IsNullOrEmpty(ProblemName))
                throw new ArgumentException($"El argumento <ProblemName> no puede ser nulo o estar vacío.");
            this.Vectorized_Configuration = Vectorized_Configuration;
            this.ProblemName = ProblemName;
            CountdownToRepair = 3;
        }

        //Evalúa la configuración: Este método debe ejecutar PythonScripter.Run para iniciar el script de python que utiliza la metaheurística de bajo nivel.
        //En este caso, ejecuta NSGA-II.py
        public float Evaluate(int max_iterations = 1, int seed = 1) {
            string path_to_script = @"C:\Users\Trifenix\Desktop\Tesis_Learnheuristics\Python\Main.py";
            var arguments = new Dictionary<string, object> {
                ["problem"] = ProblemName,
                ["algorithm"] = "NSGAII",
                ["pop_size"] = Parameter("pop_size"),
                ["crossover_probability"] = Parameter("crossover_probability") / 100,
                ["n_gen"] = max_iterations,
                ["seed"] = seed
            };
            var output = PythonScripter.Run(path_to_script, arguments);
            var IGD = output.FirstOrDefault().Substring(5);
            Performance = Convert.ToSingle(IGD);
            return Performance;
        }

        //Representa las coordenadas de la configuración vectorizada de acuerdo con el siguiente formato: (x,y,z)
        public override string ToString() => base.ToString(); //TODO: Revisar si es necesario o si se hereda el override

    }

}