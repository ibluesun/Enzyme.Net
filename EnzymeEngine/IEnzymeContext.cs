using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enzyme
{

    public interface IEnzymeContext
    {
        FieldHandle GetFieldHandle(string fieldName);


        object GetFieldValue(string fieldName);

        object GetField(FieldHandle field);



        object SetFieldValue(string fieldName, object fieldValue);

        object SetField(FieldHandle field, object fieldValue);

    }
}
