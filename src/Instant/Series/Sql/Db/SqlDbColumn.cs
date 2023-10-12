namespace Radical.Instant.Series.Sql.Db
{
    using System;
    using System.Collections.Generic;
    using Rubrics;

    public class SqlDbColumn
    {
        public SqlDbColumn()
        {
            isDBNull = false;
            isIdentity = false;
            isKey = false;
            isAutoincrement = false;
            MaxLength = -1;
        }

        public string ColumnName { get; set; }

        public int DbColumnSize { get; set; }

        public int DbOrdinal { get; set; }

        public bool isAutoincrement { get; set; }

        public bool isDBNull { get; set; }

        public bool isIdentity { get; set; }

        public bool isKey { get; set; }

        public int MaxLength { get; set; }

        public List<MemberRubric> Rubrics { get; set; }

        public Type RubricType { get; set; }
    }
}
