using System;
using System.Linq;

namespace Learnheuristics.Services {

    //Tabla dinámica: Permite actualizar sus valores y sobreescribir la tabla en consola dinámicamente.
    public class DynamicTable {

        //Posición inicial (columna) en consola.
        private readonly int InitialRowPosition;

        //Título de la tabla.
        private readonly string Title;

        //Cabeceras de la tabla.
        private readonly string[] Headers;

        //Tabla.
        private string[][] Table;

        //Mensaje a los pies de la tabla.
        private string Footer_Message;

        //Ancho total de la tabla: Considera el ancho de cada columna y los caracteres especiales que delimitan la tabla.
        private int Width => Headers.Select((column, index) => GetWidth(index)).Sum() + 2 + (Headers.Length - 1);

        //Objeto utilizado para evitar la actualización simultánea (mutex) de la tabla.
        private static readonly object Lock = new object();

        public DynamicTable(int InitialRowPosition, string Title, string[] Headers, string[][] Table, string Footer_Message = "") {
            this.InitialRowPosition = InitialRowPosition;
            this.Title = Title;
            this.Headers = Headers;
            this.Table = Table;
            this.Footer_Message = Footer_Message;
        }

        //Centra un <text> rellenando sus costados con <paddingChar> para alcanzar el <width>.
        private string AlignCenter(string text, int width, char paddingChar = ' ') {
            if (string.IsNullOrEmpty(text))
                return new string(paddingChar, width);
            else {
                var centered_text = text.PadRight(width - (width - text.Length) / 2, paddingChar).PadLeft(width, paddingChar);
                return centered_text;
            }
        }

        //Desplaza el cursor de consola horizontalmente.
        private void ShiftPosition(int displacement) => Console.CursorLeft += displacement;

        //Desplaza el cursor de consola verticalmente.
        private void ShiftLine(int displacement) => Console.SetCursorPosition(0, Console.CursorTop + displacement);

        //Escribe en consola considerando una tabulación, un desplazamiento horizontal previo y/o un desplazamiento vertical posterior.
        private void Write(string message, int beforeHorizontalDisplacement = 0, int laterVerticalDisplacement = 0, bool tabulate = false) {
            if (beforeHorizontalDisplacement != 0)
                ShiftPosition(beforeHorizontalDisplacement);
            if (tabulate)
                message = $"\t{message}";
            Console.Write(message);
            if (laterVerticalDisplacement != 0)
                ShiftLine(laterVerticalDisplacement);
        }

        //Limpia <count> lineas a partir de <row>.
        private void ClearLines(int row, int count = 1) {
            Console.SetCursorPosition(0, row);
            for (int line = 0; line < count; line++)
                Write(new string(' ', Console.WindowWidth), laterVerticalDisplacement: 1);
            Console.SetCursorPosition(0, row);
        }

        //Obtiene el ancho de la columna según el valor más ancho en ella.
        private int GetWidth(int column) {
            var length = Headers[column].Length;
            for (int row = 0; row < Table.Length; row++)
                if (Table[row][column].Length > length)
                    length = Table[row][column].Length;
            return length;
        }

        //Imprime las cabeceras seguidas por la tabla.
        private void PrintTable() {

            static void Fill(ref string line, int start_index, char content, int count = 1) {
                line = line.Insert(start_index, new string(content, count));
            }

            var top_line = "┌┐";
            var dividing_line = "││";
            var bottom_line = "└┘";

            for (int column = 0, width; column < Table[0].Length; column++) {
                if (column != 0) {
                    Fill(ref top_line, top_line.Length - 1, '┬');
                    Fill(ref dividing_line, dividing_line.Length - 1, '┼');
                    Fill(ref bottom_line, bottom_line.Length - 1, '┴');
                }
                width = GetWidth(column);
                Fill(ref top_line, top_line.Length - 1, '─', width);
                Fill(ref dividing_line, dividing_line.Length - 1, '─', width);
                Fill(ref bottom_line, bottom_line.Length - 1, '─', width);
            }

            Write(top_line, laterVerticalDisplacement: 1, tabulate: true);
            string row = "│";
            for (int column = 0; column < Headers.Length; column++)
                row += AlignCenter(Headers[column], GetWidth(column)) + "│";
            Write(row, laterVerticalDisplacement: 1, tabulate: true);
            Write(dividing_line, laterVerticalDisplacement: 1, tabulate: true);
            for (int line = 0; line < Table.Length; line++) {
                row = "│";
                for (int column = 0; column < Table[line].Length; column++)
                    row += AlignCenter(Table[line][column], GetWidth(column)) + "│";
                Write(row, laterVerticalDisplacement: 1, tabulate: true);
                Write(dividing_line, laterVerticalDisplacement: 1, tabulate: true);
            }
            ShiftLine(-1);
            Write(bottom_line, laterVerticalDisplacement: 1, tabulate: true);

        }

        //Borra la tabla de la consola.
        public void Clear() {
            ClearLines(InitialRowPosition, (Table.Length * 2 + 1) + 5);
        }

        //Muestra la tabla dinámica en la consola.
        public void Display(string new_footer_message = null) {
            if (new_footer_message != null)
                Footer_Message = new_footer_message;
            Clear();
            Write(AlignCenter(Title, Width), laterVerticalDisplacement: 1, tabulate: true);
            Write(AlignCenter(new string('═', Title.Length), Width), laterVerticalDisplacement: 1, tabulate: true);
            PrintTable();
            Write(AlignCenter(Footer_Message, Width), laterVerticalDisplacement: 1, tabulate: true);
        }

        //Reemplaza la tabla completa.
        public void Replace(string[][] new_table, string new_footer_message = null) {
            Clear();
            Table = new_table;
            Display(new_footer_message);
        }

        //Actualiza el valor de una casilla de la tabla.
        public void Update(string new_value, int row, int column, string new_footer_message = null) {
            lock (Lock) {
                Table[row][column] = new_value;
                Display(new_footer_message);
            }
        }

    }

}