using Learnheuristics.Model.Configurations;
using Learnheuristics.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Learnheuristics.Components.Q_Learning.Services {

    //Q-Tabla dinámica: Intrínseca de Q-learning, permite actualizar sus valores y sobreescribir la tabla en consola dinámicamente.
    public class Dynamic_Q_Table {

        //Diccionario Acción-Valor
        public Dictionary<IConfiguration, float> Table;

        //Tabla dinámica
        private readonly DynamicTable dynamicTable;

        public Dynamic_Q_Table(int InitialRowPosition, Dictionary<IConfiguration, float> Table, string Footer_Message = null) {
            this.Table = Table;
            dynamicTable = new DynamicTable(InitialRowPosition, "Q-Table", new string[] { "Configuración", "Q-Value" }, Transform(Table), Footer_Message);
        }

        //Actualiza el valor de una casilla de la tabla.
        public void Update(IConfiguration Configuration, float Q_value, string footer_message = null) {
            Table[Configuration] = Q_value;
            dynamicTable.Update(Q_value.ToString(), Table.Keys.ToList().IndexOf(Configuration), 1, footer_message);
        }

        //Reemplaza la tabla completa.
        public void Replace(Dictionary<IConfiguration, float> newTable, string footer_message = null) {
            Table = newTable;
            dynamicTable.Replace(Transform(newTable), footer_message);
        }

        //Convierte el diccionario en una tabla matricial de strings.
        static string[][] Transform(Dictionary<IConfiguration, float> Table) {
            var matrix_table = new string[Table.Count][];
            for (int row = 0; row < Table.Count; row++) {
                var key_value = Table.ElementAt(row);
                matrix_table[row] = new string[] { key_value.Key.ToString(), decimal.Parse(key_value.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture).ToString() };
            }
            return matrix_table;
        }

    }

}