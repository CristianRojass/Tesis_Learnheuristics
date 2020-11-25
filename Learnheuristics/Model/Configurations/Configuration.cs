using System;
using System.Collections.Generic;
using System.Linq;

namespace Learnheuristics.Model.Configurations {

    //Propiedades y operaciones compartidas entre los distintos tipos de configuración.
    public abstract class Configuration<Parameters_Enum> where Parameters_Enum : Enum {

        //Conjunto de restricciones y operaciones intrínsecas de cada parametro.
        public static Parameter[] Parameters { get; set; }

        //Representación vectorial de la configuración.
        public Vector Vectorized_Configuration { get; set; }

        //Métrica de rendimiento: En este caso Distancia Generacional Inversa (IGD).
        public float Performance { get; set; }

        //Cuenta atrás para reparar parámetros infactibles.
        public int CountdownToRepair { get; set; }

        //Obtiene la referencia (ref float) de una coordenada según su <parameter_index> desde la configuración vectorizada.
        public ref float Parameter(int parameter_index) => ref Vectorized_Configuration.coordinates[parameter_index];

        //Obtiene la referencia (ref float) de una coordenada según su <parameter:Parameters_Enum> desde la configuración vectorizada.
        public ref float Parameter(Parameters_Enum parameter) => ref Parameter(Convert.ToInt32(parameter));

        //Obtiene la referencia (ref float) de una coordenada según su <parameterName> desde la configuración vectorizada.
        public ref float Parameter(string parameterName) {
            if (Enum.TryParse(typeof(Parameters_Enum), parameterName, out var index))
                return ref Parameter((Parameters_Enum)index);
            throw new KeyNotFoundException($"No se encontró el parámetro <{parameterName}> en la enumeración <{typeof(Parameters_Enum).Name}>");
        }

        //Obtiene el conjunto de índices definido en la enumeración de los parámetros de la configuración.
        private int[] ParameterIndex => Enum.GetValues(typeof(Parameters_Enum)).Cast<int>().ToArray();

        //Evalúa si la configuración es o no factible, evaluando parámetro por parámetro si los valores están dentro de los rangos permitidos, además de restricciones adicionales.
        public bool IsFeasible() {
            foreach (int index in ParameterIndex)
                if (!Parameters[index].IsFeasible(Parameter(index))) {
                    CountdownToRepair--;
                    return false;
                }
            return true;
        }

        //Transforma la configuración reemplazando su representación vectorial.
        public virtual void Transform(Vector new_position) {
            Vectorized_Configuration = new_position;
        }

        //Transforma la configuración de acuerdo a restricciones de dominio.
        public void TransformByDomain() {
            foreach (int index in ParameterIndex)
                Parameters[index].Transform(ref Parameter(index));
        }

        //Repara los parámetros infactibles, generando nuevos valores aleatorios de acuerdo a un conjunto de restricciones de tipo cota inferior y/o superior, además de restricciones adicionales.
        public void Repair((float? minimum_constraint, float? maximum_constraint)[] min_max_constraints) {
            if (min_max_constraints.Length != ParameterIndex.Length)
                throw new ArgumentException("La cantidad de restricciones min/max debe coincidir con la cantidad de parámetros.");
            foreach (int index in ParameterIndex)
                Parameters[index].Repair(ref Parameter(index), min_max_constraints[index].minimum_constraint, min_max_constraints[index].maximum_constraint);
            CountdownToRepair = 3;
        }

        //Genera una nueva configuración aleatoria de acuerdo a un conjunto de restricciones de tipo cota inferior y/o superior, además de restricciones adicionales.
        public void RegenerateSubjectConstraints((float? minimum_constraint, float? maximum_constraint)[] min_max_constraints) {
            if (min_max_constraints.Length != ParameterIndex.Length)
                throw new ArgumentException("La cantidad de restricciones min/max debe coincidir con la cantidad de parámetros.");
            foreach (int index in ParameterIndex)
                Parameters[index].GenerateSubjectConstraints(ref Parameter(index), min_max_constraints[index].minimum_constraint, min_max_constraints[index].maximum_constraint);
        }

        //Evalúa la igualdad de configuraciones según las coordenadas de la configuración vectorizada.
        public override bool Equals(object obj) => obj is Configuration<Parameters_Enum> configuration && configuration.Vectorized_Configuration.Equals(Vectorized_Configuration);

        //HashCode es utilizado por diccionarios y/o LINQ para identificar de forma única a este objeto.
        //En este caso, el HashCode está compuesto por las coordenadas del vector.
        public override int GetHashCode() => HashCode.Combine(Vectorized_Configuration.GetHashCode());

        //Representa las coordenadas de la configuración vectorizada de acuerdo con el siguiente formato: (x,y,z)
        public override string ToString() => Vectorized_Configuration.ToString();

    }

}