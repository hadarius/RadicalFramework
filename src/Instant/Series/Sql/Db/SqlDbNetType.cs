namespace Radical.Instant.Series.Sql.Db
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Radical.Uniques;

    public static class SqlDbNetType
    {
        public static string NetTypeToSql(Type netType)
        {
            if (SqlDbNetTypes.SqlNetTypes.ContainsKey(netType))
                return SqlDbNetTypes.SqlNetTypes[netType];
            else
                return "varbinary";
        }

        public static object SqlNetVal(
            IInstant fieldRow,
            string fieldName,
            string prefix = "",
            string tableName = null
        )
        {
            object sqlNetVal = new object();
            try
            {
                CultureInfo cci = CultureInfo.CurrentCulture;
                string decRep = (cci.NumberFormat.NumberDecimalSeparator == ".") ? "," : ".";
                string decSep = cci.NumberFormat.NumberDecimalSeparator,
                    _tableName = "";
                if (tableName != null)
                    _tableName = tableName;
                else
                    _tableName = fieldRow.GetType().BaseType.Name;
                if (!SqlDbRegistry.Schema.Tables.Have(_tableName))
                    _tableName = prefix + _tableName;
                if (SqlDbRegistry.Schema.Tables.Have(_tableName))
                {
                    Type ft = SqlDbRegistry.Schema.Tables[_tableName].Columns[
                        fieldName + "#"
                    ].RubricType;

                    if (DBNull.Value != fieldRow[fieldName])
                    {
                        if (ft == typeof(decimal) || ft == typeof(float) || ft == typeof(double))
                            sqlNetVal = Convert.ChangeType(
                                fieldRow[fieldName].ToString().Replace(decRep, decSep),
                                ft
                            );
                        else if (ft == typeof(string))
                        {
                            int maxLength = SqlDbRegistry.Schema.Tables[_tableName].Columns[
                                fieldName + "#"
                            ].MaxLength;
                            if (fieldRow[fieldName].ToString().Length > maxLength)
                                sqlNetVal = Convert.ChangeType(
                                    fieldRow[fieldName].ToString().Substring(0, maxLength),
                                    ft
                                );
                            else
                                sqlNetVal = Convert.ChangeType(fieldRow[fieldName], ft);
                        }
                        else if (ft == typeof(long) && fieldRow[fieldName] is Usid)
                            sqlNetVal = ((Usid)fieldRow[fieldName]).Key;
                        else if (ft == typeof(byte[]) && fieldRow[fieldName] is Uscn)
                            sqlNetVal = ((Uscn)fieldRow[fieldName]).GetBytes();
                        else
                            sqlNetVal = Convert.ChangeType(fieldRow[fieldName], ft);
                    }
                    else
                    {
                        fieldRow[fieldName] = SqlDbNetTypes.SqlNetDefaults[ft];
                        sqlNetVal = Convert.ChangeType(fieldRow[fieldName], ft);
                    }
                }
                else
                {
                    sqlNetVal = fieldRow[fieldName];
                }
            }
            catch (Exception ex) { }
            return sqlNetVal;
        }

        public static Type SqlTypeToNet(string sqlType)
        {
            if (SqlDbNetTypes.SqlNetTypes.ContainsValue(sqlType))
                return SqlDbNetTypes.SqlNetTypes.Where(v => v.Value == sqlType).First().Key;
            else
                return typeof(object);
        }
    }
}
