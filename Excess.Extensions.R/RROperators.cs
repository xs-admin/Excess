

using System;
namespace Excess.Extensions.R
{

	public static partial class RR {
		
			public static Double ps (Double val) 
			{
				return val;
			}

			public static Vector<Double> ps (Vector<Double> val) 
			{
				return val;
			}

			public static Double ns (Double val) 
			{
				return -val;
			}

			public static Vector<Double> ns (Vector<Double> val) 
			{
				return Vector<Double>.create(val, value => -value );
			}

			
				
					public static Double add (Double val1, Double val2) 
					{
						return val1 + val2;
					}

					public static Vector<Double> add (Vector<Double> val1, Double val2) 
					{
						return Vector<Double>.create(val1, value => value + val2);
					}

					public static Vector<Double> add (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 + value2);
					}

									
					public static Double sub (Double val1, Double val2) 
					{
						return val1 - val2;
					}

					public static Vector<Double> sub (Vector<Double> val1, Double val2) 
					{
						return Vector<Double>.create(val1, value => value - val2);
					}

					public static Vector<Double> sub (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 - value2);
					}

									
					public static Double mul (Double val1, Double val2) 
					{
						return val1 * val2;
					}

					public static Vector<Double> mul (Vector<Double> val1, Double val2) 
					{
						return Vector<Double>.create(val1, value => value * val2);
					}

					public static Vector<Double> mul (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 * value2);
					}

									
					public static Double div (Double val1, Double val2) 
					{
						return val1 / val2;
					}

					public static Vector<Double> div (Vector<Double> val1, Double val2) 
					{
						return Vector<Double>.create(val1, value => value / val2);
					}

					public static Vector<Double> div (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 / value2);
					}

									
				
					public static bool gt (Double val1, Double val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Double> val1, Double val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<bool>.create<Double, Double>(val1, val2, (value1, value2) => value1 > value2);
					}

									
					public static bool ge (Double val1, Double val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Double> val1, Double val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<bool>.create<Double, Double>(val1, val2, (value1, value2) => value1 >= value2);
					}

									
					public static bool lt (Double val1, Double val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Double> val1, Double val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<bool>.create<Double, Double>(val1, val2, (value1, value2) => value1 < value2);
					}

									
					public static bool le (Double val1, Double val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Double> val1, Double val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<bool>.create<Double, Double>(val1, val2, (value1, value2) => value1 <= value2);
					}

									
					public static bool eq (Double val1, Double val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Double> val1, Double val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<bool>.create<Double, Double>(val1, val2, (value1, value2) => value1 == value2);
					}

									
					public static bool neq (Double val1, Double val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Double> val1, Double val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Double> val1, Vector<Double> val2) 
					{
						return Vector<bool>.create<Double, Double>(val1, val2, (value1, value2) => value1 != value2);
					}

												
				
					public static Double add (Double val1, Single val2) 
					{
						return val1 + val2;
					}

					public static Vector<Double> add (Vector<Double> val1, Single val2) 
					{
						return Vector<Double>.create(val1, value => value + val2);
					}

					public static Vector<Double> add (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 + value2);
					}

											public static Vector<Double> add (Vector<Single> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value + val2);
						}

						public static Vector<Double> add (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 + value2);
						}
									
					public static Double sub (Double val1, Single val2) 
					{
						return val1 - val2;
					}

					public static Vector<Double> sub (Vector<Double> val1, Single val2) 
					{
						return Vector<Double>.create(val1, value => value - val2);
					}

					public static Vector<Double> sub (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 - value2);
					}

											public static Vector<Double> sub (Vector<Single> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value - val2);
						}

						public static Vector<Double> sub (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 - value2);
						}
									
					public static Double mul (Double val1, Single val2) 
					{
						return val1 * val2;
					}

					public static Vector<Double> mul (Vector<Double> val1, Single val2) 
					{
						return Vector<Double>.create(val1, value => value * val2);
					}

					public static Vector<Double> mul (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 * value2);
					}

											public static Vector<Double> mul (Vector<Single> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value * val2);
						}

						public static Vector<Double> mul (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 * value2);
						}
									
					public static Double div (Double val1, Single val2) 
					{
						return val1 / val2;
					}

					public static Vector<Double> div (Vector<Double> val1, Single val2) 
					{
						return Vector<Double>.create(val1, value => value / val2);
					}

					public static Vector<Double> div (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 / value2);
					}

											public static Vector<Double> div (Vector<Single> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value / val2);
						}

						public static Vector<Double> div (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 / value2);
						}
									
				
					public static bool gt (Double val1, Single val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Double> val1, Single val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Double, Single>(val1, val2, (value1, value2) => value1 > value2);
					}

											public static bool gt (Single val1, Double val2) 
						{
							return val1 > val2;
						}

						public static Vector<bool> gt (Vector<Single> val1, Double val2) 
						{
							return Vector<bool>.create<Single>(val1, value => value > val2);
						}

						public static Vector<bool> gt (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Single, Double>(val1, val2, (value1, value2) => value1 > value2);
						}
									
					public static bool ge (Double val1, Single val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Double> val1, Single val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Double, Single>(val1, val2, (value1, value2) => value1 >= value2);
					}

											public static bool ge (Single val1, Double val2) 
						{
							return val1 >= val2;
						}

						public static Vector<bool> ge (Vector<Single> val1, Double val2) 
						{
							return Vector<bool>.create<Single>(val1, value => value >= val2);
						}

						public static Vector<bool> ge (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Single, Double>(val1, val2, (value1, value2) => value1 >= value2);
						}
									
					public static bool lt (Double val1, Single val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Double> val1, Single val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Double, Single>(val1, val2, (value1, value2) => value1 < value2);
					}

											public static bool lt (Single val1, Double val2) 
						{
							return val1 < val2;
						}

						public static Vector<bool> lt (Vector<Single> val1, Double val2) 
						{
							return Vector<bool>.create<Single>(val1, value => value < val2);
						}

						public static Vector<bool> lt (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Single, Double>(val1, val2, (value1, value2) => value1 < value2);
						}
									
					public static bool le (Double val1, Single val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Double> val1, Single val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Double, Single>(val1, val2, (value1, value2) => value1 <= value2);
					}

											public static bool le (Single val1, Double val2) 
						{
							return val1 <= val2;
						}

						public static Vector<bool> le (Vector<Single> val1, Double val2) 
						{
							return Vector<bool>.create<Single>(val1, value => value <= val2);
						}

						public static Vector<bool> le (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Single, Double>(val1, val2, (value1, value2) => value1 <= value2);
						}
									
					public static bool eq (Double val1, Single val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Double> val1, Single val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Double, Single>(val1, val2, (value1, value2) => value1 == value2);
					}

											public static bool eq (Single val1, Double val2) 
						{
							return val1 == val2;
						}

						public static Vector<bool> eq (Vector<Single> val1, Double val2) 
						{
							return Vector<bool>.create<Single>(val1, value => value == val2);
						}

						public static Vector<bool> eq (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Single, Double>(val1, val2, (value1, value2) => value1 == value2);
						}
									
					public static bool neq (Double val1, Single val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Double> val1, Single val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Double> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Double, Single>(val1, val2, (value1, value2) => value1 != value2);
					}

											public static bool neq (Single val1, Double val2) 
						{
							return val1 != val2;
						}

						public static Vector<bool> neq (Vector<Single> val1, Double val2) 
						{
							return Vector<bool>.create<Single>(val1, value => value != val2);
						}

						public static Vector<bool> neq (Vector<Single> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Single, Double>(val1, val2, (value1, value2) => value1 != value2);
						}
												
				
					public static Double add (Double val1, Int64 val2) 
					{
						return val1 + val2;
					}

					public static Vector<Double> add (Vector<Double> val1, Int64 val2) 
					{
						return Vector<Double>.create(val1, value => value + val2);
					}

					public static Vector<Double> add (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 + value2);
					}

											public static Vector<Double> add (Vector<Int64> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value + val2);
						}

						public static Vector<Double> add (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 + value2);
						}
									
					public static Double sub (Double val1, Int64 val2) 
					{
						return val1 - val2;
					}

					public static Vector<Double> sub (Vector<Double> val1, Int64 val2) 
					{
						return Vector<Double>.create(val1, value => value - val2);
					}

					public static Vector<Double> sub (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 - value2);
					}

											public static Vector<Double> sub (Vector<Int64> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value - val2);
						}

						public static Vector<Double> sub (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 - value2);
						}
									
					public static Double mul (Double val1, Int64 val2) 
					{
						return val1 * val2;
					}

					public static Vector<Double> mul (Vector<Double> val1, Int64 val2) 
					{
						return Vector<Double>.create(val1, value => value * val2);
					}

					public static Vector<Double> mul (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 * value2);
					}

											public static Vector<Double> mul (Vector<Int64> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value * val2);
						}

						public static Vector<Double> mul (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 * value2);
						}
									
					public static Double div (Double val1, Int64 val2) 
					{
						return val1 / val2;
					}

					public static Vector<Double> div (Vector<Double> val1, Int64 val2) 
					{
						return Vector<Double>.create(val1, value => value / val2);
					}

					public static Vector<Double> div (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 / value2);
					}

											public static Vector<Double> div (Vector<Int64> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value / val2);
						}

						public static Vector<Double> div (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 / value2);
						}
									
				
					public static bool gt (Double val1, Int64 val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Double> val1, Int64 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Double, Int64>(val1, val2, (value1, value2) => value1 > value2);
					}

											public static bool gt (Int64 val1, Double val2) 
						{
							return val1 > val2;
						}

						public static Vector<bool> gt (Vector<Int64> val1, Double val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value > val2);
						}

						public static Vector<bool> gt (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int64, Double>(val1, val2, (value1, value2) => value1 > value2);
						}
									
					public static bool ge (Double val1, Int64 val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Double> val1, Int64 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Double, Int64>(val1, val2, (value1, value2) => value1 >= value2);
					}

											public static bool ge (Int64 val1, Double val2) 
						{
							return val1 >= val2;
						}

						public static Vector<bool> ge (Vector<Int64> val1, Double val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value >= val2);
						}

						public static Vector<bool> ge (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int64, Double>(val1, val2, (value1, value2) => value1 >= value2);
						}
									
					public static bool lt (Double val1, Int64 val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Double> val1, Int64 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Double, Int64>(val1, val2, (value1, value2) => value1 < value2);
					}

											public static bool lt (Int64 val1, Double val2) 
						{
							return val1 < val2;
						}

						public static Vector<bool> lt (Vector<Int64> val1, Double val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value < val2);
						}

						public static Vector<bool> lt (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int64, Double>(val1, val2, (value1, value2) => value1 < value2);
						}
									
					public static bool le (Double val1, Int64 val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Double> val1, Int64 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Double, Int64>(val1, val2, (value1, value2) => value1 <= value2);
					}

											public static bool le (Int64 val1, Double val2) 
						{
							return val1 <= val2;
						}

						public static Vector<bool> le (Vector<Int64> val1, Double val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value <= val2);
						}

						public static Vector<bool> le (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int64, Double>(val1, val2, (value1, value2) => value1 <= value2);
						}
									
					public static bool eq (Double val1, Int64 val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Double> val1, Int64 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Double, Int64>(val1, val2, (value1, value2) => value1 == value2);
					}

											public static bool eq (Int64 val1, Double val2) 
						{
							return val1 == val2;
						}

						public static Vector<bool> eq (Vector<Int64> val1, Double val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value == val2);
						}

						public static Vector<bool> eq (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int64, Double>(val1, val2, (value1, value2) => value1 == value2);
						}
									
					public static bool neq (Double val1, Int64 val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Double> val1, Int64 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Double> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Double, Int64>(val1, val2, (value1, value2) => value1 != value2);
					}

											public static bool neq (Int64 val1, Double val2) 
						{
							return val1 != val2;
						}

						public static Vector<bool> neq (Vector<Int64> val1, Double val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value != val2);
						}

						public static Vector<bool> neq (Vector<Int64> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int64, Double>(val1, val2, (value1, value2) => value1 != value2);
						}
												
				
					public static Double add (Double val1, Int32 val2) 
					{
						return val1 + val2;
					}

					public static Vector<Double> add (Vector<Double> val1, Int32 val2) 
					{
						return Vector<Double>.create(val1, value => value + val2);
					}

					public static Vector<Double> add (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 + value2);
					}

											public static Vector<Double> add (Vector<Int32> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value + val2);
						}

						public static Vector<Double> add (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 + value2);
						}
									
					public static Double sub (Double val1, Int32 val2) 
					{
						return val1 - val2;
					}

					public static Vector<Double> sub (Vector<Double> val1, Int32 val2) 
					{
						return Vector<Double>.create(val1, value => value - val2);
					}

					public static Vector<Double> sub (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 - value2);
					}

											public static Vector<Double> sub (Vector<Int32> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value - val2);
						}

						public static Vector<Double> sub (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 - value2);
						}
									
					public static Double mul (Double val1, Int32 val2) 
					{
						return val1 * val2;
					}

					public static Vector<Double> mul (Vector<Double> val1, Int32 val2) 
					{
						return Vector<Double>.create(val1, value => value * val2);
					}

					public static Vector<Double> mul (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 * value2);
					}

											public static Vector<Double> mul (Vector<Int32> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value * val2);
						}

						public static Vector<Double> mul (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 * value2);
						}
									
					public static Double div (Double val1, Int32 val2) 
					{
						return val1 / val2;
					}

					public static Vector<Double> div (Vector<Double> val1, Int32 val2) 
					{
						return Vector<Double>.create(val1, value => value / val2);
					}

					public static Vector<Double> div (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<Double>.create(val1, val2, (value1, value2) => value1 / value2);
					}

											public static Vector<Double> div (Vector<Int32> val1, Double val2) 
						{
							return Vector<Double>.create(val1, value => value / val2);
						}

						public static Vector<Double> div (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<Double>.create(val1, val2, (value1, value2) => value1 / value2);
						}
									
				
					public static bool gt (Double val1, Int32 val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Double> val1, Int32 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Double, Int32>(val1, val2, (value1, value2) => value1 > value2);
					}

											public static bool gt (Int32 val1, Double val2) 
						{
							return val1 > val2;
						}

						public static Vector<bool> gt (Vector<Int32> val1, Double val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value > val2);
						}

						public static Vector<bool> gt (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int32, Double>(val1, val2, (value1, value2) => value1 > value2);
						}
									
					public static bool ge (Double val1, Int32 val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Double> val1, Int32 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Double, Int32>(val1, val2, (value1, value2) => value1 >= value2);
					}

											public static bool ge (Int32 val1, Double val2) 
						{
							return val1 >= val2;
						}

						public static Vector<bool> ge (Vector<Int32> val1, Double val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value >= val2);
						}

						public static Vector<bool> ge (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int32, Double>(val1, val2, (value1, value2) => value1 >= value2);
						}
									
					public static bool lt (Double val1, Int32 val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Double> val1, Int32 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Double, Int32>(val1, val2, (value1, value2) => value1 < value2);
					}

											public static bool lt (Int32 val1, Double val2) 
						{
							return val1 < val2;
						}

						public static Vector<bool> lt (Vector<Int32> val1, Double val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value < val2);
						}

						public static Vector<bool> lt (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int32, Double>(val1, val2, (value1, value2) => value1 < value2);
						}
									
					public static bool le (Double val1, Int32 val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Double> val1, Int32 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Double, Int32>(val1, val2, (value1, value2) => value1 <= value2);
					}

											public static bool le (Int32 val1, Double val2) 
						{
							return val1 <= val2;
						}

						public static Vector<bool> le (Vector<Int32> val1, Double val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value <= val2);
						}

						public static Vector<bool> le (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int32, Double>(val1, val2, (value1, value2) => value1 <= value2);
						}
									
					public static bool eq (Double val1, Int32 val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Double> val1, Int32 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Double, Int32>(val1, val2, (value1, value2) => value1 == value2);
					}

											public static bool eq (Int32 val1, Double val2) 
						{
							return val1 == val2;
						}

						public static Vector<bool> eq (Vector<Int32> val1, Double val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value == val2);
						}

						public static Vector<bool> eq (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int32, Double>(val1, val2, (value1, value2) => value1 == value2);
						}
									
					public static bool neq (Double val1, Int32 val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Double> val1, Int32 val2) 
					{
						return Vector<bool>.create<Double>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Double> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Double, Int32>(val1, val2, (value1, value2) => value1 != value2);
					}

											public static bool neq (Int32 val1, Double val2) 
						{
							return val1 != val2;
						}

						public static Vector<bool> neq (Vector<Int32> val1, Double val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value != val2);
						}

						public static Vector<bool> neq (Vector<Int32> val1, Vector<Double> val2) 
						{
							return Vector<bool>.create<Int32, Double>(val1, val2, (value1, value2) => value1 != value2);
						}
														
			public static Single ps (Single val) 
			{
				return val;
			}

			public static Vector<Single> ps (Vector<Single> val) 
			{
				return val;
			}

			public static Single ns (Single val) 
			{
				return -val;
			}

			public static Vector<Single> ns (Vector<Single> val) 
			{
				return Vector<Single>.create(val, value => -value );
			}

			
				
					public static Single add (Single val1, Single val2) 
					{
						return val1 + val2;
					}

					public static Vector<Single> add (Vector<Single> val1, Single val2) 
					{
						return Vector<Single>.create(val1, value => value + val2);
					}

					public static Vector<Single> add (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 + value2);
					}

									
					public static Single sub (Single val1, Single val2) 
					{
						return val1 - val2;
					}

					public static Vector<Single> sub (Vector<Single> val1, Single val2) 
					{
						return Vector<Single>.create(val1, value => value - val2);
					}

					public static Vector<Single> sub (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 - value2);
					}

									
					public static Single mul (Single val1, Single val2) 
					{
						return val1 * val2;
					}

					public static Vector<Single> mul (Vector<Single> val1, Single val2) 
					{
						return Vector<Single>.create(val1, value => value * val2);
					}

					public static Vector<Single> mul (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 * value2);
					}

									
					public static Single div (Single val1, Single val2) 
					{
						return val1 / val2;
					}

					public static Vector<Single> div (Vector<Single> val1, Single val2) 
					{
						return Vector<Single>.create(val1, value => value / val2);
					}

					public static Vector<Single> div (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 / value2);
					}

									
				
					public static bool gt (Single val1, Single val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Single> val1, Single val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Single, Single>(val1, val2, (value1, value2) => value1 > value2);
					}

									
					public static bool ge (Single val1, Single val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Single> val1, Single val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Single, Single>(val1, val2, (value1, value2) => value1 >= value2);
					}

									
					public static bool lt (Single val1, Single val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Single> val1, Single val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Single, Single>(val1, val2, (value1, value2) => value1 < value2);
					}

									
					public static bool le (Single val1, Single val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Single> val1, Single val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Single, Single>(val1, val2, (value1, value2) => value1 <= value2);
					}

									
					public static bool eq (Single val1, Single val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Single> val1, Single val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Single, Single>(val1, val2, (value1, value2) => value1 == value2);
					}

									
					public static bool neq (Single val1, Single val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Single> val1, Single val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Single> val1, Vector<Single> val2) 
					{
						return Vector<bool>.create<Single, Single>(val1, val2, (value1, value2) => value1 != value2);
					}

												
				
					public static Single add (Single val1, Int64 val2) 
					{
						return val1 + val2;
					}

					public static Vector<Single> add (Vector<Single> val1, Int64 val2) 
					{
						return Vector<Single>.create(val1, value => value + val2);
					}

					public static Vector<Single> add (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 + value2);
					}

											public static Vector<Single> add (Vector<Int64> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value + val2);
						}

						public static Vector<Single> add (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 + value2);
						}
									
					public static Single sub (Single val1, Int64 val2) 
					{
						return val1 - val2;
					}

					public static Vector<Single> sub (Vector<Single> val1, Int64 val2) 
					{
						return Vector<Single>.create(val1, value => value - val2);
					}

					public static Vector<Single> sub (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 - value2);
					}

											public static Vector<Single> sub (Vector<Int64> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value - val2);
						}

						public static Vector<Single> sub (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 - value2);
						}
									
					public static Single mul (Single val1, Int64 val2) 
					{
						return val1 * val2;
					}

					public static Vector<Single> mul (Vector<Single> val1, Int64 val2) 
					{
						return Vector<Single>.create(val1, value => value * val2);
					}

					public static Vector<Single> mul (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 * value2);
					}

											public static Vector<Single> mul (Vector<Int64> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value * val2);
						}

						public static Vector<Single> mul (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 * value2);
						}
									
					public static Single div (Single val1, Int64 val2) 
					{
						return val1 / val2;
					}

					public static Vector<Single> div (Vector<Single> val1, Int64 val2) 
					{
						return Vector<Single>.create(val1, value => value / val2);
					}

					public static Vector<Single> div (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 / value2);
					}

											public static Vector<Single> div (Vector<Int64> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value / val2);
						}

						public static Vector<Single> div (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 / value2);
						}
									
				
					public static bool gt (Single val1, Int64 val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Single> val1, Int64 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Single, Int64>(val1, val2, (value1, value2) => value1 > value2);
					}

											public static bool gt (Int64 val1, Single val2) 
						{
							return val1 > val2;
						}

						public static Vector<bool> gt (Vector<Int64> val1, Single val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value > val2);
						}

						public static Vector<bool> gt (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int64, Single>(val1, val2, (value1, value2) => value1 > value2);
						}
									
					public static bool ge (Single val1, Int64 val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Single> val1, Int64 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Single, Int64>(val1, val2, (value1, value2) => value1 >= value2);
					}

											public static bool ge (Int64 val1, Single val2) 
						{
							return val1 >= val2;
						}

						public static Vector<bool> ge (Vector<Int64> val1, Single val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value >= val2);
						}

						public static Vector<bool> ge (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int64, Single>(val1, val2, (value1, value2) => value1 >= value2);
						}
									
					public static bool lt (Single val1, Int64 val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Single> val1, Int64 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Single, Int64>(val1, val2, (value1, value2) => value1 < value2);
					}

											public static bool lt (Int64 val1, Single val2) 
						{
							return val1 < val2;
						}

						public static Vector<bool> lt (Vector<Int64> val1, Single val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value < val2);
						}

						public static Vector<bool> lt (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int64, Single>(val1, val2, (value1, value2) => value1 < value2);
						}
									
					public static bool le (Single val1, Int64 val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Single> val1, Int64 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Single, Int64>(val1, val2, (value1, value2) => value1 <= value2);
					}

											public static bool le (Int64 val1, Single val2) 
						{
							return val1 <= val2;
						}

						public static Vector<bool> le (Vector<Int64> val1, Single val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value <= val2);
						}

						public static Vector<bool> le (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int64, Single>(val1, val2, (value1, value2) => value1 <= value2);
						}
									
					public static bool eq (Single val1, Int64 val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Single> val1, Int64 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Single, Int64>(val1, val2, (value1, value2) => value1 == value2);
					}

											public static bool eq (Int64 val1, Single val2) 
						{
							return val1 == val2;
						}

						public static Vector<bool> eq (Vector<Int64> val1, Single val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value == val2);
						}

						public static Vector<bool> eq (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int64, Single>(val1, val2, (value1, value2) => value1 == value2);
						}
									
					public static bool neq (Single val1, Int64 val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Single> val1, Int64 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Single> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Single, Int64>(val1, val2, (value1, value2) => value1 != value2);
					}

											public static bool neq (Int64 val1, Single val2) 
						{
							return val1 != val2;
						}

						public static Vector<bool> neq (Vector<Int64> val1, Single val2) 
						{
							return Vector<bool>.create<Int64>(val1, value => value != val2);
						}

						public static Vector<bool> neq (Vector<Int64> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int64, Single>(val1, val2, (value1, value2) => value1 != value2);
						}
												
				
					public static Single add (Single val1, Int32 val2) 
					{
						return val1 + val2;
					}

					public static Vector<Single> add (Vector<Single> val1, Int32 val2) 
					{
						return Vector<Single>.create(val1, value => value + val2);
					}

					public static Vector<Single> add (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 + value2);
					}

											public static Vector<Single> add (Vector<Int32> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value + val2);
						}

						public static Vector<Single> add (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 + value2);
						}
									
					public static Single sub (Single val1, Int32 val2) 
					{
						return val1 - val2;
					}

					public static Vector<Single> sub (Vector<Single> val1, Int32 val2) 
					{
						return Vector<Single>.create(val1, value => value - val2);
					}

					public static Vector<Single> sub (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 - value2);
					}

											public static Vector<Single> sub (Vector<Int32> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value - val2);
						}

						public static Vector<Single> sub (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 - value2);
						}
									
					public static Single mul (Single val1, Int32 val2) 
					{
						return val1 * val2;
					}

					public static Vector<Single> mul (Vector<Single> val1, Int32 val2) 
					{
						return Vector<Single>.create(val1, value => value * val2);
					}

					public static Vector<Single> mul (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 * value2);
					}

											public static Vector<Single> mul (Vector<Int32> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value * val2);
						}

						public static Vector<Single> mul (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 * value2);
						}
									
					public static Single div (Single val1, Int32 val2) 
					{
						return val1 / val2;
					}

					public static Vector<Single> div (Vector<Single> val1, Int32 val2) 
					{
						return Vector<Single>.create(val1, value => value / val2);
					}

					public static Vector<Single> div (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<Single>.create(val1, val2, (value1, value2) => value1 / value2);
					}

											public static Vector<Single> div (Vector<Int32> val1, Single val2) 
						{
							return Vector<Single>.create(val1, value => value / val2);
						}

						public static Vector<Single> div (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<Single>.create(val1, val2, (value1, value2) => value1 / value2);
						}
									
				
					public static bool gt (Single val1, Int32 val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Single> val1, Int32 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Single, Int32>(val1, val2, (value1, value2) => value1 > value2);
					}

											public static bool gt (Int32 val1, Single val2) 
						{
							return val1 > val2;
						}

						public static Vector<bool> gt (Vector<Int32> val1, Single val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value > val2);
						}

						public static Vector<bool> gt (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int32, Single>(val1, val2, (value1, value2) => value1 > value2);
						}
									
					public static bool ge (Single val1, Int32 val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Single> val1, Int32 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Single, Int32>(val1, val2, (value1, value2) => value1 >= value2);
					}

											public static bool ge (Int32 val1, Single val2) 
						{
							return val1 >= val2;
						}

						public static Vector<bool> ge (Vector<Int32> val1, Single val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value >= val2);
						}

						public static Vector<bool> ge (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int32, Single>(val1, val2, (value1, value2) => value1 >= value2);
						}
									
					public static bool lt (Single val1, Int32 val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Single> val1, Int32 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Single, Int32>(val1, val2, (value1, value2) => value1 < value2);
					}

											public static bool lt (Int32 val1, Single val2) 
						{
							return val1 < val2;
						}

						public static Vector<bool> lt (Vector<Int32> val1, Single val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value < val2);
						}

						public static Vector<bool> lt (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int32, Single>(val1, val2, (value1, value2) => value1 < value2);
						}
									
					public static bool le (Single val1, Int32 val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Single> val1, Int32 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Single, Int32>(val1, val2, (value1, value2) => value1 <= value2);
					}

											public static bool le (Int32 val1, Single val2) 
						{
							return val1 <= val2;
						}

						public static Vector<bool> le (Vector<Int32> val1, Single val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value <= val2);
						}

						public static Vector<bool> le (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int32, Single>(val1, val2, (value1, value2) => value1 <= value2);
						}
									
					public static bool eq (Single val1, Int32 val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Single> val1, Int32 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Single, Int32>(val1, val2, (value1, value2) => value1 == value2);
					}

											public static bool eq (Int32 val1, Single val2) 
						{
							return val1 == val2;
						}

						public static Vector<bool> eq (Vector<Int32> val1, Single val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value == val2);
						}

						public static Vector<bool> eq (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int32, Single>(val1, val2, (value1, value2) => value1 == value2);
						}
									
					public static bool neq (Single val1, Int32 val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Single> val1, Int32 val2) 
					{
						return Vector<bool>.create<Single>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Single> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Single, Int32>(val1, val2, (value1, value2) => value1 != value2);
					}

											public static bool neq (Int32 val1, Single val2) 
						{
							return val1 != val2;
						}

						public static Vector<bool> neq (Vector<Int32> val1, Single val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value != val2);
						}

						public static Vector<bool> neq (Vector<Int32> val1, Vector<Single> val2) 
						{
							return Vector<bool>.create<Int32, Single>(val1, val2, (value1, value2) => value1 != value2);
						}
														
			public static Int64 ps (Int64 val) 
			{
				return val;
			}

			public static Vector<Int64> ps (Vector<Int64> val) 
			{
				return val;
			}

			public static Int64 ns (Int64 val) 
			{
				return -val;
			}

			public static Vector<Int64> ns (Vector<Int64> val) 
			{
				return Vector<Int64>.create(val, value => -value );
			}

			
				
					public static Int64 add (Int64 val1, Int64 val2) 
					{
						return val1 + val2;
					}

					public static Vector<Int64> add (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<Int64>.create(val1, value => value + val2);
					}

					public static Vector<Int64> add (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 + value2);
					}

									
					public static Int64 sub (Int64 val1, Int64 val2) 
					{
						return val1 - val2;
					}

					public static Vector<Int64> sub (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<Int64>.create(val1, value => value - val2);
					}

					public static Vector<Int64> sub (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 - value2);
					}

									
					public static Int64 mul (Int64 val1, Int64 val2) 
					{
						return val1 * val2;
					}

					public static Vector<Int64> mul (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<Int64>.create(val1, value => value * val2);
					}

					public static Vector<Int64> mul (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 * value2);
					}

									
					public static Int64 div (Int64 val1, Int64 val2) 
					{
						return val1 / val2;
					}

					public static Vector<Int64> div (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<Int64>.create(val1, value => value / val2);
					}

					public static Vector<Int64> div (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 / value2);
					}

									
				
					public static bool gt (Int64 val1, Int64 val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Int64, Int64>(val1, val2, (value1, value2) => value1 > value2);
					}

									
					public static bool ge (Int64 val1, Int64 val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Int64, Int64>(val1, val2, (value1, value2) => value1 >= value2);
					}

									
					public static bool lt (Int64 val1, Int64 val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Int64, Int64>(val1, val2, (value1, value2) => value1 < value2);
					}

									
					public static bool le (Int64 val1, Int64 val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Int64, Int64>(val1, val2, (value1, value2) => value1 <= value2);
					}

									
					public static bool eq (Int64 val1, Int64 val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Int64, Int64>(val1, val2, (value1, value2) => value1 == value2);
					}

									
					public static bool neq (Int64 val1, Int64 val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Int64> val1, Int64 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Int64> val1, Vector<Int64> val2) 
					{
						return Vector<bool>.create<Int64, Int64>(val1, val2, (value1, value2) => value1 != value2);
					}

												
				
					public static Int64 add (Int64 val1, Int32 val2) 
					{
						return val1 + val2;
					}

					public static Vector<Int64> add (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<Int64>.create(val1, value => value + val2);
					}

					public static Vector<Int64> add (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 + value2);
					}

											public static Vector<Int64> add (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<Int64>.create(val1, value => value + val2);
						}

						public static Vector<Int64> add (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<Int64>.create(val1, val2, (value1, value2) => value1 + value2);
						}
									
					public static Int64 sub (Int64 val1, Int32 val2) 
					{
						return val1 - val2;
					}

					public static Vector<Int64> sub (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<Int64>.create(val1, value => value - val2);
					}

					public static Vector<Int64> sub (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 - value2);
					}

											public static Vector<Int64> sub (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<Int64>.create(val1, value => value - val2);
						}

						public static Vector<Int64> sub (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<Int64>.create(val1, val2, (value1, value2) => value1 - value2);
						}
									
					public static Int64 mul (Int64 val1, Int32 val2) 
					{
						return val1 * val2;
					}

					public static Vector<Int64> mul (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<Int64>.create(val1, value => value * val2);
					}

					public static Vector<Int64> mul (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 * value2);
					}

											public static Vector<Int64> mul (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<Int64>.create(val1, value => value * val2);
						}

						public static Vector<Int64> mul (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<Int64>.create(val1, val2, (value1, value2) => value1 * value2);
						}
									
					public static Int64 div (Int64 val1, Int32 val2) 
					{
						return val1 / val2;
					}

					public static Vector<Int64> div (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<Int64>.create(val1, value => value / val2);
					}

					public static Vector<Int64> div (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<Int64>.create(val1, val2, (value1, value2) => value1 / value2);
					}

											public static Vector<Int64> div (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<Int64>.create(val1, value => value / val2);
						}

						public static Vector<Int64> div (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<Int64>.create(val1, val2, (value1, value2) => value1 / value2);
						}
									
				
					public static bool gt (Int64 val1, Int32 val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int64, Int32>(val1, val2, (value1, value2) => value1 > value2);
					}

											public static bool gt (Int32 val1, Int64 val2) 
						{
							return val1 > val2;
						}

						public static Vector<bool> gt (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value > val2);
						}

						public static Vector<bool> gt (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<bool>.create<Int32, Int64>(val1, val2, (value1, value2) => value1 > value2);
						}
									
					public static bool ge (Int64 val1, Int32 val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int64, Int32>(val1, val2, (value1, value2) => value1 >= value2);
					}

											public static bool ge (Int32 val1, Int64 val2) 
						{
							return val1 >= val2;
						}

						public static Vector<bool> ge (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value >= val2);
						}

						public static Vector<bool> ge (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<bool>.create<Int32, Int64>(val1, val2, (value1, value2) => value1 >= value2);
						}
									
					public static bool lt (Int64 val1, Int32 val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int64, Int32>(val1, val2, (value1, value2) => value1 < value2);
					}

											public static bool lt (Int32 val1, Int64 val2) 
						{
							return val1 < val2;
						}

						public static Vector<bool> lt (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value < val2);
						}

						public static Vector<bool> lt (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<bool>.create<Int32, Int64>(val1, val2, (value1, value2) => value1 < value2);
						}
									
					public static bool le (Int64 val1, Int32 val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int64, Int32>(val1, val2, (value1, value2) => value1 <= value2);
					}

											public static bool le (Int32 val1, Int64 val2) 
						{
							return val1 <= val2;
						}

						public static Vector<bool> le (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value <= val2);
						}

						public static Vector<bool> le (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<bool>.create<Int32, Int64>(val1, val2, (value1, value2) => value1 <= value2);
						}
									
					public static bool eq (Int64 val1, Int32 val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int64, Int32>(val1, val2, (value1, value2) => value1 == value2);
					}

											public static bool eq (Int32 val1, Int64 val2) 
						{
							return val1 == val2;
						}

						public static Vector<bool> eq (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value == val2);
						}

						public static Vector<bool> eq (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<bool>.create<Int32, Int64>(val1, val2, (value1, value2) => value1 == value2);
						}
									
					public static bool neq (Int64 val1, Int32 val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Int64> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int64>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Int64> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int64, Int32>(val1, val2, (value1, value2) => value1 != value2);
					}

											public static bool neq (Int32 val1, Int64 val2) 
						{
							return val1 != val2;
						}

						public static Vector<bool> neq (Vector<Int32> val1, Int64 val2) 
						{
							return Vector<bool>.create<Int32>(val1, value => value != val2);
						}

						public static Vector<bool> neq (Vector<Int32> val1, Vector<Int64> val2) 
						{
							return Vector<bool>.create<Int32, Int64>(val1, val2, (value1, value2) => value1 != value2);
						}
														
			public static Int32 ps (Int32 val) 
			{
				return val;
			}

			public static Vector<Int32> ps (Vector<Int32> val) 
			{
				return val;
			}

			public static Int32 ns (Int32 val) 
			{
				return -val;
			}

			public static Vector<Int32> ns (Vector<Int32> val) 
			{
				return Vector<Int32>.create(val, value => -value );
			}

			
				
					public static Int32 add (Int32 val1, Int32 val2) 
					{
						return val1 + val2;
					}

					public static Vector<Int32> add (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<Int32>.create(val1, value => value + val2);
					}

					public static Vector<Int32> add (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<Int32>.create(val1, val2, (value1, value2) => value1 + value2);
					}

									
					public static Int32 sub (Int32 val1, Int32 val2) 
					{
						return val1 - val2;
					}

					public static Vector<Int32> sub (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<Int32>.create(val1, value => value - val2);
					}

					public static Vector<Int32> sub (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<Int32>.create(val1, val2, (value1, value2) => value1 - value2);
					}

									
					public static Int32 mul (Int32 val1, Int32 val2) 
					{
						return val1 * val2;
					}

					public static Vector<Int32> mul (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<Int32>.create(val1, value => value * val2);
					}

					public static Vector<Int32> mul (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<Int32>.create(val1, val2, (value1, value2) => value1 * value2);
					}

									
					public static Int32 div (Int32 val1, Int32 val2) 
					{
						return val1 / val2;
					}

					public static Vector<Int32> div (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<Int32>.create(val1, value => value / val2);
					}

					public static Vector<Int32> div (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<Int32>.create(val1, val2, (value1, value2) => value1 / value2);
					}

									
				
					public static bool gt (Int32 val1, Int32 val2) 
					{
						return val1 > val2;
					}

					public static Vector<bool> gt (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int32>(val1, value => value > val2);
					}

					public static Vector<bool> gt (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int32, Int32>(val1, val2, (value1, value2) => value1 > value2);
					}

									
					public static bool ge (Int32 val1, Int32 val2) 
					{
						return val1 >= val2;
					}

					public static Vector<bool> ge (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int32>(val1, value => value >= val2);
					}

					public static Vector<bool> ge (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int32, Int32>(val1, val2, (value1, value2) => value1 >= value2);
					}

									
					public static bool lt (Int32 val1, Int32 val2) 
					{
						return val1 < val2;
					}

					public static Vector<bool> lt (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int32>(val1, value => value < val2);
					}

					public static Vector<bool> lt (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int32, Int32>(val1, val2, (value1, value2) => value1 < value2);
					}

									
					public static bool le (Int32 val1, Int32 val2) 
					{
						return val1 <= val2;
					}

					public static Vector<bool> le (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int32>(val1, value => value <= val2);
					}

					public static Vector<bool> le (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int32, Int32>(val1, val2, (value1, value2) => value1 <= value2);
					}

									
					public static bool eq (Int32 val1, Int32 val2) 
					{
						return val1 == val2;
					}

					public static Vector<bool> eq (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int32>(val1, value => value == val2);
					}

					public static Vector<bool> eq (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int32, Int32>(val1, val2, (value1, value2) => value1 == value2);
					}

									
					public static bool neq (Int32 val1, Int32 val2) 
					{
						return val1 != val2;
					}

					public static Vector<bool> neq (Vector<Int32> val1, Int32 val2) 
					{
						return Vector<bool>.create<Int32>(val1, value => value != val2);
					}

					public static Vector<bool> neq (Vector<Int32> val1, Vector<Int32> val2) 
					{
						return Vector<bool>.create<Int32, Int32>(val1, val2, (value1, value2) => value1 != value2);
					}

															}
}