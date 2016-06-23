

using System;
using System.Collections.Generic;
using System.Linq;

namespace Excess.Extensions.R
{
	public static partial class RR {
	
		public static Double sum(Vector<Double> val)
		{
			Double result = 0;
			foreach (var value in val.data)
				result += value;
			return result;
		}

        public static Vector<Double> seq(Double from = 1, Double to = 1, Double by = 0, int length_out = 0, IVector along_with = null)
        {
            if (length_out > 0)
            {
                by = (to - from) / (length_out - 1);
            }
            else
            {
                if (by == 0)
                    by = from < to? 1 : -1;

                length_out = (int)((to - from) / (by + 1));
            }

            if (length_out == 0)
                return null;

            return Vector<Double>.create(length_out, DoubleSequence(from, by, length_out));
        }

        private static IEnumerable<Double> DoubleSequence(Double from, Double by, int len)
        {
            for (int i = 0; i < len; i++)
            {
                yield return from;
                from += by;
            }
        }

		public static Vector<Double> rep(Double val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out == 0)
				length_out = times;

			if (length_out == 0)
				length_out = each;

			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-zero parameter");

			return Vector<Double>.create(length_out, val);
		}

		public static Vector<Double> rep(Vector<Double> val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out <= 0)
			{
				if (each > 0)
					length_out = val.length*times;
				else if (times > 0)
					length_out = val.length*times;
			}
			
			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-negative parameter");

			return Vector<Double>.create(length_out, Rep<Double> (val, length_out, each));
		}

	
		public static Single sum(Vector<Single> val)
		{
			Single result = 0;
			foreach (var value in val.data)
				result += value;
			return result;
		}

        public static Vector<Single> seq(Single from = 1, Single to = 1, Single by = 0, int length_out = 0, IVector along_with = null)
        {
            if (length_out > 0)
            {
                by = (to - from) / (length_out - 1);
            }
            else
            {
                if (by == 0)
                    by = from < to? 1 : -1;

                length_out = (int)((to - from) / (by + 1));
            }

            if (length_out == 0)
                return null;

            return Vector<Single>.create(length_out, SingleSequence(from, by, length_out));
        }

        private static IEnumerable<Single> SingleSequence(Single from, Single by, int len)
        {
            for (int i = 0; i < len; i++)
            {
                yield return from;
                from += by;
            }
        }

		public static Vector<Single> rep(Single val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out == 0)
				length_out = times;

			if (length_out == 0)
				length_out = each;

			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-zero parameter");

			return Vector<Single>.create(length_out, val);
		}

		public static Vector<Single> rep(Vector<Single> val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out <= 0)
			{
				if (each > 0)
					length_out = val.length*times;
				else if (times > 0)
					length_out = val.length*times;
			}
			
			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-negative parameter");

			return Vector<Single>.create(length_out, Rep<Single> (val, length_out, each));
		}

	
		public static Int64 sum(Vector<Int64> val)
		{
			Int64 result = 0;
			foreach (var value in val.data)
				result += value;
			return result;
		}

        public static Vector<Int64> seq(Int64 from = 1, Int64 to = 1, Int64 by = 0, int length_out = 0, IVector along_with = null)
        {
            if (length_out > 0)
            {
                by = (to - from) / (length_out - 1);
            }
            else
            {
                if (by == 0)
                    by = from < to? 1 : -1;

                length_out = (int)((to - from) / (by + 1));
            }

            if (length_out == 0)
                return null;

            return Vector<Int64>.create(length_out, Int64Sequence(from, by, length_out));
        }

        private static IEnumerable<Int64> Int64Sequence(Int64 from, Int64 by, int len)
        {
            for (int i = 0; i < len; i++)
            {
                yield return from;
                from += by;
            }
        }

		public static Vector<Int64> rep(Int64 val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out == 0)
				length_out = times;

			if (length_out == 0)
				length_out = each;

			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-zero parameter");

			return Vector<Int64>.create(length_out, val);
		}

		public static Vector<Int64> rep(Vector<Int64> val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out <= 0)
			{
				if (each > 0)
					length_out = val.length*times;
				else if (times > 0)
					length_out = val.length*times;
			}
			
			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-negative parameter");

			return Vector<Int64>.create(length_out, Rep<Int64> (val, length_out, each));
		}

	
		public static Int32 sum(Vector<Int32> val)
		{
			Int32 result = 0;
			foreach (var value in val.data)
				result += value;
			return result;
		}

        public static Vector<Int32> seq(Int32 from = 1, Int32 to = 1, Int32 by = 0, int length_out = 0, IVector along_with = null)
        {
            if (length_out > 0)
            {
                by = (to - from) / (length_out - 1);
            }
            else
            {
                if (by == 0)
                    by = from < to? 1 : -1;

                length_out = (int)((to - from) / (by + 1));
            }

            if (length_out == 0)
                return null;

            return Vector<Int32>.create(length_out, Int32Sequence(from, by, length_out));
        }

        private static IEnumerable<Int32> Int32Sequence(Int32 from, Int32 by, int len)
        {
            for (int i = 0; i < len; i++)
            {
                yield return from;
                from += by;
            }
        }

		public static Vector<Int32> rep(Int32 val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out == 0)
				length_out = times;

			if (length_out == 0)
				length_out = each;

			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-zero parameter");

			return Vector<Int32>.create(length_out, val);
		}

		public static Vector<Int32> rep(Vector<Int32> val, int times = 0, int length_out = 0, int each = 0)
		{
			if (length_out <= 0)
			{
				if (each > 0)
					length_out = val.length*times;
				else if (times > 0)
					length_out = val.length*times;
			}
			
			if (length_out == 0)
				throw new InvalidOperationException("must supply a non-negative parameter");

			return Vector<Int32>.create(length_out, Rep<Int32> (val, length_out, each));
		}

	
	}
}