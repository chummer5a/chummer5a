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
        /// Returns a 16px x 16px image of a country/region's flag based on its two-letter, ISO-3166 code.
        /// </summary>
        /// <param name="strCode">The ISO-3166 code of the country/region's flag.</param>
        /// <returns>16px x 16px image of the country/region's flag if available, null otherwise</returns>
        public static System.Drawing.Image GetFlagFromCountryCode(string strCode)
        {
            System.Drawing.Image objReturn;
            if (string.IsNullOrEmpty(strCode))
                strCode = string.Empty;
            switch (strCode.ToUpperInvariant())
            {
                case "AD":
                    objReturn = Properties.Resources.flag_andorra;
                    break;
                case "AE":
                    objReturn = Properties.Resources.flag_united_arab_emirates;
                    break;
                case "AF":
                    objReturn = Properties.Resources.flag_afghanistan;
                    break;
                case "AG":
                    objReturn = Properties.Resources.flag_antigua_and_barbuda;
                    break;
                case "AI":
                    objReturn = Properties.Resources.flag_anguilla;
                    break;
                case "AL":
                    objReturn = Properties.Resources.flag_albania;
                    break;
                case "AM":
                    objReturn = Properties.Resources.flag_armenia;
                    break;
                case "AN":
                    objReturn = Properties.Resources.flag_netherlands_antilles;
                    break;
                case "AO":
                    objReturn = Properties.Resources.flag_angola;
                    break;
                case "AR":
                    objReturn = Properties.Resources.flag_argentina;
                    break;
                case "AS":
                    objReturn = Properties.Resources.flag_american_samoa;
                    break;
                case "AT":
                    objReturn = Properties.Resources.flag_austria;
                    break;
                case "AU":
                    objReturn = Properties.Resources.flag_australia;
                    break;
                case "AW":
                    objReturn = Properties.Resources.flag_aruba;
                    break;
                case "AZ":
                    objReturn = Properties.Resources.flag_azerbaijan;
                    break;
                case "BA":
                    objReturn = Properties.Resources.flag_bosnia;
                    break;
                case "BB":
                    objReturn = Properties.Resources.flag_barbados;
                    break;
                case "BD":
                    objReturn = Properties.Resources.flag_bangladesh;
                    break;
                case "BE":
                    objReturn = Properties.Resources.flag_belgium;
                    break;
                case "BF":
                    objReturn = Properties.Resources.flag_burkina_faso;
                    break;
                case "BG":
                    objReturn = Properties.Resources.flag_bulgaria;
                    break;
                case "BH":
                    objReturn = Properties.Resources.flag_bahrain;
                    break;
                case "BI":
                    objReturn = Properties.Resources.flag_burundi;
                    break;
                case "BJ":
                    objReturn = Properties.Resources.flag_benin;
                    break;
                case "BM":
                    objReturn = Properties.Resources.flag_bermuda;
                    break;
                case "BN":
                    objReturn = Properties.Resources.flag_brunei;
                    break;
                case "BO":
                    objReturn = Properties.Resources.flag_bolivia;
                    break;
                case "BR":
                    objReturn = Properties.Resources.flag_brazil;
                    break;
                case "BS":
                    objReturn = Properties.Resources.flag_bahamas;
                    break;
                case "BT":
                    objReturn = Properties.Resources.flag_bhutan;
                    break;
                case "BW":
                    objReturn = Properties.Resources.flag_botswana;
                    break;
                case "BY":
                    objReturn = Properties.Resources.flag_belarus;
                    break;
                case "BZ":
                    objReturn = Properties.Resources.flag_belize;
                    break;
                case "CA":
                    objReturn = Properties.Resources.flag_canada;
                    break;
                case "CD":
                    objReturn = Properties.Resources.flag_congo_democratic_republic;
                    break;
                case "CF":
                    objReturn = Properties.Resources.flag_central_african_republic;
                    break;
                case "CG":
                    objReturn = Properties.Resources.flag_congo_republic;
                    break;
                case "CH":
                    objReturn = Properties.Resources.flag_switzerland;
                    break;
                case "CI":
                    objReturn = Properties.Resources.flag_cote_divoire;
                    break;
                case "CK":
                    objReturn = Properties.Resources.flag_cook_islands;
                    break;
                case "CL":
                    objReturn = Properties.Resources.flag_chile;
                    break;
                case "CM":
                    objReturn = Properties.Resources.flag_cameroon;
                    break;
                case "CN":
                    objReturn = Properties.Resources.flag_china;
                    break;
                case "CO":
                    objReturn = Properties.Resources.flag_colombia;
                    break;
                case "CR":
                    objReturn = Properties.Resources.flag_costa_rica;
                    break;
                case "CU":
                    objReturn = Properties.Resources.flag_cuba;
                    break;
                case "CV":
                    objReturn = Properties.Resources.flag_cape_verde;
                    break;
                case "CY":
                    objReturn = Properties.Resources.flag_cyprus;
                    break;
                case "CZ":
                    objReturn = Properties.Resources.flag_czech_republic;
                    break;
                case "DE":
                    objReturn = Properties.Resources.flag_germany;
                    break;
                case "DJ":
                    objReturn = Properties.Resources.flag_djibouti;
                    break;
                case "DK":
                    objReturn = Properties.Resources.flag_denmark;
                    break;
                case "DM":
                    objReturn = Properties.Resources.flag_dominica;
                    break;
                case "DO":
                    objReturn = Properties.Resources.flag_dominican_republic;
                    break;
                case "DZ":
                    objReturn = Properties.Resources.flag_algeria;
                    break;
                case "EC":
                    objReturn = Properties.Resources.flag_equador;
                    break;
                case "EE":
                    objReturn = Properties.Resources.flag_estonia;
                    break;
                case "EG":
                    objReturn = Properties.Resources.flag_egypt;
                    break;
                case "ER":
                    objReturn = Properties.Resources.flag_eritrea;
                    break;
                case "ES":
                    objReturn = Properties.Resources.flag_spain;
                    break;
                case "ET":
                    objReturn = Properties.Resources.flag_ethiopia;
                    break;
                case "EU":
                    objReturn = Properties.Resources.flag_european_union;
                    break;
                case "FI":
                    objReturn = Properties.Resources.flag_finland;
                    break;
                case "FJ":
                    objReturn = Properties.Resources.flag_fiji;
                    break;
                case "FK":
                    objReturn = Properties.Resources.flag_falkland_islands;
                    break;
                case "FM":
                    objReturn = Properties.Resources.flag_micronesia;
                    break;
                case "FO":
                    objReturn = Properties.Resources.flag_faroe_islands;
                    break;
                case "FR":
                    objReturn = Properties.Resources.flag_france;
                    break;
                case "GA":
                    objReturn = Properties.Resources.flag_gabon;
                    break;
                case "GB":
                    objReturn = Properties.Resources.flag_great_britain;
                    break;
                case "GD":
                    objReturn = Properties.Resources.flag_grenada;
                    break;
                case "GE":
                    objReturn = Properties.Resources.flag_georgia;
                    break;
                case "GG":
                    objReturn = Properties.Resources.flag_guernsey;
                    break;
                case "GH":
                    objReturn = Properties.Resources.flag_ghana;
                    break;
                case "GI":
                    objReturn = Properties.Resources.flag_gibraltar;
                    break;
                case "GL":
                    objReturn = Properties.Resources.flag_greenland;
                    break;
                case "GM":
                    objReturn = Properties.Resources.flag_gambia;
                    break;
                case "GN":
                    objReturn = Properties.Resources.flag_guinea;
                    break;
                case "GQ":
                    objReturn = Properties.Resources.flag_equatorial_guinea;
                    break;
                case "GR":
                    objReturn = Properties.Resources.flag_greece;
                    break;
                case "GS":
                    objReturn = Properties.Resources.flag_south_georgia;
                    break;
                case "GT":
                    objReturn = Properties.Resources.flag_guatemala;
                    break;
                case "GU":
                    objReturn = Properties.Resources.flag_guam;
                    break;
                case "GW":
                    objReturn = Properties.Resources.flag_guinea_bissau;
                    break;
                case "GY":
                    objReturn = Properties.Resources.flag_guyana;
                    break;
                case "HK":
                    objReturn = Properties.Resources.flag_hong_kong;
                    break;
                case "HN":
                    objReturn = Properties.Resources.flag_honduras;
                    break;
                case "HR":
                    objReturn = Properties.Resources.flag_croatia;
                    break;
                case "HT":
                    objReturn = Properties.Resources.flag_haiti;
                    break;
                case "HU":
                    objReturn = Properties.Resources.flag_hungary;
                    break;
                case "ID":
                    objReturn = Properties.Resources.flag_indonesia;
                    break;
                case "IE":
                    objReturn = Properties.Resources.flag_ireland;
                    break;
                case "IL":
                    objReturn = Properties.Resources.flag_israel;
                    break;
                case "IM":
                    objReturn = Properties.Resources.flag_isle_of_man;
                    break;
                case "IN":
                    objReturn = Properties.Resources.flag_india;
                    break;
                case "IO":
                    objReturn = Properties.Resources.flag_british_indian_ocean;
                    break;
                case "IQ":
                    objReturn = Properties.Resources.flag_iraq;
                    break;
                case "IR":
                    objReturn = Properties.Resources.flag_iran;
                    break;
                case "IS":
                    objReturn = Properties.Resources.flag_iceland;
                    break;
                case "IT":
                    objReturn = Properties.Resources.flag_italy;
                    break;
                case "JE":
                    objReturn = Properties.Resources.flag_jersey;
                    break;
                case "JM":
                    objReturn = Properties.Resources.flag_jamaica;
                    break;
                case "JO":
                    objReturn = Properties.Resources.flag_jordan;
                    break;
                case "JP":
                    objReturn = Properties.Resources.flag_japan;
                    break;
                case "KE":
                    objReturn = Properties.Resources.flag_kenya;
                    break;
                case "KG":
                    objReturn = Properties.Resources.flag_kyrgyzstan;
                    break;
                case "KH":
                    objReturn = Properties.Resources.flag_cambodia;
                    break;
                case "KI":
                    objReturn = Properties.Resources.flag_kiribati;
                    break;
                case "KM":
                    objReturn = Properties.Resources.flag_comoros;
                    break;
                case "KN":
                    objReturn = Properties.Resources.flag_saint_kitts_and_nevis;
                    break;
                case "KP":
                    objReturn = Properties.Resources.flag_north_korea;
                    break;
                case "KR":
                    objReturn = Properties.Resources.flag_south_korea;
                    break;
                case "KW":
                    objReturn = Properties.Resources.flag_kuwait;
                    break;
                case "KY":
                    objReturn = Properties.Resources.flag_cayman_islands;
                    break;
                case "KZ":
                    objReturn = Properties.Resources.flag_kazakhstan;
                    break;
                case "LA":
                    objReturn = Properties.Resources.flag_laos;
                    break;
                case "LB":
                    objReturn = Properties.Resources.flag_lebanon;
                    break;
                case "LC":
                    objReturn = Properties.Resources.flag_saint_lucia;
                    break;
                case "LI":
                    objReturn = Properties.Resources.flag_liechtenstein;
                    break;
                case "LK":
                    objReturn = Properties.Resources.flag_sri_lanka;
                    break;
                case "LR":
                    objReturn = Properties.Resources.flag_liberia;
                    break;
                case "LS":
                    objReturn = Properties.Resources.flag_lesotho;
                    break;
                case "LT":
                    objReturn = Properties.Resources.flag_lithuania;
                    break;
                case "LU":
                    objReturn = Properties.Resources.flag_luxembourg;
                    break;
                case "LV":
                    objReturn = Properties.Resources.flag_latvia;
                    break;
                case "LY":
                    objReturn = Properties.Resources.flag_libya;
                    break;
                case "MA":
                    objReturn = Properties.Resources.flag_morocco;
                    break;
                case "MC":
                    objReturn = Properties.Resources.flag_monaco;
                    break;
                case "MD":
                    objReturn = Properties.Resources.flag_moldova;
                    break;
                case "MG":
                    objReturn = Properties.Resources.flag_madagascar;
                    break;
                case "MH":
                    objReturn = Properties.Resources.flag_marshall_islands;
                    break;
                case "MK":
                    objReturn = Properties.Resources.flag_macedonia;
                    break;
                case "ML":
                    objReturn = Properties.Resources.flag_mali;
                    break;
                case "MM":
                    objReturn = Properties.Resources.flag_burma;
                    break;
                case "MN":
                    objReturn = Properties.Resources.flag_mongolia;
                    break;
                case "MO":
                    objReturn = Properties.Resources.flag_macau;
                    break;
                case "MP":
                    objReturn = Properties.Resources.flag_northern_mariana_islands;
                    break;
                case "MQ":
                    objReturn = Properties.Resources.flag_martinique;
                    break;
                case "MR":
                    objReturn = Properties.Resources.flag_mauretania;
                    break;
                case "MS":
                    objReturn = Properties.Resources.flag_montserrat;
                    break;
                case "MT":
                    objReturn = Properties.Resources.flag_malta;
                    break;
                case "MU":
                    objReturn = Properties.Resources.flag_mauritius;
                    break;
                case "MV":
                    objReturn = Properties.Resources.flag_maledives;
                    break;
                case "MW":
                    objReturn = Properties.Resources.flag_malawi;
                    break;
                case "MX":
                    objReturn = Properties.Resources.flag_mexico;
                    break;
                case "MY":
                    objReturn = Properties.Resources.flag_malaysia;
                    break;
                case "MZ":
                    objReturn = Properties.Resources.flag_mozambique;
                    break;
                case "NA":
                    objReturn = Properties.Resources.flag_namibia;
                    break;
                case "NE":
                    objReturn = Properties.Resources.flag_niger;
                    break;
                case "NF":
                    objReturn = Properties.Resources.flag_norfolk_islands;
                    break;
                case "NG":
                    objReturn = Properties.Resources.flag_nigeria;
                    break;
                case "NI":
                    objReturn = Properties.Resources.flag_nicaragua;
                    break;
                case "NL":
                    objReturn = Properties.Resources.flag_netherlands;
                    break;
                case "NO":
                    objReturn = Properties.Resources.flag_norway;
                    break;
                case "NP":
                    objReturn = Properties.Resources.flag_nepal;
                    break;
                case "NR":
                    objReturn = Properties.Resources.flag_nauru;
                    break;
                case "NU":
                    objReturn = Properties.Resources.flag_niue;
                    break;
                case "NZ":
                    objReturn = Properties.Resources.flag_new_zealand;
                    break;
                case "OM":
                    objReturn = Properties.Resources.flag_oman;
                    break;
                case "PA":
                    objReturn = Properties.Resources.flag_panama;
                    break;
                case "PE":
                    objReturn = Properties.Resources.flag_peru;
                    break;
                case "PF":
                    objReturn = Properties.Resources.flag_french_polynesia;
                    break;
                case "PG":
                    objReturn = Properties.Resources.flag_papua_new_guinea;
                    break;
                case "PH":
                    objReturn = Properties.Resources.flag_philippines;
                    break;
                case "PK":
                    objReturn = Properties.Resources.flag_pakistan;
                    break;
                case "PL":
                    objReturn = Properties.Resources.flag_poland;
                    break;
                case "PM":
                    objReturn = Properties.Resources.flag_saint_pierre_and_miquelon;
                    break;
                case "PN":
                    objReturn = Properties.Resources.flag_pitcairn_islands;
                    break;
                case "PR":
                    objReturn = Properties.Resources.flag_puerto_rico;
                    break;
                case "PT":
                    objReturn = Properties.Resources.flag_portugal;
                    break;
                case "PW":
                    objReturn = Properties.Resources.flag_palau;
                    break;
                case "PY":
                    objReturn = Properties.Resources.flag_paraquay;
                    break;
                case "QA":
                    objReturn = Properties.Resources.flag_qatar;
                    break;
                case "RO":
                    objReturn = Properties.Resources.flag_romania;
                    break;
                case "RS":
                    objReturn = Properties.Resources.flag_serbia_montenegro;
                    break;
                case "RU":
                    objReturn = Properties.Resources.flag_russia;
                    break;
                case "RW":
                    objReturn = Properties.Resources.flag_rwanda;
                    break;
                case "SA":
                    objReturn = Properties.Resources.flag_saudi_arabia;
                    break;
                case "SB":
                    objReturn = Properties.Resources.flag_solomon_islands;
                    break;
                case "SC":
                    objReturn = Properties.Resources.flag_seychelles;
                    break;
                case "SD":
                    objReturn = Properties.Resources.flag_sudan;
                    break;
                case "SE":
                    objReturn = Properties.Resources.flag_sweden;
                    break;
                case "SG":
                    objReturn = Properties.Resources.flag_singapore;
                    break;
                case "SH":
                    objReturn = Properties.Resources.flag_saint_helena;
                    break;
                case "SI":
                    objReturn = Properties.Resources.flag_slovenia;
                    break;
                case "SK":
                    objReturn = Properties.Resources.flag_slovakia;
                    break;
                case "SL":
                    objReturn = Properties.Resources.flag_sierra_leone;
                    break;
                case "SM":
                    objReturn = Properties.Resources.flag_san_marino;
                    break;
                case "SN":
                    objReturn = Properties.Resources.flag_senegal;
                    break;
                case "SO":
                    objReturn = Properties.Resources.flag_somalia;
                    break;
                case "SR":
                    objReturn = Properties.Resources.flag_suriname;
                    break;
                case "ST":
                    objReturn = Properties.Resources.flag_sao_tome_and_principe;
                    break;
                case "SV":
                    objReturn = Properties.Resources.flag_el_salvador;
                    break;
                case "SY":
                    objReturn = Properties.Resources.flag_syria;
                    break;
                case "SZ":
                    objReturn = Properties.Resources.flag_swaziland;
                    break;
                case "TC":
                    objReturn = Properties.Resources.flag_turks_and_caicos_islands;
                    break;
                case "TD":
                    objReturn = Properties.Resources.flag_chad;
                    break;
                case "TG":
                    objReturn = Properties.Resources.flag_togo;
                    break;
                case "TH":
                    objReturn = Properties.Resources.flag_thailand;
                    break;
                case "TI":
                    objReturn = Properties.Resources.flag_tibet;
                    break;
                case "TJ":
                    objReturn = Properties.Resources.flag_tajikistan;
                    break;
                case "TL":
                    objReturn = Properties.Resources.flag_east_timor;
                    break;
                case "TM":
                    objReturn = Properties.Resources.flag_turkmenistan;
                    break;
                case "TN":
                    objReturn = Properties.Resources.flag_tunisia;
                    break;
                case "TO":
                    objReturn = Properties.Resources.flag_tonga;
                    break;
                case "TR":
                    objReturn = Properties.Resources.flag_turkey;
                    break;
                case "TT":
                    objReturn = Properties.Resources.flag_trinidad_and_tobago;
                    break;
                case "TV":
                    objReturn = Properties.Resources.flag_tuvalu;
                    break;
                case "TW":
                    objReturn = Properties.Resources.flag_taiwan;
                    break;
                case "TZ":
                    objReturn = Properties.Resources.flag_tanzania;
                    break;
                case "UA":
                    objReturn = Properties.Resources.flag_ukraine;
                    break;
                case "UG":
                    objReturn = Properties.Resources.flag_uganda;
                    break;
                case "US":
                    objReturn = Properties.Resources.flag_usa;
                    break;
                case "UY":
                    objReturn = Properties.Resources.flag_uruquay;
                    break;
                case "UZ":
                    objReturn = Properties.Resources.flag_uzbekistan;
                    break;
                case "VA":
                    objReturn = Properties.Resources.flag_vatican_city;
                    break;
                case "VC":
                    objReturn = Properties.Resources.flag_saint_vincent_and_grenadines;
                    break;
                case "VE":
                    objReturn = Properties.Resources.flag_venezuela;
                    break;
                case "VG":
                    objReturn = Properties.Resources.flag_british_virgin_islands;
                    break;
                case "VI":
                    objReturn = Properties.Resources.flag_virgin_islands;
                    break;
                case "VN":
                    objReturn = Properties.Resources.flag_vietnam;
                    break;
                case "VU":
                    objReturn = Properties.Resources.flag_vanuatu;
                    break;
                case "WF":
                    objReturn = Properties.Resources.flag_wallis_and_futuna;
                    break;
                case "WS":
                    objReturn = Properties.Resources.flag_samoa;
                    break;
                case "XE":
                    objReturn = Properties.Resources.flag_england;
                    break;
                case "XS":
                    objReturn = Properties.Resources.flag_scotland;
                    break;
                case "XW":
                    objReturn = Properties.Resources.flag_wales;
                    break;
                case "YE":
                    objReturn = Properties.Resources.flag_yemen;
                    break;
                case "ZA":
                    objReturn = Properties.Resources.flag_south_africa;
                    break;
                case "ZM":
                    objReturn = Properties.Resources.flag_zambia;
                    break;
                case "ZW":
                    objReturn = Properties.Resources.flag_zimbabwe;
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
