using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enzyme
{
    /// <summary>
    /// Dynamic Field Provider that accept any sort of fields
    /// </summary>
    public class EnzymeDynamicContext : EnzymeContextBase
    {
        private Dictionary<string, object> Fields = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        
        #region IFieldsProvider Members

        public override object GetFieldValue(string fieldName)
        {
            object value;

            Fields.TryGetValue(fieldName, out value);

            return value;
        }

        public override object SetFieldValue(string fieldName, object fieldValue)
        {
            Fields[fieldName] = fieldValue;
            return fieldValue;
        }

        #endregion

    }
}
