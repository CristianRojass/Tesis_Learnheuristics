using Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Black_Hole.HyperSpace {

    public class HyperCube {

        //Puntos de esquina que definen el hipercubo.
        public List<Vector> corner_hyper_points;

        //Puntos contenidos en el hipercubo. Serán las estrellas del Black Hole.                PENDIENTE POR IMPLEMENTAR
        public List<Vector> inner_hyper_points;

        //Punto central sobre el cual se encuentra centrado el hipercubo.
        public Vector middle_hyper_point;

        //Obtiene la cantidad de dimensiones que tiene el hipercubo mediante la cantidad de dimensiones que tiene el punto central.
        public int DimensionalSize => middle_hyper_point.DimensionalSize;

        public HyperCube(Vector middle_hyper_point, float length = 10, int factor = 1, int inner_depth = 1) {
            this.middle_hyper_point = middle_hyper_point;
            corner_hyper_points = GetWrapper(length * factor);
            inner_hyper_points = GetInnerPointsNetwork(inner_depth);
        }

        //Obtiene la secuencia de componentes que definen el n-ésimo punto de esquina según su representación en binario.
        private string GetFullyBinaryRepresentation(int value) {
            var binary_string_representation = Convert.ToString(value, 2);
            while (binary_string_representation.Length != DimensionalSize)
                binary_string_representation = binary_string_representation.Insert(0, "0");
            return binary_string_representation;
        }

        //Crea los vectores que representan los puntos de esquina que definen el hipercubo (envoltura) centrado en el origen.
        //La dimension del hipercubo creado depende de la dimensión del punto central del hipercubo.
        private List<Vector> CreateCenterWrapper(float scale) {
            var hypercube_corner_points = new List<Vector>();
            var amount_of_points = Math.Pow(2, DimensionalSize);
            for (int point = 0; point < amount_of_points; point++) {
                var hyper_point = GetFullyBinaryRepresentation(point).Select(coordinate_representation => {
                    if (float.TryParse(coordinate_representation.ToString(), out var coordinate))
                        return coordinate;
                    else
                        throw new InvalidCastException($"No se puede convertir a float la coordenada <{coordinate}>.");
                }).ToArray();
                hypercube_corner_points.Add(new Vector(hyper_point, scale));
            }
            var scroll_to_center = Vector.Scale(hypercube_corner_points.Last(), (float)-1 / 2);
            var centered_hypercube_corner_points = hypercube_corner_points.Select(hyper_corner_point => Vector.Add(hyper_corner_point, scroll_to_center)).ToList();

            #region Comprobación
            /* Para comprobar su funcionamiento:
                1. Comprobar cantidad de puntos generados (#corner_points)
                2. La distancia de cada esquina al punto central debe ser la misma.
                3. Con la figura centrada en el origen, al sumar los vectores que definen la figura (corner_points) debe resultar el vector nulo (0,...,0).
            Lo anterior sugiere figura simetrica. Además, la secuencia generada por la representación binaria para crear los vectores sugiere red cubica. */
            var success = amount_of_points == centered_hypercube_corner_points.Count && Vector.Module(Vector.Reduce(centered_hypercube_corner_points)) == 0 && centered_hypercube_corner_points.Select(vector => Vector.Module(vector)).Distinct().Count() == 1;
            if (!success)
                throw new ApplicationException($"El hipercubo {middle_hyper_point.DimensionalSize} dimensional que ha sido creado no cumple con las validaciones.");
            #endregion

            return centered_hypercube_corner_points;
        }

        //Desplaza el hipercubo centrandolo en el punto central.
        private List<Vector> GetWrapper(float scale) {
            var hypercube = CreateCenterWrapper(scale).Select(corner_hyper_point => Vector.Add(corner_hyper_point, middle_hyper_point)).ToList();
            return hypercube;
        }

        //Crea red de puntos contenidos en el hipercubo. Estos puntos representarán las estrellass (configuraciones) del algoritmo Black Hole.
        private List<Vector> GetInnerPointsNetwork(int depth) {
            var hypercube_length = Vector.Module(Vector.Subtract(corner_hyper_points[0], corner_hyper_points[1]));
            var amount_of_spaces = depth * 2;
            var distance_inner_points = hypercube_length / amount_of_spaces;
            var inner_points_network = GetHypercubicNetwork(middle_hyper_point, distance_inner_points, depth);
            return inner_points_network;
        }

        //Amplifica el hipercubo multiplicando cada uno de sus vectores por <scale>.
        public static HyperCube Scale(HyperCube hypercube, float scale) {
            hypercube.middle_hyper_point = Vector.Scale(hypercube.middle_hyper_point, scale);
            hypercube.corner_hyper_points = hypercube.corner_hyper_points.Select(corner_hyper_point => Vector.Scale(corner_hyper_point, scale)).ToList();
            hypercube.inner_hyper_points = hypercube.inner_hyper_points.Select(inner_hyper_point => Vector.Scale(inner_hyper_point, scale)).ToList();
            return hypercube;
        }

        //Desplaza el hipercubo sumandole el <scroll_vector> a cada uno de sus vectores.
        public static HyperCube Scroll(HyperCube hypercube, Vector scroll_vector) {
            hypercube.middle_hyper_point = Vector.Add(hypercube.middle_hyper_point, scroll_vector);
            hypercube.corner_hyper_points = hypercube.corner_hyper_points.Select(hyper_point => Vector.Add(hyper_point, scroll_vector)).ToList();
            hypercube.inner_hyper_points = hypercube.inner_hyper_points.Select(inner_hyper_point => Vector.Add(inner_hyper_point, scroll_vector)).ToList();
            return hypercube;
        }

        //Centra el hipercubo sumandole a cada uno de sus vectores el opuesto al vector posición del punto central.
        public static HyperCube Center(HyperCube hypercube) {
            var scroll_vector_to_center = Vector.Scale(hypercube.middle_hyper_point, -1);
            hypercube = Scroll(hypercube, scroll_vector_to_center);
            return hypercube;
        }

        //Crea red hipercubica de puntos en el hiperespacio.
        public static List<Vector> CreateCenterHypercubicNetwork(int dimensional_size, float scale, int depth) {
            var amount_of_points_for_line = 2 * depth - 1;
            var amount_of_points = Math.Pow(amount_of_points_for_line, dimensional_size);
            var vectors = new List<Vector>();
            for (int point = 0; point < amount_of_points; point++) {
                var hyper_point = new List<float>();
                for (int dimension = 0; dimension < dimensional_size; dimension++) {
                    var how_often_reset = Convert.ToInt32(Math.Pow(amount_of_points_for_line, dimension + 1));
                    var how_often_change = Convert.ToInt32(Math.Pow(amount_of_points_for_line, dimension));
                    var reseted_point = point % how_often_reset;
                    var expanded_point = reseted_point / how_often_change;
                    var coordinate = expanded_point;
                    var centered_coordinate = coordinate - (depth - 1);
                    hyper_point.Add(centered_coordinate);
                }
                vectors.Add(new Vector(hyper_point.ToArray(), scale));
            }

            #region Comprobación
            /* Para comprobar su funcionamiento:
                1. Comprobar cantidad de puntos generados (#points && #corner_points)
                2. La distancia de cada esquina al punto central debe ser la misma.
                3. Con la figura centrada en el origen, al sumar los vectores que definen la figura debe resultar el vector nulo (0,...,0).
                   Esto aplica tanto para las esquinas como para el total de puntos.
            Lo anterior sugiere figura simetrica. Además, la forma iterativa para crear los vectores a través de for anidados sugiere red cúbica. */
            var corner_module = Vector.Module(vectors.Last());
            var corner_points = vectors.Where(vector => Vector.Module(vector) == corner_module).ToList();
            var amount_of_corner_points = depth == 1 ? 1 : Math.Pow(2, dimensional_size);
            var success = corner_points.Count == amount_of_corner_points && Vector.Module(Vector.Reduce(corner_points)) == 0 && vectors.Count == amount_of_points && Vector.Module(Vector.Reduce(vectors)) == 0;
            if (!success)
                throw new ApplicationException($"La red de puntos en el hiperespacio {dimensional_size} dimensional que ha sido creada no cumple con las validaciones.");
            #endregion

            return vectors;
        }

        //Crea red hipercubica de puntos en el hiperespacio centrados en el middle_hyper_point.
        public static List<Vector> GetHypercubicNetwork(Vector middle_hyper_point, float scale, int depth) {
            var vectors = CreateCenterHypercubicNetwork(middle_hyper_point.DimensionalSize, scale, depth);
            var centered_on_the_middle_point = vectors.Select(vector => Vector.Add(vector, middle_hyper_point)).ToList();
            return centered_on_the_middle_point;
        }

        //Crea red de hipercubos en el hiperespacio centrados en el middle_hyper_point.
        public static List<HyperCube> CreateHyperCubeNetwork(Vector middle_hyper_point, float length = 10, int factor = 1, int depth = 2, int inner_depth = 5) {
            var vectors = GetHypercubicNetwork(middle_hyper_point, length * factor, depth);
            var hyper_cubes = vectors.Select(vector => new HyperCube(vector, length, factor, inner_depth)).ToList();
            return hyper_cubes;
        }

    }

}