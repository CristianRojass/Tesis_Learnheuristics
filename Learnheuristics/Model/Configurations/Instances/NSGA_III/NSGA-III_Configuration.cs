using Learnheuristics.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Learnheuristics.Model.Configurations.Instances.NSGA_III {

    //Representa una instancia de configuración.
    public class NSGA_III_Configuration : Configuration<NSGA_III_Parameters>, IConfiguration {

        //Asigna restricciones de forma estática a la clase, ya que estas restricciones se comparten entre todas las instancias de la configuración.
        static NSGA_III_Configuration() {
            var number_of_parameters = Enum.GetNames(typeof(NSGA_III_Parameters)).Length;
            Parameters = new Parameter[number_of_parameters];
            Parameters[(int)NSGA_III_Parameters.n_partitions] = new Parameter {
                min_value = 1,
                max_value = 100,
                TransformByDomain = (value) => Convert.ToInt32(Math.Round(value))
            };
            Parameters[(int)NSGA_III_Parameters.pop_size] = new Parameter {
                min_value = 1,
                max_value = 100,
                TransformByDomain = (value) => Convert.ToInt32(Math.Round(value))
            };
            //Parameters[(int)NSGA_III_Parameters.probability] = new Parameter {
            //    min_value = 0,
            //    max_value = 100,
            //    TransformByDomain = (value) => Convert.ToInt32(Math.Round(value))
            //};
        }

        public NSGA_III_Configuration(Vector Vectorized_Configuration) {
            if (Vectorized_Configuration.DimensionalSize != Parameters.Length)
                throw new ArgumentException($"La dimensión del vector {Vectorized_Configuration} no coincide con la cantidad de parámetros registrados en {GetType().Name}.");
            this.Vectorized_Configuration = Vectorized_Configuration;
            CountdownToRepair = 3;
        }

        //Evalúa la configuración: Este método debe ejecutar PythonScripter.Run para iniciar el script de python que utiliza la metaheurística de bajo nivel.
        //En este caso, ejecuta NSGA-III.py
        public float Evaluate(int max_iterations = 1, int seed = 1) {
            string path_to_script = @"C:\Users\Trifenix\Desktop\Tesis_Learnheuristics\NSGA-III.py";
            var arguments = new Dictionary<string, object>();
            foreach (var parameterName in Enum.GetNames(typeof(NSGA_III_Parameters)))
                arguments.Add(parameterName, Parameter(parameterName));
            arguments["n_gen"] = max_iterations;
            arguments["seed"] = seed;
            var output = PythonScripter.Run(path_to_script, arguments).FirstOrDefault();
            Performance = Convert.ToSingle(output.Substring(5));
            return Performance;
        }

        //Representa las coordenadas de la configuración vectorizada de acuerdo con el siguiente formato: (x,y,z)
        public override string ToString() => base.ToString(); //TODO: Revisar si es necesario o si se hereda el override

    }

}