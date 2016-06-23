

using System;
using System.Collections.Generic;
using System.Linq; 

namespace Excess.Extensions.R
{
	public static partial class RR {
		
			public static object add (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return add((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return add((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return add((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return add((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return add((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return add((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return add((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return add((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return add((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return add((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return add((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return add((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return add((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return add((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return add((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator add does not support this type combination");
			}
		
			public static object sub (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return sub((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return sub((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return sub((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return sub((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return sub((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return sub((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return sub((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return sub((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return sub((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return sub((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return sub((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return sub((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return sub((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return sub((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return sub((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator sub does not support this type combination");
			}
		
			public static object mul (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return mul((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return mul((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return mul((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return mul((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return mul((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return mul((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return mul((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return mul((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return mul((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return mul((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return mul((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return mul((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return mul((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return mul((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return mul((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator mul does not support this type combination");
			}
		
			public static object div (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return div((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return div((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return div((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return div((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return div((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return div((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return div((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return div((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return div((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return div((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return div((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return div((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return div((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return div((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return div((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator div does not support this type combination");
			}
		
			public static object gt (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return gt((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return gt((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return gt((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return gt((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return gt((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return gt((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return gt((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return gt((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return gt((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return gt((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return gt((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return gt((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return gt((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return gt((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return gt((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator gt does not support this type combination");
			}
		
			public static object ge (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return ge((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return ge((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return ge((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return ge((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return ge((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return ge((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return ge((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return ge((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return ge((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return ge((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return ge((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return ge((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return ge((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return ge((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return ge((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator ge does not support this type combination");
			}
		
			public static object lt (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return lt((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return lt((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return lt((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return lt((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return lt((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return lt((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return lt((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return lt((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return lt((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return lt((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return lt((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return lt((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return lt((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return lt((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return lt((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator lt does not support this type combination");
			}
		
			public static object le (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return le((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return le((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return le((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return le((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return le((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return le((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return le((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return le((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return le((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return le((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return le((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return le((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return le((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return le((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return le((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator le does not support this type combination");
			}
		
			public static object eq (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return eq((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return eq((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return eq((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return eq((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return eq((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return eq((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return eq((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return eq((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return eq((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return eq((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return eq((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return eq((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return eq((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return eq((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return eq((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator eq does not support this type combination");
			}
		
			public static object neq (object val1, object val2) 
			{
				
					if (val1 is Double)
					{
						
							if (val2 is Double)
								return neq((Double)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Double)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Double)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Double)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Double)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Double)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Double)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Double)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Double>)
					{
						
							if (val2 is Double)
								return neq((Vector<Double>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Vector<Double>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Vector<Double>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Vector<Double>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Vector<Double>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Vector<Double>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Vector<Double>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Vector<Double>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Single)
					{
						
							if (val2 is Double)
								return neq((Single)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Single)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Single)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Single)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Single)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Single)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Single)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Single)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Single>)
					{
						
							if (val2 is Double)
								return neq((Vector<Single>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Vector<Single>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Vector<Single>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Vector<Single>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Vector<Single>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Vector<Single>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Vector<Single>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Vector<Single>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int64)
					{
						
							if (val2 is Double)
								return neq((Int64)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Int64)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Int64)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Int64)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Int64)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Int64)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Int64)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Int64)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int64>)
					{
						
							if (val2 is Double)
								return neq((Vector<Int64>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Vector<Int64>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Vector<Int64>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Vector<Int64>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Vector<Int64>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Vector<Int64>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Vector<Int64>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Vector<Int64>)val1, (Vector<Int32>)val2);
											}

				
					if (val1 is Int32)
					{
						
							if (val2 is Double)
								return neq((Int32)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Int32)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Int32)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Int32)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Int32)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Int32)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Int32)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Int32)val1, (Vector<Int32>)val2);
											}

					if (val1 is Vector<Int32>)
					{
						
							if (val2 is Double)
								return neq((Vector<Int32>)val1, (Double)val2);

							if (val2 is Vector<Double>)
								return neq((Vector<Int32>)val1, (Vector<Double>)val2);
						
							if (val2 is Single)
								return neq((Vector<Int32>)val1, (Single)val2);

							if (val2 is Vector<Single>)
								return neq((Vector<Int32>)val1, (Vector<Single>)val2);
						
							if (val2 is Int64)
								return neq((Vector<Int32>)val1, (Int64)val2);

							if (val2 is Vector<Int64>)
								return neq((Vector<Int32>)val1, (Vector<Int64>)val2);
						
							if (val2 is Int32)
								return neq((Vector<Int32>)val1, (Int32)val2);

							if (val2 is Vector<Int32>)
								return neq((Vector<Int32>)val1, (Vector<Int32>)val2);
											}

								throw new InvalidOperationException("operator neq does not support this type combination");
			}
		
		
			public static object and (object val1, object val2) 
			{
				if (val1 is bool)
				{
					if (val2 is bool)
						return and((bool)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return and((bool)val1, (Vector<bool>)val2);
				}

				if (val1 is Vector<bool>)
				{
					if (val2 is bool)
						return and((Vector<bool>)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return and((Vector<bool>)val1, (Vector<bool>)val2);
				}
				throw new InvalidOperationException("operator and does not support this type combination");
			}
		
			public static object bnd (object val1, object val2) 
			{
				if (val1 is bool)
				{
					if (val2 is bool)
						return bnd((bool)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return bnd((bool)val1, (Vector<bool>)val2);
				}

				if (val1 is Vector<bool>)
				{
					if (val2 is bool)
						return bnd((Vector<bool>)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return bnd((Vector<bool>)val1, (Vector<bool>)val2);
				}
				throw new InvalidOperationException("operator bnd does not support this type combination");
			}
		
			public static object or (object val1, object val2) 
			{
				if (val1 is bool)
				{
					if (val2 is bool)
						return or((bool)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return or((bool)val1, (Vector<bool>)val2);
				}

				if (val1 is Vector<bool>)
				{
					if (val2 is bool)
						return or((Vector<bool>)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return or((Vector<bool>)val1, (Vector<bool>)val2);
				}
				throw new InvalidOperationException("operator or does not support this type combination");
			}
		
			public static object bor (object val1, object val2) 
			{
				if (val1 is bool)
				{
					if (val2 is bool)
						return bor((bool)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return bor((bool)val1, (Vector<bool>)val2);
				}

				if (val1 is Vector<bool>)
				{
					if (val2 is bool)
						return bor((Vector<bool>)val1, (bool)val2);

					if (val2 is Vector<bool>)
						return bor((Vector<bool>)val1, (Vector<bool>)val2);
				}
				throw new InvalidOperationException("operator bor does not support this type combination");
			}
		
        private static bool higher(Type type1, Type type2)
        {
			if (type1 == type2)
				return false;

			
				if (type1 == typeof(Double))
				{
											
						if (type2 == typeof(Single))
							return true;
											
						if (type2 == typeof(Int64))
							return true;
											
						if (type2 == typeof(Int32))
							return true;
									}
			
				if (type1 == typeof(Single))
				{
											
						if (type2 == typeof(Int64))
							return true;
											
						if (type2 == typeof(Int32))
							return true;
									}
			
				if (type1 == typeof(Int64))
				{
											
						if (type2 == typeof(Int32))
							return true;
									}
			
			return false;
        }

		//dynamic operations
		public static object sum(object val)
		{
			 
				if (val is Vector<Double>)
					return sum(val as Vector<Double>);
			 
				if (val is Vector<Single>)
					return sum(val as Vector<Single>);
			 
				if (val is Vector<Int64>)
					return sum(val as Vector<Int64>);
			 
				if (val is Vector<Int32>)
					return sum(val as Vector<Int32>);
			
			throw new InvalidOperationException("sum expects numeric vectors");
		}

        public static object index(object val, object vec)
		{
			if (vec is Vector<bool>)
			{
				 
					if (val is Vector<Double>)
						return index((Vector<Double>)val, (Vector<bool>)vec);
				 
					if (val is Vector<Single>)
						return index((Vector<Single>)val, (Vector<bool>)vec);
				 
					if (val is Vector<Int64>)
						return index((Vector<Int64>)val, (Vector<bool>)vec);
				 
					if (val is Vector<Int32>)
						return index((Vector<Int32>)val, (Vector<bool>)vec);
				 
					if (val is Vector<Boolean>)
						return index((Vector<Boolean>)val, (Vector<bool>)vec);
				 
					if (val is Vector<String>)
						return index((Vector<String>)val, (Vector<bool>)vec);
							}
			else if (vec is Vector<int>)
			{
				 
					if (val is Vector<Double>)
						return index((Vector<Double>)val, (Vector<int>)vec);
				 
					if (val is Vector<Single>)
						return index((Vector<Single>)val, (Vector<int>)vec);
				 
					if (val is Vector<Int64>)
						return index((Vector<Int64>)val, (Vector<int>)vec);
				 
					if (val is Vector<Int32>)
						return index((Vector<Int32>)val, (Vector<int>)vec);
				 
					if (val is Vector<Boolean>)
						return index((Vector<Boolean>)val, (Vector<int>)vec);
				 
					if (val is Vector<String>)
						return index((Vector<String>)val, (Vector<int>)vec);
							}

			throw new InvalidOperationException("index vectors expects only boolean or int");
		}

        private static object concat(IEnumerable<IVector> values, int len, Type type)
        {
			 
				if (type == typeof(Double) || type == typeof(Vector<Double>))
					return Vector<Double>.create(len, EnumerateVectors<Double>(values));
			 
				if (type == typeof(Single) || type == typeof(Vector<Single>))
					return Vector<Single>.create(len, EnumerateVectors<Single>(values));
			 
				if (type == typeof(Int64) || type == typeof(Vector<Int64>))
					return Vector<Int64>.create(len, EnumerateVectors<Int64>(values));
			 
				if (type == typeof(Int32) || type == typeof(Vector<Int32>))
					return Vector<Int32>.create(len, EnumerateVectors<Int32>(values));
			 
				if (type == typeof(Boolean) || type == typeof(Vector<Boolean>))
					return Vector<Boolean>.create(len, EnumerateVectors<Boolean>(values));
			 
				if (type == typeof(String) || type == typeof(Vector<String>))
					return Vector<String>.create(len, EnumerateVectors<String>(values));
			
			throw new InvalidOperationException("cannont concatenate type: " + type.FullName);
        }
	}
}