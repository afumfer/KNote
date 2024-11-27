using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace System.Windows.Forms
{
    public class LabelNoCopy : Label
    {
        private string text;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }

                if (text != value)
                {
                    text = value;
                    Refresh();
                    OnTextChanged(EventArgs.Empty);
                }
            }
        }
    }
}
