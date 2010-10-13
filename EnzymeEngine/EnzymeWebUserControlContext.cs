using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Globalization;

namespace Enzyme
{
    public class EnzymeWebUserControlContext : EnzymeContextBase
    {
        private readonly System.Web.UI.UserControl _Instance;
        private readonly Type _InstanceType;

        public Type InstanceType
        {
            get { return _InstanceType; }
        }

        public System.Web.UI.UserControl Instance
        {
            get { return _Instance; }
        }

        public EnzymeWebUserControlContext(System.Web.UI.UserControl instance)
        {
            _Instance = instance;
            _InstanceType = _Instance.GetType();
        }

        public override object GetFieldValue(string fieldName)
        {
            var control = _Instance.FindControl(fieldName) as WebControl;


            if (control is TextBox)
            {
                return ((TextBox)control).Text;
            }
            else if (control is Label)
            {
                return ((Label)control).Text;
            }
            else if (control is DropDownList)
            {
                return ((DropDownList)control).SelectedValue;
            }
            else
            {
                throw new UnRecognizedException("The control type " + control.GetType() + " wasn't identified");
            }

        }

        public override object SetFieldValue(string fieldName, object fieldValue)
        {
            var control = _Instance.FindControl(fieldName) as WebControl;


            if (control is TextBox)
            {
                ((TextBox)control).Text = fieldValue.ToString();
            }
            else if (control is Label)
            {
                ((Label)control).Text = fieldValue.ToString();
            }
            else if (control is DropDownList)
            {
                ((DropDownList)control).SelectedValue = fieldValue.ToString();
            }
            else
            {
                throw new UnRecognizedException("The control type " + control.GetType() + " wasn't identified");
            }


            return fieldValue;
        }
    }
}
