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
    public enum FlagImageSize
    {
        None = 0,
        Dpi96 = 16,
        Dpi120 = 20,
        Dpi144 = 24,
        Dpi192 = 32,
        Dpi288 = 48,
        Dpi384 = 64
    }

    public static class FlagImageGetter
    {
        /// <summary>
        /// Returns a square image of a country/region's flag based on its two-letter, ISO-3166 code.
        /// </summary>
        /// <param name="strCode">The ISO-3166 code of the country/region's flag.</param>
        /// <param name="intSize">Size of the image to get.</param>
        /// <returns>Image of the country/region's flag if available, null otherwise</returns>
        public static System.Drawing.Bitmap GetFlagFromCountryCode(string strCode, int intSize = 16)
        {
            if (intSize <= 0)
                return null;
            FlagImageSize eSize = FlagImageSize.Dpi96;
            if (intSize >= 32)
            {
                if (intSize >= 48)
                    eSize = intSize >= 64 ? FlagImageSize.Dpi384 : FlagImageSize.Dpi288;
                else
                    eSize = FlagImageSize.Dpi192;
            }
            else if (intSize >= 20)
                eSize = intSize >= 24 ? FlagImageSize.Dpi144 : FlagImageSize.Dpi120;

            return GetFlagFromCountryCode(strCode, eSize);
        }

        /// <summary>
        /// Returns a square image of a country/region's flag based on its two-letter, ISO-3166 code.
        /// </summary>
        /// <param name="strCode">The ISO-3166 code of the country/region's flag.</param>
        /// <param name="eSize">Size of the image to get based on DPI numbers.</param>
        /// <returns>Image of the country/region's flag if available, null otherwise</returns>
        public static System.Drawing.Bitmap GetFlagFromCountryCode(string strCode, FlagImageSize eSize)
        {
            if (eSize == FlagImageSize.None)
                return null;
            System.Drawing.Bitmap objReturn;
            if (string.IsNullOrEmpty(strCode))
                strCode = string.Empty;
            strCode = strCode.ToUpperInvariant();
            switch (eSize)
            {
                case FlagImageSize.Dpi96:
                {
                    switch (strCode)
                    {
                        case "AD":
                            objReturn = Properties.Resources.flag_andorra_16;
                            break;

                        case "AE":
                            objReturn = Properties.Resources.flag_united_arab_emirates_16;
                            break;

                        case "AF":
                            objReturn = Properties.Resources.flag_afghanistan_16;
                            break;

                        case "AG":
                            objReturn = Properties.Resources.flag_antigua_and_barbuda_16;
                            break;

                        case "AI":
                            objReturn = Properties.Resources.flag_anguilla_16;
                            break;

                        case "AL":
                            objReturn = Properties.Resources.flag_albania_16;
                            break;

                        case "AM":
                            objReturn = Properties.Resources.flag_armenia_16;
                            break;

                        case "AN":
                            objReturn = Properties.Resources.flag_netherlands_antilles_16;
                            break;

                        case "AO":
                            objReturn = Properties.Resources.flag_angola_16;
                            break;

                        case "AR":
                            objReturn = Properties.Resources.flag_argentina_16;
                            break;

                        case "AS":
                            objReturn = Properties.Resources.flag_american_samoa_16;
                            break;

                        case "AT":
                            objReturn = Properties.Resources.flag_austria_16;
                            break;

                        case "AU":
                            objReturn = Properties.Resources.flag_australia_16;
                            break;

                        case "AW":
                            objReturn = Properties.Resources.flag_aruba_16;
                            break;

                        case "AZ":
                            objReturn = Properties.Resources.flag_azerbaijan_16;
                            break;

                        case "BA":
                            objReturn = Properties.Resources.flag_bosnia_16;
                            break;

                        case "BB":
                            objReturn = Properties.Resources.flag_barbados_16;
                            break;

                        case "BD":
                            objReturn = Properties.Resources.flag_bangladesh_16;
                            break;

                        case "BE":
                            objReturn = Properties.Resources.flag_belgium_16;
                            break;

                        case "BF":
                            objReturn = Properties.Resources.flag_burkina_faso_16;
                            break;

                        case "BG":
                            objReturn = Properties.Resources.flag_bulgaria_16;
                            break;

                        case "BH":
                            objReturn = Properties.Resources.flag_bahrain_16;
                            break;

                        case "BI":
                            objReturn = Properties.Resources.flag_burundi_16;
                            break;

                        case "BJ":
                            objReturn = Properties.Resources.flag_benin_16;
                            break;

                        case "BM":
                            objReturn = Properties.Resources.flag_bermuda_16;
                            break;

                        case "BN":
                            objReturn = Properties.Resources.flag_brunei_16;
                            break;

                        case "BO":
                            objReturn = Properties.Resources.flag_bolivia_16;
                            break;

                        case "BR":
                            objReturn = Properties.Resources.flag_brazil_16;
                            break;

                        case "BS":
                            objReturn = Properties.Resources.flag_bahamas_16;
                            break;

                        case "BT":
                            objReturn = Properties.Resources.flag_bhutan_16;
                            break;

                        case "BW":
                            objReturn = Properties.Resources.flag_botswana_16;
                            break;

                        case "BY":
                            objReturn = Properties.Resources.flag_belarus_16;
                            break;

                        case "BZ":
                            objReturn = Properties.Resources.flag_belize_16;
                            break;

                        case "CA":
                            objReturn = Properties.Resources.flag_canada_16;
                            break;

                        case "CD":
                            objReturn = Properties.Resources.flag_congo_democratic_republic_16;
                            break;

                        case "CF":
                            objReturn = Properties.Resources.flag_central_african_republic_16;
                            break;

                        case "CG":
                            objReturn = Properties.Resources.flag_congo_republic_16;
                            break;

                        case "CH":
                            objReturn = Properties.Resources.flag_switzerland_16;
                            break;

                        case "CI":
                            objReturn = Properties.Resources.flag_cote_divoire_16;
                            break;

                        case "CK":
                            objReturn = Properties.Resources.flag_cook_islands_16;
                            break;

                        case "CL":
                            objReturn = Properties.Resources.flag_chile_16;
                            break;

                        case "CM":
                            objReturn = Properties.Resources.flag_cameroon_16;
                            break;

                        case "CN":
                            objReturn = Properties.Resources.flag_china_16;
                            break;

                        case "CO":
                            objReturn = Properties.Resources.flag_colombia_16;
                            break;

                        case "CR":
                            objReturn = Properties.Resources.flag_costa_rica_16;
                            break;

                        case "CU":
                            objReturn = Properties.Resources.flag_cuba_16;
                            break;

                        case "CV":
                            objReturn = Properties.Resources.flag_cape_verde_16;
                            break;

                        case "CY":
                            objReturn = Properties.Resources.flag_cyprus_16;
                            break;

                        case "CZ":
                            objReturn = Properties.Resources.flag_czech_republic_16;
                            break;

                        case "DE":
                            objReturn = Properties.Resources.flag_germany_16;
                            break;

                        case "DJ":
                            objReturn = Properties.Resources.flag_djibouti_16;
                            break;

                        case "DK":
                            objReturn = Properties.Resources.flag_denmark_16;
                            break;

                        case "DM":
                            objReturn = Properties.Resources.flag_dominica_16;
                            break;

                        case "DO":
                            objReturn = Properties.Resources.flag_dominican_republic_16;
                            break;

                        case "DZ":
                            objReturn = Properties.Resources.flag_algeria_16;
                            break;

                        case "EC":
                            objReturn = Properties.Resources.flag_equador_16;
                            break;

                        case "EE":
                            objReturn = Properties.Resources.flag_estonia_16;
                            break;

                        case "EG":
                            objReturn = Properties.Resources.flag_egypt_16;
                            break;

                        case "ER":
                            objReturn = Properties.Resources.flag_eritrea_16;
                            break;

                        case "ES":
                            objReturn = Properties.Resources.flag_spain_16;
                            break;

                        case "ET":
                            objReturn = Properties.Resources.flag_ethiopia_16;
                            break;

                        case "EU":
                            objReturn = Properties.Resources.flag_european_union_16;
                            break;

                        case "FI":
                            objReturn = Properties.Resources.flag_finland_16;
                            break;

                        case "FJ":
                            objReturn = Properties.Resources.flag_fiji_16;
                            break;

                        case "FK":
                            objReturn = Properties.Resources.flag_falkland_islands_16;
                            break;

                        case "FM":
                            objReturn = Properties.Resources.flag_micronesia_16;
                            break;

                        case "FO":
                            objReturn = Properties.Resources.flag_faroe_islands_16;
                            break;

                        case "FR":
                            objReturn = Properties.Resources.flag_france_16;
                            break;

                        case "GA":
                            objReturn = Properties.Resources.flag_gabon_16;
                            break;

                        case "GB":
                            objReturn = Properties.Resources.flag_great_britain_16;
                            break;

                        case "GD":
                            objReturn = Properties.Resources.flag_grenada_16;
                            break;

                        case "GE":
                            objReturn = Properties.Resources.flag_georgia_16;
                            break;

                        case "GG":
                            objReturn = Properties.Resources.flag_guernsey_16;
                            break;

                        case "GH":
                            objReturn = Properties.Resources.flag_ghana_16;
                            break;

                        case "GI":
                            objReturn = Properties.Resources.flag_gibraltar_16;
                            break;

                        case "GL":
                            objReturn = Properties.Resources.flag_greenland_16;
                            break;

                        case "GM":
                            objReturn = Properties.Resources.flag_gambia_16;
                            break;

                        case "GN":
                            objReturn = Properties.Resources.flag_guinea_16;
                            break;

                        case "GQ":
                            objReturn = Properties.Resources.flag_equatorial_guinea_16;
                            break;

                        case "GR":
                            objReturn = Properties.Resources.flag_greece_16;
                            break;

                        case "GS":
                            objReturn = Properties.Resources.flag_south_georgia_16;
                            break;

                        case "GT":
                            objReturn = Properties.Resources.flag_guatemala_16;
                            break;

                        case "GU":
                            objReturn = Properties.Resources.flag_guam_16;
                            break;

                        case "GW":
                            objReturn = Properties.Resources.flag_guinea_bissau_16;
                            break;

                        case "GY":
                            objReturn = Properties.Resources.flag_guyana_16;
                            break;

                        case "HK":
                            objReturn = Properties.Resources.flag_hong_kong_16;
                            break;

                        case "HN":
                            objReturn = Properties.Resources.flag_honduras_16;
                            break;

                        case "HR":
                            objReturn = Properties.Resources.flag_croatia_16;
                            break;

                        case "HT":
                            objReturn = Properties.Resources.flag_haiti_16;
                            break;

                        case "HU":
                            objReturn = Properties.Resources.flag_hungary_16;
                            break;

                        case "ID":
                            objReturn = Properties.Resources.flag_indonesia_16;
                            break;

                        case "IE":
                            objReturn = Properties.Resources.flag_ireland_16;
                            break;

                        case "IL":
                            objReturn = Properties.Resources.flag_israel_16;
                            break;

                        case "IM":
                            objReturn = Properties.Resources.flag_isle_of_man_16;
                            break;

                        case "IN":
                            objReturn = Properties.Resources.flag_india_16;
                            break;

                        case "IO":
                            objReturn = Properties.Resources.flag_british_indian_ocean_16;
                            break;

                        case "IQ":
                            objReturn = Properties.Resources.flag_iraq_16;
                            break;

                        case "IR":
                            objReturn = Properties.Resources.flag_iran_16;
                            break;

                        case "IS":
                            objReturn = Properties.Resources.flag_iceland_16;
                            break;

                        case "IT":
                            objReturn = Properties.Resources.flag_italy_16;
                            break;

                        case "JE":
                            objReturn = Properties.Resources.flag_jersey_16;
                            break;

                        case "JM":
                            objReturn = Properties.Resources.flag_jamaica_16;
                            break;

                        case "JO":
                            objReturn = Properties.Resources.flag_jordan_16;
                            break;

                        case "JP":
                            objReturn = Properties.Resources.flag_japan_16;
                            break;

                        case "KE":
                            objReturn = Properties.Resources.flag_kenya_16;
                            break;

                        case "KG":
                            objReturn = Properties.Resources.flag_kyrgyzstan_16;
                            break;

                        case "KH":
                            objReturn = Properties.Resources.flag_cambodia_16;
                            break;

                        case "KI":
                            objReturn = Properties.Resources.flag_kiribati_16;
                            break;

                        case "KM":
                            objReturn = Properties.Resources.flag_comoros_16;
                            break;

                        case "KN":
                            objReturn = Properties.Resources.flag_saint_kitts_and_nevis_16;
                            break;

                        case "KP":
                            objReturn = Properties.Resources.flag_north_korea_16;
                            break;

                        case "KR":
                            objReturn = Properties.Resources.flag_south_korea_16;
                            break;

                        case "KW":
                            objReturn = Properties.Resources.flag_kuwait_16;
                            break;

                        case "KY":
                            objReturn = Properties.Resources.flag_cayman_islands_16;
                            break;

                        case "KZ":
                            objReturn = Properties.Resources.flag_kazakhstan_16;
                            break;

                        case "LA":
                            objReturn = Properties.Resources.flag_laos_16;
                            break;

                        case "LB":
                            objReturn = Properties.Resources.flag_lebanon_16;
                            break;

                        case "LC":
                            objReturn = Properties.Resources.flag_saint_lucia_16;
                            break;

                        case "LI":
                            objReturn = Properties.Resources.flag_liechtenstein_16;
                            break;

                        case "LK":
                            objReturn = Properties.Resources.flag_sri_lanka_16;
                            break;

                        case "LR":
                            objReturn = Properties.Resources.flag_liberia_16;
                            break;

                        case "LS":
                            objReturn = Properties.Resources.flag_lesotho_16;
                            break;

                        case "LT":
                            objReturn = Properties.Resources.flag_lithuania_16;
                            break;

                        case "LU":
                            objReturn = Properties.Resources.flag_luxembourg_16;
                            break;

                        case "LV":
                            objReturn = Properties.Resources.flag_latvia_16;
                            break;

                        case "LY":
                            objReturn = Properties.Resources.flag_libya_16;
                            break;

                        case "MA":
                            objReturn = Properties.Resources.flag_morocco_16;
                            break;

                        case "MC":
                            objReturn = Properties.Resources.flag_monaco_16;
                            break;

                        case "MD":
                            objReturn = Properties.Resources.flag_moldova_16;
                            break;

                        case "MG":
                            objReturn = Properties.Resources.flag_madagascar_16;
                            break;

                        case "MH":
                            objReturn = Properties.Resources.flag_marshall_islands_16;
                            break;

                        case "MK":
                            objReturn = Properties.Resources.flag_macedonia_16;
                            break;

                        case "ML":
                            objReturn = Properties.Resources.flag_mali_16;
                            break;

                        case "MM":
                            objReturn = Properties.Resources.flag_burma_16;
                            break;

                        case "MN":
                            objReturn = Properties.Resources.flag_mongolia_16;
                            break;

                        case "MO":
                            objReturn = Properties.Resources.flag_macau_16;
                            break;

                        case "MP":
                            objReturn = Properties.Resources.flag_northern_mariana_islands_16;
                            break;

                        case "MQ":
                            objReturn = Properties.Resources.flag_martinique_16;
                            break;

                        case "MR":
                            objReturn = Properties.Resources.flag_mauretania_16;
                            break;

                        case "MS":
                            objReturn = Properties.Resources.flag_montserrat_16;
                            break;

                        case "MT":
                            objReturn = Properties.Resources.flag_malta_16;
                            break;

                        case "MU":
                            objReturn = Properties.Resources.flag_mauritius_16;
                            break;

                        case "MV":
                            objReturn = Properties.Resources.flag_maledives_16;
                            break;

                        case "MW":
                            objReturn = Properties.Resources.flag_malawi_16;
                            break;

                        case "MX":
                            objReturn = Properties.Resources.flag_mexico_16;
                            break;

                        case "MY":
                            objReturn = Properties.Resources.flag_malaysia_16;
                            break;

                        case "MZ":
                            objReturn = Properties.Resources.flag_mozambique_16;
                            break;

                        case "NA":
                            objReturn = Properties.Resources.flag_namibia_16;
                            break;

                        case "NE":
                            objReturn = Properties.Resources.flag_niger_16;
                            break;

                        case "NF":
                            objReturn = Properties.Resources.flag_norfolk_islands_16;
                            break;

                        case "NG":
                            objReturn = Properties.Resources.flag_nigeria_16;
                            break;

                        case "NI":
                            objReturn = Properties.Resources.flag_nicaragua_16;
                            break;

                        case "NL":
                            objReturn = Properties.Resources.flag_netherlands_16;
                            break;

                        case "NO":
                            objReturn = Properties.Resources.flag_norway_16;
                            break;

                        case "NP":
                            objReturn = Properties.Resources.flag_nepal_16;
                            break;

                        case "NR":
                            objReturn = Properties.Resources.flag_nauru_16;
                            break;

                        case "NU":
                            objReturn = Properties.Resources.flag_niue_16;
                            break;

                        case "NZ":
                            objReturn = Properties.Resources.flag_new_zealand_16;
                            break;

                        case "OM":
                            objReturn = Properties.Resources.flag_oman_16;
                            break;

                        case "PA":
                            objReturn = Properties.Resources.flag_panama_16;
                            break;

                        case "PE":
                            objReturn = Properties.Resources.flag_peru_16;
                            break;

                        case "PF":
                            objReturn = Properties.Resources.flag_french_polynesia_16;
                            break;

                        case "PG":
                            objReturn = Properties.Resources.flag_papua_new_guinea_16;
                            break;

                        case "PH":
                            objReturn = Properties.Resources.flag_philippines_16;
                            break;

                        case "PK":
                            objReturn = Properties.Resources.flag_pakistan_16;
                            break;

                        case "PL":
                            objReturn = Properties.Resources.flag_poland_16;
                            break;

                        case "PM":
                            objReturn = Properties.Resources.flag_saint_pierre_and_miquelon_16;
                            break;

                        case "PN":
                            objReturn = Properties.Resources.flag_pitcairn_islands_16;
                            break;

                        case "PR":
                            objReturn = Properties.Resources.flag_puerto_rico_16;
                            break;

                        case "PT":
                            objReturn = Properties.Resources.flag_portugal_16;
                            break;

                        case "PW":
                            objReturn = Properties.Resources.flag_palau_16;
                            break;

                        case "PY":
                            objReturn = Properties.Resources.flag_paraquay_16;
                            break;

                        case "QA":
                            objReturn = Properties.Resources.flag_qatar_16;
                            break;

                        case "RO":
                            objReturn = Properties.Resources.flag_romania_16;
                            break;

                        case "RS":
                            objReturn = Properties.Resources.flag_serbia_montenegro_16;
                            break;

                        case "RU":
                            objReturn = Properties.Resources.flag_russia_16;
                            break;

                        case "RW":
                            objReturn = Properties.Resources.flag_rwanda_16;
                            break;

                        case "SA":
                            objReturn = Properties.Resources.flag_saudi_arabia_16;
                            break;

                        case "SB":
                            objReturn = Properties.Resources.flag_solomon_islands_16;
                            break;

                        case "SC":
                            objReturn = Properties.Resources.flag_seychelles_16;
                            break;

                        case "SD":
                            objReturn = Properties.Resources.flag_sudan_16;
                            break;

                        case "SE":
                            objReturn = Properties.Resources.flag_sweden_16;
                            break;

                        case "SG":
                            objReturn = Properties.Resources.flag_singapore_16;
                            break;

                        case "SH":
                            objReturn = Properties.Resources.flag_saint_helena_16;
                            break;

                        case "SI":
                            objReturn = Properties.Resources.flag_slovenia_16;
                            break;

                        case "SK":
                            objReturn = Properties.Resources.flag_slovakia_16;
                            break;

                        case "SL":
                            objReturn = Properties.Resources.flag_sierra_leone_16;
                            break;

                        case "SM":
                            objReturn = Properties.Resources.flag_san_marino_16;
                            break;

                        case "SN":
                            objReturn = Properties.Resources.flag_senegal_16;
                            break;

                        case "SO":
                            objReturn = Properties.Resources.flag_somalia_16;
                            break;

                        case "SR":
                            objReturn = Properties.Resources.flag_suriname_16;
                            break;

                        case "ST":
                            objReturn = Properties.Resources.flag_sao_tome_and_principe_16;
                            break;

                        case "SV":
                            objReturn = Properties.Resources.flag_el_salvador_16;
                            break;

                        case "SY":
                            objReturn = Properties.Resources.flag_syria_16;
                            break;

                        case "SZ":
                            objReturn = Properties.Resources.flag_swaziland_16;
                            break;

                        case "TC":
                            objReturn = Properties.Resources.flag_turks_and_caicos_islands_16;
                            break;

                        case "TD":
                            objReturn = Properties.Resources.flag_chad_16;
                            break;

                        case "TG":
                            objReturn = Properties.Resources.flag_togo_16;
                            break;

                        case "TH":
                            objReturn = Properties.Resources.flag_thailand_16;
                            break;

                        case "TI":
                            objReturn = Properties.Resources.flag_tibet_16;
                            break;

                        case "TJ":
                            objReturn = Properties.Resources.flag_tajikistan_16;
                            break;

                        case "TL":
                            objReturn = Properties.Resources.flag_east_timor_16;
                            break;

                        case "TM":
                            objReturn = Properties.Resources.flag_turkmenistan_16;
                            break;

                        case "TN":
                            objReturn = Properties.Resources.flag_tunisia_16;
                            break;

                        case "TO":
                            objReturn = Properties.Resources.flag_tonga_16;
                            break;

                        case "TR":
                            objReturn = Properties.Resources.flag_turkey_16;
                            break;

                        case "TT":
                            objReturn = Properties.Resources.flag_trinidad_and_tobago_16;
                            break;

                        case "TV":
                            objReturn = Properties.Resources.flag_tuvalu_16;
                            break;

                        case "TW":
                            objReturn = Properties.Resources.flag_taiwan_16;
                            break;

                        case "TZ":
                            objReturn = Properties.Resources.flag_tanzania_16;
                            break;

                        case "UA":
                            objReturn = Properties.Resources.flag_ukraine_16;
                            break;

                        case "UG":
                            objReturn = Properties.Resources.flag_uganda_16;
                            break;

                        case "US":
                            objReturn = Properties.Resources.flag_usa_16;
                            break;

                        case "UY":
                            objReturn = Properties.Resources.flag_uruquay_16;
                            break;

                        case "UZ":
                            objReturn = Properties.Resources.flag_uzbekistan_16;
                            break;

                        case "VA":
                            objReturn = Properties.Resources.flag_vatican_city_16;
                            break;

                        case "VC":
                            objReturn = Properties.Resources.flag_saint_vincent_and_grenadines_16;
                            break;

                        case "VE":
                            objReturn = Properties.Resources.flag_venezuela_16;
                            break;

                        case "VG":
                            objReturn = Properties.Resources.flag_british_virgin_islands_16;
                            break;

                        case "VI":
                            objReturn = Properties.Resources.flag_virgin_islands_16;
                            break;

                        case "VN":
                            objReturn = Properties.Resources.flag_vietnam_16;
                            break;

                        case "VU":
                            objReturn = Properties.Resources.flag_vanuatu_16;
                            break;

                        case "WF":
                            objReturn = Properties.Resources.flag_wallis_and_futuna_16;
                            break;

                        case "WS":
                            objReturn = Properties.Resources.flag_samoa_16;
                            break;

                        case "XE":
                            objReturn = Properties.Resources.flag_england_16;
                            break;

                        case "XS":
                            objReturn = Properties.Resources.flag_scotland_16;
                            break;

                        case "XW":
                            objReturn = Properties.Resources.flag_wales_16;
                            break;

                        case "YE":
                            objReturn = Properties.Resources.flag_yemen_16;
                            break;

                        case "ZA":
                            objReturn = Properties.Resources.flag_south_africa_16;
                            break;

                        case "ZM":
                            objReturn = Properties.Resources.flag_zambia_16;
                            break;

                        case "ZW":
                            objReturn = Properties.Resources.flag_zimbabwe_16;
                            break;

                        case "DEFAULT":
                            objReturn = Properties.Resources.defaulted_16;
                            break;

                        case "NOIMAGEDOTS":
                            objReturn = Properties.Resources.noimagedots_16;
                            break;

                        default:
                            Utils.BreakIfDebug();
                            goto case "DEFAULT";
                    }

                    break;
                }
                case FlagImageSize.Dpi120:
                {
                    switch (strCode)
                    {
                        case "AD":
                            objReturn = Properties.Resources.flag_andorra_20;
                            break;

                        case "AE":
                            objReturn = Properties.Resources.flag_united_arab_emirates_20;
                            break;

                        case "AF":
                            objReturn = Properties.Resources.flag_afghanistan_20;
                            break;

                        case "AG":
                            objReturn = Properties.Resources.flag_antigua_and_barbuda_20;
                            break;

                        case "AI":
                            objReturn = Properties.Resources.flag_anguilla_20;
                            break;

                        case "AL":
                            objReturn = Properties.Resources.flag_albania_20;
                            break;

                        case "AM":
                            objReturn = Properties.Resources.flag_armenia_20;
                            break;

                        case "AN":
                            objReturn = Properties.Resources.flag_netherlands_antilles_20;
                            break;

                        case "AO":
                            objReturn = Properties.Resources.flag_angola_20;
                            break;

                        case "AR":
                            objReturn = Properties.Resources.flag_argentina_20;
                            break;

                        case "AS":
                            objReturn = Properties.Resources.flag_american_samoa_20;
                            break;

                        case "AT":
                            objReturn = Properties.Resources.flag_austria_20;
                            break;

                        case "AU":
                            objReturn = Properties.Resources.flag_australia_20;
                            break;

                        case "AW":
                            objReturn = Properties.Resources.flag_aruba_20;
                            break;

                        case "AZ":
                            objReturn = Properties.Resources.flag_azerbaijan_20;
                            break;

                        case "BA":
                            objReturn = Properties.Resources.flag_bosnia_20;
                            break;

                        case "BB":
                            objReturn = Properties.Resources.flag_barbados_20;
                            break;

                        case "BD":
                            objReturn = Properties.Resources.flag_bangladesh_20;
                            break;

                        case "BE":
                            objReturn = Properties.Resources.flag_belgium_20;
                            break;

                        case "BF":
                            objReturn = Properties.Resources.flag_burkina_faso_20;
                            break;

                        case "BG":
                            objReturn = Properties.Resources.flag_bulgaria_20;
                            break;

                        case "BH":
                            objReturn = Properties.Resources.flag_bahrain_20;
                            break;

                        case "BI":
                            objReturn = Properties.Resources.flag_burundi_20;
                            break;

                        case "BJ":
                            objReturn = Properties.Resources.flag_benin_20;
                            break;

                        case "BM":
                            objReturn = Properties.Resources.flag_bermuda_20;
                            break;

                        case "BN":
                            objReturn = Properties.Resources.flag_brunei_20;
                            break;

                        case "BO":
                            objReturn = Properties.Resources.flag_bolivia_20;
                            break;

                        case "BR":
                            objReturn = Properties.Resources.flag_brazil_20;
                            break;

                        case "BS":
                            objReturn = Properties.Resources.flag_bahamas_20;
                            break;

                        case "BT":
                            objReturn = Properties.Resources.flag_bhutan_20;
                            break;

                        case "BW":
                            objReturn = Properties.Resources.flag_botswana_20;
                            break;

                        case "BY":
                            objReturn = Properties.Resources.flag_belarus_20;
                            break;

                        case "BZ":
                            objReturn = Properties.Resources.flag_belize_20;
                            break;

                        case "CA":
                            objReturn = Properties.Resources.flag_canada_20;
                            break;

                        case "CD":
                            objReturn = Properties.Resources.flag_congo_democratic_republic_20;
                            break;

                        case "CF":
                            objReturn = Properties.Resources.flag_central_african_republic_20;
                            break;

                        case "CG":
                            objReturn = Properties.Resources.flag_congo_republic_20;
                            break;

                        case "CH":
                            objReturn = Properties.Resources.flag_switzerland_20;
                            break;

                        case "CI":
                            objReturn = Properties.Resources.flag_cote_divoire_20;
                            break;

                        case "CK":
                            objReturn = Properties.Resources.flag_cook_islands_20;
                            break;

                        case "CL":
                            objReturn = Properties.Resources.flag_chile_20;
                            break;

                        case "CM":
                            objReturn = Properties.Resources.flag_cameroon_20;
                            break;

                        case "CN":
                            objReturn = Properties.Resources.flag_china_20;
                            break;

                        case "CO":
                            objReturn = Properties.Resources.flag_colombia_20;
                            break;

                        case "CR":
                            objReturn = Properties.Resources.flag_costa_rica_20;
                            break;

                        case "CU":
                            objReturn = Properties.Resources.flag_cuba_20;
                            break;

                        case "CV":
                            objReturn = Properties.Resources.flag_cape_verde_20;
                            break;

                        case "CY":
                            objReturn = Properties.Resources.flag_cyprus_20;
                            break;

                        case "CZ":
                            objReturn = Properties.Resources.flag_czech_republic_20;
                            break;

                        case "DE":
                            objReturn = Properties.Resources.flag_germany_20;
                            break;

                        case "DJ":
                            objReturn = Properties.Resources.flag_djibouti_20;
                            break;

                        case "DK":
                            objReturn = Properties.Resources.flag_denmark_20;
                            break;

                        case "DM":
                            objReturn = Properties.Resources.flag_dominica_20;
                            break;

                        case "DO":
                            objReturn = Properties.Resources.flag_dominican_republic_20;
                            break;

                        case "DZ":
                            objReturn = Properties.Resources.flag_algeria_20;
                            break;

                        case "EC":
                            objReturn = Properties.Resources.flag_equador_20;
                            break;

                        case "EE":
                            objReturn = Properties.Resources.flag_estonia_20;
                            break;

                        case "EG":
                            objReturn = Properties.Resources.flag_egypt_20;
                            break;

                        case "ER":
                            objReturn = Properties.Resources.flag_eritrea_20;
                            break;

                        case "ES":
                            objReturn = Properties.Resources.flag_spain_20;
                            break;

                        case "ET":
                            objReturn = Properties.Resources.flag_ethiopia_20;
                            break;

                        case "EU":
                            objReturn = Properties.Resources.flag_european_union_20;
                            break;

                        case "FI":
                            objReturn = Properties.Resources.flag_finland_20;
                            break;

                        case "FJ":
                            objReturn = Properties.Resources.flag_fiji_20;
                            break;

                        case "FK":
                            objReturn = Properties.Resources.flag_falkland_islands_20;
                            break;

                        case "FM":
                            objReturn = Properties.Resources.flag_micronesia_20;
                            break;

                        case "FO":
                            objReturn = Properties.Resources.flag_faroe_islands_20;
                            break;

                        case "FR":
                            objReturn = Properties.Resources.flag_france_20;
                            break;

                        case "GA":
                            objReturn = Properties.Resources.flag_gabon_20;
                            break;

                        case "GB":
                            objReturn = Properties.Resources.flag_great_britain_20;
                            break;

                        case "GD":
                            objReturn = Properties.Resources.flag_grenada_20;
                            break;

                        case "GE":
                            objReturn = Properties.Resources.flag_georgia_20;
                            break;

                        case "GG":
                            objReturn = Properties.Resources.flag_guernsey_20;
                            break;

                        case "GH":
                            objReturn = Properties.Resources.flag_ghana_20;
                            break;

                        case "GI":
                            objReturn = Properties.Resources.flag_gibraltar_20;
                            break;

                        case "GL":
                            objReturn = Properties.Resources.flag_greenland_20;
                            break;

                        case "GM":
                            objReturn = Properties.Resources.flag_gambia_20;
                            break;

                        case "GN":
                            objReturn = Properties.Resources.flag_guinea_20;
                            break;

                        case "GQ":
                            objReturn = Properties.Resources.flag_equatorial_guinea_20;
                            break;

                        case "GR":
                            objReturn = Properties.Resources.flag_greece_20;
                            break;

                        case "GS":
                            objReturn = Properties.Resources.flag_south_georgia_20;
                            break;

                        case "GT":
                            objReturn = Properties.Resources.flag_guatemala_20;
                            break;

                        case "GU":
                            objReturn = Properties.Resources.flag_guam_20;
                            break;

                        case "GW":
                            objReturn = Properties.Resources.flag_guinea_bissau_20;
                            break;

                        case "GY":
                            objReturn = Properties.Resources.flag_guyana_20;
                            break;

                        case "HK":
                            objReturn = Properties.Resources.flag_hong_kong_20;
                            break;

                        case "HN":
                            objReturn = Properties.Resources.flag_honduras_20;
                            break;

                        case "HR":
                            objReturn = Properties.Resources.flag_croatia_20;
                            break;

                        case "HT":
                            objReturn = Properties.Resources.flag_haiti_20;
                            break;

                        case "HU":
                            objReturn = Properties.Resources.flag_hungary_20;
                            break;

                        case "ID":
                            objReturn = Properties.Resources.flag_indonesia_20;
                            break;

                        case "IE":
                            objReturn = Properties.Resources.flag_ireland_20;
                            break;

                        case "IL":
                            objReturn = Properties.Resources.flag_israel_20;
                            break;

                        case "IM":
                            objReturn = Properties.Resources.flag_isle_of_man_20;
                            break;

                        case "IN":
                            objReturn = Properties.Resources.flag_india_20;
                            break;

                        case "IO":
                            objReturn = Properties.Resources.flag_british_indian_ocean_20;
                            break;

                        case "IQ":
                            objReturn = Properties.Resources.flag_iraq_20;
                            break;

                        case "IR":
                            objReturn = Properties.Resources.flag_iran_20;
                            break;

                        case "IS":
                            objReturn = Properties.Resources.flag_iceland_20;
                            break;

                        case "IT":
                            objReturn = Properties.Resources.flag_italy_20;
                            break;

                        case "JE":
                            objReturn = Properties.Resources.flag_jersey_20;
                            break;

                        case "JM":
                            objReturn = Properties.Resources.flag_jamaica_20;
                            break;

                        case "JO":
                            objReturn = Properties.Resources.flag_jordan_20;
                            break;

                        case "JP":
                            objReturn = Properties.Resources.flag_japan_20;
                            break;

                        case "KE":
                            objReturn = Properties.Resources.flag_kenya_20;
                            break;

                        case "KG":
                            objReturn = Properties.Resources.flag_kyrgyzstan_20;
                            break;

                        case "KH":
                            objReturn = Properties.Resources.flag_cambodia_20;
                            break;

                        case "KI":
                            objReturn = Properties.Resources.flag_kiribati_20;
                            break;

                        case "KM":
                            objReturn = Properties.Resources.flag_comoros_20;
                            break;

                        case "KN":
                            objReturn = Properties.Resources.flag_saint_kitts_and_nevis_20;
                            break;

                        case "KP":
                            objReturn = Properties.Resources.flag_north_korea_20;
                            break;

                        case "KR":
                            objReturn = Properties.Resources.flag_south_korea_20;
                            break;

                        case "KW":
                            objReturn = Properties.Resources.flag_kuwait_20;
                            break;

                        case "KY":
                            objReturn = Properties.Resources.flag_cayman_islands_20;
                            break;

                        case "KZ":
                            objReturn = Properties.Resources.flag_kazakhstan_20;
                            break;

                        case "LA":
                            objReturn = Properties.Resources.flag_laos_20;
                            break;

                        case "LB":
                            objReturn = Properties.Resources.flag_lebanon_20;
                            break;

                        case "LC":
                            objReturn = Properties.Resources.flag_saint_lucia_20;
                            break;

                        case "LI":
                            objReturn = Properties.Resources.flag_liechtenstein_20;
                            break;

                        case "LK":
                            objReturn = Properties.Resources.flag_sri_lanka_20;
                            break;

                        case "LR":
                            objReturn = Properties.Resources.flag_liberia_20;
                            break;

                        case "LS":
                            objReturn = Properties.Resources.flag_lesotho_20;
                            break;

                        case "LT":
                            objReturn = Properties.Resources.flag_lithuania_20;
                            break;

                        case "LU":
                            objReturn = Properties.Resources.flag_luxembourg_20;
                            break;

                        case "LV":
                            objReturn = Properties.Resources.flag_latvia_20;
                            break;

                        case "LY":
                            objReturn = Properties.Resources.flag_libya_20;
                            break;

                        case "MA":
                            objReturn = Properties.Resources.flag_morocco_20;
                            break;

                        case "MC":
                            objReturn = Properties.Resources.flag_monaco_20;
                            break;

                        case "MD":
                            objReturn = Properties.Resources.flag_moldova_20;
                            break;

                        case "MG":
                            objReturn = Properties.Resources.flag_madagascar_20;
                            break;

                        case "MH":
                            objReturn = Properties.Resources.flag_marshall_islands_20;
                            break;

                        case "MK":
                            objReturn = Properties.Resources.flag_macedonia_20;
                            break;

                        case "ML":
                            objReturn = Properties.Resources.flag_mali_20;
                            break;

                        case "MM":
                            objReturn = Properties.Resources.flag_burma_20;
                            break;

                        case "MN":
                            objReturn = Properties.Resources.flag_mongolia_20;
                            break;

                        case "MO":
                            objReturn = Properties.Resources.flag_macau_20;
                            break;

                        case "MP":
                            objReturn = Properties.Resources.flag_northern_mariana_islands_20;
                            break;

                        case "MQ":
                            objReturn = Properties.Resources.flag_martinique_20;
                            break;

                        case "MR":
                            objReturn = Properties.Resources.flag_mauretania_20;
                            break;

                        case "MS":
                            objReturn = Properties.Resources.flag_montserrat_20;
                            break;

                        case "MT":
                            objReturn = Properties.Resources.flag_malta_20;
                            break;

                        case "MU":
                            objReturn = Properties.Resources.flag_mauritius_20;
                            break;

                        case "MV":
                            objReturn = Properties.Resources.flag_maledives_20;
                            break;

                        case "MW":
                            objReturn = Properties.Resources.flag_malawi_20;
                            break;

                        case "MX":
                            objReturn = Properties.Resources.flag_mexico_20;
                            break;

                        case "MY":
                            objReturn = Properties.Resources.flag_malaysia_20;
                            break;

                        case "MZ":
                            objReturn = Properties.Resources.flag_mozambique_20;
                            break;

                        case "NA":
                            objReturn = Properties.Resources.flag_namibia_20;
                            break;

                        case "NE":
                            objReturn = Properties.Resources.flag_niger_20;
                            break;

                        case "NF":
                            objReturn = Properties.Resources.flag_norfolk_islands_20;
                            break;

                        case "NG":
                            objReturn = Properties.Resources.flag_nigeria_20;
                            break;

                        case "NI":
                            objReturn = Properties.Resources.flag_nicaragua_20;
                            break;

                        case "NL":
                            objReturn = Properties.Resources.flag_netherlands_20;
                            break;

                        case "NO":
                            objReturn = Properties.Resources.flag_norway_20;
                            break;

                        case "NP":
                            objReturn = Properties.Resources.flag_nepal_20;
                            break;

                        case "NR":
                            objReturn = Properties.Resources.flag_nauru_20;
                            break;

                        case "NU":
                            objReturn = Properties.Resources.flag_niue_20;
                            break;

                        case "NZ":
                            objReturn = Properties.Resources.flag_new_zealand_20;
                            break;

                        case "OM":
                            objReturn = Properties.Resources.flag_oman_20;
                            break;

                        case "PA":
                            objReturn = Properties.Resources.flag_panama_20;
                            break;

                        case "PE":
                            objReturn = Properties.Resources.flag_peru_20;
                            break;

                        case "PF":
                            objReturn = Properties.Resources.flag_french_polynesia_20;
                            break;

                        case "PG":
                            objReturn = Properties.Resources.flag_papua_new_guinea_20;
                            break;

                        case "PH":
                            objReturn = Properties.Resources.flag_philippines_20;
                            break;

                        case "PK":
                            objReturn = Properties.Resources.flag_pakistan_20;
                            break;

                        case "PL":
                            objReturn = Properties.Resources.flag_poland_20;
                            break;

                        case "PM":
                            objReturn = Properties.Resources.flag_saint_pierre_and_miquelon_20;
                            break;

                        case "PN":
                            objReturn = Properties.Resources.flag_pitcairn_islands_20;
                            break;

                        case "PR":
                            objReturn = Properties.Resources.flag_puerto_rico_20;
                            break;

                        case "PT":
                            objReturn = Properties.Resources.flag_portugal_20;
                            break;

                        case "PW":
                            objReturn = Properties.Resources.flag_palau_20;
                            break;

                        case "PY":
                            objReturn = Properties.Resources.flag_paraquay_20;
                            break;

                        case "QA":
                            objReturn = Properties.Resources.flag_qatar_20;
                            break;

                        case "RO":
                            objReturn = Properties.Resources.flag_romania_20;
                            break;

                        case "RS":
                            objReturn = Properties.Resources.flag_serbia_montenegro_20;
                            break;

                        case "RU":
                            objReturn = Properties.Resources.flag_russia_20;
                            break;

                        case "RW":
                            objReturn = Properties.Resources.flag_rwanda_20;
                            break;

                        case "SA":
                            objReturn = Properties.Resources.flag_saudi_arabia_20;
                            break;

                        case "SB":
                            objReturn = Properties.Resources.flag_solomon_islands_20;
                            break;

                        case "SC":
                            objReturn = Properties.Resources.flag_seychelles_20;
                            break;

                        case "SD":
                            objReturn = Properties.Resources.flag_sudan_20;
                            break;

                        case "SE":
                            objReturn = Properties.Resources.flag_sweden_20;
                            break;

                        case "SG":
                            objReturn = Properties.Resources.flag_singapore_20;
                            break;

                        case "SH":
                            objReturn = Properties.Resources.flag_saint_helena_20;
                            break;

                        case "SI":
                            objReturn = Properties.Resources.flag_slovenia_20;
                            break;

                        case "SK":
                            objReturn = Properties.Resources.flag_slovakia_20;
                            break;

                        case "SL":
                            objReturn = Properties.Resources.flag_sierra_leone_20;
                            break;

                        case "SM":
                            objReturn = Properties.Resources.flag_san_marino_20;
                            break;

                        case "SN":
                            objReturn = Properties.Resources.flag_senegal_20;
                            break;

                        case "SO":
                            objReturn = Properties.Resources.flag_somalia_20;
                            break;

                        case "SR":
                            objReturn = Properties.Resources.flag_suriname_20;
                            break;

                        case "ST":
                            objReturn = Properties.Resources.flag_sao_tome_and_principe_20;
                            break;

                        case "SV":
                            objReturn = Properties.Resources.flag_el_salvador_20;
                            break;

                        case "SY":
                            objReturn = Properties.Resources.flag_syria_20;
                            break;

                        case "SZ":
                            objReturn = Properties.Resources.flag_swaziland_20;
                            break;

                        case "TC":
                            objReturn = Properties.Resources.flag_turks_and_caicos_islands_20;
                            break;

                        case "TD":
                            objReturn = Properties.Resources.flag_chad_20;
                            break;

                        case "TG":
                            objReturn = Properties.Resources.flag_togo_20;
                            break;

                        case "TH":
                            objReturn = Properties.Resources.flag_thailand_20;
                            break;

                        case "TI":
                            objReturn = Properties.Resources.flag_tibet_20;
                            break;

                        case "TJ":
                            objReturn = Properties.Resources.flag_tajikistan_20;
                            break;

                        case "TL":
                            objReturn = Properties.Resources.flag_east_timor_20;
                            break;

                        case "TM":
                            objReturn = Properties.Resources.flag_turkmenistan_20;
                            break;

                        case "TN":
                            objReturn = Properties.Resources.flag_tunisia_20;
                            break;

                        case "TO":
                            objReturn = Properties.Resources.flag_tonga_20;
                            break;

                        case "TR":
                            objReturn = Properties.Resources.flag_turkey_20;
                            break;

                        case "TT":
                            objReturn = Properties.Resources.flag_trinidad_and_tobago_20;
                            break;

                        case "TV":
                            objReturn = Properties.Resources.flag_tuvalu_20;
                            break;

                        case "TW":
                            objReturn = Properties.Resources.flag_taiwan_20;
                            break;

                        case "TZ":
                            objReturn = Properties.Resources.flag_tanzania_20;
                            break;

                        case "UA":
                            objReturn = Properties.Resources.flag_ukraine_20;
                            break;

                        case "UG":
                            objReturn = Properties.Resources.flag_uganda_20;
                            break;

                        case "US":
                            objReturn = Properties.Resources.flag_usa_20;
                            break;

                        case "UY":
                            objReturn = Properties.Resources.flag_uruquay_20;
                            break;

                        case "UZ":
                            objReturn = Properties.Resources.flag_uzbekistan_20;
                            break;

                        case "VA":
                            objReturn = Properties.Resources.flag_vatican_city_20;
                            break;

                        case "VC":
                            objReturn = Properties.Resources.flag_saint_vincent_and_grenadines_20;
                            break;

                        case "VE":
                            objReturn = Properties.Resources.flag_venezuela_20;
                            break;

                        case "VG":
                            objReturn = Properties.Resources.flag_british_virgin_islands_20;
                            break;

                        case "VI":
                            objReturn = Properties.Resources.flag_virgin_islands_20;
                            break;

                        case "VN":
                            objReturn = Properties.Resources.flag_vietnam_20;
                            break;

                        case "VU":
                            objReturn = Properties.Resources.flag_vanuatu_20;
                            break;

                        case "WF":
                            objReturn = Properties.Resources.flag_wallis_and_futuna_20;
                            break;

                        case "WS":
                            objReturn = Properties.Resources.flag_samoa_20;
                            break;

                        case "XE":
                            objReturn = Properties.Resources.flag_england_20;
                            break;

                        case "XS":
                            objReturn = Properties.Resources.flag_scotland_20;
                            break;

                        case "XW":
                            objReturn = Properties.Resources.flag_wales_20;
                            break;

                        case "YE":
                            objReturn = Properties.Resources.flag_yemen_20;
                            break;

                        case "ZA":
                            objReturn = Properties.Resources.flag_south_africa_20;
                            break;

                        case "ZM":
                            objReturn = Properties.Resources.flag_zambia_20;
                            break;

                        case "ZW":
                            objReturn = Properties.Resources.flag_zimbabwe_20;
                            break;

                        case "DEFAULT":
                            objReturn = Properties.Resources.defaulted_20;
                            break;

                        case "NOIMAGEDOTS":
                            objReturn = Properties.Resources.noimagedots_20;
                            break;

                        default:
                            Utils.BreakIfDebug();
                            goto case "DEFAULT";
                    }

                    break;
                }
                case FlagImageSize.Dpi144:
                {
                    switch (strCode)
                    {
                        case "AD":
                            objReturn = Properties.Resources.flag_andorra_24;
                            break;

                        case "AE":
                            objReturn = Properties.Resources.flag_united_arab_emirates_24;
                            break;

                        case "AF":
                            objReturn = Properties.Resources.flag_afghanistan_24;
                            break;

                        case "AG":
                            objReturn = Properties.Resources.flag_antigua_and_barbuda_24;
                            break;

                        case "AI":
                            objReturn = Properties.Resources.flag_anguilla_24;
                            break;

                        case "AL":
                            objReturn = Properties.Resources.flag_albania_24;
                            break;

                        case "AM":
                            objReturn = Properties.Resources.flag_armenia_24;
                            break;

                        case "AN":
                            objReturn = Properties.Resources.flag_netherlands_antilles_24;
                            break;

                        case "AO":
                            objReturn = Properties.Resources.flag_angola_24;
                            break;

                        case "AR":
                            objReturn = Properties.Resources.flag_argentina_24;
                            break;

                        case "AS":
                            objReturn = Properties.Resources.flag_american_samoa_24;
                            break;

                        case "AT":
                            objReturn = Properties.Resources.flag_austria_24;
                            break;

                        case "AU":
                            objReturn = Properties.Resources.flag_australia_24;
                            break;

                        case "AW":
                            objReturn = Properties.Resources.flag_aruba_24;
                            break;

                        case "AZ":
                            objReturn = Properties.Resources.flag_azerbaijan_24;
                            break;

                        case "BA":
                            objReturn = Properties.Resources.flag_bosnia_24;
                            break;

                        case "BB":
                            objReturn = Properties.Resources.flag_barbados_24;
                            break;

                        case "BD":
                            objReturn = Properties.Resources.flag_bangladesh_24;
                            break;

                        case "BE":
                            objReturn = Properties.Resources.flag_belgium_24;
                            break;

                        case "BF":
                            objReturn = Properties.Resources.flag_burkina_faso_24;
                            break;

                        case "BG":
                            objReturn = Properties.Resources.flag_bulgaria_24;
                            break;

                        case "BH":
                            objReturn = Properties.Resources.flag_bahrain_24;
                            break;

                        case "BI":
                            objReturn = Properties.Resources.flag_burundi_24;
                            break;

                        case "BJ":
                            objReturn = Properties.Resources.flag_benin_24;
                            break;

                        case "BM":
                            objReturn = Properties.Resources.flag_bermuda_24;
                            break;

                        case "BN":
                            objReturn = Properties.Resources.flag_brunei_24;
                            break;

                        case "BO":
                            objReturn = Properties.Resources.flag_bolivia_24;
                            break;

                        case "BR":
                            objReturn = Properties.Resources.flag_brazil_24;
                            break;

                        case "BS":
                            objReturn = Properties.Resources.flag_bahamas_24;
                            break;

                        case "BT":
                            objReturn = Properties.Resources.flag_bhutan_24;
                            break;

                        case "BW":
                            objReturn = Properties.Resources.flag_botswana_24;
                            break;

                        case "BY":
                            objReturn = Properties.Resources.flag_belarus_24;
                            break;

                        case "BZ":
                            objReturn = Properties.Resources.flag_belize_24;
                            break;

                        case "CA":
                            objReturn = Properties.Resources.flag_canada_24;
                            break;

                        case "CD":
                            objReturn = Properties.Resources.flag_congo_democratic_republic_24;
                            break;

                        case "CF":
                            objReturn = Properties.Resources.flag_central_african_republic_24;
                            break;

                        case "CG":
                            objReturn = Properties.Resources.flag_congo_republic_24;
                            break;

                        case "CH":
                            objReturn = Properties.Resources.flag_switzerland_24;
                            break;

                        case "CI":
                            objReturn = Properties.Resources.flag_cote_divoire_24;
                            break;

                        case "CK":
                            objReturn = Properties.Resources.flag_cook_islands_24;
                            break;

                        case "CL":
                            objReturn = Properties.Resources.flag_chile_24;
                            break;

                        case "CM":
                            objReturn = Properties.Resources.flag_cameroon_24;
                            break;

                        case "CN":
                            objReturn = Properties.Resources.flag_china_24;
                            break;

                        case "CO":
                            objReturn = Properties.Resources.flag_colombia_24;
                            break;

                        case "CR":
                            objReturn = Properties.Resources.flag_costa_rica_24;
                            break;

                        case "CU":
                            objReturn = Properties.Resources.flag_cuba_24;
                            break;

                        case "CV":
                            objReturn = Properties.Resources.flag_cape_verde_24;
                            break;

                        case "CY":
                            objReturn = Properties.Resources.flag_cyprus_24;
                            break;

                        case "CZ":
                            objReturn = Properties.Resources.flag_czech_republic_24;
                            break;

                        case "DE":
                            objReturn = Properties.Resources.flag_germany_24;
                            break;

                        case "DJ":
                            objReturn = Properties.Resources.flag_djibouti_24;
                            break;

                        case "DK":
                            objReturn = Properties.Resources.flag_denmark_24;
                            break;

                        case "DM":
                            objReturn = Properties.Resources.flag_dominica_24;
                            break;

                        case "DO":
                            objReturn = Properties.Resources.flag_dominican_republic_24;
                            break;

                        case "DZ":
                            objReturn = Properties.Resources.flag_algeria_24;
                            break;

                        case "EC":
                            objReturn = Properties.Resources.flag_equador_24;
                            break;

                        case "EE":
                            objReturn = Properties.Resources.flag_estonia_24;
                            break;

                        case "EG":
                            objReturn = Properties.Resources.flag_egypt_24;
                            break;

                        case "ER":
                            objReturn = Properties.Resources.flag_eritrea_24;
                            break;

                        case "ES":
                            objReturn = Properties.Resources.flag_spain_24;
                            break;

                        case "ET":
                            objReturn = Properties.Resources.flag_ethiopia_24;
                            break;

                        case "EU":
                            objReturn = Properties.Resources.flag_european_union_24;
                            break;

                        case "FI":
                            objReturn = Properties.Resources.flag_finland_24;
                            break;

                        case "FJ":
                            objReturn = Properties.Resources.flag_fiji_24;
                            break;

                        case "FK":
                            objReturn = Properties.Resources.flag_falkland_islands_24;
                            break;

                        case "FM":
                            objReturn = Properties.Resources.flag_micronesia_24;
                            break;

                        case "FO":
                            objReturn = Properties.Resources.flag_faroe_islands_24;
                            break;

                        case "FR":
                            objReturn = Properties.Resources.flag_france_24;
                            break;

                        case "GA":
                            objReturn = Properties.Resources.flag_gabon_24;
                            break;

                        case "GB":
                            objReturn = Properties.Resources.flag_great_britain_24;
                            break;

                        case "GD":
                            objReturn = Properties.Resources.flag_grenada_24;
                            break;

                        case "GE":
                            objReturn = Properties.Resources.flag_georgia_24;
                            break;

                        case "GG":
                            objReturn = Properties.Resources.flag_guernsey_24;
                            break;

                        case "GH":
                            objReturn = Properties.Resources.flag_ghana_24;
                            break;

                        case "GI":
                            objReturn = Properties.Resources.flag_gibraltar_24;
                            break;

                        case "GL":
                            objReturn = Properties.Resources.flag_greenland_24;
                            break;

                        case "GM":
                            objReturn = Properties.Resources.flag_gambia_24;
                            break;

                        case "GN":
                            objReturn = Properties.Resources.flag_guinea_24;
                            break;

                        case "GQ":
                            objReturn = Properties.Resources.flag_equatorial_guinea_24;
                            break;

                        case "GR":
                            objReturn = Properties.Resources.flag_greece_24;
                            break;

                        case "GS":
                            objReturn = Properties.Resources.flag_south_georgia_24;
                            break;

                        case "GT":
                            objReturn = Properties.Resources.flag_guatemala_24;
                            break;

                        case "GU":
                            objReturn = Properties.Resources.flag_guam_24;
                            break;

                        case "GW":
                            objReturn = Properties.Resources.flag_guinea_bissau_24;
                            break;

                        case "GY":
                            objReturn = Properties.Resources.flag_guyana_24;
                            break;

                        case "HK":
                            objReturn = Properties.Resources.flag_hong_kong_24;
                            break;

                        case "HN":
                            objReturn = Properties.Resources.flag_honduras_24;
                            break;

                        case "HR":
                            objReturn = Properties.Resources.flag_croatia_24;
                            break;

                        case "HT":
                            objReturn = Properties.Resources.flag_haiti_24;
                            break;

                        case "HU":
                            objReturn = Properties.Resources.flag_hungary_24;
                            break;

                        case "ID":
                            objReturn = Properties.Resources.flag_indonesia_24;
                            break;

                        case "IE":
                            objReturn = Properties.Resources.flag_ireland_24;
                            break;

                        case "IL":
                            objReturn = Properties.Resources.flag_israel_24;
                            break;

                        case "IM":
                            objReturn = Properties.Resources.flag_isle_of_man_24;
                            break;

                        case "IN":
                            objReturn = Properties.Resources.flag_india_24;
                            break;

                        case "IO":
                            objReturn = Properties.Resources.flag_british_indian_ocean_24;
                            break;

                        case "IQ":
                            objReturn = Properties.Resources.flag_iraq_24;
                            break;

                        case "IR":
                            objReturn = Properties.Resources.flag_iran_24;
                            break;

                        case "IS":
                            objReturn = Properties.Resources.flag_iceland_24;
                            break;

                        case "IT":
                            objReturn = Properties.Resources.flag_italy_24;
                            break;

                        case "JE":
                            objReturn = Properties.Resources.flag_jersey_24;
                            break;

                        case "JM":
                            objReturn = Properties.Resources.flag_jamaica_24;
                            break;

                        case "JO":
                            objReturn = Properties.Resources.flag_jordan_24;
                            break;

                        case "JP":
                            objReturn = Properties.Resources.flag_japan_24;
                            break;

                        case "KE":
                            objReturn = Properties.Resources.flag_kenya_24;
                            break;

                        case "KG":
                            objReturn = Properties.Resources.flag_kyrgyzstan_24;
                            break;

                        case "KH":
                            objReturn = Properties.Resources.flag_cambodia_24;
                            break;

                        case "KI":
                            objReturn = Properties.Resources.flag_kiribati_24;
                            break;

                        case "KM":
                            objReturn = Properties.Resources.flag_comoros_24;
                            break;

                        case "KN":
                            objReturn = Properties.Resources.flag_saint_kitts_and_nevis_24;
                            break;

                        case "KP":
                            objReturn = Properties.Resources.flag_north_korea_24;
                            break;

                        case "KR":
                            objReturn = Properties.Resources.flag_south_korea_24;
                            break;

                        case "KW":
                            objReturn = Properties.Resources.flag_kuwait_24;
                            break;

                        case "KY":
                            objReturn = Properties.Resources.flag_cayman_islands_24;
                            break;

                        case "KZ":
                            objReturn = Properties.Resources.flag_kazakhstan_24;
                            break;

                        case "LA":
                            objReturn = Properties.Resources.flag_laos_24;
                            break;

                        case "LB":
                            objReturn = Properties.Resources.flag_lebanon_24;
                            break;

                        case "LC":
                            objReturn = Properties.Resources.flag_saint_lucia_24;
                            break;

                        case "LI":
                            objReturn = Properties.Resources.flag_liechtenstein_24;
                            break;

                        case "LK":
                            objReturn = Properties.Resources.flag_sri_lanka_24;
                            break;

                        case "LR":
                            objReturn = Properties.Resources.flag_liberia_24;
                            break;

                        case "LS":
                            objReturn = Properties.Resources.flag_lesotho_24;
                            break;

                        case "LT":
                            objReturn = Properties.Resources.flag_lithuania_24;
                            break;

                        case "LU":
                            objReturn = Properties.Resources.flag_luxembourg_24;
                            break;

                        case "LV":
                            objReturn = Properties.Resources.flag_latvia_24;
                            break;

                        case "LY":
                            objReturn = Properties.Resources.flag_libya_24;
                            break;

                        case "MA":
                            objReturn = Properties.Resources.flag_morocco_24;
                            break;

                        case "MC":
                            objReturn = Properties.Resources.flag_monaco_24;
                            break;

                        case "MD":
                            objReturn = Properties.Resources.flag_moldova_24;
                            break;

                        case "MG":
                            objReturn = Properties.Resources.flag_madagascar_24;
                            break;

                        case "MH":
                            objReturn = Properties.Resources.flag_marshall_islands_24;
                            break;

                        case "MK":
                            objReturn = Properties.Resources.flag_macedonia_24;
                            break;

                        case "ML":
                            objReturn = Properties.Resources.flag_mali_24;
                            break;

                        case "MM":
                            objReturn = Properties.Resources.flag_burma_24;
                            break;

                        case "MN":
                            objReturn = Properties.Resources.flag_mongolia_24;
                            break;

                        case "MO":
                            objReturn = Properties.Resources.flag_macau_24;
                            break;

                        case "MP":
                            objReturn = Properties.Resources.flag_northern_mariana_islands_24;
                            break;

                        case "MQ":
                            objReturn = Properties.Resources.flag_martinique_24;
                            break;

                        case "MR":
                            objReturn = Properties.Resources.flag_mauretania_24;
                            break;

                        case "MS":
                            objReturn = Properties.Resources.flag_montserrat_24;
                            break;

                        case "MT":
                            objReturn = Properties.Resources.flag_malta_24;
                            break;

                        case "MU":
                            objReturn = Properties.Resources.flag_mauritius_24;
                            break;

                        case "MV":
                            objReturn = Properties.Resources.flag_maledives_24;
                            break;

                        case "MW":
                            objReturn = Properties.Resources.flag_malawi_24;
                            break;

                        case "MX":
                            objReturn = Properties.Resources.flag_mexico_24;
                            break;

                        case "MY":
                            objReturn = Properties.Resources.flag_malaysia_24;
                            break;

                        case "MZ":
                            objReturn = Properties.Resources.flag_mozambique_24;
                            break;

                        case "NA":
                            objReturn = Properties.Resources.flag_namibia_24;
                            break;

                        case "NE":
                            objReturn = Properties.Resources.flag_niger_24;
                            break;

                        case "NF":
                            objReturn = Properties.Resources.flag_norfolk_islands_24;
                            break;

                        case "NG":
                            objReturn = Properties.Resources.flag_nigeria_24;
                            break;

                        case "NI":
                            objReturn = Properties.Resources.flag_nicaragua_24;
                            break;

                        case "NL":
                            objReturn = Properties.Resources.flag_netherlands_24;
                            break;

                        case "NO":
                            objReturn = Properties.Resources.flag_norway_24;
                            break;

                        case "NP":
                            objReturn = Properties.Resources.flag_nepal_24;
                            break;

                        case "NR":
                            objReturn = Properties.Resources.flag_nauru_24;
                            break;

                        case "NU":
                            objReturn = Properties.Resources.flag_niue_24;
                            break;

                        case "NZ":
                            objReturn = Properties.Resources.flag_new_zealand_24;
                            break;

                        case "OM":
                            objReturn = Properties.Resources.flag_oman_24;
                            break;

                        case "PA":
                            objReturn = Properties.Resources.flag_panama_24;
                            break;

                        case "PE":
                            objReturn = Properties.Resources.flag_peru_24;
                            break;

                        case "PF":
                            objReturn = Properties.Resources.flag_french_polynesia_24;
                            break;

                        case "PG":
                            objReturn = Properties.Resources.flag_papua_new_guinea_24;
                            break;

                        case "PH":
                            objReturn = Properties.Resources.flag_philippines_24;
                            break;

                        case "PK":
                            objReturn = Properties.Resources.flag_pakistan_24;
                            break;

                        case "PL":
                            objReturn = Properties.Resources.flag_poland_24;
                            break;

                        case "PM":
                            objReturn = Properties.Resources.flag_saint_pierre_and_miquelon_24;
                            break;

                        case "PN":
                            objReturn = Properties.Resources.flag_pitcairn_islands_24;
                            break;

                        case "PR":
                            objReturn = Properties.Resources.flag_puerto_rico_24;
                            break;

                        case "PT":
                            objReturn = Properties.Resources.flag_portugal_24;
                            break;

                        case "PW":
                            objReturn = Properties.Resources.flag_palau_24;
                            break;

                        case "PY":
                            objReturn = Properties.Resources.flag_paraquay_24;
                            break;

                        case "QA":
                            objReturn = Properties.Resources.flag_qatar_24;
                            break;

                        case "RO":
                            objReturn = Properties.Resources.flag_romania_24;
                            break;

                        case "RS":
                            objReturn = Properties.Resources.flag_serbia_montenegro_24;
                            break;

                        case "RU":
                            objReturn = Properties.Resources.flag_russia_24;
                            break;

                        case "RW":
                            objReturn = Properties.Resources.flag_rwanda_24;
                            break;

                        case "SA":
                            objReturn = Properties.Resources.flag_saudi_arabia_24;
                            break;

                        case "SB":
                            objReturn = Properties.Resources.flag_solomon_islands_24;
                            break;

                        case "SC":
                            objReturn = Properties.Resources.flag_seychelles_24;
                            break;

                        case "SD":
                            objReturn = Properties.Resources.flag_sudan_24;
                            break;

                        case "SE":
                            objReturn = Properties.Resources.flag_sweden_24;
                            break;

                        case "SG":
                            objReturn = Properties.Resources.flag_singapore_24;
                            break;

                        case "SH":
                            objReturn = Properties.Resources.flag_saint_helena_24;
                            break;

                        case "SI":
                            objReturn = Properties.Resources.flag_slovenia_24;
                            break;

                        case "SK":
                            objReturn = Properties.Resources.flag_slovakia_24;
                            break;

                        case "SL":
                            objReturn = Properties.Resources.flag_sierra_leone_24;
                            break;

                        case "SM":
                            objReturn = Properties.Resources.flag_san_marino_24;
                            break;

                        case "SN":
                            objReturn = Properties.Resources.flag_senegal_24;
                            break;

                        case "SO":
                            objReturn = Properties.Resources.flag_somalia_24;
                            break;

                        case "SR":
                            objReturn = Properties.Resources.flag_suriname_24;
                            break;

                        case "ST":
                            objReturn = Properties.Resources.flag_sao_tome_and_principe_24;
                            break;

                        case "SV":
                            objReturn = Properties.Resources.flag_el_salvador_24;
                            break;

                        case "SY":
                            objReturn = Properties.Resources.flag_syria_24;
                            break;

                        case "SZ":
                            objReturn = Properties.Resources.flag_swaziland_24;
                            break;

                        case "TC":
                            objReturn = Properties.Resources.flag_turks_and_caicos_islands_24;
                            break;

                        case "TD":
                            objReturn = Properties.Resources.flag_chad_24;
                            break;

                        case "TG":
                            objReturn = Properties.Resources.flag_togo_24;
                            break;

                        case "TH":
                            objReturn = Properties.Resources.flag_thailand_24;
                            break;

                        case "TI":
                            objReturn = Properties.Resources.flag_tibet_24;
                            break;

                        case "TJ":
                            objReturn = Properties.Resources.flag_tajikistan_24;
                            break;

                        case "TL":
                            objReturn = Properties.Resources.flag_east_timor_24;
                            break;

                        case "TM":
                            objReturn = Properties.Resources.flag_turkmenistan_24;
                            break;

                        case "TN":
                            objReturn = Properties.Resources.flag_tunisia_24;
                            break;

                        case "TO":
                            objReturn = Properties.Resources.flag_tonga_24;
                            break;

                        case "TR":
                            objReturn = Properties.Resources.flag_turkey_24;
                            break;

                        case "TT":
                            objReturn = Properties.Resources.flag_trinidad_and_tobago_24;
                            break;

                        case "TV":
                            objReturn = Properties.Resources.flag_tuvalu_24;
                            break;

                        case "TW":
                            objReturn = Properties.Resources.flag_taiwan_24;
                            break;

                        case "TZ":
                            objReturn = Properties.Resources.flag_tanzania_24;
                            break;

                        case "UA":
                            objReturn = Properties.Resources.flag_ukraine_24;
                            break;

                        case "UG":
                            objReturn = Properties.Resources.flag_uganda_24;
                            break;

                        case "US":
                            objReturn = Properties.Resources.flag_usa_24;
                            break;

                        case "UY":
                            objReturn = Properties.Resources.flag_uruquay_24;
                            break;

                        case "UZ":
                            objReturn = Properties.Resources.flag_uzbekistan_24;
                            break;

                        case "VA":
                            objReturn = Properties.Resources.flag_vatican_city_24;
                            break;

                        case "VC":
                            objReturn = Properties.Resources.flag_saint_vincent_and_grenadines_24;
                            break;

                        case "VE":
                            objReturn = Properties.Resources.flag_venezuela_24;
                            break;

                        case "VG":
                            objReturn = Properties.Resources.flag_british_virgin_islands_24;
                            break;

                        case "VI":
                            objReturn = Properties.Resources.flag_virgin_islands_24;
                            break;

                        case "VN":
                            objReturn = Properties.Resources.flag_vietnam_24;
                            break;

                        case "VU":
                            objReturn = Properties.Resources.flag_vanuatu_24;
                            break;

                        case "WF":
                            objReturn = Properties.Resources.flag_wallis_and_futuna_24;
                            break;

                        case "WS":
                            objReturn = Properties.Resources.flag_samoa_24;
                            break;

                        case "XE":
                            objReturn = Properties.Resources.flag_england_24;
                            break;

                        case "XS":
                            objReturn = Properties.Resources.flag_scotland_24;
                            break;

                        case "XW":
                            objReturn = Properties.Resources.flag_wales_24;
                            break;

                        case "YE":
                            objReturn = Properties.Resources.flag_yemen_24;
                            break;

                        case "ZA":
                            objReturn = Properties.Resources.flag_south_africa_24;
                            break;

                        case "ZM":
                            objReturn = Properties.Resources.flag_zambia_24;
                            break;

                        case "ZW":
                            objReturn = Properties.Resources.flag_zimbabwe_24;
                            break;

                        case "DEFAULT":
                            objReturn = Properties.Resources.defaulted_24;
                            break;

                        case "NOIMAGEDOTS":
                            objReturn = Properties.Resources.noimagedots_24;
                            break;

                        default:
                            Utils.BreakIfDebug();
                            goto case "DEFAULT";
                    }

                    break;
                }
                case FlagImageSize.Dpi192:
                {
                    switch (strCode)
                    {
                        case "AD":
                            objReturn = Properties.Resources.flag_andorra_32;
                            break;

                        case "AE":
                            objReturn = Properties.Resources.flag_united_arab_emirates_32;
                            break;

                        case "AF":
                            objReturn = Properties.Resources.flag_afghanistan_32;
                            break;

                        case "AG":
                            objReturn = Properties.Resources.flag_antigua_and_barbuda_32;
                            break;

                        case "AI":
                            objReturn = Properties.Resources.flag_anguilla_32;
                            break;

                        case "AL":
                            objReturn = Properties.Resources.flag_albania_32;
                            break;

                        case "AM":
                            objReturn = Properties.Resources.flag_armenia_32;
                            break;

                        case "AN":
                            objReturn = Properties.Resources.flag_netherlands_antilles_32;
                            break;

                        case "AO":
                            objReturn = Properties.Resources.flag_angola_32;
                            break;

                        case "AR":
                            objReturn = Properties.Resources.flag_argentina_32;
                            break;

                        case "AS":
                            objReturn = Properties.Resources.flag_american_samoa_32;
                            break;

                        case "AT":
                            objReturn = Properties.Resources.flag_austria_32;
                            break;

                        case "AU":
                            objReturn = Properties.Resources.flag_australia_32;
                            break;

                        case "AW":
                            objReturn = Properties.Resources.flag_aruba_32;
                            break;

                        case "AZ":
                            objReturn = Properties.Resources.flag_azerbaijan_32;
                            break;

                        case "BA":
                            objReturn = Properties.Resources.flag_bosnia_32;
                            break;

                        case "BB":
                            objReturn = Properties.Resources.flag_barbados_32;
                            break;

                        case "BD":
                            objReturn = Properties.Resources.flag_bangladesh_32;
                            break;

                        case "BE":
                            objReturn = Properties.Resources.flag_belgium_32;
                            break;

                        case "BF":
                            objReturn = Properties.Resources.flag_burkina_faso_32;
                            break;

                        case "BG":
                            objReturn = Properties.Resources.flag_bulgaria_32;
                            break;

                        case "BH":
                            objReturn = Properties.Resources.flag_bahrain_32;
                            break;

                        case "BI":
                            objReturn = Properties.Resources.flag_burundi_32;
                            break;

                        case "BJ":
                            objReturn = Properties.Resources.flag_benin_32;
                            break;

                        case "BM":
                            objReturn = Properties.Resources.flag_bermuda_32;
                            break;

                        case "BN":
                            objReturn = Properties.Resources.flag_brunei_32;
                            break;

                        case "BO":
                            objReturn = Properties.Resources.flag_bolivia_32;
                            break;

                        case "BR":
                            objReturn = Properties.Resources.flag_brazil_32;
                            break;

                        case "BS":
                            objReturn = Properties.Resources.flag_bahamas_32;
                            break;

                        case "BT":
                            objReturn = Properties.Resources.flag_bhutan_32;
                            break;

                        case "BW":
                            objReturn = Properties.Resources.flag_botswana_32;
                            break;

                        case "BY":
                            objReturn = Properties.Resources.flag_belarus_32;
                            break;

                        case "BZ":
                            objReturn = Properties.Resources.flag_belize_32;
                            break;

                        case "CA":
                            objReturn = Properties.Resources.flag_canada_32;
                            break;

                        case "CD":
                            objReturn = Properties.Resources.flag_congo_democratic_republic_32;
                            break;

                        case "CF":
                            objReturn = Properties.Resources.flag_central_african_republic_32;
                            break;

                        case "CG":
                            objReturn = Properties.Resources.flag_congo_republic_32;
                            break;

                        case "CH":
                            objReturn = Properties.Resources.flag_switzerland_32;
                            break;

                        case "CI":
                            objReturn = Properties.Resources.flag_cote_divoire_32;
                            break;

                        case "CK":
                            objReturn = Properties.Resources.flag_cook_islands_32;
                            break;

                        case "CL":
                            objReturn = Properties.Resources.flag_chile_32;
                            break;

                        case "CM":
                            objReturn = Properties.Resources.flag_cameroon_32;
                            break;

                        case "CN":
                            objReturn = Properties.Resources.flag_china_32;
                            break;

                        case "CO":
                            objReturn = Properties.Resources.flag_colombia_32;
                            break;

                        case "CR":
                            objReturn = Properties.Resources.flag_costa_rica_32;
                            break;

                        case "CU":
                            objReturn = Properties.Resources.flag_cuba_32;
                            break;

                        case "CV":
                            objReturn = Properties.Resources.flag_cape_verde_32;
                            break;

                        case "CY":
                            objReturn = Properties.Resources.flag_cyprus_32;
                            break;

                        case "CZ":
                            objReturn = Properties.Resources.flag_czech_republic_32;
                            break;

                        case "DE":
                            objReturn = Properties.Resources.flag_germany_32;
                            break;

                        case "DJ":
                            objReturn = Properties.Resources.flag_djibouti_32;
                            break;

                        case "DK":
                            objReturn = Properties.Resources.flag_denmark_32;
                            break;

                        case "DM":
                            objReturn = Properties.Resources.flag_dominica_32;
                            break;

                        case "DO":
                            objReturn = Properties.Resources.flag_dominican_republic_32;
                            break;

                        case "DZ":
                            objReturn = Properties.Resources.flag_algeria_32;
                            break;

                        case "EC":
                            objReturn = Properties.Resources.flag_equador_32;
                            break;

                        case "EE":
                            objReturn = Properties.Resources.flag_estonia_32;
                            break;

                        case "EG":
                            objReturn = Properties.Resources.flag_egypt_32;
                            break;

                        case "ER":
                            objReturn = Properties.Resources.flag_eritrea_32;
                            break;

                        case "ES":
                            objReturn = Properties.Resources.flag_spain_32;
                            break;

                        case "ET":
                            objReturn = Properties.Resources.flag_ethiopia_32;
                            break;

                        case "EU":
                            objReturn = Properties.Resources.flag_european_union_32;
                            break;

                        case "FI":
                            objReturn = Properties.Resources.flag_finland_32;
                            break;

                        case "FJ":
                            objReturn = Properties.Resources.flag_fiji_32;
                            break;

                        case "FK":
                            objReturn = Properties.Resources.flag_falkland_islands_32;
                            break;

                        case "FM":
                            objReturn = Properties.Resources.flag_micronesia_32;
                            break;

                        case "FO":
                            objReturn = Properties.Resources.flag_faroe_islands_32;
                            break;

                        case "FR":
                            objReturn = Properties.Resources.flag_france_32;
                            break;

                        case "GA":
                            objReturn = Properties.Resources.flag_gabon_32;
                            break;

                        case "GB":
                            objReturn = Properties.Resources.flag_great_britain_32;
                            break;

                        case "GD":
                            objReturn = Properties.Resources.flag_grenada_32;
                            break;

                        case "GE":
                            objReturn = Properties.Resources.flag_georgia_32;
                            break;

                        case "GG":
                            objReturn = Properties.Resources.flag_guernsey_32;
                            break;

                        case "GH":
                            objReturn = Properties.Resources.flag_ghana_32;
                            break;

                        case "GI":
                            objReturn = Properties.Resources.flag_gibraltar_32;
                            break;

                        case "GL":
                            objReturn = Properties.Resources.flag_greenland_32;
                            break;

                        case "GM":
                            objReturn = Properties.Resources.flag_gambia_32;
                            break;

                        case "GN":
                            objReturn = Properties.Resources.flag_guinea_32;
                            break;

                        case "GQ":
                            objReturn = Properties.Resources.flag_equatorial_guinea_32;
                            break;

                        case "GR":
                            objReturn = Properties.Resources.flag_greece_32;
                            break;

                        case "GS":
                            objReturn = Properties.Resources.flag_south_georgia_32;
                            break;

                        case "GT":
                            objReturn = Properties.Resources.flag_guatemala_32;
                            break;

                        case "GU":
                            objReturn = Properties.Resources.flag_guam_32;
                            break;

                        case "GW":
                            objReturn = Properties.Resources.flag_guinea_bissau_32;
                            break;

                        case "GY":
                            objReturn = Properties.Resources.flag_guyana_32;
                            break;

                        case "HK":
                            objReturn = Properties.Resources.flag_hong_kong_32;
                            break;

                        case "HN":
                            objReturn = Properties.Resources.flag_honduras_32;
                            break;

                        case "HR":
                            objReturn = Properties.Resources.flag_croatia_32;
                            break;

                        case "HT":
                            objReturn = Properties.Resources.flag_haiti_32;
                            break;

                        case "HU":
                            objReturn = Properties.Resources.flag_hungary_32;
                            break;

                        case "ID":
                            objReturn = Properties.Resources.flag_indonesia_32;
                            break;

                        case "IE":
                            objReturn = Properties.Resources.flag_ireland_32;
                            break;

                        case "IL":
                            objReturn = Properties.Resources.flag_israel_32;
                            break;

                        case "IM":
                            objReturn = Properties.Resources.flag_isle_of_man_32;
                            break;

                        case "IN":
                            objReturn = Properties.Resources.flag_india_32;
                            break;

                        case "IO":
                            objReturn = Properties.Resources.flag_british_indian_ocean_32;
                            break;

                        case "IQ":
                            objReturn = Properties.Resources.flag_iraq_32;
                            break;

                        case "IR":
                            objReturn = Properties.Resources.flag_iran_32;
                            break;

                        case "IS":
                            objReturn = Properties.Resources.flag_iceland_32;
                            break;

                        case "IT":
                            objReturn = Properties.Resources.flag_italy_32;
                            break;

                        case "JE":
                            objReturn = Properties.Resources.flag_jersey_32;
                            break;

                        case "JM":
                            objReturn = Properties.Resources.flag_jamaica_32;
                            break;

                        case "JO":
                            objReturn = Properties.Resources.flag_jordan_32;
                            break;

                        case "JP":
                            objReturn = Properties.Resources.flag_japan_32;
                            break;

                        case "KE":
                            objReturn = Properties.Resources.flag_kenya_32;
                            break;

                        case "KG":
                            objReturn = Properties.Resources.flag_kyrgyzstan_32;
                            break;

                        case "KH":
                            objReturn = Properties.Resources.flag_cambodia_32;
                            break;

                        case "KI":
                            objReturn = Properties.Resources.flag_kiribati_32;
                            break;

                        case "KM":
                            objReturn = Properties.Resources.flag_comoros_32;
                            break;

                        case "KN":
                            objReturn = Properties.Resources.flag_saint_kitts_and_nevis_32;
                            break;

                        case "KP":
                            objReturn = Properties.Resources.flag_north_korea_32;
                            break;

                        case "KR":
                            objReturn = Properties.Resources.flag_south_korea_32;
                            break;

                        case "KW":
                            objReturn = Properties.Resources.flag_kuwait_32;
                            break;

                        case "KY":
                            objReturn = Properties.Resources.flag_cayman_islands_32;
                            break;

                        case "KZ":
                            objReturn = Properties.Resources.flag_kazakhstan_32;
                            break;

                        case "LA":
                            objReturn = Properties.Resources.flag_laos_32;
                            break;

                        case "LB":
                            objReturn = Properties.Resources.flag_lebanon_32;
                            break;

                        case "LC":
                            objReturn = Properties.Resources.flag_saint_lucia_32;
                            break;

                        case "LI":
                            objReturn = Properties.Resources.flag_liechtenstein_32;
                            break;

                        case "LK":
                            objReturn = Properties.Resources.flag_sri_lanka_32;
                            break;

                        case "LR":
                            objReturn = Properties.Resources.flag_liberia_32;
                            break;

                        case "LS":
                            objReturn = Properties.Resources.flag_lesotho_32;
                            break;

                        case "LT":
                            objReturn = Properties.Resources.flag_lithuania_32;
                            break;

                        case "LU":
                            objReturn = Properties.Resources.flag_luxembourg_32;
                            break;

                        case "LV":
                            objReturn = Properties.Resources.flag_latvia_32;
                            break;

                        case "LY":
                            objReturn = Properties.Resources.flag_libya_32;
                            break;

                        case "MA":
                            objReturn = Properties.Resources.flag_morocco_32;
                            break;

                        case "MC":
                            objReturn = Properties.Resources.flag_monaco_32;
                            break;

                        case "MD":
                            objReturn = Properties.Resources.flag_moldova_32;
                            break;

                        case "MG":
                            objReturn = Properties.Resources.flag_madagascar_32;
                            break;

                        case "MH":
                            objReturn = Properties.Resources.flag_marshall_islands_32;
                            break;

                        case "MK":
                            objReturn = Properties.Resources.flag_macedonia_32;
                            break;

                        case "ML":
                            objReturn = Properties.Resources.flag_mali_32;
                            break;

                        case "MM":
                            objReturn = Properties.Resources.flag_burma_32;
                            break;

                        case "MN":
                            objReturn = Properties.Resources.flag_mongolia_32;
                            break;

                        case "MO":
                            objReturn = Properties.Resources.flag_macau_32;
                            break;

                        case "MP":
                            objReturn = Properties.Resources.flag_northern_mariana_islands_32;
                            break;

                        case "MQ":
                            objReturn = Properties.Resources.flag_martinique_32;
                            break;

                        case "MR":
                            objReturn = Properties.Resources.flag_mauretania_32;
                            break;

                        case "MS":
                            objReturn = Properties.Resources.flag_montserrat_32;
                            break;

                        case "MT":
                            objReturn = Properties.Resources.flag_malta_32;
                            break;

                        case "MU":
                            objReturn = Properties.Resources.flag_mauritius_32;
                            break;

                        case "MV":
                            objReturn = Properties.Resources.flag_maledives_32;
                            break;

                        case "MW":
                            objReturn = Properties.Resources.flag_malawi_32;
                            break;

                        case "MX":
                            objReturn = Properties.Resources.flag_mexico_32;
                            break;

                        case "MY":
                            objReturn = Properties.Resources.flag_malaysia_32;
                            break;

                        case "MZ":
                            objReturn = Properties.Resources.flag_mozambique_32;
                            break;

                        case "NA":
                            objReturn = Properties.Resources.flag_namibia_32;
                            break;

                        case "NE":
                            objReturn = Properties.Resources.flag_niger_32;
                            break;

                        case "NF":
                            objReturn = Properties.Resources.flag_norfolk_islands_32;
                            break;

                        case "NG":
                            objReturn = Properties.Resources.flag_nigeria_32;
                            break;

                        case "NI":
                            objReturn = Properties.Resources.flag_nicaragua_32;
                            break;

                        case "NL":
                            objReturn = Properties.Resources.flag_netherlands_32;
                            break;

                        case "NO":
                            objReturn = Properties.Resources.flag_norway_32;
                            break;

                        case "NP":
                            objReturn = Properties.Resources.flag_nepal_32;
                            break;

                        case "NR":
                            objReturn = Properties.Resources.flag_nauru_32;
                            break;

                        case "NU":
                            objReturn = Properties.Resources.flag_niue_32;
                            break;

                        case "NZ":
                            objReturn = Properties.Resources.flag_new_zealand_32;
                            break;

                        case "OM":
                            objReturn = Properties.Resources.flag_oman_32;
                            break;

                        case "PA":
                            objReturn = Properties.Resources.flag_panama_32;
                            break;

                        case "PE":
                            objReturn = Properties.Resources.flag_peru_32;
                            break;

                        case "PF":
                            objReturn = Properties.Resources.flag_french_polynesia_32;
                            break;

                        case "PG":
                            objReturn = Properties.Resources.flag_papua_new_guinea_32;
                            break;

                        case "PH":
                            objReturn = Properties.Resources.flag_philippines_32;
                            break;

                        case "PK":
                            objReturn = Properties.Resources.flag_pakistan_32;
                            break;

                        case "PL":
                            objReturn = Properties.Resources.flag_poland_32;
                            break;

                        case "PM":
                            objReturn = Properties.Resources.flag_saint_pierre_and_miquelon_32;
                            break;

                        case "PN":
                            objReturn = Properties.Resources.flag_pitcairn_islands_32;
                            break;

                        case "PR":
                            objReturn = Properties.Resources.flag_puerto_rico_32;
                            break;

                        case "PT":
                            objReturn = Properties.Resources.flag_portugal_32;
                            break;

                        case "PW":
                            objReturn = Properties.Resources.flag_palau_32;
                            break;

                        case "PY":
                            objReturn = Properties.Resources.flag_paraquay_32;
                            break;

                        case "QA":
                            objReturn = Properties.Resources.flag_qatar_32;
                            break;

                        case "RO":
                            objReturn = Properties.Resources.flag_romania_32;
                            break;

                        case "RS":
                            objReturn = Properties.Resources.flag_serbia_montenegro_32;
                            break;

                        case "RU":
                            objReturn = Properties.Resources.flag_russia_32;
                            break;

                        case "RW":
                            objReturn = Properties.Resources.flag_rwanda_32;
                            break;

                        case "SA":
                            objReturn = Properties.Resources.flag_saudi_arabia_32;
                            break;

                        case "SB":
                            objReturn = Properties.Resources.flag_solomon_islands_32;
                            break;

                        case "SC":
                            objReturn = Properties.Resources.flag_seychelles_32;
                            break;

                        case "SD":
                            objReturn = Properties.Resources.flag_sudan_32;
                            break;

                        case "SE":
                            objReturn = Properties.Resources.flag_sweden_32;
                            break;

                        case "SG":
                            objReturn = Properties.Resources.flag_singapore_32;
                            break;

                        case "SH":
                            objReturn = Properties.Resources.flag_saint_helena_32;
                            break;

                        case "SI":
                            objReturn = Properties.Resources.flag_slovenia_32;
                            break;

                        case "SK":
                            objReturn = Properties.Resources.flag_slovakia_32;
                            break;

                        case "SL":
                            objReturn = Properties.Resources.flag_sierra_leone_32;
                            break;

                        case "SM":
                            objReturn = Properties.Resources.flag_san_marino_32;
                            break;

                        case "SN":
                            objReturn = Properties.Resources.flag_senegal_32;
                            break;

                        case "SO":
                            objReturn = Properties.Resources.flag_somalia_32;
                            break;

                        case "SR":
                            objReturn = Properties.Resources.flag_suriname_32;
                            break;

                        case "ST":
                            objReturn = Properties.Resources.flag_sao_tome_and_principe_32;
                            break;

                        case "SV":
                            objReturn = Properties.Resources.flag_el_salvador_32;
                            break;

                        case "SY":
                            objReturn = Properties.Resources.flag_syria_32;
                            break;

                        case "SZ":
                            objReturn = Properties.Resources.flag_swaziland_32;
                            break;

                        case "TC":
                            objReturn = Properties.Resources.flag_turks_and_caicos_islands_32;
                            break;

                        case "TD":
                            objReturn = Properties.Resources.flag_chad_32;
                            break;

                        case "TG":
                            objReturn = Properties.Resources.flag_togo_32;
                            break;

                        case "TH":
                            objReturn = Properties.Resources.flag_thailand_32;
                            break;

                        case "TI":
                            objReturn = Properties.Resources.flag_tibet_32;
                            break;

                        case "TJ":
                            objReturn = Properties.Resources.flag_tajikistan_32;
                            break;

                        case "TL":
                            objReturn = Properties.Resources.flag_east_timor_32;
                            break;

                        case "TM":
                            objReturn = Properties.Resources.flag_turkmenistan_32;
                            break;

                        case "TN":
                            objReturn = Properties.Resources.flag_tunisia_32;
                            break;

                        case "TO":
                            objReturn = Properties.Resources.flag_tonga_32;
                            break;

                        case "TR":
                            objReturn = Properties.Resources.flag_turkey_32;
                            break;

                        case "TT":
                            objReturn = Properties.Resources.flag_trinidad_and_tobago_32;
                            break;

                        case "TV":
                            objReturn = Properties.Resources.flag_tuvalu_32;
                            break;

                        case "TW":
                            objReturn = Properties.Resources.flag_taiwan_32;
                            break;

                        case "TZ":
                            objReturn = Properties.Resources.flag_tanzania_32;
                            break;

                        case "UA":
                            objReturn = Properties.Resources.flag_ukraine_32;
                            break;

                        case "UG":
                            objReturn = Properties.Resources.flag_uganda_32;
                            break;

                        case "US":
                            objReturn = Properties.Resources.flag_usa_32;
                            break;

                        case "UY":
                            objReturn = Properties.Resources.flag_uruquay_32;
                            break;

                        case "UZ":
                            objReturn = Properties.Resources.flag_uzbekistan_32;
                            break;

                        case "VA":
                            objReturn = Properties.Resources.flag_vatican_city_32;
                            break;

                        case "VC":
                            objReturn = Properties.Resources.flag_saint_vincent_and_grenadines_32;
                            break;

                        case "VE":
                            objReturn = Properties.Resources.flag_venezuela_32;
                            break;

                        case "VG":
                            objReturn = Properties.Resources.flag_british_virgin_islands_32;
                            break;

                        case "VI":
                            objReturn = Properties.Resources.flag_virgin_islands_32;
                            break;

                        case "VN":
                            objReturn = Properties.Resources.flag_vietnam_32;
                            break;

                        case "VU":
                            objReturn = Properties.Resources.flag_vanuatu_32;
                            break;

                        case "WF":
                            objReturn = Properties.Resources.flag_wallis_and_futuna_32;
                            break;

                        case "WS":
                            objReturn = Properties.Resources.flag_samoa_32;
                            break;

                        case "XE":
                            objReturn = Properties.Resources.flag_england_32;
                            break;

                        case "XS":
                            objReturn = Properties.Resources.flag_scotland_32;
                            break;

                        case "XW":
                            objReturn = Properties.Resources.flag_wales_32;
                            break;

                        case "YE":
                            objReturn = Properties.Resources.flag_yemen_32;
                            break;

                        case "ZA":
                            objReturn = Properties.Resources.flag_south_africa_32;
                            break;

                        case "ZM":
                            objReturn = Properties.Resources.flag_zambia_32;
                            break;

                        case "ZW":
                            objReturn = Properties.Resources.flag_zimbabwe_32;
                            break;

                        case "DEFAULT":
                            objReturn = Properties.Resources.defaulted_32;
                            break;

                        case "NOIMAGEDOTS":
                            objReturn = Properties.Resources.noimagedots_32;
                            break;

                        default:
                            Utils.BreakIfDebug();
                            goto case "DEFAULT";
                    }

                    break;
                }
                case FlagImageSize.Dpi288:
                {
                    switch (strCode)
                    {
                        case "AD":
                            objReturn = Properties.Resources.flag_andorra_48;
                            break;

                        case "AE":
                            objReturn = Properties.Resources.flag_united_arab_emirates_48;
                            break;

                        case "AF":
                            objReturn = Properties.Resources.flag_afghanistan_48;
                            break;

                        case "AG":
                            objReturn = Properties.Resources.flag_antigua_and_barbuda_48;
                            break;

                        case "AI":
                            objReturn = Properties.Resources.flag_anguilla_48;
                            break;

                        case "AL":
                            objReturn = Properties.Resources.flag_albania_48;
                            break;

                        case "AM":
                            objReturn = Properties.Resources.flag_armenia_48;
                            break;

                        case "AN":
                            objReturn = Properties.Resources.flag_netherlands_antilles_48;
                            break;

                        case "AO":
                            objReturn = Properties.Resources.flag_angola_48;
                            break;

                        case "AR":
                            objReturn = Properties.Resources.flag_argentina_48;
                            break;

                        case "AS":
                            objReturn = Properties.Resources.flag_american_samoa_48;
                            break;

                        case "AT":
                            objReturn = Properties.Resources.flag_austria_48;
                            break;

                        case "AU":
                            objReturn = Properties.Resources.flag_australia_48;
                            break;

                        case "AW":
                            objReturn = Properties.Resources.flag_aruba_48;
                            break;

                        case "AZ":
                            objReturn = Properties.Resources.flag_azerbaijan_48;
                            break;

                        case "BA":
                            objReturn = Properties.Resources.flag_bosnia_48;
                            break;

                        case "BB":
                            objReturn = Properties.Resources.flag_barbados_48;
                            break;

                        case "BD":
                            objReturn = Properties.Resources.flag_bangladesh_48;
                            break;

                        case "BE":
                            objReturn = Properties.Resources.flag_belgium_48;
                            break;

                        case "BF":
                            objReturn = Properties.Resources.flag_burkina_faso_48;
                            break;

                        case "BG":
                            objReturn = Properties.Resources.flag_bulgaria_48;
                            break;

                        case "BH":
                            objReturn = Properties.Resources.flag_bahrain_48;
                            break;

                        case "BI":
                            objReturn = Properties.Resources.flag_burundi_48;
                            break;

                        case "BJ":
                            objReturn = Properties.Resources.flag_benin_48;
                            break;

                        case "BM":
                            objReturn = Properties.Resources.flag_bermuda_48;
                            break;

                        case "BN":
                            objReturn = Properties.Resources.flag_brunei_48;
                            break;

                        case "BO":
                            objReturn = Properties.Resources.flag_bolivia_48;
                            break;

                        case "BR":
                            objReturn = Properties.Resources.flag_brazil_48;
                            break;

                        case "BS":
                            objReturn = Properties.Resources.flag_bahamas_48;
                            break;

                        case "BT":
                            objReturn = Properties.Resources.flag_bhutan_48;
                            break;

                        case "BW":
                            objReturn = Properties.Resources.flag_botswana_48;
                            break;

                        case "BY":
                            objReturn = Properties.Resources.flag_belarus_48;
                            break;

                        case "BZ":
                            objReturn = Properties.Resources.flag_belize_48;
                            break;

                        case "CA":
                            objReturn = Properties.Resources.flag_canada_48;
                            break;

                        case "CD":
                            objReturn = Properties.Resources.flag_congo_democratic_republic_48;
                            break;

                        case "CF":
                            objReturn = Properties.Resources.flag_central_african_republic_48;
                            break;

                        case "CG":
                            objReturn = Properties.Resources.flag_congo_republic_48;
                            break;

                        case "CH":
                            objReturn = Properties.Resources.flag_switzerland_48;
                            break;

                        case "CI":
                            objReturn = Properties.Resources.flag_cote_divoire_48;
                            break;

                        case "CK":
                            objReturn = Properties.Resources.flag_cook_islands_48;
                            break;

                        case "CL":
                            objReturn = Properties.Resources.flag_chile_48;
                            break;

                        case "CM":
                            objReturn = Properties.Resources.flag_cameroon_48;
                            break;

                        case "CN":
                            objReturn = Properties.Resources.flag_china_48;
                            break;

                        case "CO":
                            objReturn = Properties.Resources.flag_colombia_48;
                            break;

                        case "CR":
                            objReturn = Properties.Resources.flag_costa_rica_48;
                            break;

                        case "CU":
                            objReturn = Properties.Resources.flag_cuba_48;
                            break;

                        case "CV":
                            objReturn = Properties.Resources.flag_cape_verde_48;
                            break;

                        case "CY":
                            objReturn = Properties.Resources.flag_cyprus_48;
                            break;

                        case "CZ":
                            objReturn = Properties.Resources.flag_czech_republic_48;
                            break;

                        case "DE":
                            objReturn = Properties.Resources.flag_germany_48;
                            break;

                        case "DJ":
                            objReturn = Properties.Resources.flag_djibouti_48;
                            break;

                        case "DK":
                            objReturn = Properties.Resources.flag_denmark_48;
                            break;

                        case "DM":
                            objReturn = Properties.Resources.flag_dominica_48;
                            break;

                        case "DO":
                            objReturn = Properties.Resources.flag_dominican_republic_48;
                            break;

                        case "DZ":
                            objReturn = Properties.Resources.flag_algeria_48;
                            break;

                        case "EC":
                            objReturn = Properties.Resources.flag_equador_48;
                            break;

                        case "EE":
                            objReturn = Properties.Resources.flag_estonia_48;
                            break;

                        case "EG":
                            objReturn = Properties.Resources.flag_egypt_48;
                            break;

                        case "ER":
                            objReturn = Properties.Resources.flag_eritrea_48;
                            break;

                        case "ES":
                            objReturn = Properties.Resources.flag_spain_48;
                            break;

                        case "ET":
                            objReturn = Properties.Resources.flag_ethiopia_48;
                            break;

                        case "EU":
                            objReturn = Properties.Resources.flag_european_union_48;
                            break;

                        case "FI":
                            objReturn = Properties.Resources.flag_finland_48;
                            break;

                        case "FJ":
                            objReturn = Properties.Resources.flag_fiji_48;
                            break;

                        case "FK":
                            objReturn = Properties.Resources.flag_falkland_islands_48;
                            break;

                        case "FM":
                            objReturn = Properties.Resources.flag_micronesia_48;
                            break;

                        case "FO":
                            objReturn = Properties.Resources.flag_faroe_islands_48;
                            break;

                        case "FR":
                            objReturn = Properties.Resources.flag_france_48;
                            break;

                        case "GA":
                            objReturn = Properties.Resources.flag_gabon_48;
                            break;

                        case "GB":
                            objReturn = Properties.Resources.flag_great_britain_48;
                            break;

                        case "GD":
                            objReturn = Properties.Resources.flag_grenada_48;
                            break;

                        case "GE":
                            objReturn = Properties.Resources.flag_georgia_48;
                            break;

                        case "GG":
                            objReturn = Properties.Resources.flag_guernsey_48;
                            break;

                        case "GH":
                            objReturn = Properties.Resources.flag_ghana_48;
                            break;

                        case "GI":
                            objReturn = Properties.Resources.flag_gibraltar_48;
                            break;

                        case "GL":
                            objReturn = Properties.Resources.flag_greenland_48;
                            break;

                        case "GM":
                            objReturn = Properties.Resources.flag_gambia_48;
                            break;

                        case "GN":
                            objReturn = Properties.Resources.flag_guinea_48;
                            break;

                        case "GQ":
                            objReturn = Properties.Resources.flag_equatorial_guinea_48;
                            break;

                        case "GR":
                            objReturn = Properties.Resources.flag_greece_48;
                            break;

                        case "GS":
                            objReturn = Properties.Resources.flag_south_georgia_48;
                            break;

                        case "GT":
                            objReturn = Properties.Resources.flag_guatemala_48;
                            break;

                        case "GU":
                            objReturn = Properties.Resources.flag_guam_48;
                            break;

                        case "GW":
                            objReturn = Properties.Resources.flag_guinea_bissau_48;
                            break;

                        case "GY":
                            objReturn = Properties.Resources.flag_guyana_48;
                            break;

                        case "HK":
                            objReturn = Properties.Resources.flag_hong_kong_48;
                            break;

                        case "HN":
                            objReturn = Properties.Resources.flag_honduras_48;
                            break;

                        case "HR":
                            objReturn = Properties.Resources.flag_croatia_48;
                            break;

                        case "HT":
                            objReturn = Properties.Resources.flag_haiti_48;
                            break;

                        case "HU":
                            objReturn = Properties.Resources.flag_hungary_48;
                            break;

                        case "ID":
                            objReturn = Properties.Resources.flag_indonesia_48;
                            break;

                        case "IE":
                            objReturn = Properties.Resources.flag_ireland_48;
                            break;

                        case "IL":
                            objReturn = Properties.Resources.flag_israel_48;
                            break;

                        case "IM":
                            objReturn = Properties.Resources.flag_isle_of_man_48;
                            break;

                        case "IN":
                            objReturn = Properties.Resources.flag_india_48;
                            break;

                        case "IO":
                            objReturn = Properties.Resources.flag_british_indian_ocean_48;
                            break;

                        case "IQ":
                            objReturn = Properties.Resources.flag_iraq_48;
                            break;

                        case "IR":
                            objReturn = Properties.Resources.flag_iran_48;
                            break;

                        case "IS":
                            objReturn = Properties.Resources.flag_iceland_48;
                            break;

                        case "IT":
                            objReturn = Properties.Resources.flag_italy_48;
                            break;

                        case "JE":
                            objReturn = Properties.Resources.flag_jersey_48;
                            break;

                        case "JM":
                            objReturn = Properties.Resources.flag_jamaica_48;
                            break;

                        case "JO":
                            objReturn = Properties.Resources.flag_jordan_48;
                            break;

                        case "JP":
                            objReturn = Properties.Resources.flag_japan_48;
                            break;

                        case "KE":
                            objReturn = Properties.Resources.flag_kenya_48;
                            break;

                        case "KG":
                            objReturn = Properties.Resources.flag_kyrgyzstan_48;
                            break;

                        case "KH":
                            objReturn = Properties.Resources.flag_cambodia_48;
                            break;

                        case "KI":
                            objReturn = Properties.Resources.flag_kiribati_48;
                            break;

                        case "KM":
                            objReturn = Properties.Resources.flag_comoros_48;
                            break;

                        case "KN":
                            objReturn = Properties.Resources.flag_saint_kitts_and_nevis_48;
                            break;

                        case "KP":
                            objReturn = Properties.Resources.flag_north_korea_48;
                            break;

                        case "KR":
                            objReturn = Properties.Resources.flag_south_korea_48;
                            break;

                        case "KW":
                            objReturn = Properties.Resources.flag_kuwait_48;
                            break;

                        case "KY":
                            objReturn = Properties.Resources.flag_cayman_islands_48;
                            break;

                        case "KZ":
                            objReturn = Properties.Resources.flag_kazakhstan_48;
                            break;

                        case "LA":
                            objReturn = Properties.Resources.flag_laos_48;
                            break;

                        case "LB":
                            objReturn = Properties.Resources.flag_lebanon_48;
                            break;

                        case "LC":
                            objReturn = Properties.Resources.flag_saint_lucia_48;
                            break;

                        case "LI":
                            objReturn = Properties.Resources.flag_liechtenstein_48;
                            break;

                        case "LK":
                            objReturn = Properties.Resources.flag_sri_lanka_48;
                            break;

                        case "LR":
                            objReturn = Properties.Resources.flag_liberia_48;
                            break;

                        case "LS":
                            objReturn = Properties.Resources.flag_lesotho_48;
                            break;

                        case "LT":
                            objReturn = Properties.Resources.flag_lithuania_48;
                            break;

                        case "LU":
                            objReturn = Properties.Resources.flag_luxembourg_48;
                            break;

                        case "LV":
                            objReturn = Properties.Resources.flag_latvia_48;
                            break;

                        case "LY":
                            objReturn = Properties.Resources.flag_libya_48;
                            break;

                        case "MA":
                            objReturn = Properties.Resources.flag_morocco_48;
                            break;

                        case "MC":
                            objReturn = Properties.Resources.flag_monaco_48;
                            break;

                        case "MD":
                            objReturn = Properties.Resources.flag_moldova_48;
                            break;

                        case "MG":
                            objReturn = Properties.Resources.flag_madagascar_48;
                            break;

                        case "MH":
                            objReturn = Properties.Resources.flag_marshall_islands_48;
                            break;

                        case "MK":
                            objReturn = Properties.Resources.flag_macedonia_48;
                            break;

                        case "ML":
                            objReturn = Properties.Resources.flag_mali_48;
                            break;

                        case "MM":
                            objReturn = Properties.Resources.flag_burma_48;
                            break;

                        case "MN":
                            objReturn = Properties.Resources.flag_mongolia_48;
                            break;

                        case "MO":
                            objReturn = Properties.Resources.flag_macau_48;
                            break;

                        case "MP":
                            objReturn = Properties.Resources.flag_northern_mariana_islands_48;
                            break;

                        case "MQ":
                            objReturn = Properties.Resources.flag_martinique_48;
                            break;

                        case "MR":
                            objReturn = Properties.Resources.flag_mauretania_48;
                            break;

                        case "MS":
                            objReturn = Properties.Resources.flag_montserrat_48;
                            break;

                        case "MT":
                            objReturn = Properties.Resources.flag_malta_48;
                            break;

                        case "MU":
                            objReturn = Properties.Resources.flag_mauritius_48;
                            break;

                        case "MV":
                            objReturn = Properties.Resources.flag_maledives_48;
                            break;

                        case "MW":
                            objReturn = Properties.Resources.flag_malawi_48;
                            break;

                        case "MX":
                            objReturn = Properties.Resources.flag_mexico_48;
                            break;

                        case "MY":
                            objReturn = Properties.Resources.flag_malaysia_48;
                            break;

                        case "MZ":
                            objReturn = Properties.Resources.flag_mozambique_48;
                            break;

                        case "NA":
                            objReturn = Properties.Resources.flag_namibia_48;
                            break;

                        case "NE":
                            objReturn = Properties.Resources.flag_niger_48;
                            break;

                        case "NF":
                            objReturn = Properties.Resources.flag_norfolk_islands_48;
                            break;

                        case "NG":
                            objReturn = Properties.Resources.flag_nigeria_48;
                            break;

                        case "NI":
                            objReturn = Properties.Resources.flag_nicaragua_48;
                            break;

                        case "NL":
                            objReturn = Properties.Resources.flag_netherlands_48;
                            break;

                        case "NO":
                            objReturn = Properties.Resources.flag_norway_48;
                            break;

                        case "NP":
                            objReturn = Properties.Resources.flag_nepal_48;
                            break;

                        case "NR":
                            objReturn = Properties.Resources.flag_nauru_48;
                            break;

                        case "NU":
                            objReturn = Properties.Resources.flag_niue_48;
                            break;

                        case "NZ":
                            objReturn = Properties.Resources.flag_new_zealand_48;
                            break;

                        case "OM":
                            objReturn = Properties.Resources.flag_oman_48;
                            break;

                        case "PA":
                            objReturn = Properties.Resources.flag_panama_48;
                            break;

                        case "PE":
                            objReturn = Properties.Resources.flag_peru_48;
                            break;

                        case "PF":
                            objReturn = Properties.Resources.flag_french_polynesia_48;
                            break;

                        case "PG":
                            objReturn = Properties.Resources.flag_papua_new_guinea_48;
                            break;

                        case "PH":
                            objReturn = Properties.Resources.flag_philippines_48;
                            break;

                        case "PK":
                            objReturn = Properties.Resources.flag_pakistan_48;
                            break;

                        case "PL":
                            objReturn = Properties.Resources.flag_poland_48;
                            break;

                        case "PM":
                            objReturn = Properties.Resources.flag_saint_pierre_and_miquelon_48;
                            break;

                        case "PN":
                            objReturn = Properties.Resources.flag_pitcairn_islands_48;
                            break;

                        case "PR":
                            objReturn = Properties.Resources.flag_puerto_rico_48;
                            break;

                        case "PT":
                            objReturn = Properties.Resources.flag_portugal_48;
                            break;

                        case "PW":
                            objReturn = Properties.Resources.flag_palau_48;
                            break;

                        case "PY":
                            objReturn = Properties.Resources.flag_paraquay_48;
                            break;

                        case "QA":
                            objReturn = Properties.Resources.flag_qatar_48;
                            break;

                        case "RO":
                            objReturn = Properties.Resources.flag_romania_48;
                            break;

                        case "RS":
                            objReturn = Properties.Resources.flag_serbia_montenegro_48;
                            break;

                        case "RU":
                            objReturn = Properties.Resources.flag_russia_48;
                            break;

                        case "RW":
                            objReturn = Properties.Resources.flag_rwanda_48;
                            break;

                        case "SA":
                            objReturn = Properties.Resources.flag_saudi_arabia_48;
                            break;

                        case "SB":
                            objReturn = Properties.Resources.flag_solomon_islands_48;
                            break;

                        case "SC":
                            objReturn = Properties.Resources.flag_seychelles_48;
                            break;

                        case "SD":
                            objReturn = Properties.Resources.flag_sudan_48;
                            break;

                        case "SE":
                            objReturn = Properties.Resources.flag_sweden_48;
                            break;

                        case "SG":
                            objReturn = Properties.Resources.flag_singapore_48;
                            break;

                        case "SH":
                            objReturn = Properties.Resources.flag_saint_helena_48;
                            break;

                        case "SI":
                            objReturn = Properties.Resources.flag_slovenia_48;
                            break;

                        case "SK":
                            objReturn = Properties.Resources.flag_slovakia_48;
                            break;

                        case "SL":
                            objReturn = Properties.Resources.flag_sierra_leone_48;
                            break;

                        case "SM":
                            objReturn = Properties.Resources.flag_san_marino_48;
                            break;

                        case "SN":
                            objReturn = Properties.Resources.flag_senegal_48;
                            break;

                        case "SO":
                            objReturn = Properties.Resources.flag_somalia_48;
                            break;

                        case "SR":
                            objReturn = Properties.Resources.flag_suriname_48;
                            break;

                        case "ST":
                            objReturn = Properties.Resources.flag_sao_tome_and_principe_48;
                            break;

                        case "SV":
                            objReturn = Properties.Resources.flag_el_salvador_48;
                            break;

                        case "SY":
                            objReturn = Properties.Resources.flag_syria_48;
                            break;

                        case "SZ":
                            objReturn = Properties.Resources.flag_swaziland_48;
                            break;

                        case "TC":
                            objReturn = Properties.Resources.flag_turks_and_caicos_islands_48;
                            break;

                        case "TD":
                            objReturn = Properties.Resources.flag_chad_48;
                            break;

                        case "TG":
                            objReturn = Properties.Resources.flag_togo_48;
                            break;

                        case "TH":
                            objReturn = Properties.Resources.flag_thailand_48;
                            break;

                        case "TI":
                            objReturn = Properties.Resources.flag_tibet_48;
                            break;

                        case "TJ":
                            objReturn = Properties.Resources.flag_tajikistan_48;
                            break;

                        case "TL":
                            objReturn = Properties.Resources.flag_east_timor_48;
                            break;

                        case "TM":
                            objReturn = Properties.Resources.flag_turkmenistan_48;
                            break;

                        case "TN":
                            objReturn = Properties.Resources.flag_tunisia_48;
                            break;

                        case "TO":
                            objReturn = Properties.Resources.flag_tonga_48;
                            break;

                        case "TR":
                            objReturn = Properties.Resources.flag_turkey_48;
                            break;

                        case "TT":
                            objReturn = Properties.Resources.flag_trinidad_and_tobago_48;
                            break;

                        case "TV":
                            objReturn = Properties.Resources.flag_tuvalu_48;
                            break;

                        case "TW":
                            objReturn = Properties.Resources.flag_taiwan_48;
                            break;

                        case "TZ":
                            objReturn = Properties.Resources.flag_tanzania_48;
                            break;

                        case "UA":
                            objReturn = Properties.Resources.flag_ukraine_48;
                            break;

                        case "UG":
                            objReturn = Properties.Resources.flag_uganda_48;
                            break;

                        case "US":
                            objReturn = Properties.Resources.flag_usa_48;
                            break;

                        case "UY":
                            objReturn = Properties.Resources.flag_uruquay_48;
                            break;

                        case "UZ":
                            objReturn = Properties.Resources.flag_uzbekistan_48;
                            break;

                        case "VA":
                            objReturn = Properties.Resources.flag_vatican_city_48;
                            break;

                        case "VC":
                            objReturn = Properties.Resources.flag_saint_vincent_and_grenadines_48;
                            break;

                        case "VE":
                            objReturn = Properties.Resources.flag_venezuela_48;
                            break;

                        case "VG":
                            objReturn = Properties.Resources.flag_british_virgin_islands_48;
                            break;

                        case "VI":
                            objReturn = Properties.Resources.flag_virgin_islands_48;
                            break;

                        case "VN":
                            objReturn = Properties.Resources.flag_vietnam_48;
                            break;

                        case "VU":
                            objReturn = Properties.Resources.flag_vanuatu_48;
                            break;

                        case "WF":
                            objReturn = Properties.Resources.flag_wallis_and_futuna_48;
                            break;

                        case "WS":
                            objReturn = Properties.Resources.flag_samoa_48;
                            break;

                        case "XE":
                            objReturn = Properties.Resources.flag_england_48;
                            break;

                        case "XS":
                            objReturn = Properties.Resources.flag_scotland_48;
                            break;

                        case "XW":
                            objReturn = Properties.Resources.flag_wales_48;
                            break;

                        case "YE":
                            objReturn = Properties.Resources.flag_yemen_48;
                            break;

                        case "ZA":
                            objReturn = Properties.Resources.flag_south_africa_48;
                            break;

                        case "ZM":
                            objReturn = Properties.Resources.flag_zambia_48;
                            break;

                        case "ZW":
                            objReturn = Properties.Resources.flag_zimbabwe_48;
                            break;

                        case "DEFAULT":
                            objReturn = Properties.Resources.defaulted_48;
                            break;

                        case "NOIMAGEDOTS":
                            objReturn = Properties.Resources.noimagedots_48;
                            break;

                        default:
                            Utils.BreakIfDebug();
                            goto case "DEFAULT";
                    }

                    break;
                }
                case FlagImageSize.Dpi384:
                {
                    switch (strCode)
                    {
                        case "AD":
                            objReturn = Properties.Resources.flag_andorra_64;
                            break;

                        case "AE":
                            objReturn = Properties.Resources.flag_united_arab_emirates_64;
                            break;

                        case "AF":
                            objReturn = Properties.Resources.flag_afghanistan_64;
                            break;

                        case "AG":
                            objReturn = Properties.Resources.flag_antigua_and_barbuda_64;
                            break;

                        case "AI":
                            objReturn = Properties.Resources.flag_anguilla_64;
                            break;

                        case "AL":
                            objReturn = Properties.Resources.flag_albania_64;
                            break;

                        case "AM":
                            objReturn = Properties.Resources.flag_armenia_64;
                            break;

                        case "AN":
                            objReturn = Properties.Resources.flag_netherlands_antilles_64;
                            break;

                        case "AO":
                            objReturn = Properties.Resources.flag_angola_64;
                            break;

                        case "AR":
                            objReturn = Properties.Resources.flag_argentina_64;
                            break;

                        case "AS":
                            objReturn = Properties.Resources.flag_american_samoa_64;
                            break;

                        case "AT":
                            objReturn = Properties.Resources.flag_austria_64;
                            break;

                        case "AU":
                            objReturn = Properties.Resources.flag_australia_64;
                            break;

                        case "AW":
                            objReturn = Properties.Resources.flag_aruba_64;
                            break;

                        case "AZ":
                            objReturn = Properties.Resources.flag_azerbaijan_64;
                            break;

                        case "BA":
                            objReturn = Properties.Resources.flag_bosnia_64;
                            break;

                        case "BB":
                            objReturn = Properties.Resources.flag_barbados_64;
                            break;

                        case "BD":
                            objReturn = Properties.Resources.flag_bangladesh_64;
                            break;

                        case "BE":
                            objReturn = Properties.Resources.flag_belgium_64;
                            break;

                        case "BF":
                            objReturn = Properties.Resources.flag_burkina_faso_64;
                            break;

                        case "BG":
                            objReturn = Properties.Resources.flag_bulgaria_64;
                            break;

                        case "BH":
                            objReturn = Properties.Resources.flag_bahrain_64;
                            break;

                        case "BI":
                            objReturn = Properties.Resources.flag_burundi_64;
                            break;

                        case "BJ":
                            objReturn = Properties.Resources.flag_benin_64;
                            break;

                        case "BM":
                            objReturn = Properties.Resources.flag_bermuda_64;
                            break;

                        case "BN":
                            objReturn = Properties.Resources.flag_brunei_64;
                            break;

                        case "BO":
                            objReturn = Properties.Resources.flag_bolivia_64;
                            break;

                        case "BR":
                            objReturn = Properties.Resources.flag_brazil_64;
                            break;

                        case "BS":
                            objReturn = Properties.Resources.flag_bahamas_64;
                            break;

                        case "BT":
                            objReturn = Properties.Resources.flag_bhutan_64;
                            break;

                        case "BW":
                            objReturn = Properties.Resources.flag_botswana_64;
                            break;

                        case "BY":
                            objReturn = Properties.Resources.flag_belarus_64;
                            break;

                        case "BZ":
                            objReturn = Properties.Resources.flag_belize_64;
                            break;

                        case "CA":
                            objReturn = Properties.Resources.flag_canada_64;
                            break;

                        case "CD":
                            objReturn = Properties.Resources.flag_congo_democratic_republic_64;
                            break;

                        case "CF":
                            objReturn = Properties.Resources.flag_central_african_republic_64;
                            break;

                        case "CG":
                            objReturn = Properties.Resources.flag_congo_republic_64;
                            break;

                        case "CH":
                            objReturn = Properties.Resources.flag_switzerland_64;
                            break;

                        case "CI":
                            objReturn = Properties.Resources.flag_cote_divoire_64;
                            break;

                        case "CK":
                            objReturn = Properties.Resources.flag_cook_islands_64;
                            break;

                        case "CL":
                            objReturn = Properties.Resources.flag_chile_64;
                            break;

                        case "CM":
                            objReturn = Properties.Resources.flag_cameroon_64;
                            break;

                        case "CN":
                            objReturn = Properties.Resources.flag_china_64;
                            break;

                        case "CO":
                            objReturn = Properties.Resources.flag_colombia_64;
                            break;

                        case "CR":
                            objReturn = Properties.Resources.flag_costa_rica_64;
                            break;

                        case "CU":
                            objReturn = Properties.Resources.flag_cuba_64;
                            break;

                        case "CV":
                            objReturn = Properties.Resources.flag_cape_verde_64;
                            break;

                        case "CY":
                            objReturn = Properties.Resources.flag_cyprus_64;
                            break;

                        case "CZ":
                            objReturn = Properties.Resources.flag_czech_republic_64;
                            break;

                        case "DE":
                            objReturn = Properties.Resources.flag_germany_64;
                            break;

                        case "DJ":
                            objReturn = Properties.Resources.flag_djibouti_64;
                            break;

                        case "DK":
                            objReturn = Properties.Resources.flag_denmark_64;
                            break;

                        case "DM":
                            objReturn = Properties.Resources.flag_dominica_64;
                            break;

                        case "DO":
                            objReturn = Properties.Resources.flag_dominican_republic_64;
                            break;

                        case "DZ":
                            objReturn = Properties.Resources.flag_algeria_64;
                            break;

                        case "EC":
                            objReturn = Properties.Resources.flag_equador_64;
                            break;

                        case "EE":
                            objReturn = Properties.Resources.flag_estonia_64;
                            break;

                        case "EG":
                            objReturn = Properties.Resources.flag_egypt_64;
                            break;

                        case "ER":
                            objReturn = Properties.Resources.flag_eritrea_64;
                            break;

                        case "ES":
                            objReturn = Properties.Resources.flag_spain_64;
                            break;

                        case "ET":
                            objReturn = Properties.Resources.flag_ethiopia_64;
                            break;

                        case "EU":
                            objReturn = Properties.Resources.flag_european_union_64;
                            break;

                        case "FI":
                            objReturn = Properties.Resources.flag_finland_64;
                            break;

                        case "FJ":
                            objReturn = Properties.Resources.flag_fiji_64;
                            break;

                        case "FK":
                            objReturn = Properties.Resources.flag_falkland_islands_64;
                            break;

                        case "FM":
                            objReturn = Properties.Resources.flag_micronesia_64;
                            break;

                        case "FO":
                            objReturn = Properties.Resources.flag_faroe_islands_64;
                            break;

                        case "FR":
                            objReturn = Properties.Resources.flag_france_64;
                            break;

                        case "GA":
                            objReturn = Properties.Resources.flag_gabon_64;
                            break;

                        case "GB":
                            objReturn = Properties.Resources.flag_great_britain_64;
                            break;

                        case "GD":
                            objReturn = Properties.Resources.flag_grenada_64;
                            break;

                        case "GE":
                            objReturn = Properties.Resources.flag_georgia_64;
                            break;

                        case "GG":
                            objReturn = Properties.Resources.flag_guernsey_64;
                            break;

                        case "GH":
                            objReturn = Properties.Resources.flag_ghana_64;
                            break;

                        case "GI":
                            objReturn = Properties.Resources.flag_gibraltar_64;
                            break;

                        case "GL":
                            objReturn = Properties.Resources.flag_greenland_64;
                            break;

                        case "GM":
                            objReturn = Properties.Resources.flag_gambia_64;
                            break;

                        case "GN":
                            objReturn = Properties.Resources.flag_guinea_64;
                            break;

                        case "GQ":
                            objReturn = Properties.Resources.flag_equatorial_guinea_64;
                            break;

                        case "GR":
                            objReturn = Properties.Resources.flag_greece_64;
                            break;

                        case "GS":
                            objReturn = Properties.Resources.flag_south_georgia_64;
                            break;

                        case "GT":
                            objReturn = Properties.Resources.flag_guatemala_64;
                            break;

                        case "GU":
                            objReturn = Properties.Resources.flag_guam_64;
                            break;

                        case "GW":
                            objReturn = Properties.Resources.flag_guinea_bissau_64;
                            break;

                        case "GY":
                            objReturn = Properties.Resources.flag_guyana_64;
                            break;

                        case "HK":
                            objReturn = Properties.Resources.flag_hong_kong_64;
                            break;

                        case "HN":
                            objReturn = Properties.Resources.flag_honduras_64;
                            break;

                        case "HR":
                            objReturn = Properties.Resources.flag_croatia_64;
                            break;

                        case "HT":
                            objReturn = Properties.Resources.flag_haiti_64;
                            break;

                        case "HU":
                            objReturn = Properties.Resources.flag_hungary_64;
                            break;

                        case "ID":
                            objReturn = Properties.Resources.flag_indonesia_64;
                            break;

                        case "IE":
                            objReturn = Properties.Resources.flag_ireland_64;
                            break;

                        case "IL":
                            objReturn = Properties.Resources.flag_israel_64;
                            break;

                        case "IM":
                            objReturn = Properties.Resources.flag_isle_of_man_64;
                            break;

                        case "IN":
                            objReturn = Properties.Resources.flag_india_64;
                            break;

                        case "IO":
                            objReturn = Properties.Resources.flag_british_indian_ocean_64;
                            break;

                        case "IQ":
                            objReturn = Properties.Resources.flag_iraq_64;
                            break;

                        case "IR":
                            objReturn = Properties.Resources.flag_iran_64;
                            break;

                        case "IS":
                            objReturn = Properties.Resources.flag_iceland_64;
                            break;

                        case "IT":
                            objReturn = Properties.Resources.flag_italy_64;
                            break;

                        case "JE":
                            objReturn = Properties.Resources.flag_jersey_64;
                            break;

                        case "JM":
                            objReturn = Properties.Resources.flag_jamaica_64;
                            break;

                        case "JO":
                            objReturn = Properties.Resources.flag_jordan_64;
                            break;

                        case "JP":
                            objReturn = Properties.Resources.flag_japan_64;
                            break;

                        case "KE":
                            objReturn = Properties.Resources.flag_kenya_64;
                            break;

                        case "KG":
                            objReturn = Properties.Resources.flag_kyrgyzstan_64;
                            break;

                        case "KH":
                            objReturn = Properties.Resources.flag_cambodia_64;
                            break;

                        case "KI":
                            objReturn = Properties.Resources.flag_kiribati_64;
                            break;

                        case "KM":
                            objReturn = Properties.Resources.flag_comoros_64;
                            break;

                        case "KN":
                            objReturn = Properties.Resources.flag_saint_kitts_and_nevis_64;
                            break;

                        case "KP":
                            objReturn = Properties.Resources.flag_north_korea_64;
                            break;

                        case "KR":
                            objReturn = Properties.Resources.flag_south_korea_64;
                            break;

                        case "KW":
                            objReturn = Properties.Resources.flag_kuwait_64;
                            break;

                        case "KY":
                            objReturn = Properties.Resources.flag_cayman_islands_64;
                            break;

                        case "KZ":
                            objReturn = Properties.Resources.flag_kazakhstan_64;
                            break;

                        case "LA":
                            objReturn = Properties.Resources.flag_laos_64;
                            break;

                        case "LB":
                            objReturn = Properties.Resources.flag_lebanon_64;
                            break;

                        case "LC":
                            objReturn = Properties.Resources.flag_saint_lucia_64;
                            break;

                        case "LI":
                            objReturn = Properties.Resources.flag_liechtenstein_64;
                            break;

                        case "LK":
                            objReturn = Properties.Resources.flag_sri_lanka_64;
                            break;

                        case "LR":
                            objReturn = Properties.Resources.flag_liberia_64;
                            break;

                        case "LS":
                            objReturn = Properties.Resources.flag_lesotho_64;
                            break;

                        case "LT":
                            objReturn = Properties.Resources.flag_lithuania_64;
                            break;

                        case "LU":
                            objReturn = Properties.Resources.flag_luxembourg_64;
                            break;

                        case "LV":
                            objReturn = Properties.Resources.flag_latvia_64;
                            break;

                        case "LY":
                            objReturn = Properties.Resources.flag_libya_64;
                            break;

                        case "MA":
                            objReturn = Properties.Resources.flag_morocco_64;
                            break;

                        case "MC":
                            objReturn = Properties.Resources.flag_monaco_64;
                            break;

                        case "MD":
                            objReturn = Properties.Resources.flag_moldova_64;
                            break;

                        case "MG":
                            objReturn = Properties.Resources.flag_madagascar_64;
                            break;

                        case "MH":
                            objReturn = Properties.Resources.flag_marshall_islands_64;
                            break;

                        case "MK":
                            objReturn = Properties.Resources.flag_macedonia_64;
                            break;

                        case "ML":
                            objReturn = Properties.Resources.flag_mali_64;
                            break;

                        case "MM":
                            objReturn = Properties.Resources.flag_burma_64;
                            break;

                        case "MN":
                            objReturn = Properties.Resources.flag_mongolia_64;
                            break;

                        case "MO":
                            objReturn = Properties.Resources.flag_macau_64;
                            break;

                        case "MP":
                            objReturn = Properties.Resources.flag_northern_mariana_islands_64;
                            break;

                        case "MQ":
                            objReturn = Properties.Resources.flag_martinique_64;
                            break;

                        case "MR":
                            objReturn = Properties.Resources.flag_mauretania_64;
                            break;

                        case "MS":
                            objReturn = Properties.Resources.flag_montserrat_64;
                            break;

                        case "MT":
                            objReturn = Properties.Resources.flag_malta_64;
                            break;

                        case "MU":
                            objReturn = Properties.Resources.flag_mauritius_64;
                            break;

                        case "MV":
                            objReturn = Properties.Resources.flag_maledives_64;
                            break;

                        case "MW":
                            objReturn = Properties.Resources.flag_malawi_64;
                            break;

                        case "MX":
                            objReturn = Properties.Resources.flag_mexico_64;
                            break;

                        case "MY":
                            objReturn = Properties.Resources.flag_malaysia_64;
                            break;

                        case "MZ":
                            objReturn = Properties.Resources.flag_mozambique_64;
                            break;

                        case "NA":
                            objReturn = Properties.Resources.flag_namibia_64;
                            break;

                        case "NE":
                            objReturn = Properties.Resources.flag_niger_64;
                            break;

                        case "NF":
                            objReturn = Properties.Resources.flag_norfolk_islands_64;
                            break;

                        case "NG":
                            objReturn = Properties.Resources.flag_nigeria_64;
                            break;

                        case "NI":
                            objReturn = Properties.Resources.flag_nicaragua_64;
                            break;

                        case "NL":
                            objReturn = Properties.Resources.flag_netherlands_64;
                            break;

                        case "NO":
                            objReturn = Properties.Resources.flag_norway_64;
                            break;

                        case "NP":
                            objReturn = Properties.Resources.flag_nepal_64;
                            break;

                        case "NR":
                            objReturn = Properties.Resources.flag_nauru_64;
                            break;

                        case "NU":
                            objReturn = Properties.Resources.flag_niue_64;
                            break;

                        case "NZ":
                            objReturn = Properties.Resources.flag_new_zealand_64;
                            break;

                        case "OM":
                            objReturn = Properties.Resources.flag_oman_64;
                            break;

                        case "PA":
                            objReturn = Properties.Resources.flag_panama_64;
                            break;

                        case "PE":
                            objReturn = Properties.Resources.flag_peru_64;
                            break;

                        case "PF":
                            objReturn = Properties.Resources.flag_french_polynesia_64;
                            break;

                        case "PG":
                            objReturn = Properties.Resources.flag_papua_new_guinea_64;
                            break;

                        case "PH":
                            objReturn = Properties.Resources.flag_philippines_64;
                            break;

                        case "PK":
                            objReturn = Properties.Resources.flag_pakistan_64;
                            break;

                        case "PL":
                            objReturn = Properties.Resources.flag_poland_64;
                            break;

                        case "PM":
                            objReturn = Properties.Resources.flag_saint_pierre_and_miquelon_64;
                            break;

                        case "PN":
                            objReturn = Properties.Resources.flag_pitcairn_islands_64;
                            break;

                        case "PR":
                            objReturn = Properties.Resources.flag_puerto_rico_64;
                            break;

                        case "PT":
                            objReturn = Properties.Resources.flag_portugal_64;
                            break;

                        case "PW":
                            objReturn = Properties.Resources.flag_palau_64;
                            break;

                        case "PY":
                            objReturn = Properties.Resources.flag_paraquay_64;
                            break;

                        case "QA":
                            objReturn = Properties.Resources.flag_qatar_64;
                            break;

                        case "RO":
                            objReturn = Properties.Resources.flag_romania_64;
                            break;

                        case "RS":
                            objReturn = Properties.Resources.flag_serbia_montenegro_64;
                            break;

                        case "RU":
                            objReturn = Properties.Resources.flag_russia_64;
                            break;

                        case "RW":
                            objReturn = Properties.Resources.flag_rwanda_64;
                            break;

                        case "SA":
                            objReturn = Properties.Resources.flag_saudi_arabia_64;
                            break;

                        case "SB":
                            objReturn = Properties.Resources.flag_solomon_islands_64;
                            break;

                        case "SC":
                            objReturn = Properties.Resources.flag_seychelles_64;
                            break;

                        case "SD":
                            objReturn = Properties.Resources.flag_sudan_64;
                            break;

                        case "SE":
                            objReturn = Properties.Resources.flag_sweden_64;
                            break;

                        case "SG":
                            objReturn = Properties.Resources.flag_singapore_64;
                            break;

                        case "SH":
                            objReturn = Properties.Resources.flag_saint_helena_64;
                            break;

                        case "SI":
                            objReturn = Properties.Resources.flag_slovenia_64;
                            break;

                        case "SK":
                            objReturn = Properties.Resources.flag_slovakia_64;
                            break;

                        case "SL":
                            objReturn = Properties.Resources.flag_sierra_leone_64;
                            break;

                        case "SM":
                            objReturn = Properties.Resources.flag_san_marino_64;
                            break;

                        case "SN":
                            objReturn = Properties.Resources.flag_senegal_64;
                            break;

                        case "SO":
                            objReturn = Properties.Resources.flag_somalia_64;
                            break;

                        case "SR":
                            objReturn = Properties.Resources.flag_suriname_64;
                            break;

                        case "ST":
                            objReturn = Properties.Resources.flag_sao_tome_and_principe_64;
                            break;

                        case "SV":
                            objReturn = Properties.Resources.flag_el_salvador_64;
                            break;

                        case "SY":
                            objReturn = Properties.Resources.flag_syria_64;
                            break;

                        case "SZ":
                            objReturn = Properties.Resources.flag_swaziland_64;
                            break;

                        case "TC":
                            objReturn = Properties.Resources.flag_turks_and_caicos_islands_64;
                            break;

                        case "TD":
                            objReturn = Properties.Resources.flag_chad_64;
                            break;

                        case "TG":
                            objReturn = Properties.Resources.flag_togo_64;
                            break;

                        case "TH":
                            objReturn = Properties.Resources.flag_thailand_64;
                            break;

                        case "TI":
                            objReturn = Properties.Resources.flag_tibet_64;
                            break;

                        case "TJ":
                            objReturn = Properties.Resources.flag_tajikistan_64;
                            break;

                        case "TL":
                            objReturn = Properties.Resources.flag_east_timor_64;
                            break;

                        case "TM":
                            objReturn = Properties.Resources.flag_turkmenistan_64;
                            break;

                        case "TN":
                            objReturn = Properties.Resources.flag_tunisia_64;
                            break;

                        case "TO":
                            objReturn = Properties.Resources.flag_tonga_64;
                            break;

                        case "TR":
                            objReturn = Properties.Resources.flag_turkey_64;
                            break;

                        case "TT":
                            objReturn = Properties.Resources.flag_trinidad_and_tobago_64;
                            break;

                        case "TV":
                            objReturn = Properties.Resources.flag_tuvalu_64;
                            break;

                        case "TW":
                            objReturn = Properties.Resources.flag_taiwan_64;
                            break;

                        case "TZ":
                            objReturn = Properties.Resources.flag_tanzania_64;
                            break;

                        case "UA":
                            objReturn = Properties.Resources.flag_ukraine_64;
                            break;

                        case "UG":
                            objReturn = Properties.Resources.flag_uganda_64;
                            break;

                        case "US":
                            objReturn = Properties.Resources.flag_usa_64;
                            break;

                        case "UY":
                            objReturn = Properties.Resources.flag_uruquay_64;
                            break;

                        case "UZ":
                            objReturn = Properties.Resources.flag_uzbekistan_64;
                            break;

                        case "VA":
                            objReturn = Properties.Resources.flag_vatican_city_64;
                            break;

                        case "VC":
                            objReturn = Properties.Resources.flag_saint_vincent_and_grenadines_64;
                            break;

                        case "VE":
                            objReturn = Properties.Resources.flag_venezuela_64;
                            break;

                        case "VG":
                            objReturn = Properties.Resources.flag_british_virgin_islands_64;
                            break;

                        case "VI":
                            objReturn = Properties.Resources.flag_virgin_islands_64;
                            break;

                        case "VN":
                            objReturn = Properties.Resources.flag_vietnam_64;
                            break;

                        case "VU":
                            objReturn = Properties.Resources.flag_vanuatu_64;
                            break;

                        case "WF":
                            objReturn = Properties.Resources.flag_wallis_and_futuna_64;
                            break;

                        case "WS":
                            objReturn = Properties.Resources.flag_samoa_64;
                            break;

                        case "XE":
                            objReturn = Properties.Resources.flag_england_64;
                            break;

                        case "XS":
                            objReturn = Properties.Resources.flag_scotland_64;
                            break;

                        case "XW":
                            objReturn = Properties.Resources.flag_wales_64;
                            break;

                        case "YE":
                            objReturn = Properties.Resources.flag_yemen_64;
                            break;

                        case "ZA":
                            objReturn = Properties.Resources.flag_south_africa_64;
                            break;

                        case "ZM":
                            objReturn = Properties.Resources.flag_zambia_64;
                            break;

                        case "ZW":
                            objReturn = Properties.Resources.flag_zimbabwe_64;
                            break;

                        case "DEFAULT":
                            objReturn = Properties.Resources.defaulted_64;
                            break;

                        case "NOIMAGEDOTS":
                            objReturn = Properties.Resources.noimagedots_64;
                            break;

                        default:
                            Utils.BreakIfDebug();
                            goto case "DEFAULT";
                    }

                    break;
                }
                default:
                {
                    Utils.BreakIfDebug();
                    objReturn = null;
                    break;
                }
            }
            return objReturn;
        }
    }
}
