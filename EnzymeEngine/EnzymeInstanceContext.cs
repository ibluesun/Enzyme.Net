using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enzyme
{
    public sealed class EnzymeInstanceContext : EnzymeContextBase
    {
        private readonly object _Instance;
        private readonly Type _InstanceType;

        public Type InstanceType
        {
            get { return _InstanceType; }
        } 

        public object Instance
        {
            get { return _Instance; }
        } 

        public EnzymeInstanceContext(object instance)
        {
            _Instance = instance;
            _InstanceType = _Instance.GetType();
        }

        public override object GetFieldValue(string fieldName)
        {
            var pinfo = _InstanceType.GetProperty(fieldName);

            return pinfo.GetValue(_Instance, null);
        }

        public override object SetFieldValue(string fieldName, object fieldValue)
        {
            var pinfo = _InstanceType.GetProperty(fieldName);

            pinfo.SetValue(_Instance, fieldValue, null);

            return fieldValue;
        }
    }
}
