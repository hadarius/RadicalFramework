namespace Radical.Instant.Series.Sql.Db
{
    using System.Collections.Generic;
    using System.Linq;
    using Rubrics;

    public class SqlDbColumns
    {
        public SqlDbColumns()
        {
            list = new List<SqlDbColumn>();
        }

        private List<SqlDbColumn> list;
        public List<SqlDbColumn> List
        {
            get { return list; }
            set { list.AddRange(value.Where(c => !this.Have(c.ColumnName)).ToList()); }
        }

        public void Add(SqlDbColumn column)
        {
            if (!this.Have(column.ColumnName))
                List.Add(column);
        }

        public void AddRange(List<SqlDbColumn> _columns)
        {
            list.AddRange(_columns.Where(c => !this.Have(c.ColumnName)).ToList());
        }

        public void Remove(SqlDbColumn column)
        {
            list.Remove(column);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public void Clear()
        {
            List.Clear();
        }

        public bool Have(string ColumnName)
        {
            return list.Where(c => c.ColumnName == ColumnName).Any();
        }

        public SqlDbColumn this[string ColumnName]
        {
            get { return list.Where(c => ColumnName == c.ColumnName).First(); }
        }
        public SqlDbColumn this[int Ordinal]
        {
            get { return list.Where(c => Ordinal == c.DbOrdinal).First(); }
        }

        public SqlDbColumn GetDbColumn(string ColumnName)
        {
            return list.Where(c => c.ColumnName == ColumnName).First();
        }

        public SqlDbColumn[] GetDbColumns(List<string> ColumnNames)
        {
            return list.Where(c => ColumnNames.Contains(c.ColumnName)).ToArray();
        }

        public List<MemberRubric> GetRubrics(string ColumnNames)
        {
            return list.Where(c => ColumnNames == c.ColumnName).SelectMany(r => r.Rubrics).ToList();
        }

        public List<MemberRubric> GetRubrics(List<string> ColumnNames)
        {
            return list.Where(c => ColumnNames.Contains(c.ColumnName))
                .SelectMany(r => r.Rubrics)
                .ToList();
        }
    }
}
