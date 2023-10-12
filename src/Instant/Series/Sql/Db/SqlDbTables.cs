namespace Radical.Instant.Series.Sql.Db
{
    using System.Collections.Generic;
    using System.Linq;

    public class SqlDbTables
    {
        private object holder = new object();
        public object Holder
        {
            get { return holder; }
        }

        public SqlDbTables()
        {
            tables = new List<SqlDbTable>();
        }

        private List<SqlDbTable> tables;
        public List<SqlDbTable> List
        {
            get { return tables; }
            set { tables.AddRange(value.Where(c => !this.Have(c.TableName)).ToList()); }
        }

        public void Add(SqlDbTable table)
        {
            if (!this.Have(table.TableName))
            {
                tables.Add(table);
            }
        }

        public void AddRange(List<SqlDbTable> _tables)
        {
            tables.AddRange(_tables.Where(c => !this.Have(c.TableName)).ToList());
        }

        public void Remove(SqlDbTable table)
        {
            tables.Remove(table);
        }

        public void RemoveAt(int index)
        {
            tables.RemoveAt(index);
        }

        public bool Have(string TableName)
        {
            return tables.Where(t => t.TableName == TableName).Any();
        }

        public void Clear()
        {
            tables.Clear();
        }

        public SqlDbTable this[string TableName]
        {
            get { return tables.Where(c => TableName == c.TableName).First(); }
        }
        public SqlDbTable this[int TableIndex]
        {
            get { return tables[TableIndex]; }
        }

        public SqlDbTable GetDbTable(string TableName)
        {
            return tables.Where(c => TableName == c.TableName).First();
        }

        public List<SqlDbTable> GetDbTables(List<string> TableNames)
        {
            return tables.Where(c => TableNames.Contains(c.TableName)).ToList();
        }
    }
}
