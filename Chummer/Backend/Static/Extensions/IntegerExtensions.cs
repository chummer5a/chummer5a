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
using System.Runtime.CompilerServices;
using Chummer.Backend.Enums;

namespace Chummer
{
    public static class IntegerExtensions
    {
        /// <summary>
        /// Syntactic sugar for doing integer division that always rounds away from zero instead of towards zero.
        /// </summary>
        /// <param name="intA">Dividend integer.</param>
        /// <param name="intB">Divisor integer.</param>
        /// <returns><paramref name="intA"/> divided by <paramref name="intB"/>, rounded towards the nearest number away from zero (up if positive, down if negative).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DivAwayFromZero(this int intA, int intB)
        {
            if (intB == 0)
                throw new DivideByZeroException();
            if (intA == 0)
                return 0;
            // Adding 1 if modulo > 0 would require a separate modulo operation that is as slow as division
            int intParityA = intA > 0 ? 1 : -1;
            int intParityB = intB > 0 ? 1 : -1;
            return (intA - intParityA + intParityA * intParityB * intB) / intB;
        }

        /// <summary>
        /// Checks if a year (given by an integer) would be considered a long year (has 53 weeks instead of 52) under the ISO 8601 standard.
        /// </summary>
        /// <param name="intYear">Year to check.</param>
        /// <returns>True if <paramref name="intYear"/> would be a long year under the ISO 8601 standard, false if it would be a short year.</returns>
        internal static bool IsYearLongYear(this int intYear)
        {
            return intYear.IsYearLongYear(out bool _);
        }

        /// <summary>
        /// Checks if a year (given by an integer) would be considered a long year (has 53 weeks instead of 52) under the ISO 8601 standard.
        /// </summary>
        /// <param name="intYear">Year to check.</param>
        /// <param name="blnIsLeapYear">True if the year is a leap year, false otherwise (the calculation is done for free as part of the larger calculation).</param>
        /// <returns>True if <paramref name="intYear"/> would be a long year under the ISO 8601 standard, false if it would be a short year.</returns>
        internal static bool IsYearLongYear(this int intYear, out bool blnIsLeapYear)
        {
            if (intYear == 0)
                throw new ArgumentOutOfRangeException(nameof(intYear));
            int intYearDiv4 = intYear.DivRem(4, out int intYearMod4);
            int intYearDiv100 = intYear.DivRem(100, out int intYearMod100);
            int intYearDiv400 = intYear.DivRem(400, out int intYearMod400);
            blnIsLeapYear = intYearMod4 == 0 && (intYearMod100 != 0 || intYearMod400 == 0);
            int intDayOfTheWeekOfDec31 = (intYear + intYearDiv4 - intYearDiv100 + intYearDiv400) % 7;
            return intDayOfTheWeekOfDec31 == (blnIsLeapYear ? 5 : 4);
        }

        /// <summary>
        /// Check if a year would be a leap year.
        /// </summary>
        internal static bool IsYearLeapYear(this int intYear)
        {
            if (intYear == 0)
                throw new ArgumentOutOfRangeException(nameof(intYear));
            return intYear % 4 == 0 && (intYear % 100 != 0 || intYear % 400 == 0);
        }

        /// <summary>
        /// Return the ISO 8601 Week Calendar corresponding to a year.
        /// </summary>
        internal static IsoWeekCalendar GetWeekCalenderForYear(this int intYear)
        {
            if (intYear == 0)
                throw new ArgumentOutOfRangeException(nameof(intYear));
            if (intYear < 0) // Adjust for year 0 not existing
                ++intYear;
            int intCycleYear = intYear % 400;
            // Lookup table might look goofy, but it is honestly easier to do it this way than to run an algorithm to work it all out
            switch (intCycleYear)
            {
                case 6:
                case 17:
                case 23:
                case 34:
                case 45:
                case 51:
                case 62:
                case 73:
                case 79:
                case 90:
                case 102:
                case 113:
                case 119:
                case 130:
                case 141:
                case 147:
                case 158:
                case 169:
                case 175:
                case 186:
                case 197:
                case 209:
                case 215:
                case 226:
                case 237:
                case 243:
                case 254:
                case 265:
                case 271:
                case 282:
                case 293:
                case 299:
                case 305:
                case 311:
                case 322:
                case 333:
                case 339:
                case 350:
                case 361:
                case 367:
                case 378:
                case 389:
                case 395:
                    return IsoWeekCalendar.LetterA;
                case 11:
                case 22:
                case 39:
                case 50:
                case 67:
                case 78:
                case 95:
                case 101:
                case 107:
                case 118:
                case 135:
                case 146:
                case 163:
                case 174:
                case 191:
                case 203:
                case 214:
                case 231:
                case 242:
                case 259:
                case 270:
                case 287:
                case 298:
                case 310:
                case 327:
                case 338:
                case 355:
                case 366:
                case 383:
                case 394:
                    return IsoWeekCalendar.LetterBpreC;
                case 5:
                case 33:
                case 61:
                case 89:
                case 129:
                case 157:
                case 185:
                case 225:
                case 253:
                case 281:
                case 321:
                case 349:
                case 377:
                    return IsoWeekCalendar.LetterBpreDC;
                case 10:
                case 21:
                case 27:
                case 38:
                case 49:
                case 55:
                case 66:
                case 77:
                case 83:
                case 94:
                case 100:
                case 106:
                case 117:
                case 123:
                case 134:
                case 145:
                case 151:
                case 162:
                case 173:
                case 179:
                case 190:
                case 202:
                case 213:
                case 219:
                case 230:
                case 241:
                case 247:
                case 258:
                case 269:
                case 275:
                case 286:
                case 297:
                case 309:
                case 315:
                case 326:
                case 337:
                case 343:
                case 354:
                case 365:
                case 371:
                case 382:
                case 393:
                case 399:
                    return IsoWeekCalendar.LetterC;
                case 9:
                case 15:
                case 26:
                case 37:
                case 43:
                case 54:
                case 65:
                case 71:
                case 82:
                case 93:
                case 99:
                case 105:
                case 111:
                case 122:
                case 133:
                case 139:
                case 150:
                case 161:
                case 167:
                case 178:
                case 189:
                case 195:
                case 201:
                case 207:
                case 218:
                case 229:
                case 235:
                case 246:
                case 257:
                case 263:
                case 274:
                case 285:
                case 291:
                case 303:
                case 314:
                case 325:
                case 331:
                case 342:
                case 353:
                case 359:
                case 370:
                case 381:
                case 387:
                case 398:
                    return IsoWeekCalendar.LetterD;
                case 3:
                case 14:
                case 25:
                case 31:
                case 42:
                case 53:
                case 59:
                case 70:
                case 81:
                case 87:
                case 98:
                case 110:
                case 121:
                case 127:
                case 138:
                case 149:
                case 155:
                case 166:
                case 177:
                case 183:
                case 194:
                case 200:
                case 206:
                case 217:
                case 223:
                case 234:
                case 245:
                case 251:
                case 262:
                case 273:
                case 279:
                case 290:
                case 302:
                case 313:
                case 319:
                case 330:
                case 341:
                case 347:
                case 358:
                case 369:
                case 375:
                case 386:
                case 397:
                    return IsoWeekCalendar.LetterE;
                case 2:
                case 13:
                case 19:
                case 30:
                case 41:
                case 47:
                case 58:
                case 69:
                case 75:
                case 86:
                case 97:
                case 109:
                case 115:
                case 126:
                case 137:
                case 143:
                case 154:
                case 165:
                case 171:
                case 182:
                case 193:
                case 199:
                case 205:
                case 211:
                case 222:
                case 233:
                case 239:
                case 250:
                case 261:
                case 267:
                case 278:
                case 289:
                case 295:
                case 301:
                case 307:
                case 318:
                case 329:
                case 335:
                case 346:
                case 357:
                case 363:
                case 374:
                case 385:
                case 391:
                    return IsoWeekCalendar.LetterF;
                case 1:
                case 7:
                case 18:
                case 29:
                case 35:
                case 46:
                case 57:
                case 63:
                case 74:
                case 85:
                case 91:
                case 103:
                case 114:
                case 125:
                case 131:
                case 142:
                case 153:
                case 159:
                case 170:
                case 181:
                case 187:
                case 198:
                case 210:
                case 221:
                case 227:
                case 238:
                case 249:
                case 255:
                case 266:
                case 277:
                case 283:
                case 294:
                case 300:
                case 306:
                case 317:
                case 323:
                case 334:
                case 345:
                case 351:
                case 362:
                case 373:
                case 379:
                case 390:
                    return IsoWeekCalendar.LetterG;
                case 12:
                case 40:
                case 68:
                case 96:
                case 108:
                case 136:
                case 164:
                case 192:
                case 204:
                case 232:
                case 260:
                case 288:
                case 328:
                case 356:
                case 384:
                    return IsoWeekCalendar.LetterAG;
                case 0:
                case 28:
                case 56:
                case 84:
                case 124:
                case 152:
                case 180:
                case 220:
                case 248:
                case 276:
                case 316:
                case 344:
                case 372:
                    return IsoWeekCalendar.LetterBA;
                case 16:
                case 44:
                case 72:
                case 112:
                case 140:
                case 168:
                case 196:
                case 208:
                case 236:
                case 264:
                case 292:
                case 304:
                case 332:
                case 360:
                case 388:
                    return IsoWeekCalendar.LetterCB;
                case 4:
                case 32:
                case 60:
                case 88:
                case 128:
                case 156:
                case 184:
                case 224:
                case 252:
                case 280:
                case 320:
                case 348:
                case 376:
                    return IsoWeekCalendar.LetterDC;
                case 20:
                case 48:
                case 76:
                case 116:
                case 144:
                case 172:
                case 212:
                case 240:
                case 268:
                case 296:
                case 308:
                case 336:
                case 364:
                case 392:
                    return IsoWeekCalendar.LetterED;
                case 8:
                case 36:
                case 64:
                case 92:
                case 104:
                case 132:
                case 160:
                case 188:
                case 228:
                case 256:
                case 284:
                case 324:
                case 352:
                case 380:
                    return IsoWeekCalendar.LetterFE;
                case 24:
                case 52:
                case 80:
                case 120:
                case 148:
                case 176:
                case 216:
                case 244:
                case 272:
                case 312:
                case 340:
                case 368:
                case 396:
                    return IsoWeekCalendar.LetterGF;
            }
            return IsoWeekCalendar.None;
        }

        /// <summary>
        /// Get the day of the week for January 4 in a given year (January 4 is the first day guaranteed to be in week 1 according to ISO 8601).
        /// </summary>
        internal static int GetWeekOfTheDayForJan4(this int intYear)
        {
            if (intYear == 0)
                throw new ArgumentOutOfRangeException(nameof(intYear));
            switch (intYear.GetWeekCalenderForYear())
            {
                case IsoWeekCalendar.LetterA:
                case IsoWeekCalendar.LetterAG:
                    return 3;
                case IsoWeekCalendar.LetterBpreC:
                case IsoWeekCalendar.LetterBpreDC:
                case IsoWeekCalendar.LetterBA:
                    return 2;
                case IsoWeekCalendar.LetterC:
                case IsoWeekCalendar.LetterCB:
                    return 1;
                case IsoWeekCalendar.LetterD:
                case IsoWeekCalendar.LetterDC:
                    return 7;
                case IsoWeekCalendar.LetterE:
                case IsoWeekCalendar.LetterED:
                    return 6;
                case IsoWeekCalendar.LetterF:
                case IsoWeekCalendar.LetterFE:
                    return 5;
                case IsoWeekCalendar.LetterG:
                case IsoWeekCalendar.LetterGF:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intYear));
            }
        }
    }
}
