using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enzyme
{
    public struct FieldHandle
    {
        public string Name;
        public IEnzymeContext Context;

        public FieldHandle(string name, IEnzymeContext context)
        {
            Name = name;
            Context = context;
        }

        public object GetValue()
        {
            return Context.GetFieldValue(Name);
        }

        public object SetValue(object value)
        {
            return Context.SetFieldValue(Name, value);
        }

    }
}
