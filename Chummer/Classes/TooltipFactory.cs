using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Chummer
{
    public static class ToolTipFactory
    {
        private static HtmlToolTip _tp;
        public static HtmlToolTip ToolTip { get; } = _tp ?? (_tp = new HtmlToolTip
        {
            AllowLinksHandling = true,
            AutoPopDelay = 3600000,
            BaseStylesheet = null,
            InitialDelay = 250,
            IsBalloon = false,
            MaximumSize = new System.Drawing.Size(0, 0),
            OwnerDraw = true,
            ReshowDelay = 100,
            TooltipCssClass = "htmltooltip",
            //UseAnimation = true,
            //UseFading = true
        });

        public static void SetToolTip(this Control c, string caption)
        {
            ToolTip.SetToolTip(c, caption);
        }
    }
}
