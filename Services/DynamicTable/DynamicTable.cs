using System.Linq;

namespace Services.DynamicTable {

    public class DynamicTable : IDynamicTable {

        public int InitialRowPosition { get; set; }
        public string Title { get; }
        public string[][] Table { get; set; }

        public int Width => Table[0].Select((column, index) => GetWidth(index)).Sum() + 2 + (Table[0].Length - 1);

        public DynamicTable(int InitialRowPosition, string Title, string[][] Table) {
            this.InitialRowPosition = InitialRowPosition;
            this.Title = Title;
            this.Table = Table;
        }

        private string AlignCenter(string text, int width, char paddingChar = ' ') {
            if (string.IsNullOrEmpty(text))
                return new string(paddingChar, width);
            else {
                var centered_text = text.PadRight(width - (width - text.Length) / 2, paddingChar).PadLeft(width, paddingChar);
                return centered_text;
            }
        }

        protected void ShiftPosition(int displacement) => System.Console.CursorLeft += displacement;

        protected void ShiftLine(int displacement) => System.Console.SetCursorPosition(0, System.Console.CursorTop + displacement);

        protected void Write(string message, int beforeHorizontalDisplacement = 0, int laterVerticalDisplacement = 0, bool tabulate = false) {
            if (beforeHorizontalDisplacement != 0)
                ShiftPosition(beforeHorizontalDisplacement);
            if (tabulate)
                message = $"\t{message}";
            System.Console.Write(message);
            if (laterVerticalDisplacement != 0)
                ShiftLine(laterVerticalDisplacement);
        }

        protected void ClearLines(int row, int count = 1) {
            System.Console.SetCursorPosition(0, row);
            for (int line = 0; line < count; line++)
                Write(new string(' ', System.Console.WindowWidth), laterVerticalDisplacement: 1);
            System.Console.SetCursorPosition(0, row);
        }

        protected void PrintTable() {

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
            string row;
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

        protected int GetWidth(int column) {
            var length = 0;
            for (int row = 0; row < Table.Length; row++)
                if (Table[row][column].Length > length)
                    length = Table[row][column].Length;
            return length;
        }

        public void Clear() {
            ClearLines(InitialRowPosition, (Table.Length * 2 + 1) + 2);
        }

        public void Display(string footer = "") {
            Clear();
            Write(AlignCenter(Title, Width), laterVerticalDisplacement: 1, tabulate: true);
            PrintTable();
            Write(AlignCenter(footer, Width), laterVerticalDisplacement: 1, tabulate: true);
        }

        public void Replace(string[][] new_table, string footer = default) {
            Clear();
            Table = new_table;
            Display(footer);
        }

        public void Update(string new_value, int row, int column, string footer = default) {
            Table[row][column] = new_value;
            Display(footer);
        }

    }

}