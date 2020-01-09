using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer.Backend.Datastructures
{
    internal class Vector4 : IEnumerable<double>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

        public Vector4(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4()
        {
        }

        public Vector3 AsVector3 => new Vector3(X, Y, Z);
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<double> GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
            yield return W;
        }
    }
}
