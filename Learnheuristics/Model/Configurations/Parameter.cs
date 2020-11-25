using System;
using System.Collections.Generic;
using System.Linq;

namespace Learnheuristics.Model.Configurations {

    //Restricciones y operaciones intrínsecas de parametro.
    public class Parameter {

        //Cota inferior: Es nullable para convertirlo en opcional.
        public float? min_value;

        //Cota superior: Es nullable para convertirlo en opcional.
        public float? max_value;

        //Lista de funciones delegadas que evalúan restricciones adicionales.
        public List<Func<float, bool>> AdditionalConstraints;

        //Función delegada que transforma el valor según restricciones de dominio.
        public Func<float, float> TransformByDomain;

        //Obtiene los límites (Rango) dentro de los cuales está permitido el valor del parámetro.
        private (float?, float?) GetBounds(float? arbitrary_min_value = null, float? arbitrary_max_value = null) {
            float? minimum_value = null;
            float? maximum_value = null;

            if (min_value.HasValue && arbitrary_min_value.HasValue)
                minimum_value = Math.Max(min_value.Value, arbitrary_min_value.Value);
            else if (min_value.HasValue)
                minimum_value = min_value.Value;
            else if (arbitrary_min_value.HasValue)
                minimum_value = arbitrary_min_value.Value;

            if (max_value.HasValue && arbitrary_max_value.HasValue)
                maximum_value = Math.Min(max_value.Value, arbitrary_max_value.Value);
            else if (max_value.HasValue)
                maximum_value = max_value.Value;
            else if (arbitrary_max_value.HasValue)
                maximum_value = arbitrary_max_value.Value;

            return (minimum_value, maximum_value);
        }

        //Evalúa si el valor del parámetro es o no factible de acuerdo a un conjunto de restricciones de tipo cota inferior y/o superior, además de restricciones adicionales.
        public bool IsFeasible(float value, float? arbitrary_min_value = null, float? arbitrary_max_value = null) {
            (float? minimum, float? maximum) = GetBounds(arbitrary_min_value, arbitrary_max_value);
            if ((minimum.HasValue && value < minimum.Value) || (maximum.HasValue && value > maximum.Value))
                return false;
            else if (AdditionalConstraints != null && AdditionalConstraints.Any())
                return AdditionalConstraints.All(Constraint => Constraint(value));
            return true;
        }

        //Transforma el valor de acuerdo a restricciones de dominio.
        public void Transform(ref float value) {
            if (TransformByDomain != null)
                value = TransformByDomain.Invoke(value);
        }

        //Repara el valor del parámetro infactible, generando un nuevo valor aleatorio de acuerdo a un conjunto de restricciones de tipo cota inferior y/o superior, además de restricciones adicionales.
        public void Repair(ref float value, float? arbitrary_min_value = null, float? arbitrary_max_value = null) {
            if (!IsFeasible(value, arbitrary_min_value, arbitrary_max_value))
                GenerateSubjectConstraints(ref value, arbitrary_min_value, arbitrary_max_value);
        }

        //Genera un nuevo valor aleatorio de acuerdo a un conjunto de restricciones de tipo cota inferior y/o superior, además de restricciones adicionales.
        public void GenerateSubjectConstraints(ref float value, float? arbitrary_min_value = null, float? arbitrary_max_value = null) {

            (float? minimum, float? maximum) = GetBounds(arbitrary_min_value, arbitrary_max_value);

            var random = new Random();
            float local_copy_of_value;
            do {
                if (minimum.HasValue && maximum.HasValue)
                    value = random.Next((int)minimum.Value, (int)maximum.Value);
                else if (minimum.HasValue)
                    value = random.Next() + minimum.Value;
                else if (maximum.HasValue)
                    value = random.Next((int)maximum.Value);
                else
                    value = random.Next();
                local_copy_of_value = value;
            } while (AdditionalConstraints != null && AdditionalConstraints.All(Constraint => Constraint(local_copy_of_value)));

        }

    }

}