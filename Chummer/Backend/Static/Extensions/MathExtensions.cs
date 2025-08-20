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

namespace Chummer
{
    public static class MathExtensions
    {
        /// <inheritdoc cref="Math.Min(int, int)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(params int[] aintValues)
        {
            switch (aintValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(aintValues));
                case 1:
                    return aintValues[0];
            }

            int intReturn = aintValues[0];
            for (int i = 1; i < aintValues.Length - 1; ++i)
            {
                int intLoop = aintValues[i];
                if (intReturn > intLoop)
                    intReturn = intLoop;
            }
            return intReturn;
        }

        /// <inheritdoc cref="Math.Min(long, long)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Min(params long[] aintValues)
        {
            switch (aintValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(aintValues));
                case 1:
                    return aintValues[0];
            }

            long lngReturn = aintValues[0];
            for (int i = 1; i < aintValues.Length - 1; ++i)
            {
                long lngLoop = aintValues[i];
                if (lngReturn > lngLoop)
                    lngReturn = lngLoop;
            }
            return lngReturn;
        }

        /// <inheritdoc cref="Math.Min(float, float)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(params float[] afltValues)
        {
            switch (afltValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(afltValues));
                case 1:
                    return afltValues[0];
            }

            float fltReturn = afltValues[0];
            for (int i = 1; i < afltValues.Length - 1; ++i)
            {
                float fltLoop = afltValues[i];
                if (float.IsNaN(fltLoop))
                    return float.NaN;
                if (fltReturn > fltLoop)
                    fltReturn = fltLoop;
            }
            return fltReturn;
        }

        /// <inheritdoc cref="Math.Min(double, double)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(params double[] adblValues)
        {
            switch (adblValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adblValues));
                case 1:
                    return adblValues[0];
            }

            double dblReturn = adblValues[0];
            for (int i = 1; i < adblValues.Length - 1; ++i)
            {
                double dblLoop = adblValues[i];
                if (double.IsNaN(dblLoop))
                    return double.NaN;
                if (dblReturn > dblLoop)
                    dblReturn = dblLoop;
            }
            return dblReturn;
        }

        /// <inheritdoc cref="Math.Min(decimal, decimal)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Min(params decimal[] adecValues)
        {
            switch (adecValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adecValues));
                case 1:
                    return adecValues[0];
            }

            decimal decReturn = adecValues[0];
            for (int i = 1; i < adecValues.Length - 1; ++i)
            {
                decimal decLoop = adecValues[i];
                if (decReturn > decLoop)
                    decReturn = decLoop;
            }
            return decReturn;
        }

        /// <inheritdoc cref="Math.Max(int, int)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(params int[] aintValues)
        {
            switch (aintValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(aintValues));
                case 1:
                    return aintValues[0];
            }

            int intReturn = aintValues[0];
            for (int i = 1; i < aintValues.Length - 1; ++i)
            {
                int intLoop = aintValues[i];
                if (intReturn < intLoop)
                    intReturn = intLoop;
            }
            return intReturn;
        }

        /// <inheritdoc cref="Math.Max(long, long)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Max(params long[] aintValues)
        {
            switch (aintValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(aintValues));
                case 1:
                    return aintValues[0];
            }

            long lngReturn = aintValues[0];
            for (int i = 1; i < aintValues.Length - 1; ++i)
            {
                long lngLoop = aintValues[i];
                if (lngReturn < lngLoop)
                    lngReturn = lngLoop;
            }
            return lngReturn;
        }

        /// <inheritdoc cref="Math.Max(float, float)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(params float[] afltValues)
        {
            switch (afltValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(afltValues));
                case 1:
                    return afltValues[0];
            }

            float fltReturn = afltValues[0];
            for (int i = 1; i < afltValues.Length - 1; ++i)
            {
                float fltLoop = afltValues[i];
                if (float.IsNaN(fltLoop))
                    return float.NaN;
                if (fltReturn < fltLoop)
                    fltReturn = fltLoop;
            }
            return fltReturn;
        }

        /// <inheritdoc cref="Math.Max(double, double)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(params double[] adblValues)
        {
            switch (adblValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adblValues));
                case 1:
                    return adblValues[0];
            }

            double dblReturn = adblValues[0];
            for (int i = 1; i < adblValues.Length - 1; ++i)
            {
                double dblLoop = adblValues[i];
                if (double.IsNaN(dblLoop))
                    return double.NaN;
                if (dblReturn < dblLoop)
                    dblReturn = dblLoop;
            }
            return dblReturn;
        }

        /// <inheritdoc cref="Math.Max(decimal, decimal)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Max(params decimal[] adecValues)
        {
            switch (adecValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adecValues));
                case 1:
                    return adecValues[0];
            }

            decimal decReturn = adecValues[0];
            for (int i = 1; i < adecValues.Length - 1; ++i)
            {
                decimal decLoop = adecValues[i];
                if (decReturn < decLoop)
                    decReturn = decLoop;
            }
            return decReturn;
        }

        /// <inheritdoc cref="Math.DivRem(int, int, out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DivRem(this int a, int b, out int result)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            int div;
            if ((a & 0xFF) == a)
            {
                switch (b)
                {
                    // Common powers of two
                    case 2:
                        div = a >> 1;
                        result = a & 1;
                        break;
                    case 4:
                        div = a >> 2;
                        result = a & 0x3;
                        break;
                    case 8:
                        div = a >> 3;
                        result = a & 0x7;
                        break;
                    case 16:
                        div = a >> 4;
                        result = a & 0xF;
                        break;
                    case 32:
                        div = a >> 5;
                        result = a & 0x1F;
                        break;
                    case 64:
                        div = a >> 6;
                        result = a & 0x3F;
                        break;
                    case 128:
                        div = a >> 7;
                        result = a & 0x7F;
                        break;
                    default:
                        div = a / b;
                        result = a - (div * b);
                        break;
                }
            }
            else
            {
                div = a / b;
                result = a - (div * b);
            }
            return div;
        }

        /// <inheritdoc cref="Math.DivRem(int, int, out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<int, int> DivRem(this int a, int b)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            int div;
            int result;
            if ((a & 0xFF) == a)
            {
                switch (b)
                {
                    // Common powers of two
                    case 2:
                        div = a >> 1;
                        result = a & 1;
                        break;
                    case 4:
                        div = a >> 2;
                        result = a & 0x3;
                        break;
                    case 8:
                        div = a >> 3;
                        result = a & 0x7;
                        break;
                    case 16:
                        div = a >> 4;
                        result = a & 0xF;
                        break;
                    case 32:
                        div = a >> 5;
                        result = a & 0x1F;
                        break;
                    case 64:
                        div = a >> 6;
                        result = a & 0x3F;
                        break;
                    case 128:
                        div = a >> 7;
                        result = a & 0x7F;
                        break;
                    default:
                        div = a / b;
                        result = a - (div * b);
                        break;
                }
            }
            else
            {
                div = a / b;
                result = a - (div * b);
            }
            return new Tuple<int, int>(div, result);
        }

        /// <inheritdoc cref="Math.DivRem(long, long, out long)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long DivRem(this long a, long b, out long result)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            long div;
            if ((a & 0xFF) == a)
            {
                switch (b)
                {
                    // Common powers of two
                    case 2:
                        div = a >> 1;
                        result = a & 1;
                        break;
                    case 4:
                        div = a >> 2;
                        result = a & 0x3;
                        break;
                    case 8:
                        div = a >> 3;
                        result = a & 0x7;
                        break;
                    case 16:
                        div = a >> 4;
                        result = a & 0xF;
                        break;
                    case 32:
                        div = a >> 5;
                        result = a & 0x1F;
                        break;
                    case 64:
                        div = a >> 6;
                        result = a & 0x3F;
                        break;
                    case 128:
                        div = a >> 7;
                        result = a & 0x7F;
                        break;
                    default:
                        div = a / b;
                        result = a - (div * b);
                        break;
                }
            }
            else
            {
                div = a / b;
                result = a - (div * b);
            }
            return div;
        }

        /// <inheritdoc cref="Math.DivRem(long, long, out long)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<long, long> DivRem(this long a, long b)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            long div;
            long result;
            if ((a & 0xFF) == a)
            {
                switch (b)
                {
                    // Common powers of two
                    case 2:
                        div = a >> 1;
                        result = a & 1;
                        break;
                    case 4:
                        div = a >> 2;
                        result = a & 0x3;
                        break;
                    case 8:
                        div = a >> 3;
                        result = a & 0x7;
                        break;
                    case 16:
                        div = a >> 4;
                        result = a & 0xF;
                        break;
                    case 32:
                        div = a >> 5;
                        result = a & 0x1F;
                        break;
                    case 64:
                        div = a >> 6;
                        result = a & 0x3F;
                        break;
                    case 128:
                        div = a >> 7;
                        result = a & 0x7F;
                        break;
                    default:
                        div = a / b;
                        result = a - (div * b);
                        break;
                }
            }
            else
            {
                div = a / b;
                result = a - (div * b);
            }
            return new Tuple<long, long>(div, result);
        }

        /// <summary>
        /// Quick way to get the floor of the base-2 logarithm of an integer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorLog2(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            // n is between 2^31 - 1 and 0, which means the fastest way is going to be to binary search between 30 and 0
            if (n >= (1 << 15))
            {
                if (n >= (1 << 23))
                {
                    if (n >= (1 << 27))
                    {
                        if (n >= (1 << 29))
                        {
                            if (n >= (1 << 30))
                                return 30;
                            else
                                return 29;
                        }
                        else if (n >= (1 << 28))
                            return 28;
                        else
                            return 27;
                    }
                    else if (n >= (1 << 25))
                    {
                        if (n >= (1 << 26))
                            return 26;
                        else
                            return 25;
                    }
                    else if (n >= (1 << 24))
                        return 24;
                    else
                        return 23;
                }
                else if (n >= (1 << 19))
                {
                    if (n >= (1 << 21))
                    {
                        if (n >= (1 << 22))
                            return 22;
                        else
                            return 21;
                    }
                    else if (n >= (1 << 20))
                        return 20;
                    else
                        return 19;
                }
                else if (n >= (1 << 17))
                {
                    if (n >= (1 << 18))
                        return 18;
                    else
                        return 17;
                }
                else if (n >= (1 << 16))
                    return 16;
                else
                    return 15;
            }
            else if (n >= (1 << 8))
            {
                if (n >= (1 << 12))
                {
                    if (n >= (1 << 14))
                        return 14;
                    else if (n >= (1 << 13))
                        return 13;
                    else
                        return 12;
                }
                else if (n >= (1 << 10))
                {
                    if (n >= (1 << 11))
                        return 11;
                    else
                        return 10;
                }
                else if (n >= (1 << 9))
                    return 9;
                else
                    return 8;
            }
            else if (n >= (1 << 4))
            {
                if (n >= (1 << 6))
                {
                    if (n >= (1 << 7))
                        return 7;
                    else
                        return 6;
                }
                else if (n >= (1 << 5))
                    return 5;
                else
                    return 4;
            }
            else if (n >= (1 << 2))
            {
                if (n >= (1 << 3))
                    return 3;
                else
                    return 2;
            }
            else if (n >= 2)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Quick way to get the ceiling of the base-2 logarithm of an integer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilingLog2(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            return FloorLog2(n) + (n & 1);
        }

        /// <summary>
        /// Quick way to get the floor of the base-10 logarithm of an integer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorLog10(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            // n is between 2*10^9 and 0, which means the fastest way is going to be to binary search between 9 and 0.
            if (n >= 100000)
            {
                if (n >= 10000000)
                {
                    if (n >= 1000000000)
                        return 9;
                    else if (n >= 100000000)
                        return 8;
                    else
                        return 7;
                }
                else if (n >= 1000000)
                    return 6;
                else
                    return 5;
            }
            else if (n >= 1000)
            {
                if (n >= 10000)
                    return 4;
                else
                    return 3;
            }
            else if (n >= 10)
            {
                if (n >= 100)
                    return 2;
                else
                    return 1;
            }
            else
                return 0;
        }

        /// <summary>
        /// Quick way to get the ceiling of the base-2 logarithm of an integer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilingLog10(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            int num = FloorLog10(n);
            if (n % 10 != 0)
                ++num;
            return num;
        }

        /// <summary>
        /// Euler's number
        /// </summary>
        private const decimal Euler = 2.71828182845904523536028747135266249775724709369995957496696762772407663035m;
        /// <summary>
        /// Natural logarithm of 2
        /// </summary>
        private const decimal Ln_2 = 0.69314718055994530941723212145817656807550013436025525412068000949339362196m;
        /// <summary>
        /// Base-2 logarithm of Euler's number
        /// </summary>
        private const decimal Log2_e = 1.44269504088896340735992468100189213742664595415298593413544940693110921918m;
        /// <summary>
        /// Base-2 logarithm of 10
        /// </summary>
        private const decimal Log2_10 = 3.32192809488736234787031942948939017586483139302458061205475639581593477660m;
        /// <summary>
        /// Square root of 2
        /// </summary>
        private const decimal Sqrt_2 = 1.41421356237309504880168872420969807856967187537694807317667973799073247846m;
        /// <summary>
        /// Square root of Euler's number
        /// </summary>
        private const decimal Sqrt_e = 1.64872127070012814684865078781416357165377610071014801157507931164066102119m;

        /// <summary>
        /// Table for the square roots of integers between 0 and 100.
        /// Might look dumb, but this stupid simple method is faster than most "fancier" methods. It's just limited by its domain.
        /// </summary>
        private static readonly decimal[] s_adecIntRoots =
        {
            0m,
            1m,
            Sqrt_2,
            1.73205080756887729352744634150587236694280525381038062805580697945193301690m,
            2m,
            2.23606797749978969640917366873127623544061835961152572427089724541052092563m,
            2.44948974278317809819728407470589139196594748065667012843269256725096037745m,
            2.64575131106459059050161575363926042571025918308245018036833445920106882323m,
            2.82842712474619009760337744841939615713934375075389614635335947598146495692m,
            3m,
            3.16227766016837933199889354443271853371955513932521682685750485279259443863m,
            3.31662479035539984911493273667068668392708854558935359705868214611648464260m,
            3.46410161513775458705489268301174473388561050762076125611161395890386603381m,
            3.60555127546398929311922126747049594625129657384524621271045305622716694829m,
            3.74165738677394138558374873231654930175601980777872694630374546732003515630m,
            3.87298334620741688517926539978239961083292170529159082658757376611348309193m,
            4m,
            4.12310562561766054982140985597407702514719922537362043439863357309495434633m,
            4.24264068711928514640506617262909423570901562613084421953003921397219743538m,
            4.35889894354067355223698198385961565913700392523244493689034413815955732820m,
            4.47213595499957939281834733746255247088123671922305144854179449082104185127m,
            4.58257569495584000658804719372800848898445657676797190260724212390686842554m,
            4.69041575982342955456563011354446628058822835341173715360570189101702463275m,
            4.79583152331271954159743806416269391999670704190412934648530911444825723590m,
            4.89897948556635619639456814941178278393189496131334025686538513450192075491m,
            5m,
            5.09901951359278483002822410902278198956377094609959640758497080442593363206m,
            5.19615242270663188058233902451761710082841576143114188416742093835579905072m,
            5.29150262212918118100323150727852085142051836616490036073666891840213764646m,
            5.38516480713450403125071049154032955629512016164478883768038867001664596282m,
            5.47722557505166113456969782800802133952744694997983254226894449732493277122m,
            5.56776436283002192211947129891854952047639337757041430396843258560358983925m,
            5.65685424949238019520675489683879231427868750150779229270671895196292991384m,
            5.74456264653802865985061146821892931822026445798279236769987747056590072145m,
            5.83095189484530047087415287754558307652139833488597195445000674486781006199m,
            5.91607978309961604256732829156161704841550123079434032287971966914282245910m,
            6m,
            6.08276253029821968899968424520206706208497009478641118641915304648633272531m,
            6.16441400296897645025019238145424422523562402344457454487457207245839965026m,
            6.24499799839839820584689312093979446107295997799165630845297193060961120058m,
            6.32455532033675866399778708886543706743911027865043365371500970558518887727m,
            6.40312423743284868648821767462181326452042013262101888552927262666818275819m,
            6.48074069840786023096596743608799665770520430705834654971135439780961737784m,
            6.55743852430200065234410999763600162792696631988378976986546010558565985348m,
            6.63324958071079969822986547334137336785417709117870719411736429223296928521m,
            6.70820393249936908922752100619382870632185507883457717281269173623156277691m,
            6.78232998312526813906455632662596910519574832392328823275021958208072826398m,
            6.85565460040104412493587144908484896046064346100132627548510818567851711513m,
            6.92820323027550917410978536602348946777122101524152251222322791780773206763m,
            7m,
            7.07106781186547524400844362104849039284835937688474036588339868995366239231m,
            7.14142842854284999799939981136726527876617115990273383320843088276582040644m,
            7.21110255092797858623844253494099189250259314769049242542090611245433389658m,
            7.28010988928051827109730249152703279377766968257647743837818179128411573862m,
            7.34846922834953429459185222411767417589784244197001038529807770175288113237m,
            7.41619848709566294871139744080071306097990431909750159873262326434383018431m,
            7.48331477354788277116749746463309860351203961555745389260749093464007031261m,
            7.54983443527074969723668480694611705822219470462338013829862690571072195391m,
            7.61577310586390828566141102715832300536070559254659846895048424052035215737m,
            7.68114574786860817576968702173137247306245107761488392802473648393554165799m,
            7.74596669241483377035853079956479922166584341058318165317514753222696618387m,
            7.81024967590665439412972273575910141356830513664856330017724376019078558893m,
            7.87400787401181101968503444881200786368108612202085379459884255031376084681m,
            7.93725393319377177150484726091778127713077754924735054110500337760320646969m,
            8m,
            8.06225774829854965236661323030377113113439630560857338793659238926387495102m,
            8.12403840463596036045988356826604034850420408672531282765157559453291680284m,
            8.18535277187244996995370372473392945888048681549803996306671520272366761446m,
            8.24621125123532109964281971194815405029439845074724086879726714618990869267m,
            8.30662386291807485258426274490749201023221424895565577943218836903758503342m,
            8.36660026534075547978172025785187489392815369298672199811191543080418772594m,
            8.42614977317635863063413990620273603160800240156075001366781112932722550275m,
            8.48528137423857029281013234525818847141803125226168843906007842794439487077m,
            8.54400374531753116787164832623970643459445532953328224190865125377164881932m,
            8.60232526704262677172947353504971363202753555729073561950804564123742693466m,
            8.66025403784438646763723170752936183471402626905190314027903489725966508454m,
            8.71779788708134710447396396771923131827400785046488987378068827631911465640m,
            8.77496438739212206040638830741630956087587682755450359092769562978276464621m,
            8.83176086632784685476404272695925396417463948093141782621020297255713993823m,
            8.88819441731558885009144167540872781707645060372952629835472011637610059962m,
            8.94427190999915878563669467492510494176247343844610289708358898164208370255m,
            9m,
            9.05538513813741662657380816698406641305212446409694027658174123001865798076m,
            9.11043357914429888194562610468866919009913916826495585249693846506602119428m,
            9.16515138991168001317609438745601697796891315353594380521448424781373685109m,
            9.21954445729288731000227428176279315724680504872246400800775220544267102680m,
            9.27361849549570375251641607399017462626346891207629821337382659832823683646m,
            9.32737905308881504555447554232055698327624069419165467105619729844678454880m,
            9.38083151964685910913126022708893256117645670682347430721140378203404926550m,
            9.43398113205660381132066037762264071698362263341512132066298144898002290958m,
            9.48683298050513799599668063329815560115866541797565048057251455837778331591m,
            9.53939201416945649152621586023226540254623425250545753908151852910362552305m,
            9.59166304662543908319487612832538783999341408380825869297061822889651447181m,
            9.64365076099295499576003104743266318390690369306325240730017688773128641866m,
            9.69535971483265802814888115084531339365215098795467959053971748623303986757m,
            9.74679434480896390683841319989960029925258390033749103199175000572008177246m,
            9.79795897113271239278913629882356556786378992262668051373077026900384150982m,
            9.84885780179610472174621141491762448169613628744276417172315452983644058370m,
            9.89949493661166534161182106946788654998770312763863651223675816593512734923m,
            9.94987437106619954734479821001206005178126563676806079117604643834945392782m,
            10m
        };

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Sqrt(double)"/> that tries to get a decimal-precision result if it's possible to do it quickly without casting to floating-point.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than 0, which is outside the domain for a square root.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal FastSqrt(this decimal d)
        {
            return d.Sqrt(decimal.MaxValue);
        }

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Sqrt(double)"/> that tries to get the result to decimal precision by using a Newton-Raphson process after obtaining a guess from casting.
        /// NOTE: If <paramref name="decEpsilon"/> is not decimal.MaxValue, is often slower compared to casting and using the floating-point version, so only use this if the extra precision is absolutely needed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than 0, which is outside the domain for a square root.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Sqrt(this decimal d, decimal decEpsilon = DecimalExtensions.Epsilon)
        {
            if (d < 0)
                throw new ArgumentOutOfRangeException(nameof(d), "Cannot get square root of a negative number");

            // Couple of common and/or trivial cases
            if (d < s_adecIntRoots.Length)
            {
                int intNumber = d.ToInt32();
                if (intNumber == d)
                {
                    return s_adecIntRoots[intNumber];
                }
            }

            switch (decEpsilon)
            {
                case decimal.MaxValue:
                    // Use Math.Sqrt for doing square roots because we kind of have to for the initial guess, there's no easy way to do roots with built-in decimal arithmetic
                    return Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(d)));
                case 0:
                    decEpsilon = DecimalExtensions.Epsilon;
                    break;
                default:
                    if (decEpsilon < 0)
                        throw new ArgumentOutOfRangeException(nameof(decEpsilon), "Epsilons should not be negative");
                    break;
            }

            // Start doing Newton-Raphson iterations to find the root of the function f(x) = x^2 - a, which is equivalent to finding x when x = sqrt(a)
            decimal decPrevious = d;
            // Use Math.Sqrt for doing square roots because we kind of have to for the initial guess, there's no easy way to do roots with built-in decimal arithmetic
            decimal decCurrent = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(d)));
            while (Math.Abs(decCurrent - decPrevious) > decEpsilon && decCurrent != 0.0m)
            {
                decPrevious = decCurrent;
                decCurrent = (decCurrent + d / decCurrent) / 2;
            }
            return decCurrent;
        }

        /// <summary>
        /// A fast way of taking the square root of an integer and returning the result rounded up to the nearest integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="intBase"/> is less than 0, which is outside the domain for a square root.</exception>
        public static int FastSqrtAndStandardRound(this int intBase)
        {
            if (intBase < 0)
                throw new ArgumentOutOfRangeException(nameof(intBase), "Cannot get square root of a negative number");
            // Handle trivial cases first (up to 100)
            if (intBase <= 1)
                return intBase;
            if (intBase <= 100)
            {
                if (intBase <= 36)
                {
                    if (intBase <= 16)
                    {
                        if (intBase <= 9)
                        {
                            if (intBase <= 4)
                                return 2;
                            else
                                return 3;
                        }
                        else
                            return 4;
                    }
                    else if (intBase <= 25)
                        return 5;
                    else
                        return 6;
                }
                else if (intBase <= 64)
                {
                    if (intBase <= 49)
                        return 7;
                    else
                        return 8;
                }
                else if (intBase <= 81)
                    return 9;
                else
                    return 10;
            }
            // We use a digit-by-digit calculation in binary base
            int intReturn = 0;
            // We take advantage of the fact that sqrt factors and so start factoring out powers of 4 (since we know their sqrt will be some form of 2^m)
            int intMask = 1 << 30;
            while (intMask > intBase)
                intMask >>= 2;
            while (intMask != 0)
            {
                int intTemp = intReturn + intMask;
                if (intBase >= intTemp)
                {
                    intBase -= intTemp;
                    intReturn = (intReturn >> 1) + intMask;
                }
                else
                    intReturn >>= 1;
                intMask >>= 2;
            }
            return intReturn;
        }

        /// <summary>
        /// A fast way to take the square root of a decimal and then round up to the nearest integer in a single step.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than 0, which is outside the domain for a square root.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastSqrtAndStandardRound(this decimal d)
        {
            if (d < 0)
                throw new ArgumentOutOfRangeException(nameof(d), "Cannot get square root of a negative number");
            // Square roots of non-integers cannot be integers and sqrt is a monotonic function
            // Therefore, ceil(sqrt(d)) == ceil(sqrt(ceil(d)))
            return decimal.ToInt32(Math.Ceiling(d)).FastSqrtAndStandardRound();
        }

        /// <summary>
        /// A fast way to take the square root of a decimal and then round up to the nearest integer in a single step.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than 0, which is outside the domain for a square root.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastSqrtAndStandardRound(this double d)
        {
            if (d < 0)
                throw new ArgumentOutOfRangeException(nameof(d), "Cannot get square root of a negative number");
            // Square roots of non-integers cannot be integers and sqrt is a monotonic function
            // Therefore, ceil(sqrt(d)) == ceil(sqrt(ceil(d)))
            return Convert.ToInt32(Math.Ceiling(d)).FastSqrtAndStandardRound();
        }

        /// <summary>
        /// A fast way to take the square root of a decimal and then round up to the nearest integer in a single step.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than 0, which is outside the domain for a square root.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastSqrtAndStandardRound(this float d)
        {
            if (d < 0)
                throw new ArgumentOutOfRangeException(nameof(d), "Cannot get square root of a negative number");
            // Square roots of non-integers cannot be integers and sqrt is a monotonic function
            // Therefore, ceil(sqrt(d)) == ceil(sqrt(ceil(d)))
            return Convert.ToInt32(Math.Ceiling(d)).FastSqrtAndStandardRound();
        }

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Pow(double, double)"/> that tries to get a decimal-precision result if it's possible to do it quickly without casting to floating-point.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than 0 while <paramref name="y"/> is not an integer, which is undefined.</exception>
        /// <exception cref="DivideByZeroException"><paramref name="x"/> is 0 while <paramref name="y"/> is negative, which would result in a division by zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal FastPow(this decimal x, decimal y)
        {
            return x.Pow(y, decimal.MaxValue);
        }

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Pow(double, double)"/> that tries to get the result to decimal precision by using a Newton-Raphson process after obtaining a guess from casting.
        /// NOTE: If <paramref name="decEpsilon"/> is not decimal.MaxValue, is often slower compared to casting and using the floating-point version, so only use this if the extra precision is absolutely needed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than 0 while <paramref name="y"/> is not an integer, which is undefined.</exception>
        /// <exception cref="DivideByZeroException"><paramref name="x"/> is 0 while <paramref name="y"/> is negative, which would result in a division by zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Pow(this decimal x, decimal y, decimal decEpsilon = DecimalExtensions.Epsilon)
        {
            // Special case for square roots because they are common enough
            if (y == 0.5m)
                return x.Sqrt(decEpsilon);
            if (y < 0 && x == decimal.Zero)
                throw new DivideByZeroException();

            // If we have an integer power, then we can maintain absolute precision without any conversion to floating-point
            int intPower = y.ToInt32();
            if (intPower == y)
                return x.Pow(intPower);

            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), "Cannot raise negative number to a fractional power");

            switch (decEpsilon)
            {
                case decimal.MaxValue: // Don't do any iterations if our epsilon is massive
                    // Use Math.Pow for doing roots and fractional exponents because we kind of have to, there's no easy way to do roots with built-in decimal arithmetic
                    return Convert.ToDecimal(Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y)));
                case 0:
                    decEpsilon = DecimalExtensions.Epsilon;
                    break;
                default:
                    if (decEpsilon < 0)
                        throw new ArgumentOutOfRangeException(nameof(decEpsilon), "Epsilons should not be negative");
                    break;
            }
            // Start doing Newton-Raphson iterations to find the root of the function f(x) = x^(1/b) - a, which is equivalent to finding x when x = a^b
            decimal decPrevious = x;
            // Use Math.Pow for doing roots and fractional exponents because we kind of have to for the initial guess, there's no easy way to do roots with built-in decimal arithmetic
            decimal decCurrent = Convert.ToDecimal(Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y)));
            while (Math.Abs(decCurrent - decPrevious) > decEpsilon && decCurrent != 0.0m)
            {
                decPrevious = decCurrent;
                decCurrent = (1m - y) * decCurrent + x * y / decCurrent.Pow(1m / y - 1m, decEpsilon);
            }
            return decCurrent;
        }

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Pow(double, double)"/> that retains decimal precision thanks to the exponent being an integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than 0 while <paramref name="y"/> is not an integer, which is undefined.</exception>
        /// <exception cref="DivideByZeroException"><paramref name="x"/> is 0 while <paramref name="y"/> is negative, which would result in a division by zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Pow(this decimal x, int y)
        {
            switch (y)
            {
                case 3: // (Potentially) common case, handle explicitly
                    return x * x * x;

                case 2: // Extremely common case, so handle it explicitly
                    return x * x;

                case 1:
                    return x;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;

                case -1:
                    if (x == decimal.Zero)
                        throw new DivideByZeroException();
                    return 1.0m / x;
            }
            switch (x)
            {
                case decimal.One:
                    return 1;

                case decimal.Zero:
                    if (y < 0)
                        throw new DivideByZeroException();
                    return 0;

                case decimal.MinusOne:
                    return (Math.Abs(y) & 1) == 0 ? 1 : -1;
            }
            // Handle negative powers by dividing by the result of the positive power right before we return
            bool blnNegativePower = y < 0;
            if (blnNegativePower)
                y = -y;
            decimal decReturn = 1;
            int i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; y > 1; y -= i >> 1)
            {
                decimal decLoopElement = x;
                for (i = 2; i <= y; i <<= 1)
                {
                    decLoopElement *= decLoopElement;
                }
                decReturn *= decLoopElement;
            }

            return blnNegativePower ? 1.0m / decReturn : decReturn;
        }

        /// <summary>
        /// An integer-only version of <see cref="Math.Pow(double, double)"/> that operates exclusively in integer domains and ranges.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than 0 while <paramref name="y"/> is not an integer, which is undefined.</exception>
        /// <exception cref="DivideByZeroException"><paramref name="x"/> is 0 while <paramref name="y"/> is negative, which would result in a division by zero.</exception>
        public static int Pow(this int x, int y)
        {
            switch (y)
            {
                case 3: // (Potentially) common case, handle explicitly
                    if (x >= 1291 || x <= -1291) // cubing this will cause an overflow exception, so break
                    {
                        Utils.BreakIfDebug();
                        throw new ArgumentOutOfRangeException(nameof(x), "Number is too big to be cubed and still stay an integer.");
                    }

                    return x * x * x;

                case 2: // Extremely common case, so handle it explicitly
                    if (x >= 46341 || x <= -46341) // squaring this will cause an overflow exception, so break
                    {
                        Utils.BreakIfDebug();
                        throw new ArgumentOutOfRangeException(nameof(x), "Number is too big to be squared and still stay an integer.");
                    }

                    return x * x;

                case 1:
                    return x;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;
            }
            switch (x)
            {
                case 1:
                    return 1;

                case 0:
                    if (y < 0)
                        throw new DivideByZeroException();
                    return 0;

                case -1:
                    return (Math.Abs(y) & 1) == 0 ? 1 : -1;
            }
            // Integer division always rounds towards zero, so every base except the ones already handled ends up producing 0 after rounding
            if (y < 0)
                return 0;
            // Special case when both the base and the exponent are powers of 2, since we can make things faster by bit shifting
            if ((x & (x - 1)) == 0 && (y & (y - 1)) == 0)
            {
                for (; y > 1; y >>= 1)
                {
                    x <<= 1;
                }

                return x;
            }
            int intReturn = 1;
            int i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; y > 1; y -= i >> 1)
            {
                int intLoopElement = x;
                for (i = 2; i <= y; i <<= 1)
                {
                    intLoopElement *= intLoopElement;
                }
                intReturn *= intLoopElement;
            }
            return intReturn;
        }

        /// <summary>
        /// A long-only version of <see cref="Math.Pow(double, double)"/> that operates exclusively in long domains and ranges.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than 0 while <paramref name="y"/> is not an integer, which is undefined.</exception>
        /// <exception cref="DivideByZeroException"><paramref name="x"/> is 0 while <paramref name="y"/> is negative, which would result in a division by zero.</exception>
        public static long Pow(this long x, long y)
        {
            switch (y)
            {
                case 3:
                    if (x >= 2097152 || x <= -2097152) // cubing this will cause an overflow exception, so break
                    {
                        Utils.BreakIfDebug();
                        throw new ArgumentOutOfRangeException(nameof(x), "Number is too big to be cubed and still stay a 64-bit integer.");
                    }

                    return x * x * x;

                case 2:
                    if (x >= 3037000500L || x <= -3037000500L) // squaring this will cause an overflow exception, so break
                    {
                        Utils.BreakIfDebug();
                        throw new ArgumentOutOfRangeException(nameof(x), "Number is too big to be squared and still stay a 64-bit integer.");
                    }

                    return x * x;

                case 1:
                    return x;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;
            }
            switch (x)
            {
                case 1:
                    return 1;

                case 0:
                    if (y < 0)
                        throw new DivideByZeroException();
                    return 0;

                case -1:
                    return (Math.Abs(y) & 1) == 0 ? 1 : -1;
            }
            // Integer division always rounds towards zero, so every base except the ones already handled ends up producing 0 after rounding
            if (y < 0)
                return 0;
            // Special case when both the base and the exponent are powers of 2, since we can make things faster by bit shifting
            if ((x & (x - 1)) == 0 && (y & (y - 1)) == 0)
            {
                long lngBase = x;
                for (; y > 1; y >>= 1)
                {
                    lngBase <<= 1;
                }

                return lngBase;
            }
            long lngReturn = 1;
            int i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; y > 1; y -= i >> 1)
            {
                long lngLoopElement = x;
                for (i = 2; i <= y; i <<= 1)
                {
                    lngLoopElement *= lngLoopElement;
                }
                lngReturn *= lngLoopElement;
            }
            return lngReturn;
        }

        
        /// <summary>
        /// Decimal types have ca. 30 digits of precision, and this is enough terms to get &lt; 10^-30 for the last term for the series for Ln(x).
        /// </summary>
        private const int TaylorSeriesNumTerms_Ln_x = 75;

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Log(double, double)"/> that never casts to floating-point.
        /// NOTE: Can be *much* slower compared to casting and using the floating-point version, so only use this if the extra precision is absolutely needed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> is less than or equal to 0, which is outside the domain for a logarithm.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Log(this decimal a, decimal newBase, decimal decEpsilon)
        {
            decimal decReturn = Log2(a, decEpsilon);
            if (newBase != 2.0m)
                decReturn /= Log2(newBase, decEpsilon);
            return decReturn;
        }

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Log(double)"/> that never casts to floating-point.
        /// NOTE: Can be *much* slower compared to casting and using the floating-point version, so only use this if the extra precision is absolutely needed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than or equal to 0, which is outside the domain for a logarithm.</exception>
        public static decimal Log(this decimal d, decimal decEpsilon = DecimalExtensions.Epsilon)
        {
            if (d <= 0)
                throw new ArgumentOutOfRangeException(nameof(d));
            if (d == 1)
                return 0;
            // Calculate as log_2 instead of as ln(x) directly because integer part can be calculated more exactly with integer division
            if (d >= Euler || d <= 1 / Euler)
                return Log2(d, decEpsilon) / Log2_e; // Log_2(e) for transforming the base
            if (decEpsilon < 0)
                throw new ArgumentOutOfRangeException(nameof(decEpsilon), "Epsilons should not be negative");
            if (decEpsilon == 0)
                decEpsilon = DecimalExtensions.Epsilon;
            // We know we are between 1/e and e, so our integer part will be 0, and we can jump straight to a Taylor series of ln(x)
            // First detect case where 1 > x > 1/e and invert it so that we know we are between 1 and e
            bool blnNegate = d < 1;
            if (blnNegate)
                d = 1m / d;
            // We are now 1 < x < e (because == 1 was handled at the top), so now we can do our Taylor series
            decimal decReturn = 0;
            // To help with precision, center around 1 if we are less than sqrt(e), around 2 if greater or equal. x = sqrt(e) should be where ln(x) is halfway between ln(1) and ln(e).
            if (d >= Sqrt_e)
            {
                // Taylor series if centered around 2 is: ln(2) + sum_k (-1/2)^(k+1) (x-2)^k / k
                // We are centering around 2 and not around e because the Taylor series terms retain precision better this way
                decReturn = Ln_2;
                for (int i = 1; i <= TaylorSeriesNumTerms_Ln_x; ++i)
                {
                    decimal decOldValue = decReturn;
                    if ((i & 1) == 1)
                        decReturn += (d / 4m - 0.5m).Pow(i) / i;
                    else
                        decReturn -= (d / 4m - 0.5m).Pow(i) / i;
                    if (Math.Abs(decOldValue - decReturn) < decEpsilon)
                        break; // Terminate early if our delta is small enough to be less than what decimal precision supports
                }
            }
            else
            {
                // Taylor series if centered around 1 is: sum_k (-1)^(k+1) * (x-1)^k / k
                for (int i = 1; i <= TaylorSeriesNumTerms_Ln_x; ++i)
                {
                    decimal decOldValue = decReturn;
                    if ((i & 1) == 1)
                        decReturn += (d - 1m).Pow(i) / i;
                    else
                        decReturn -= (d - 1m).Pow(i) / i;
                    if (Math.Abs(decOldValue - decReturn) < decEpsilon)
                        break; // Terminate early if our delta is small enough to be less than what decimal precision supports
                }
            }
            return blnNegate ? -decReturn : decReturn;
        }

        /// <summary>
        /// A decimal-precision version of <see cref="Math.Log10(double)"/> that never casts to floating-point.
        /// NOTE: Can be *much* slower compared to casting and using the floating-point version, so only use this if the extra precision is absolutely needed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than or equal to 0, which is outside the domain for a logarithm.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Log10(this decimal d, decimal decEpsilon = DecimalExtensions.Epsilon)
        {
            return Log2(d, decEpsilon) / Log2_10;
        }

        /// <summary>
        /// A high-precision version of calculating a base-2 logarithm, used for an arbitrary precision number
        /// Note: can be quite slow compared to just using Math.Log.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than or equal to 0, which is outside the domain for a logarithm.</exception>
        private static decimal Log2(this decimal d, decimal decEpsilon = DecimalExtensions.Epsilon)
        {
            if (d <= 0)
                throw new ArgumentOutOfRangeException(nameof(d));
            if (d == 1)
                return 0;
            bool blnNegate = d < 1;
            if (blnNegate)
                d = 1m / d;
            decimal decReturn = 0;
            // First handle the integer part
            for (; d >= 18446744073709551615m; d /= 18446744073709551615m)
                decReturn += 64;
            for (; d >= 4294967296m; d /= 4294967296m)
                decReturn += 32;
            for (; d >= 65536m; d /= 65536m)
                decReturn += 16;
            for (; d >= 256m; d /= 256m)
                decReturn += 8;
            for (; d >= 16m; d /= 16m)
                decReturn += 4;
            for (; d >= 2m; d /= 2m)
                ++decReturn;
            // d is now between 1 and 2, so the remaining part should be a fraction.
            if (d == 1)
                return blnNegate ? -decReturn : decReturn;
            if (decEpsilon < 0)
                throw new ArgumentOutOfRangeException(nameof(decEpsilon), "Epsilons should not be negative");
            if (decEpsilon == 0)
                decEpsilon = DecimalExtensions.Epsilon;
            decimal decReturnFraction = 0;
            // Handle this through a Taylor series of ln(x)/ln(2), meaning a Taylor series of ln(x) whose end result is divided by ln(2)
            // To help with precision, center around 1 if we are less than sqrt(2), around 2 if greater or equal. x = sqrt(2) should be where ln(x) is halfway between ln(1) and ln(2).
            if (d >= Sqrt_2)
            {
                // Taylor series if centered around 2 is: ln(2) + sum_k (-1/2)^(k+1) (x-2)^k / k
                ++decReturn; // The ln(2) part, but adding it directly to the output as 1 because we will be dividing the fraction part by ln(2) at the end anyway 
                for (int i = 1; i <= TaylorSeriesNumTerms_Ln_x; ++i)
                {
                    decimal decOldValue = decReturnFraction;
                    if ((i & 1) == 1)
                        decReturnFraction += (d / 4m - 0.5m).Pow(i) / i;
                    else
                        decReturnFraction -= (d / 4m - 0.5m).Pow(i) / i;
                    if (Math.Abs(decOldValue - decReturnFraction) < decEpsilon)
                        break; // Terminate early if our delta is small enough to be less than what decimal precision supports
                }
            }
            else
            {
                // Taylor series if centered around 1 is: sum_k (-1)^(k+1) * (x-1)^k / k
                for (int i = 1; i <= TaylorSeriesNumTerms_Ln_x; ++i)
                {
                    decimal decOldValue = decReturnFraction;
                    if ((i & 1) == 1)
                        decReturnFraction += (d - 1m).Pow(i) / i;
                    else
                        decReturnFraction -= (d - 1m).Pow(i) / i;
                    if (Math.Abs(decOldValue - decReturnFraction) < decEpsilon)
                        break; // Terminate early if our delta is small enough to be less than what decimal precision supports
                }
            }

            // divide by ln(2) to transform base from e to 2 for fraction part
            decReturn += decReturnFraction / Ln_2;
            return blnNegate ? -decReturn : decReturn;
        }
    }
}
