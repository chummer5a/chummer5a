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
        public static System.Drawing.Bitmap GetFlagFromCountryCode(string strCode)
        {
            System.Drawing.Bitmap objReturn;
            if (string.IsNullOrEmpty(strCode))
                strCode = string.Empty;
            switch (strCode.ToUpperInvariant())
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
            return objReturn;
        }

        /// <summary>
        /// Returns a 32px x 32px image of a country/region's flag based on its two-letter, ISO-3166 code.
        /// </summary>
        /// <param name="strCode">The ISO-3166 code of the country/region's flag.</param>
        /// <returns>32px x 32px image of the country/region's flag if available, null otherwise</returns>
        public static System.Drawing.Bitmap GetFlagFromCountryCode192Dpi(string strCode)
        {
            System.Drawing.Bitmap objReturn;
            if (string.IsNullOrEmpty(strCode))
                strCode = string.Empty;
            switch (strCode.ToUpperInvariant())
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
            return objReturn;
        }
    }
}
