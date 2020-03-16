#region Instructions
/*
 * You are tasked with writing an algorithm that determines the value of a used car, 
 * given several factors.
 * 
 *    AGE:    Given the number of months of how old the car is, reduce its value one-half 
 *            (0.5) percent.
 *            After 10 years, it's value cannot be reduced further by age. This is not 
 *            cumulative.
 *            
 *    MILES:    For every 1,000 miles on the car, reduce its value by one-fifth of a
 *              percent (0.2). Do not consider remaining miles. After 150,000 miles, it's 
 *              value cannot be reduced further by miles.
 *            
 *    PREVIOUS OWNER:    If the car has had more than 2 previous owners, reduce its value 
 *                       by twenty-five (25) percent. If the car has had no previous  
 *                       owners, add ten (10) percent of the FINAL car value at the end.
 *                    
 *    COLLISION:        For every reported collision the car has been in, remove two (2) 
 *                      percent of it's value up to five (5) collisions.
 *                    
 * 
 *    Each factor should be off of the result of the previous value in the order of
 *        1. AGE
 *        2. MILES
 *        3. PREVIOUS OWNER
 *        4. COLLISION
 *        
 *    E.g., Start with the current value of the car, then adjust for age, take that  
 *    result then adjust for miles, then collision, and finally previous owner. 
 *    Note that if previous owner, had a positive effect, then it should be applied 
 *    AFTER step 4. If a negative effect, then BEFORE step 4.
 */
#endregion

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarPricer
{
	public class Car
	{
		public decimal PurchaseValue { get; set; }
		public int AgeInMonths { get; set; }
		public int NumberOfMiles { get; set; }
		public int NumberOfPreviousOwners { get; set; }
		public int NumberOfCollisions { get; set; }
	}

	public class PriceDeterminator
	{
		private const int MaxMonthReductionLimit = 120;
		private const decimal AgeReductionRate = .005m;     // 1/2 of 1% => .5 * .01

		private const int MaxMileageReductionLimit = 150;   // We reduce every 1,000 miles so 150,000 / 1,000 = 150
		private const decimal MileageReductionRate = .002m; // 1/5 of 1% => .2 * .01

		private const int MaxPrevOwnerReductionLimit = 1;   // We will only ever apply this reduction once
		private const decimal PrevOwnerReductionRate = .25m;

		private const int MaxCollisionReductionLimit = 5;   // Only reduce for first 5 collisions
		private const decimal CollisionReductionRate = .02m;

		public decimal DetermineCarPrice(Car car)
		{
			var numMileReductions = car.NumberOfMiles / 1000; // 1 per thousand miles
			var prevOwnerReductions = car.NumberOfPreviousOwners > 2 ? 1 : 0; // If more than 2 owners reduce it once

			var reducedCarPrice = ReducePrice(car.PurchaseValue, car.AgeInMonths, MaxMonthReductionLimit, AgeReductionRate);
			reducedCarPrice = ReducePrice(reducedCarPrice, numMileReductions, MaxMileageReductionLimit, MileageReductionRate);
			reducedCarPrice = ReducePrice(reducedCarPrice, prevOwnerReductions, MaxPrevOwnerReductionLimit, PrevOwnerReductionRate);
			reducedCarPrice = ReducePrice(reducedCarPrice, car.NumberOfCollisions, MaxCollisionReductionLimit, CollisionReductionRate);

			if (car.NumberOfPreviousOwners == 0)
			{
				reducedCarPrice += reducedCarPrice * .1m;
			}

			return reducedCarPrice;
		}

		/**
		 * Reduces the current price of a car based on the parameters provided. The reductionRate and numTimesToReduce
		 * are used together with the currentCarPrice to determine how much to reduce by. MaxTimesToReduce is used to allow
		 * a limit of times a reduction can occur.
		 */
		public decimal ReducePrice(decimal currentCarPrice, int numTimesToReduce, int maxTimesToReduce, decimal reductionRate)
		{
			if (numTimesToReduce == 0)
			{
				return currentCarPrice;
			}
			if (numTimesToReduce > maxTimesToReduce)
			{
				numTimesToReduce = maxTimesToReduce;
			}

			return currentCarPrice - (currentCarPrice * reductionRate * numTimesToReduce);
		}

		/**
		 * The included unit test is to check against non cumulative price reduction, but I thought it would be fun to also come up with a solution that
		 * uses cumulative reduction i.e. if there is 2,000 miles on the car reduce the total value for the first 1,000 miles and then
		 * reduce the new price for the second 1,000 miles. So instead of:
		 *
		 *				endValue = startPrice - ( startPrice * reductionRate * 2 );
		 *
		 * you would do:
		 *
		 *				intermediateValue = startPrice - (startPrice * reductionRate);
		 *				endValue = intermediateValue - (intermediateValue * reductionRate);
		 *
		 * This method solves cumulative reduction using recursion and might be more likely to be used in a real scenario.  
		 */
		public decimal CumulativelyReducePrice2(decimal currentCarPrice, int numTimesToReduce, int maxTimesToReduce, decimal reductionRate)
		{
			if (numTimesToReduce == 0)
			{
				return currentCarPrice;
			}
			if (numTimesToReduce > maxTimesToReduce)
			{
				numTimesToReduce = maxTimesToReduce;
			}

			currentCarPrice -= (currentCarPrice * reductionRate);
			return CumulativelyReducePrice2(currentCarPrice, --numTimesToReduce , maxTimesToReduce, reductionRate);
		}
	}

	[TestClass]
	public class UnitTests
	{
		[TestMethod]
		public void CalculateCarValue()
		{
			AssertCarValue(25313.40m, 35000m, 3 * 12, 50000, 1, 1);
			AssertCarValue(19688.20m, 35000m, 3 * 12, 150000, 1, 1);
			AssertCarValue(19688.20m, 35000m, 3 * 12, 250000, 1, 1);
			AssertCarValue(20090.00m, 35000m, 3 * 12, 250000, 1, 0);
			AssertCarValue(21657.02m, 35000m, 3 * 12, 250000, 0, 1);
		}

		private static void AssertCarValue(decimal expectValue, decimal purchaseValue,
		int ageInMonths, int numberOfMiles, int numberOfPreviousOwners, int
		numberOfCollisions)
		{
			Car car = new Car
			{
				AgeInMonths = ageInMonths,
				NumberOfCollisions = numberOfCollisions,
				NumberOfMiles = numberOfMiles,
				NumberOfPreviousOwners = numberOfPreviousOwners,
				PurchaseValue = purchaseValue
			};
			PriceDeterminator priceDeterminator = new PriceDeterminator();
			var carPrice = priceDeterminator.DetermineCarPrice(car);
			Assert.AreEqual(expectValue, carPrice);
		}
	}
}