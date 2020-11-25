namespace Learnheuristics.Model.Configurations {

    //Propiedades y operaciones compartidas entre los distintos tipos de configuración.
    public interface IConfiguration {

        //Representación vectorial de la configuración.
        Vector Vectorized_Configuration { get; set; }

        //Métrica de rendimiento: En este caso Distancia Generacional Inversa (IGD).
        float Performance { get; set; }

        //Cuenta atrás para reparar parámetros infactibles.
        int CountdownToRepair { get; set; }

        //Evalúa la configuración: Este método debe ejecutar PythonScripter.Run para iniciar el script de python que utiliza la metaheurística de bajo nivel.
        //En este caso, ejecuta NSGA-III.py
        float Evaluate(int max_iterations = 1, int seed = 1);

        //Evalúa si la configuración es o no factible, evaluando parámetro por parámetro si los valores están dentro de los rangos permitidos, además de restricciones adicionales.
        bool IsFeasible();

        //Transforma la configuración reemplazando su representación vectorial.
        void Transform(Vector new_position);

        //Transforma la configuración de acuerdo a restricciones de dominio.
        void TransformByDomain();

        //Repara los parámetros infactibles, generando nuevos valores aleatorios de acuerdo a un conjunto de restricciones de tipo cota inferior y/o superior, además de restricciones adicionales.
        void Repair((float? minimum_constraint, float? maximum_constraint)[] min_max_constraints);

        //Genera una nueva configuración aleatoria de acuerdo a un conjunto de restricciones de tipo cota inferior y/o superior, además de restricciones adicionales.
        void RegenerateSubjectConstraints((float? minimum_constraint, float? maximum_constraint)[] min_max_constraints);

    }

}