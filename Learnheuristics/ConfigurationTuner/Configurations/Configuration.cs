using Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Learnheuristics.ConfigurationTuner.Configurations {

    public abstract class Configuration<Parameters_Enum> where Parameters_Enum : Enum {

        public static Parameter[] Parameters { get; set; }

        public Vector Vectorized_Configuration { get; set; }

        public float Performance { get; set; }

        public int CountdownToRepair { get; set; }

        public ref float Parameter(int parameter_index) => ref Vectorized_Configuration.coordinates[parameter_index];

        public ref float Parameter(Parameters_Enum parameter) => ref Parameter(Convert.ToInt32(parameter));

        public ref float Parameter(string parameterName) {
            if (Enum.TryParse(typeof(Parameters_Enum), parameterName, out var index))
                return ref Parameter((Parameters_Enum)index);
            throw new KeyNotFoundException($"No se encontró el parámetro <{parameterName}> en la enumeración <{typeof(Parameters_Enum).Name}>");
        }

        private int[] ParameterIndex => Enum.GetValues(typeof(Parameters_Enum)).Cast<int>().ToArray();

        public bool IsFeasible() {
            foreach (int index in ParameterIndex)
                if (!Parameters[index].IsFeasible(Parameter(index))) {
                    CountdownToRepair--;
                    return false;
                }
            return true;
        }

        public void Transform(Vector new_position) {
            Vectorized_Configuration = new_position;
        }

        public void TransformByDomain() {
            foreach (int index in ParameterIndex)
                Parameters[index].Transform(ref Parameter(index));
        }

        //TODO: Debo recibir por parámetros los límites del hipercubo para limitar los valores generados
        public void Repair((float?, float?)[] min_max_constraints) {
            if (min_max_constraints.Length != ParameterIndex.Length)
                throw new ArgumentException("La cantidad de restricciones min/max debe coincidir con la cantidad de parámetros.");
            foreach (int index in ParameterIndex)
                Parameters[index].Repair(ref Parameter(index), min_max_constraints[index].Item1, min_max_constraints[index].Item2);
            CountdownToRepair = 3;
        }

        //TODO: Debo recibir por parámetros los límites del hipercubo para limitar los valores generados
        public void RegenerateSubjectConstraints((float?, float?)[] min_max_constraints) {
            if (min_max_constraints.Length != ParameterIndex.Length)
                throw new ArgumentException("La cantidad de restricciones min/max debe coincidir con la cantidad de parámetros.");
            foreach (int index in ParameterIndex)
                Parameters[index].GenerateSubjectConstraints(ref Parameter(index), min_max_constraints[index].Item1, min_max_constraints[index].Item2);
        }

        public override bool Equals(object obj) {
            return obj is Configuration<Parameters_Enum> configuration && configuration.Vectorized_Configuration.Equals(Vectorized_Configuration);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Vectorized_Configuration.GetHashCode());
        }

    }

}