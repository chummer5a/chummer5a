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
            if (string.IsNullOrEmpty(strCode))
                strCode = string.Empty;
            switch (strCode.ToUpperInvariant())
            {
                case "AD":
                    objReturn = Properties.Resources.ad;
                    break;
                case "AE":
                    objReturn = Properties.Resources.ae;
                    break;
                case "AF":
                    objReturn = Properties.Resources.af;
                    break;
                case "AG":
                    objReturn = Properties.Resources.ag;
                    break;
                case "AI":
                    objReturn = Properties.Resources.ai;
                    break;
                case "AL":
                    objReturn = Properties.Resources.al;
                    break;
                case "AM":
                    objReturn = Properties.Resources.am;
                    break;
                case "AN":
                    objReturn = Properties.Resources.an;
                    break;
                case "AO":
                    objReturn = Properties.Resources.ao;
                    break;
                case "AR":
                    objReturn = Properties.Resources.ar;
                    break;
                case "AS":
                    objReturn = Properties.Resources._as;
                    break;
                case "AT":
                    objReturn = Properties.Resources.at;
                    break;
                case "AU":
                    objReturn = Properties.Resources.au;
                    break;
                case "AW":
                    objReturn = Properties.Resources.aw;
                    break;
                case "AX":
                    objReturn = Properties.Resources.ax;
                    break;
                case "AZ":
                    objReturn = Properties.Resources.az;
                    break;
                case "BA":
                    objReturn = Properties.Resources.ba;
                    break;
                case "BB":
                    objReturn = Properties.Resources.bb;
                    break;
                case "BD":
                    objReturn = Properties.Resources.bd;
                    break;
                case "BE":
                    objReturn = Properties.Resources.be;
                    break;
                case "BF":
                    objReturn = Properties.Resources.bf;
                    break;
                case "BG":
                    objReturn = Properties.Resources.bg;
                    break;
                case "BH":
                    objReturn = Properties.Resources.bh;
                    break;
                case "BI":
                    objReturn = Properties.Resources.bi;
                    break;
                case "BJ":
                    objReturn = Properties.Resources.bj;
                    break;
                case "BM":
                    objReturn = Properties.Resources.bm;
                    break;
                case "BN":
                    objReturn = Properties.Resources.bn;
                    break;
                case "BO":
                    objReturn = Properties.Resources.bo;
                    break;
                case "BR":
                    objReturn = Properties.Resources.br;
                    break;
                case "BS":
                    objReturn = Properties.Resources.bs;
                    break;
                case "BT":
                    objReturn = Properties.Resources.bt;
                    break;
                case "BV":
                    objReturn = Properties.Resources.bv;
                    break;
                case "BW":
                    objReturn = Properties.Resources.bw;
                    break;
                case "BY":
                    objReturn = Properties.Resources.by;
                    break;
                case "BZ":
                    objReturn = Properties.Resources.bz;
                    break;
                case "CA":
                    objReturn = Properties.Resources.ca;
                    break;
                case "CC":
                    objReturn = Properties.Resources.cc;
                    break;
                case "CD":
                    objReturn = Properties.Resources.cd;
                    break;
                case "CF":
                    objReturn = Properties.Resources.cf;
                    break;
                case "CG":
                    objReturn = Properties.Resources.cg;
                    break;
                case "CH":
                    objReturn = Properties.Resources.ch;
                    break;
                case "CI":
                    objReturn = Properties.Resources.ci;
                    break;
                case "CK":
                    objReturn = Properties.Resources.ck;
                    break;
                case "CL":
                    objReturn = Properties.Resources.cl;
                    break;
                case "CM":
                    objReturn = Properties.Resources.cm;
                    break;
                case "CN":
                    objReturn = Properties.Resources.cn;
                    break;
                case "CO":
                    objReturn = Properties.Resources.co;
                    break;
                case "CR":
                    objReturn = Properties.Resources.cr;
                    break;
                case "CS":
                    objReturn = Properties.Resources.cs;
                    break;
                case "CT":
                    objReturn = Properties.Resources.ct;
                    break;
                case "CU":
                    objReturn = Properties.Resources.cu;
                    break;
                case "CV":
                    objReturn = Properties.Resources.cv;
                    break;
                case "CX":
                    objReturn = Properties.Resources.cx;
                    break;
                case "CY":
                    objReturn = Properties.Resources.cy;
                    break;
                case "CZ":
                    objReturn = Properties.Resources.cz;
                    break;
                case "DE":
                    objReturn = Properties.Resources.de;
                    break;
                case "DJ":
                    objReturn = Properties.Resources.dj;
                    break;
                case "DK":
                    objReturn = Properties.Resources.dk;
                    break;
                case "DM":
                    objReturn = Properties.Resources.dm;
                    break;
                case "DO":
                    objReturn = Properties.Resources._do;
                    break;
                case "DZ":
                    objReturn = Properties.Resources.dz;
                    break;
                case "EC":
                    objReturn = Properties.Resources.ec;
                    break;
                case "EE":
                    objReturn = Properties.Resources.ee;
                    break;
                case "EG":
                    objReturn = Properties.Resources.eg;
                    break;
                case "EH":
                    objReturn = Properties.Resources.eh;
                    break;
                case "ER":
                    objReturn = Properties.Resources.er;
                    break;
                case "ES":
                    objReturn = Properties.Resources.es;
                    break;
                case "ET":
                    objReturn = Properties.Resources.et;
                    break;
                case "EU":
                    objReturn = Properties.Resources.eu;
                    break;
                case "FI":
                    objReturn = Properties.Resources.fi;
                    break;
                case "FJ":
                    objReturn = Properties.Resources.fj;
                    break;
                case "FK":
                    objReturn = Properties.Resources.fk;
                    break;
                case "FM":
                    objReturn = Properties.Resources.fm;
                    break;
                case "FO":
                    objReturn = Properties.Resources.fo;
                    break;
                case "FR":
                    objReturn = Properties.Resources.fr;
                    break;
                case "GA":
                    objReturn = Properties.Resources.ga;
                    break;
                case "GB":
                    objReturn = Properties.Resources.gb;
                    break;
                case "GD":
                    objReturn = Properties.Resources.gd;
                    break;
                case "GE":
                    objReturn = Properties.Resources.ge;
                    break;
                case "GF":
                    objReturn = Properties.Resources.gf;
                    break;
                case "GH":
                    objReturn = Properties.Resources.gh;
                    break;
                case "GI":
                    objReturn = Properties.Resources.gi;
                    break;
                case "GL":
                    objReturn = Properties.Resources.gl;
                    break;
                case "GM":
                    objReturn = Properties.Resources.gm;
                    break;
                case "GN":
                    objReturn = Properties.Resources.gn;
                    break;
                case "GP":
                    objReturn = Properties.Resources.gp;
                    break;
                case "GQ":
                    objReturn = Properties.Resources.gq;
                    break;
                case "GR":
                    objReturn = Properties.Resources.gr;
                    break;
                case "GS":
                    objReturn = Properties.Resources.gs;
                    break;
                case "GT":
                    objReturn = Properties.Resources.gt;
                    break;
                case "GU":
                    objReturn = Properties.Resources.gu;
                    break;
                case "GW":
                    objReturn = Properties.Resources.gw;
                    break;
                case "GY":
                    objReturn = Properties.Resources.gy;
                    break;
                case "HK":
                    objReturn = Properties.Resources.hk;
                    break;
                case "HM":
                    objReturn = Properties.Resources.hm;
                    break;
                case "HN":
                    objReturn = Properties.Resources.hn;
                    break;
                case "HR":
                    objReturn = Properties.Resources.hr;
                    break;
                case "HT":
                    objReturn = Properties.Resources.ht;
                    break;
                case "HU":
                    objReturn = Properties.Resources.hu;
                    break;
                case "ID":
                    objReturn = Properties.Resources.id;
                    break;
                case "IE":
                    objReturn = Properties.Resources.ie;
                    break;
                case "IL":
                    objReturn = Properties.Resources.il;
                    break;
                case "IN":
                    objReturn = Properties.Resources._in;
                    break;
                case "IO":
                    objReturn = Properties.Resources.io;
                    break;
                case "IQ":
                    objReturn = Properties.Resources.iq;
                    break;
                case "IR":
                    objReturn = Properties.Resources.ir;
                    break;
                case "IS":
                    objReturn = Properties.Resources._is;
                    break;
                case "IT":
                    objReturn = Properties.Resources.it;
                    break;
                case "JM":
                    objReturn = Properties.Resources.jm;
                    break;
                case "JO":
                    objReturn = Properties.Resources.jo;
                    break;
                case "JP":
                    objReturn = Properties.Resources.jp;
                    break;
                case "KE":
                    objReturn = Properties.Resources.ke;
                    break;
                case "KG":
                    objReturn = Properties.Resources.kg;
                    break;
                case "KH":
                    objReturn = Properties.Resources.kh;
                    break;
                case "KI":
                    objReturn = Properties.Resources.ki;
                    break;
                case "KM":
                    objReturn = Properties.Resources.km;
                    break;
                case "KN":
                    objReturn = Properties.Resources.kn;
                    break;
                case "KP":
                    objReturn = Properties.Resources.kp;
                    break;
                case "KR":
                    objReturn = Properties.Resources.kr;
                    break;
                case "KW":
                    objReturn = Properties.Resources.kw;
                    break;
                case "KY":
                    objReturn = Properties.Resources.ky;
                    break;
                case "KZ":
                    objReturn = Properties.Resources.kz;
                    break;
                case "LA":
                    objReturn = Properties.Resources.la;
                    break;
                case "LB":
                    objReturn = Properties.Resources.lb;
                    break;
                case "LC":
                    objReturn = Properties.Resources.lc;
                    break;
                case "LI":
                    objReturn = Properties.Resources.li;
                    break;
                case "LK":
                    objReturn = Properties.Resources.lk;
                    break;
                case "LR":
                    objReturn = Properties.Resources.lr;
                    break;
                case "LS":
                    objReturn = Properties.Resources.ls;
                    break;
                case "LT":
                    objReturn = Properties.Resources.lt;
                    break;
                case "LU":
                    objReturn = Properties.Resources.lu;
                    break;
                case "LV":
                    objReturn = Properties.Resources.lv;
                    break;
                case "LY":
                    objReturn = Properties.Resources.ly;
                    break;
                case "MA":
                    objReturn = Properties.Resources.ma;
                    break;
                case "MC":
                    objReturn = Properties.Resources.mc;
                    break;
                case "MD":
                    objReturn = Properties.Resources.md;
                    break;
                case "ME":
                    objReturn = Properties.Resources.me;
                    break;
                case "MG":
                    objReturn = Properties.Resources.mg;
                    break;
                case "MH":
                    objReturn = Properties.Resources.mh;
                    break;
                case "MK":
                    objReturn = Properties.Resources.mk;
                    break;
                case "ML":
                    objReturn = Properties.Resources.ml;
                    break;
                case "MM":
                    objReturn = Properties.Resources.mm;
                    break;
                case "MN":
                    objReturn = Properties.Resources.mn;
                    break;
                case "MO":
                    objReturn = Properties.Resources.mo;
                    break;
                case "MP":
                    objReturn = Properties.Resources.mp;
                    break;
                case "MQ":
                    objReturn = Properties.Resources.mq;
                    break;
                case "MR":
                    objReturn = Properties.Resources.mr;
                    break;
                case "MS":
                    objReturn = Properties.Resources.ms;
                    break;
                case "MT":
                    objReturn = Properties.Resources.mt;
                    break;
                case "MU":
                    objReturn = Properties.Resources.mu;
                    break;
                case "MV":
                    objReturn = Properties.Resources.mv;
                    break;
                case "MW":
                    objReturn = Properties.Resources.mw;
                    break;
                case "MX":
                    objReturn = Properties.Resources.mx;
                    break;
                case "MY":
                    objReturn = Properties.Resources.my;
                    break;
                case "MZ":
                    objReturn = Properties.Resources.mz;
                    break;
                case "NA":
                    objReturn = Properties.Resources.na;
                    break;
                case "NC":
                    objReturn = Properties.Resources.nc;
                    break;
                case "NE":
                    objReturn = Properties.Resources.ne;
                    break;
                case "NF":
                    objReturn = Properties.Resources.nf;
                    break;
                case "NG":
                    objReturn = Properties.Resources.ng;
                    break;
                case "NI":
                    objReturn = Properties.Resources.ni;
                    break;
                case "NL":
                    objReturn = Properties.Resources.nl;
                    break;
                case "NO":
                    objReturn = Properties.Resources.no;
                    break;
                case "NP":
                    objReturn = Properties.Resources.np;
                    break;
                case "NR":
                    objReturn = Properties.Resources.nr;
                    break;
                case "NU":
                    objReturn = Properties.Resources.nu;
                    break;
                case "NZ":
                    objReturn = Properties.Resources.nz;
                    break;
                case "OM":
                    objReturn = Properties.Resources.om;
                    break;
                case "PA":
                    objReturn = Properties.Resources.pa;
                    break;
                case "PE":
                    objReturn = Properties.Resources.pe;
                    break;
                case "PF":
                    objReturn = Properties.Resources.pf;
                    break;
                case "PG":
                    objReturn = Properties.Resources.pg;
                    break;
                case "PH":
                    objReturn = Properties.Resources.ph;
                    break;
                case "PK":
                    objReturn = Properties.Resources.pk;
                    break;
                case "PL":
                    objReturn = Properties.Resources.pl;
                    break;
                case "PM":
                    objReturn = Properties.Resources.pm;
                    break;
                case "PN":
                    objReturn = Properties.Resources.pn;
                    break;
                case "PR":
                    objReturn = Properties.Resources.pr;
                    break;
                case "PS":
                    objReturn = Properties.Resources.ps;
                    break;
                case "PT":
                    objReturn = Properties.Resources.pt;
                    break;
                case "PW":
                    objReturn = Properties.Resources.pw;
                    break;
                case "PY":
                    objReturn = Properties.Resources.py;
                    break;
                case "QA":
                    objReturn = Properties.Resources.qa;
                    break;
                case "RE":
                    objReturn = Properties.Resources.re;
                    break;
                case "RO":
                    objReturn = Properties.Resources.ro;
                    break;
                case "RS":
                    objReturn = Properties.Resources.rs;
                    break;
                case "RU":
                    objReturn = Properties.Resources.ru;
                    break;
                case "RW":
                    objReturn = Properties.Resources.rw;
                    break;
                case "SA":
                    objReturn = Properties.Resources.sa;
                    break;
                case "SB":
                    objReturn = Properties.Resources.sb;
                    break;
                case "SC":
                    objReturn = Properties.Resources.sc;
                    break;
                case "SD":
                    objReturn = Properties.Resources.sd;
                    break;
                case "SE":
                    objReturn = Properties.Resources.se;
                    break;
                case "SG":
                    objReturn = Properties.Resources.sg;
                    break;
                case "SH":
                    objReturn = Properties.Resources.sh;
                    break;
                case "SI":
                    objReturn = Properties.Resources.si;
                    break;
                case "SJ":
                    objReturn = Properties.Resources.sj;
                    break;
                case "SK":
                    objReturn = Properties.Resources.sk;
                    break;
                case "SL":
                    objReturn = Properties.Resources.sl;
                    break;
                case "SM":
                    objReturn = Properties.Resources.sm;
                    break;
                case "SN":
                    objReturn = Properties.Resources.sn;
                    break;
                case "SO":
                    objReturn = Properties.Resources.so;
                    break;
                case "SR":
                    objReturn = Properties.Resources.sr;
                    break;
                case "ST":
                    objReturn = Properties.Resources.st;
                    break;
                case "SV":
                    objReturn = Properties.Resources.sv;
                    break;
                case "SY":
                    objReturn = Properties.Resources.sy;
                    break;
                case "SZ":
                    objReturn = Properties.Resources.sz;
                    break;
                case "TC":
                    objReturn = Properties.Resources.tc;
                    break;
                case "TD":
                    objReturn = Properties.Resources.td;
                    break;
                case "TF":
                    objReturn = Properties.Resources.tf;
                    break;
                case "TG":
                    objReturn = Properties.Resources.tg;
                    break;
                case "TH":
                    objReturn = Properties.Resources.th;
                    break;
                case "TJ":
                    objReturn = Properties.Resources.tj;
                    break;
                case "TK":
                    objReturn = Properties.Resources.tk;
                    break;
                case "TL":
                    objReturn = Properties.Resources.tl;
                    break;
                case "TM":
                    objReturn = Properties.Resources.tm;
                    break;
                case "TN":
                    objReturn = Properties.Resources.tn;
                    break;
                case "TO":
                    objReturn = Properties.Resources.to;
                    break;
                case "TR":
                    objReturn = Properties.Resources.tr;
                    break;
                case "TT":
                    objReturn = Properties.Resources.tt;
                    break;
                case "TV":
                    objReturn = Properties.Resources.tv;
                    break;
                case "TW":
                    objReturn = Properties.Resources.tw;
                    break;
                case "TZ":
                    objReturn = Properties.Resources.tz;
                    break;
                case "UA":
                    objReturn = Properties.Resources.ua;
                    break;
                case "UG":
                    objReturn = Properties.Resources.ug;
                    break;
                case "UM":
                    objReturn = Properties.Resources.um;
                    break;
                case "US":
                    objReturn = Properties.Resources.us;
                    break;
                case "UY":
                    objReturn = Properties.Resources.uy;
                    break;
                case "UZ":
                    objReturn = Properties.Resources.uz;
                    break;
                case "VA":
                    objReturn = Properties.Resources.va;
                    break;
                case "VC":
                    objReturn = Properties.Resources.vc;
                    break;
                case "VE":
                    objReturn = Properties.Resources.ve;
                    break;
                case "VG":
                    objReturn = Properties.Resources.vg;
                    break;
                case "VI":
                    objReturn = Properties.Resources.vi;
                    break;
                case "VN":
                    objReturn = Properties.Resources.vn;
                    break;
                case "VU":
                    objReturn = Properties.Resources.vu;
                    break;
                case "WF":
                    objReturn = Properties.Resources.wf;
                    break;
                case "WS":
                    objReturn = Properties.Resources.ws;
                    break;
                case "XE":
                    objReturn = Properties.Resources.xe;
                    break;
                case "XS":
                    objReturn = Properties.Resources.xs;
                    break;
                case "XW":
                    objReturn = Properties.Resources.xw;
                    break;
                case "YE":
                    objReturn = Properties.Resources.ye;
                    break;
                case "YT":
                    objReturn = Properties.Resources.yt;
                    break;
                case "ZA":
                    objReturn = Properties.Resources.za;
                    break;
                case "ZM":
                    objReturn = Properties.Resources.zm;
                    break;
                case "ZW":
                    objReturn = Properties.Resources.zw;
                    break;
                case "DEFAULT":
                    objReturn = Properties.Resources.defaulted;
                    break;
                case "NOIMAGEDOTS":
                    objReturn = Properties.Resources.noimagedots;
                    break;
                default:
                    Utils.BreakIfDebug();
                    goto case "DEFAULT";
            }
            return objReturn;
        }
    }
}
