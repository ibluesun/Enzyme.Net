using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enzyme
{
    public class EnzymeStaticContext : EnzymeContextBase
    {
        #region IFieldsProvider Members

        public override object GetFieldValue(string fieldName)
        {
            double val;
            if (double.TryParse(fieldName, out val)) return val;
            else return 0.0;
        }

        public override object SetFieldValue(string fieldName, object fieldValue)
        {
            return GetFieldValue(fieldName);
        }

        #endregion
    }
}
