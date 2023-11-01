namespace AltiumFileSorter.Models
{
    internal record Row : IComparable<Row>, IEquatable<Row>
    {
        public int FirstValue {  get; set; }
        public string SecondValue { get; set; }

        public Row(string row)
        {
            string[] values = row.Split(Constants.ValueSeparator);

            if (values is null || values.Length != 2)
            {
                throw new ArgumentException("To Create Row you need to pass 2 arguments with proper separator between.");
            }

            if (!int.TryParse(values[0], out int firstValue))
            {
                throw new ArgumentException("To create Row you need to pass first argument as int.");
            }

            FirstValue = firstValue;
            SecondValue = values[1];
        }

        public override string ToString()
            => FirstValue + Constants.ValueSeparator + SecondValue;

        public int CompareTo(Row? other)
        {
            if (other is null)
            {
                return 1;
            }

            int stringComparison = SecondValue.CompareTo(other.SecondValue);

            if(stringComparison == 0)
            {
                return FirstValue.CompareTo(other.FirstValue);
            }
            else
            {
                return stringComparison;
            }
        }
    }
}