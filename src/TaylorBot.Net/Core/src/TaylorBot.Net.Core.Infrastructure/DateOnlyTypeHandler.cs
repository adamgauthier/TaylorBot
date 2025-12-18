using Dapper;
using System.Data;

namespace TaylorBot.Net.Core.Infrastructure;

// Dapper doesn't have built-in support for DateOnly
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => value is DateOnly d ? d : DateOnly.FromDateTime((DateTime)value);

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }
}
