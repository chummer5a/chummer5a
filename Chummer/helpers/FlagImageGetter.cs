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

namespace Chummer
{
    public static class FlagImageGetter
    {
        /// <summary>
        /// Returns a 16px x 11px image of a country/region's flag based on its two-letter, ISO-3166 code.
        /// </summary>
        /// <param name="strCode">The ISO-3166 code of the country/region's flag.</param>
        /// <returns>16px x 11px image of the country/region's flag if available, null otherwise</returns>
        public static System.Drawing.Image GetFlagFromCountryCode(string strCode)
        {
            System.Drawing.Image objReturn;
            if (String.IsNullOrEmpty(strCode))
                strCode = "";
            switch (strCode.ToLowerInvariant())
            {
                case "ad":
                    objReturn = Properties.Resources.ad;
                    break;
                case "ae":
                    objReturn = Properties.Resources.ae;
                    break;
                case "af":
                    objReturn = Properties.Resources.af;
                    break;
                case "ag":
                    objReturn = Properties.Resources.ag;
                    break;
                case "ai":
                    objReturn = Properties.Resources.ai;
                    break;
                case "al":
                    objReturn = Properties.Resources.al;
                    break;
                case "am":
                    objReturn = Properties.Resources.am;
                    break;
                case "an":
                    objReturn = Properties.Resources.an;
                    break;
                case "ao":
                    objReturn = Properties.Resources.ao;
                    break;
                case "ar":
                    objReturn = Properties.Resources.ar;
                    break;
                case "as":
                    objReturn = Properties.Resources._as;
                    break;
                case "at":
                    objReturn = Properties.Resources.at;
                    break;
                case "au":
                    objReturn = Properties.Resources.au;
                    break;
                case "aw":
                    objReturn = Properties.Resources.aw;
                    break;
                case "ax":
                    objReturn = Properties.Resources.ax;
                    break;
                case "az":
                    objReturn = Properties.Resources.az;
                    break;
                case "ba":
                    objReturn = Properties.Resources.ba;
                    break;
                case "bb":
                    objReturn = Properties.Resources.bb;
                    break;
                case "bd":
                    objReturn = Properties.Resources.bd;
                    break;
                case "be":
                    objReturn = Properties.Resources.be;
                    break;
                case "bf":
                    objReturn = Properties.Resources.bf;
                    break;
                case "bg":
                    objReturn = Properties.Resources.bg;
                    break;
                case "bh":
                    objReturn = Properties.Resources.bh;
                    break;
                case "bi":
                    objReturn = Properties.Resources.bi;
                    break;
                case "bj":
                    objReturn = Properties.Resources.bj;
                    break;
                case "bm":
                    objReturn = Properties.Resources.bm;
                    break;
                case "bn":
                    objReturn = Properties.Resources.bn;
                    break;
                case "bo":
                    objReturn = Properties.Resources.bo;
                    break;
                case "br":
                    objReturn = Properties.Resources.br;
                    break;
                case "bs":
                    objReturn = Properties.Resources.bs;
                    break;
                case "bt":
                    objReturn = Properties.Resources.bt;
                    break;
                case "bv":
                    objReturn = Properties.Resources.bv;
                    break;
                case "bw":
                    objReturn = Properties.Resources.bw;
                    break;
                case "by":
                    objReturn = Properties.Resources.by;
                    break;
                case "bz":
                    objReturn = Properties.Resources.bz;
                    break;
                case "ca":
                    objReturn = Properties.Resources.ca;
                    break;
                case "cc":
                    objReturn = Properties.Resources.cc;
                    break;
                case "cd":
                    objReturn = Properties.Resources.cd;
                    break;
                case "cf":
                    objReturn = Properties.Resources.cf;
                    break;
                case "cg":
                    objReturn = Properties.Resources.cg;
                    break;
                case "ch":
                    objReturn = Properties.Resources.ch;
                    break;
                case "ci":
                    objReturn = Properties.Resources.ci;
                    break;
                case "ck":
                    objReturn = Properties.Resources.ck;
                    break;
                case "cl":
                    objReturn = Properties.Resources.cl;
                    break;
                case "cm":
                    objReturn = Properties.Resources.cm;
                    break;
                case "cn":
                    objReturn = Properties.Resources.cn;
                    break;
                case "co":
                    objReturn = Properties.Resources.co;
                    break;
                case "cr":
                    objReturn = Properties.Resources.cr;
                    break;
                case "cs":
                    objReturn = Properties.Resources.cs;
                    break;
                case "ct":
                    objReturn = Properties.Resources.ct;
                    break;
                case "cu":
                    objReturn = Properties.Resources.cu;
                    break;
                case "cv":
                    objReturn = Properties.Resources.cv;
                    break;
                case "cx":
                    objReturn = Properties.Resources.cx;
                    break;
                case "cy":
                    objReturn = Properties.Resources.cy;
                    break;
                case "cz":
                    objReturn = Properties.Resources.cz;
                    break;
                case "de":
                    objReturn = Properties.Resources.de;
                    break;
                case "dj":
                    objReturn = Properties.Resources.dj;
                    break;
                case "dk":
                    objReturn = Properties.Resources.dk;
                    break;
                case "dm":
                    objReturn = Properties.Resources.dm;
                    break;
                case "do":
                    objReturn = Properties.Resources._do;
                    break;
                case "dz":
                    objReturn = Properties.Resources.dz;
                    break;
                case "ec":
                    objReturn = Properties.Resources.ec;
                    break;
                case "ee":
                    objReturn = Properties.Resources.ee;
                    break;
                case "eg":
                    objReturn = Properties.Resources.eg;
                    break;
                case "eh":
                    objReturn = Properties.Resources.eh;
                    break;
                case "er":
                    objReturn = Properties.Resources.er;
                    break;
                case "es":
                    objReturn = Properties.Resources.es;
                    break;
                case "et":
                    objReturn = Properties.Resources.et;
                    break;
                case "eu":
                    objReturn = Properties.Resources.eu;
                    break;
                case "fi":
                    objReturn = Properties.Resources.fi;
                    break;
                case "fj":
                    objReturn = Properties.Resources.fj;
                    break;
                case "fk":
                    objReturn = Properties.Resources.fk;
                    break;
                case "fm":
                    objReturn = Properties.Resources.fm;
                    break;
                case "fo":
                    objReturn = Properties.Resources.fo;
                    break;
                case "fr":
                    objReturn = Properties.Resources.fr;
                    break;
                case "ga":
                    objReturn = Properties.Resources.ga;
                    break;
                case "gb":
                    objReturn = Properties.Resources.gb;
                    break;
                case "gd":
                    objReturn = Properties.Resources.gd;
                    break;
                case "ge":
                    objReturn = Properties.Resources.ge;
                    break;
                case "gf":
                    objReturn = Properties.Resources.gf;
                    break;
                case "gh":
                    objReturn = Properties.Resources.gh;
                    break;
                case "gi":
                    objReturn = Properties.Resources.gi;
                    break;
                case "gl":
                    objReturn = Properties.Resources.gl;
                    break;
                case "gm":
                    objReturn = Properties.Resources.gm;
                    break;
                case "gn":
                    objReturn = Properties.Resources.gn;
                    break;
                case "gp":
                    objReturn = Properties.Resources.gp;
                    break;
                case "gq":
                    objReturn = Properties.Resources.gq;
                    break;
                case "gr":
                    objReturn = Properties.Resources.gr;
                    break;
                case "gs":
                    objReturn = Properties.Resources.gs;
                    break;
                case "gt":
                    objReturn = Properties.Resources.gt;
                    break;
                case "gu":
                    objReturn = Properties.Resources.gu;
                    break;
                case "gw":
                    objReturn = Properties.Resources.gw;
                    break;
                case "gy":
                    objReturn = Properties.Resources.gy;
                    break;
                case "hk":
                    objReturn = Properties.Resources.hk;
                    break;
                case "hm":
                    objReturn = Properties.Resources.hm;
                    break;
                case "hn":
                    objReturn = Properties.Resources.hn;
                    break;
                case "hr":
                    objReturn = Properties.Resources.hr;
                    break;
                case "ht":
                    objReturn = Properties.Resources.ht;
                    break;
                case "hu":
                    objReturn = Properties.Resources.hu;
                    break;
                case "id":
                    objReturn = Properties.Resources.id;
                    break;
                case "ie":
                    objReturn = Properties.Resources.ie;
                    break;
                case "il":
                    objReturn = Properties.Resources.il;
                    break;
                case "in":
                    objReturn = Properties.Resources._in;
                    break;
                case "io":
                    objReturn = Properties.Resources.io;
                    break;
                case "iq":
                    objReturn = Properties.Resources.iq;
                    break;
                case "ir":
                    objReturn = Properties.Resources.ir;
                    break;
                case "is":
                    objReturn = Properties.Resources._is;
                    break;
                case "it":
                    objReturn = Properties.Resources.it;
                    break;
                case "jm":
                    objReturn = Properties.Resources.jm;
                    break;
                case "jo":
                    objReturn = Properties.Resources.jo;
                    break;
                case "jp":
                    objReturn = Properties.Resources.jp;
                    break;
                case "ke":
                    objReturn = Properties.Resources.ke;
                    break;
                case "kg":
                    objReturn = Properties.Resources.kg;
                    break;
                case "kh":
                    objReturn = Properties.Resources.kh;
                    break;
                case "ki":
                    objReturn = Properties.Resources.ki;
                    break;
                case "km":
                    objReturn = Properties.Resources.km;
                    break;
                case "kn":
                    objReturn = Properties.Resources.kn;
                    break;
                case "kp":
                    objReturn = Properties.Resources.kp;
                    break;
                case "kr":
                    objReturn = Properties.Resources.kr;
                    break;
                case "kw":
                    objReturn = Properties.Resources.kw;
                    break;
                case "ky":
                    objReturn = Properties.Resources.ky;
                    break;
                case "kz":
                    objReturn = Properties.Resources.kz;
                    break;
                case "la":
                    objReturn = Properties.Resources.la;
                    break;
                case "lb":
                    objReturn = Properties.Resources.lb;
                    break;
                case "lc":
                    objReturn = Properties.Resources.lc;
                    break;
                case "li":
                    objReturn = Properties.Resources.li;
                    break;
                case "lk":
                    objReturn = Properties.Resources.lk;
                    break;
                case "lr":
                    objReturn = Properties.Resources.lr;
                    break;
                case "ls":
                    objReturn = Properties.Resources.ls;
                    break;
                case "lt":
                    objReturn = Properties.Resources.lt;
                    break;
                case "lu":
                    objReturn = Properties.Resources.lu;
                    break;
                case "lv":
                    objReturn = Properties.Resources.lv;
                    break;
                case "ly":
                    objReturn = Properties.Resources.ly;
                    break;
                case "ma":
                    objReturn = Properties.Resources.ma;
                    break;
                case "mc":
                    objReturn = Properties.Resources.mc;
                    break;
                case "md":
                    objReturn = Properties.Resources.md;
                    break;
                case "me":
                    objReturn = Properties.Resources.me;
                    break;
                case "mg":
                    objReturn = Properties.Resources.mg;
                    break;
                case "mh":
                    objReturn = Properties.Resources.mh;
                    break;
                case "mk":
                    objReturn = Properties.Resources.mk;
                    break;
                case "ml":
                    objReturn = Properties.Resources.ml;
                    break;
                case "mm":
                    objReturn = Properties.Resources.mm;
                    break;
                case "mn":
                    objReturn = Properties.Resources.mn;
                    break;
                case "mo":
                    objReturn = Properties.Resources.mo;
                    break;
                case "mp":
                    objReturn = Properties.Resources.mp;
                    break;
                case "mq":
                    objReturn = Properties.Resources.mq;
                    break;
                case "mr":
                    objReturn = Properties.Resources.mr;
                    break;
                case "ms":
                    objReturn = Properties.Resources.ms;
                    break;
                case "mt":
                    objReturn = Properties.Resources.mt;
                    break;
                case "mu":
                    objReturn = Properties.Resources.mu;
                    break;
                case "mv":
                    objReturn = Properties.Resources.mv;
                    break;
                case "mw":
                    objReturn = Properties.Resources.mw;
                    break;
                case "mx":
                    objReturn = Properties.Resources.mx;
                    break;
                case "my":
                    objReturn = Properties.Resources.my;
                    break;
                case "mz":
                    objReturn = Properties.Resources.mz;
                    break;
                case "na":
                    objReturn = Properties.Resources.na;
                    break;
                case "nc":
                    objReturn = Properties.Resources.nc;
                    break;
                case "ne":
                    objReturn = Properties.Resources.ne;
                    break;
                case "nf":
                    objReturn = Properties.Resources.nf;
                    break;
                case "ng":
                    objReturn = Properties.Resources.ng;
                    break;
                case "ni":
                    objReturn = Properties.Resources.ni;
                    break;
                case "nl":
                    objReturn = Properties.Resources.nl;
                    break;
                case "no":
                    objReturn = Properties.Resources.no;
                    break;
                case "np":
                    objReturn = Properties.Resources.np;
                    break;
                case "nr":
                    objReturn = Properties.Resources.nr;
                    break;
                case "nu":
                    objReturn = Properties.Resources.nu;
                    break;
                case "nz":
                    objReturn = Properties.Resources.nz;
                    break;
                case "om":
                    objReturn = Properties.Resources.om;
                    break;
                case "pa":
                    objReturn = Properties.Resources.pa;
                    break;
                case "pe":
                    objReturn = Properties.Resources.pe;
                    break;
                case "pf":
                    objReturn = Properties.Resources.pf;
                    break;
                case "pg":
                    objReturn = Properties.Resources.pg;
                    break;
                case "ph":
                    objReturn = Properties.Resources.ph;
                    break;
                case "pk":
                    objReturn = Properties.Resources.pk;
                    break;
                case "pl":
                    objReturn = Properties.Resources.pl;
                    break;
                case "pm":
                    objReturn = Properties.Resources.pm;
                    break;
                case "pn":
                    objReturn = Properties.Resources.pn;
                    break;
                case "pr":
                    objReturn = Properties.Resources.pr;
                    break;
                case "ps":
                    objReturn = Properties.Resources.ps;
                    break;
                case "pt":
                    objReturn = Properties.Resources.pt;
                    break;
                case "pw":
                    objReturn = Properties.Resources.pw;
                    break;
                case "py":
                    objReturn = Properties.Resources.py;
                    break;
                case "qa":
                    objReturn = Properties.Resources.qa;
                    break;
                case "re":
                    objReturn = Properties.Resources.re;
                    break;
                case "ro":
                    objReturn = Properties.Resources.ro;
                    break;
                case "rs":
                    objReturn = Properties.Resources.rs;
                    break;
                case "ru":
                    objReturn = Properties.Resources.ru;
                    break;
                case "rw":
                    objReturn = Properties.Resources.rw;
                    break;
                case "sa":
                    objReturn = Properties.Resources.sa;
                    break;
                case "sb":
                    objReturn = Properties.Resources.sb;
                    break;
                case "sc":
                    objReturn = Properties.Resources.sc;
                    break;
                case "sd":
                    objReturn = Properties.Resources.sd;
                    break;
                case "se":
                    objReturn = Properties.Resources.se;
                    break;
                case "sg":
                    objReturn = Properties.Resources.sg;
                    break;
                case "sh":
                    objReturn = Properties.Resources.sh;
                    break;
                case "si":
                    objReturn = Properties.Resources.si;
                    break;
                case "sj":
                    objReturn = Properties.Resources.sj;
                    break;
                case "sk":
                    objReturn = Properties.Resources.sk;
                    break;
                case "sl":
                    objReturn = Properties.Resources.sl;
                    break;
                case "sm":
                    objReturn = Properties.Resources.sm;
                    break;
                case "sn":
                    objReturn = Properties.Resources.sn;
                    break;
                case "so":
                    objReturn = Properties.Resources.so;
                    break;
                case "sr":
                    objReturn = Properties.Resources.sr;
                    break;
                case "st":
                    objReturn = Properties.Resources.st;
                    break;
                case "sv":
                    objReturn = Properties.Resources.sv;
                    break;
                case "sy":
                    objReturn = Properties.Resources.sy;
                    break;
                case "sz":
                    objReturn = Properties.Resources.sz;
                    break;
                case "tc":
                    objReturn = Properties.Resources.tc;
                    break;
                case "td":
                    objReturn = Properties.Resources.td;
                    break;
                case "tf":
                    objReturn = Properties.Resources.tf;
                    break;
                case "tg":
                    objReturn = Properties.Resources.tg;
                    break;
                case "th":
                    objReturn = Properties.Resources.th;
                    break;
                case "tj":
                    objReturn = Properties.Resources.tj;
                    break;
                case "tk":
                    objReturn = Properties.Resources.tk;
                    break;
                case "tl":
                    objReturn = Properties.Resources.tl;
                    break;
                case "tm":
                    objReturn = Properties.Resources.tm;
                    break;
                case "tn":
                    objReturn = Properties.Resources.tn;
                    break;
                case "to":
                    objReturn = Properties.Resources.to;
                    break;
                case "tr":
                    objReturn = Properties.Resources.tr;
                    break;
                case "tt":
                    objReturn = Properties.Resources.tt;
                    break;
                case "tv":
                    objReturn = Properties.Resources.tv;
                    break;
                case "tw":
                    objReturn = Properties.Resources.tw;
                    break;
                case "tz":
                    objReturn = Properties.Resources.tz;
                    break;
                case "ua":
                    objReturn = Properties.Resources.ua;
                    break;
                case "ug":
                    objReturn = Properties.Resources.ug;
                    break;
                case "um":
                    objReturn = Properties.Resources.um;
                    break;
                case "us":
                    objReturn = Properties.Resources.us;
                    break;
                case "uy":
                    objReturn = Properties.Resources.uy;
                    break;
                case "uz":
                    objReturn = Properties.Resources.uz;
                    break;
                case "va":
                    objReturn = Properties.Resources.va;
                    break;
                case "vc":
                    objReturn = Properties.Resources.vc;
                    break;
                case "ve":
                    objReturn = Properties.Resources.ve;
                    break;
                case "vg":
                    objReturn = Properties.Resources.vg;
                    break;
                case "vi":
                    objReturn = Properties.Resources.vi;
                    break;
                case "vn":
                    objReturn = Properties.Resources.vn;
                    break;
                case "vu":
                    objReturn = Properties.Resources.vu;
                    break;
                case "wf":
                    objReturn = Properties.Resources.wf;
                    break;
                case "ws":
                    objReturn = Properties.Resources.ws;
                    break;
                case "xe":
                    objReturn = Properties.Resources.xe;
                    break;
                case "xs":
                    objReturn = Properties.Resources.xs;
                    break;
                case "xw":
                    objReturn = Properties.Resources.xw;
                    break;
                case "ye":
                    objReturn = Properties.Resources.ye;
                    break;
                case "yt":
                    objReturn = Properties.Resources.yt;
                    break;
                case "za":
                    objReturn = Properties.Resources.za;
                    break;
                case "zm":
                    objReturn = Properties.Resources.zm;
                    break;
                case "zw":
                    objReturn = Properties.Resources.zw;
                    break;
                case "default":
                    objReturn = Properties.Resources.defaulted;
                    break;
                case "noimagedots":
                    objReturn = Properties.Resources.noimagedots;
                    break;
                    
                default:
                    Utils.BreakIfDebug();
                    objReturn = Properties.Resources.defaulted;
                    break;
            }
            return objReturn;
        }
    }
}
