using System;
using System.Collections.Generic;
using System.Linq;

namespace Learnheuristics.ConfigurationTuner.Configurations {

    public class Parameter {

        public float? min_value;

        public float? max_value;

        public List<Func<float, bool>> AdditionalConstraints;

        public Func<float, float> TransformByDomain;

        public bool IsFeasible(float value) {
            if (min_value.HasValue && value < min_value)
                return false;
            else if (max_value.HasValue && value > max_value)
                return false;
            else if (AdditionalConstraints != null && AdditionalConstraints.Any())
                return AdditionalConstraints.All(Constraint => Constraint(value));
            return true;
        }

        public void Transform(ref float value) {
            if (TransformByDomain != null)
                value = TransformByDomain.Invoke(value);
        }

        public void Repair(ref float value, float? arbitrary_min_value = null, float? arbitrary_max_value = null) {
            if ((min_value.HasValue && value < min_value) || (max_value.HasValue && value > max_value))
                GenerateSubjectConstraints(ref value, arbitrary_min_value, arbitrary_max_value);
        }

        public float GenerateSubjectConstraints(ref float value, float? arbitrary_min_value = null, float? arbitrary_max_value = null) {

            var random = new Random();
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

            if (minimum_value.HasValue && maximum_value.HasValue)
                value = random.Next((int)minimum_value.Value, (int)maximum_value.Value);
            else if (minimum_value.HasValue)
                value = random.Next() + minimum_value.Value;
            else if (maximum_value.HasValue)
                value = random.Next((int)maximum_value.Value);
            else
                value = random.Next();

            return value;
        }

    }

}