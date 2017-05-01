namespace CsvQuery.Csv
{
    interface ICsvLanguage
    {
        void Consume(byte ch);
    }

    class SimpleCsvLanguage:ICsvLanguage
    {
        private bool wordStart = true;
        private bool lineStart = true;

        private byte _separator;

        public SimpleCsvLanguage(char separator)
        {
            _separator = (byte)separator;
        }

        public void Consume(byte ch)
        {
            
        }
    }
}
