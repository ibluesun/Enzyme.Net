using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enzyme
{
    public abstract class EnzymeContextBase: IEnzymeContext
    {
        #region IFieldsProvider Members

        public FieldHandle GetFieldHandle(string fieldName)
        {
            FieldHandle fh = new FieldHandle(fieldName, this);
            return fh;
        }

        public abstract object GetFieldValue(string fieldName);

        public object GetField(FieldHandle field)
        {
            return GetFieldValue(field.Name);
        }

        public abstract object SetFieldValue(string fieldName, object fieldValue);

        public object SetField(FieldHandle field, object fieldValue)
        {
            return SetFieldValue(field.Name, fieldValue);
        }

        #endregion
    }
}
