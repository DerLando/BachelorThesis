﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace BachelorThesis.Core
{
    public static class Extensions
    {
        public static double GetLengthFromEnd(this Curve curve, double param, CurveEnd end)
        {
            double length = Double.NegativeInfinity;
            switch (end)
            {
                case CurveEnd.None:
                    break;
                case CurveEnd.Start:
                    length = curve.GetLength(new Interval(curve.Domain.Min, param));
                    break;
                case CurveEnd.End:
                    length = curve.GetLength(new Interval(param, curve.Domain.Max));
                    break;
                case CurveEnd.Both:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(end), end, null);
            }

            return length;
        }
    }
}