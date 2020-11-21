namespace Services.DynamicTable {

    public interface IDynamicTable {

        int InitialRowPosition { get; set; }
        string Title { get; }
        string[][] Table { get; set; }
        int Width { get; }
        void Clear();
        void Display(string footer = default);
        void Replace(string[][] new_table, string footer = default);
        void Update(string value, int row, int column, string footer = default);

    }

}