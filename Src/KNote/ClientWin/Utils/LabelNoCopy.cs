using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;

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

        /// <summary>
        /// Draws the text on a single line, vertically centered and truncated with an ellipsis when it doesn't fit. The base Label
        /// word-wraps text that doesn't fit and centers the wrapped block, which shifts the first line upwards.
        /// </summary>
        [DefaultValue(false)]
        public bool SingleLine { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!SingleLine)
            {
                base.OnPaint(e);
                return;
            }

            // Vertical alignment is always centered; only the horizontal part of TextAlign is honored.
            var flags = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix
                | TextFormatFlags.EndEllipsis;
            if ((TextAlign & (ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter)) != 0)
                flags |= TextFormatFlags.HorizontalCenter;
            else if ((TextAlign & (ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight)) != 0)
                flags |= TextFormatFlags.Right;

            var color = Enabled ? ForeColor : SystemColors.GrayText;
            var bounds = new Rectangle(Padding.Left, Padding.Top,
                ClientSize.Width - Padding.Horizontal, ClientSize.Height - Padding.Vertical);

            TextRenderer.DrawText(e.Graphics, Text, Font, bounds, color, flags);
        }
    }
}
