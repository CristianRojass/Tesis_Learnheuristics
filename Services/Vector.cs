﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Services {
    //Esta clase encapsula representación vectorial de las configuraciones de parámetros como puntos en el hiperespacio.
    public class Vector {

        //Coordenadas del vector. Cada coordenada representa el valor de un parámetro de una configuración de algoritmo.
        public float[] coordinates;

        //Obtiene la cantidad de dimensiones que tiene el vector.
        public int DimensionalSize => coordinates.Length;

        public Vector(float[] coordinates, float scale = 1) {
            this.coordinates = scale != 1 ? coordinates.Select(component => component * scale).ToArray() : coordinates;
        }

        //Suma vectores.
        public static Vector Add(Vector vector_A, Vector vector_B) => OperateVectors(vector_A, vector_B, '+');

        //Resta vectores.
        public static Vector Subtract(Vector vector_A, Vector vector_B) => OperateVectors(vector_A, vector_B, '-');

        //Opera (Suma/Resta) vectores.
        private static Vector OperateVectors(Vector vector_A, Vector vector_B, char operation) {
            if (vector_A.coordinates.Length != vector_B.coordinates.Length)
                throw new InvalidOperationException($"No se pueden {(operation.Equals('+') ? "sumar" : "restar")} los vectores ya que no poseen la misma dimensionalidad.");
            var resultant_vector = new float[vector_A.coordinates.Length];
            for (int i = 0; i < vector_A.coordinates.Length; i++)
                resultant_vector[i] = operation.Equals('+') ? vector_A.coordinates[i] + vector_B.coordinates[i] : vector_A.coordinates[i] - vector_B.coordinates[i];
            return new Vector(resultant_vector);
        }

        //Amplifica el vector multiplicando cada una de sus componentes por <scale>.
        public static Vector Scale(Vector vector, float scale) {
            var resultant_vector = new Vector(vector.coordinates, scale);
            return resultant_vector;
        }

        //Obtiene el módulo de un vector.
        public static float Module(Vector vector) {
            var module = (float)Math.Sqrt(vector.coordinates.Select(componentValue => componentValue * componentValue).Sum());
            return module;
        }

        public static Vector Reduce(List<Vector> vectors) => vectors.Aggregate((A, B) => Add(A, B));

        //Obtiene el producto cartesiano de un vector con si mismo de forma recursiva. Esto sirve para determinar las posibles direcciones a las que puedo moverme según la dimensión.
        //  Ejemplo de visualización
        //for(int i = 2; i < 5; i++) {
        //    Console.WriteLine("Dimensional_size: " + i);
        //    var cartesianProduct = Vector.CartesianProduct(new Vector(new float[] { -1, 0, 1 }), i).ToList(); /Este vector me retorna las combinaciones (direcciones) posibles según la dimensión en la que se encuentra.
        //    cartesianProduct.ForEach(point => Console.WriteLine($"<{string.Join(',', point)}>"));
        //    Console.WriteLine(cartesianProduct.Count + " puntos generados.\n");
        //}
        public static List<float[]> CartesianProduct(Vector vector, int dimensional_size = 2) {
            var sequences = new List<float[]>();
            for (int i = dimensional_size; i >= 1; i--)
                sequences.Add(vector.coordinates);
            List<float[]> result = new List<float[]> { new float[] { } };
            foreach (var sequence in sequences) {
                var localSequence = sequence;
                result = result.SelectMany(
                  _ => localSequence,
                  (seq, item) => seq.Concat(new[] { item }).ToArray()
                ).ToList();
            }
            return result;
        }

        //Grafica una lista de vectores de segunda o tercera dimensión, desde cuarta dimensión hacia arriba sólo se pueden imprimir los puntos (tuplas) en consola.
        //Tuplas que identifican rotación horizontal/vertical para Custom_Scatter3D (Gráfica 3D personalizada).
        //Cantidad de tuplas permitidas: Máximo 4 tuplas.
        //var degrees_tuple_rotation = new List<(float, float)> {
        //(45,45),
        //(0,0),
        //(90,0),
        //(0,90)
        //};
        //var degrees_rotation = $"[{string.Join(",", degrees_tuple_rotation.Select(tuple => $"('{tuple.Item1}','{tuple.Item2}')"))}]";
        //arguments.Add("degrees_rotation", degrees_rotation);
        public static void PlotVectors(List<Vector> hyper_points, Dictionary<string, object> arguments = null, bool with_output = false) {
            if (hyper_points == null || !hyper_points.Any())
                throw new ArgumentException("La lista <hyper_points> no puede ser nula o estar vacía.");
            hyper_points = hyper_points.Distinct().ToList();
            var dimensional_size = hyper_points.All(hyper_point => hyper_point.DimensionalSize > 1) ? hyper_points.First().DimensionalSize : 1;
            if (dimensional_size < 2)
                throw new ArgumentException("Solo se pueden graficar vectores con al menos dos dimensiones.");
            if (with_output) {
                hyper_points.ForEach(hyper_point => Console.WriteLine(hyper_point.ToString()));
                Console.WriteLine($"\nSe han creado {hyper_points.Count} puntos en el hiperespacio.");
            }
            string path_to_script = @"C:\Users\Trifenix\Desktop\PlotPoints.py";
            if (arguments == null)
                arguments = new Dictionary<string, object>();
            arguments["x"] = $"[{string.Join(',', hyper_points.Select(hyper_point => hyper_point.coordinates[0]).ToList())}]";
            arguments["y"] = $"[{string.Join(',', hyper_points.Select(hyper_point => hyper_point.coordinates[1]).ToList())}]";
            if (dimensional_size >= 3)
                arguments["z"] = $"[{string.Join(',', hyper_points.Select(hyper_point => hyper_point.coordinates[2]).ToList())}]";
            PythonScripter.Run(path_to_script, arguments).ForEach(output_line => Console.WriteLine(output_line));
        }

        public override string ToString() {
            return $"({string.Join(",", coordinates)})";
        }

        public override bool Equals(object obj) {
            return obj is Vector vector && coordinates.SequenceEqual(vector.coordinates);
        }

        public override int GetHashCode() {
            HashCode hash = new HashCode();
            foreach (var coordinate in coordinates)
                hash.Add(coordinate);
            return hash.ToHashCode();
        }

    }
}