using Learnheuristics.ConfigurationTuner.Configurations;
using Services.DynamicTable;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Q_Learning {

    public class Dynamic_Q_Table : DynamicTable, IDynamicTable {

        public new Dictionary<IConfiguration, float> Table;

        public Dynamic_Q_Table(int InitialRowPosition, Dictionary<IConfiguration, float> Table, string Title = "Q-Table", string[] Headers = default) : base(InitialRowPosition, Title, Transform(Headers, Table)) {
            this.Table = Table;
        }

        public void UpdateTable(IConfiguration Configuration, float Q_value, string message) {
            Table[Configuration] = Q_value;
            Update(Q_value.ToString(), Table.Keys.ToList().IndexOf(Configuration) + 1, 1, message);
        }

        public void ReplaceTable(Dictionary<IConfiguration, float> newTable, string message) {
            Table = newTable;
            Replace(Transform(default, newTable), message);
        }

        static string[][] Transform(string[] Headers, Dictionary<IConfiguration, float> Table) {
            var matrix_table = new string[Table.Count + 1][];
            matrix_table[0] = Headers ?? new string[] { "Configuración", "Q-Value" };
            for (int row = 1; row <= Table.Count; row++) {
                var key_value = Table.ElementAt(row - 1);
                matrix_table[row] = new string[] { key_value.Key.ToString(), decimal.Parse(key_value.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture).ToString() };
            }
            return matrix_table;
        }
    }

}