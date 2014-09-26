//  $Header$
using System;

namespace Community.CsharpSqlite
{
    using Vdbe = Sqlite3.Vdbe;

    /// <summary>
    /// C#-SQLite wrapper with functions for opening, closing and executing queries.
    /// </summary>
    public class SQLiteVdbe
    {
        private Vdbe vm = null;
        private string LastError = "";
        private int LastResult = 0;

        /// <summary>
        /// Creates new instance of SQLiteVdbe class by compiling a statement
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Vdbe</returns>
        public SQLiteVdbe(SQLiteDatabase db, String query)
        {
            vm = null;

            // prepare and compile 
            Sqlite3.sqlite3_prepare_v2(db.Connection(), query, query.Length, ref vm, 0);
        }

        /// <summary>
        /// Return Virtual Machine Pointer
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Vdbe</returns>
        public Vdbe VirtualMachine()
        {
            return vm;
        }

        /// <summary>
        /// BindInteger
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bInteger"></param>
        /// <returns>LastResult</returns>
        public int BindInteger(int index, int bInteger)
        {
            if ((LastResult = Sqlite3.sqlite3_bind_int(vm, index, bInteger)) == Sqlite3.SQLITE_OK)
            { LastError = ""; }
            else
            {
                LastError = "Error " + LastError + "binding Integer [" + bInteger + "]";
            }
            return LastResult;
        }

        /// <summary>
        /// BindLong
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bLong"></param>
        /// <returns>LastResult</returns>
        public int BindLong(int index, long bLong)
        {
            if ((LastResult = Sqlite3.sqlite3_bind_int64(vm, index, bLong)) == Sqlite3.SQLITE_OK)
            { LastError = ""; }
            else
            {
                LastError = "Error " + LastError + "binding Long [" + bLong + "]";
            }
            return LastResult;
        }

        // Note: added by jokedst
        public double BindDouble(int index, double bDouble)
        {
            if ((LastResult = Sqlite3.sqlite3_bind_double(vm, index, bDouble)) == Sqlite3.SQLITE_OK)
            { 
                LastError = ""; 
            }
            else
            {
                LastError = "Error " + LastError + "binding Long [" + bDouble + "]";
            }
            return LastResult;
        }

        /// <summary>
        /// BindText
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bText"></param>
        /// <returns>LastResult</returns>
        public int BindText(int index, string bText)
        {
            if ((LastResult = Sqlite3.sqlite3_bind_text(vm, index, bText, -1, null)) == Sqlite3.SQLITE_OK)
            { LastError = ""; }
            else
            {
                LastError = "Error " + LastError + "binding Text [" + bText + "]";
            }
            return LastResult;
        }

        /// <summary>
        /// Execute statement
        /// </summary>
        /// <returns>LastResult</returns>
        public int ExecuteStep()
        {
            // Execute the statement
            int LastResult = Sqlite3.sqlite3_step(vm);
            return LastResult;
        }

        /// <summary>
        /// Returns Result column as Long
        /// </summary>
        /// <returns>Result column</returns>
        public long Result_Long(int index)
        {
            return Sqlite3.sqlite3_column_int64(vm, index);
        }

        // Note: added by jokedst
        public double Result_Double(int index)
        {
            return Sqlite3.sqlite3_column_double(vm, index);
        }

        /// <summary>
        /// Returns Result column as Text
        /// </summary>
        /// <returns>Result column</returns>
        public string Result_Text(int index)
        {
            return Sqlite3.sqlite3_column_text(vm, index);
        }


        /// <summary>
        /// Returns Count of Result Rows
        /// </summary>
        /// <returns>Count of Results</returns>
        public int ResultColumnCount()
        {
            return vm.pResultSet == null ? 0 : vm.pResultSet.Length;
        }

        /// <summary>
        /// Reset statement
        /// </summary>
        public void Reset()
        {
            // Reset the statment so it's ready to use again
            Sqlite3.sqlite3_reset(vm);
        }

        /// <summary>
        /// Closes statement
        /// </summary>
        /// <returns>LastResult</returns>
        public void Close()
        {
            Sqlite3.sqlite3_finalize(vm);
        }

        // Note: added by jokedst
        /// <summary>
        /// Gets column name
        /// </summary>
        /// <param name="index">index of column</param>
        /// <returns>Name of column</returns>
        public string ColumnName(int index)
        {
            return Sqlite3.sqlite3_column_name(vm, index);
        }
    }
}
