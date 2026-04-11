/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Chummer
{
    public static class ToolTipFactory
    {
        private static readonly ConcurrentDictionary<Form, HtmlToolTip> s_dicToolTipFactories = new ConcurrentDictionary<Form, HtmlToolTip>();

        private static void TryClearToolTips(object sender, EventArgs e)
        {
            if (sender is Form form && s_dicToolTipFactories.TryRemove(form, out HtmlToolTip objToolTip))
            {
                objToolTip.RemoveAll();
                objToolTip.Dispose();
            }
        }

        /// <summary>
        /// Get the ToolTip object we want to use for a given form.
        /// In order to prevent memory leaks through HtmlToolTip's implementation, we need to create a separate object for each form.
        /// When a form is disposed, we will then clear out and dispose the associated HtmlToolTip to prevent leaks through static references.
        /// </summary>
        [CLSCompliant(false)]
        public static HtmlToolTip GetToolTipForForm(Form form)
        {
            if (form == null)
                return null;
            return s_dicToolTipFactories.GetOrAdd(form, x =>
            {
                return x.DoThreadSafeFunc(y =>
                {
                    HtmlToolTip objReturn = new HtmlToolTip
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
                        UseGdiPlusTextRendering = true,
                        //UseAnimation = true,
                        //UseFading = true
                    };
                    y.Disposed += TryClearToolTips;
                    return objReturn;
                });
            });
        }

        /// <summary>
        /// Get the ToolTip object we want to use for a given form.
        /// In order to prevent memory leaks through HtmlToolTip's implementation, we need to create a separate object for each form.
        /// When a form is disposed, we will then clear out and dispose the associated HtmlToolTip to prevent leaks through static references.
        /// </summary>
        [CLSCompliant(false)]
        public static Task<HtmlToolTip> GetToolTipForFormAsync(Form form, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<HtmlToolTip>(token);
            if (form == null)
                return Task.FromResult<HtmlToolTip>(default);
            return s_dicToolTipFactories.GetOrAddAsync(form, x =>
            {
                return x.DoThreadSafeFuncAsync(y =>
                {
                    HtmlToolTip objReturn = new HtmlToolTip
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
                        UseGdiPlusTextRendering = true,
                        //UseAnimation = true,
                        //UseFading = true
                    };
                    y.Disposed += TryClearToolTips;
                    return objReturn;
                }, token);
            }, token);
        }

        public static void SetToolTip(this Control objControl, string strCaption)
        {
            if (objControl is IControlWithToolTip objCast)
                objCast.ToolTipText = strCaption;
            else
            {
                Form frmParent = objControl.DoThreadSafeFunc(x => x.FindForm());
                SetToolTip(objControl, frmParent, strCaption);
            }
        }

        public static void SetToolTip(this Control objControl, Form frmParent, string strCaption)
        {
            strCaption = strCaption.CleanForHtml();
            HtmlToolTip objToolTipFactory = GetToolTipForForm(frmParent);
            if (objToolTipFactory != null)
                objControl.DoThreadSafe(x => objToolTipFactory.SetToolTip(x, strCaption));
            else
                Utils.BreakIfDebug();
        }

        public static Task SetToolTipAsync(this Control objControl, string strCaption, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (objControl is IControlWithToolTip objCast)
                return objCast.SetToolTipTextAsync(strCaption, token);
            return Inner();
            async Task Inner()
            {
                Form frmParent = await objControl.DoThreadSafeFuncAsync(x => x.FindForm(), token).ConfigureAwait(false);
                await SetToolTipAsync(objControl, frmParent, strCaption, token).ConfigureAwait(false);
            }
        }

        public static async Task SetToolTipAsync(this Control c, Form frmParent, string strCaption, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            strCaption = strCaption.CleanForHtml();
            HtmlToolTip objToolTipFactory = await GetToolTipForFormAsync(frmParent, token).ConfigureAwait(false);
            if (objToolTipFactory != null)
                await c.DoThreadSafeAsync(x => objToolTipFactory.SetToolTip(x, strCaption), token).ConfigureAwait(false);
            else
                Utils.BreakIfDebug();
        }
    }
}
