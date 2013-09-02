using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite;

namespace CsvQuery
{
    public static class DataStorage
    {
        // For now only in-memroy DB, perhaps later you could have a config setting for saving to disk
        private static SQLiteDatabase db = new SQLiteDatabase(":memory:");

        private static Dictionary<int, string> _createdTables = new Dictionary<int, string>();

        private static int _currentActiveBufferId = 0;
        private static int _lastCreatedTableName = 0;

        public static void SetActiveTab(int bufferId)
        {
            if (_currentActiveBufferId != bufferId && _createdTables.ContainsKey(bufferId))
            {
                if (_currentActiveBufferId != 0)
                {
                    db.ExecuteNonQuery("DROP VIEW this");
                    
                }
                db.ExecuteNonQuery("CREATE VIEW this AS SELECT * FROM " + _createdTables[bufferId]);
                _currentActiveBufferId = bufferId;
            }
        }

        public static void SaveData(int bufferId, List<string[]> data, int columns, bool hasHeader)
        {
            string tableName;
            if (_createdTables.ContainsKey(bufferId))
            {
                tableName = _createdTables[bufferId];
                db.ExecuteNonQuery("DROP TABLE IF EXISTS " + tableName);
            }
            else
            {
                tableName = "T" + ++_lastCreatedTableName;
            }

            // Figure out column types. For now just Int/Decimal/String
            // TODO


            
            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE " + tableName + "(");
            if (hasHeader)
            {

            }
            else
            {
                // Just create Col1, Col2, Col3 etc
            }
        }
    }
}
