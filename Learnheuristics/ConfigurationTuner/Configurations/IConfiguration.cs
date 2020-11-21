using Services;

namespace Learnheuristics.ConfigurationTuner.Configurations {

    public interface IConfiguration {

        Vector Vectorized_Configuration { get; set; }
        float Performance { get; set; }
        int CountdownToRepair { get; set; }
        float Evaluate(int max_iterations = 1, int seed = 1);
        bool IsFeasible();
        ref float Parameter(int parameter_index);
        ref float Parameter(string parameter_name);
        void Transform(Vector new_position);
        void TransformByDomain();
        void Repair((float?, float?)[] min_max_constraints);
        void RegenerateSubjectConstraints((float?, float?)[] min_max_constraints);

    }

}