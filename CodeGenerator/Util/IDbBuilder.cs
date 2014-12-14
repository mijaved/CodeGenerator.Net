using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CodeGenerator.MVP.Util
{
    public interface IDbBuilder
    {
        StringBuilder BuildSequence(string tableName);
        StringBuilder BuildSaveProcedure(string strTableName, DataTable dt);
        StringBuilder BuildGetProcedure(string strTableName, DataTable dt);
        StringBuilder BuildGetProcedureSingle(string strTableName, DataTable dt);
        StringBuilder BuildGetProcedureParameterized(string strTableName, DataTable dt);
    }
}
