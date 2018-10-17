using System;
using NUnit.Framework;

namespace Freedom.UnitTests
{
    [TestFixture]
    public class DateRangeTester
    {
        [Test]
        public void start_and_end_properties_work_correctly()
        {
            DateRange range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2011, 3, 10));

            Assert.That(range.Start, Is.EqualTo(new DateTime(2001, 9, 11)));
            Assert.That(range.End, Is.EqualTo(new DateTime(2011, 3, 10)));
        }

        [Test]
        public void duration_property_should_work_correctly()
        {
            DateRange range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2011, 3, 10));
            Assert.That(range.Duration, Is.EqualTo(new TimeSpan(3467, 0, 0, 0)));
        }

        [Test]
        public void contains_a_date_should_return_true_if_the_date_is_in_the_period()
        {
            DateRange range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2011, 3, 10));

            Assert.False(range.Contains(new DateTime(2000, 1, 1))); // Before the range
            Assert.True(range.Contains(new DateTime(2001, 9, 11))); // Edge case when date is the end of the range (inclusive)
            Assert.True(range.Contains(new DateTime(2005, 3, 16))); // In the range
            Assert.False(range.Contains(new DateTime(2011, 3, 10))); // Edge case when date is the end of the range (exclusive)
            Assert.False(range.Contains(new DateTime(2012, 7, 31))); // After the range
        }

        [Test]
        public void contains_a_date_range_should_return_true_if_the_date_range_is_entirely_in_the_period()
        {
            DateRange range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2011, 3, 10));

            // STARTING BEFORE THE DATE RANGE

            Assert.False(range.Contains(new DateRange(new DateTime(1999, 01, 01), new DateTime(2000, 01, 01)))); // Ends before the start date
            Assert.False(range.Contains(new DateRange(new DateTime(1999, 01, 01), new DateTime(2001, 09, 11)))); // Ends on the start date
            Assert.False(range.Contains(new DateRange(new DateTime(1999, 01, 01), new DateTime(2005, 03, 16)))); // Ends in the range
            Assert.False(range.Contains(new DateRange(new DateTime(1999, 01, 01), new DateTime(2011, 03, 10)))); // Ends on the end date
            Assert.False(range.Contains(new DateRange(new DateTime(1999, 01, 01), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING ON THE SAME START DATE DATE RANGE

            Assert.True(range.Contains(new DateRange(new DateTime(2001, 09, 11), new DateTime(2001, 09, 11)))); // Ends on the start date
            Assert.True(range.Contains(new DateRange(new DateTime(2001, 09, 11), new DateTime(2005, 03, 16)))); // Ends in the range
            Assert.True(range.Contains(new DateRange(new DateTime(2001, 09, 11), new DateTime(2011, 03, 10)))); // Ends on the end date
            Assert.False(range.Contains(new DateRange(new DateTime(2001, 09, 11), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING DURING THE RANGE

            Assert.True(range.Contains(new DateRange(new DateTime(2004, 09, 11), new DateTime(2005, 03, 16)))); // Ends in the range
            Assert.True(range.Contains(new DateRange(new DateTime(2004, 09, 11), new DateTime(2011, 03, 10)))); // Ends on the end date
            Assert.False(range.Contains(new DateRange(new DateTime(2004, 09, 11), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING ON THE END DATE

            Assert.False(range.Contains(new DateRange(new DateTime(2011, 03, 10), new DateTime(2011, 03, 10)))); // Ends on the end date
            Assert.False(range.Contains(new DateRange(new DateTime(2011, 03, 10), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING AFTER THE END DATE

            Assert.False(range.Contains(new DateRange(new DateTime(2012, 03, 10), new DateTime(2012, 07, 31)))); // Ends after the end date
        }

        [Test]
        public void intersects_a_date_range_should_return_true_if_the_date_range_is_partially_in_the_period()
        {
            DateRange range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2011, 3, 10));

            // STARTING BEFORE THE DATE RANGE

            Assert.False(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2000, 01, 01)))); // Ends before the start date
            Assert.False(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2001, 09, 11)))); // Ends on the start date
            Assert.True(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2005, 03, 16)))); // Ends in the range
            Assert.True(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2011, 03, 10)))); // Ends on the end date
            Assert.True(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING ON THE SAME START DATE DATE RANGE

            Assert.True(range.Intersects(new DateRange(new DateTime(2001, 09, 11), new DateTime(2005, 03, 16)))); // Ends in the range
            Assert.True(range.Intersects(new DateRange(new DateTime(2001, 09, 11), new DateTime(2011, 03, 10)))); // Ends on the end date
            Assert.True(range.Intersects(new DateRange(new DateTime(2001, 09, 11), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING DURING THE RANGE

            Assert.True(range.Intersects(new DateRange(new DateTime(2004, 09, 11), new DateTime(2005, 03, 16)))); // Ends in the range
            Assert.True(range.Intersects(new DateRange(new DateTime(2004, 09, 11), new DateTime(2011, 03, 10)))); // Ends on the end date
            Assert.True(range.Intersects(new DateRange(new DateTime(2004, 09, 11), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING ON THE END DATE

            Assert.False(range.Intersects(new DateRange(new DateTime(2011, 03, 10), new DateTime(2012, 07, 31)))); // Ends after the end date

            // STARTING AFTER THE END DATE

            Assert.False(range.Intersects(new DateRange(new DateTime(2012, 03, 10), new DateTime(2012, 07, 31)))); // Ends after the end date
        }

        [Test]
        public void intersects_a_date_range_should_correctly_handle_corner_case_when_one_of_the_date_ranges_has_zero_length()
        {
            // *** NON-ZERO RANGE Intersecting a ZERO RANGE

            DateRange range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2011, 3, 10));

            Assert.True(range.Intersects(new DateRange(new DateTime(2001, 09, 11), new DateTime(2001, 09, 11))));  // Zero range on the start date
            Assert.True(range.Intersects(new DateRange(new DateTime(2004, 09, 11), new DateTime(2004, 09, 11))));  // Zero range during the range
            Assert.False(range.Intersects(new DateRange(new DateTime(2011, 03, 10), new DateTime(2011, 03, 10)))); // Zero range on the end date

            // *** ZERO RANGE Intersecting NON-ZERO RANGES

            range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2001, 9, 11));

            Assert.False(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2000, 01, 01)))); // Starts before and Ends before that date
            Assert.False(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2001, 09, 11)))); // Starts before and Ends on that date
            Assert.True(range.Intersects(new DateRange(new DateTime(1999, 01, 01), new DateTime(2012, 07, 31))));  // Starts before and Ends after that date
            Assert.True(range.Intersects(new DateRange(new DateTime(2001, 09, 11), new DateTime(2012, 07, 31))));  // Starts on and Ends after the end date
            Assert.False(range.Intersects(new DateRange(new DateTime(2012, 03, 10), new DateTime(2012, 07, 31)))); // Starts after and Ends after the end date

            // *** ZERO RANGE Intersecting ZERO RANGE

            Assert.False(range.Intersects(new DateRange(new DateTime(2000, 09, 11), new DateTime(2000, 09, 11))));  // BEFORE
            Assert.True(range.Intersects(new DateRange(new DateTime(2001, 09, 11), new DateTime(2001, 09, 11))));   // SAME DAY
            Assert.False(range.Intersects(new DateRange(new DateTime(2002, 09, 11), new DateTime(2002, 09, 11))));  // AFTER
        }

        [Test]
        public void GetIntersection_should_give_the_intersection_of_two_ranges()
        {
            DateRange range = new DateRange(new DateTime(2001, 9, 11), new DateTime(2011, 3, 10));

            // STARTING BEFORE THE DATE RANGE

            Assert.That(range.GetIntersection(new DateRange(new DateTime(1999, 01, 01), new DateTime(2005, 03, 16))), // Ends in the range
                        Is.EqualTo(new DateRange(new DateTime(2001, 09, 11), new DateTime(2005, 03, 16))));
            Assert.That(range.GetIntersection(new DateRange(new DateTime(1999, 01, 01), new DateTime(2011, 03, 10))), // Ends on the end date
                        Is.EqualTo(new DateRange(new DateTime(2001, 09, 11), new DateTime(2011, 03, 10))));
            Assert.That(range.GetIntersection(new DateRange(new DateTime(1999, 01, 01), new DateTime(2012, 07, 31))), // Ends after the end date
                        Is.EqualTo(new DateRange(new DateTime(2001, 09, 11), new DateTime(2011, 03, 10))));

            // STARTING ON THE SAME START DATE DATE RANGE

            Assert.That(range.GetIntersection(new DateRange(new DateTime(2001, 09, 11), new DateTime(2005, 03, 16))), // Ends in the range
                        Is.EqualTo(new DateRange(new DateTime(2001, 09, 11), new DateTime(2005, 03, 16))));
            Assert.That(range.GetIntersection(new DateRange(new DateTime(2001, 09, 11), new DateTime(2011, 03, 10))), // Ends on the end date
                        Is.EqualTo(new DateRange(new DateTime(2001, 09, 11), new DateTime(2011, 03, 10))));
            Assert.That(range.GetIntersection(new DateRange(new DateTime(2001, 09, 11), new DateTime(2012, 07, 31))), // Ends after the end date
                        Is.EqualTo(new DateRange(new DateTime(2001, 09, 11), new DateTime(2011, 03, 10))));

            // STARTING DURING THE RANGE

            Assert.That(range.GetIntersection(new DateRange(new DateTime(2004, 09, 11), new DateTime(2005, 03, 16))), // Ends in the range
                        Is.EqualTo(new DateRange(new DateTime(2004, 09, 11), new DateTime(2005, 03, 16))));
            Assert.That(range.GetIntersection(new DateRange(new DateTime(2004, 09, 11), new DateTime(2011, 03, 10))), // Ends on the end date
                        Is.EqualTo(new DateRange(new DateTime(2004, 09, 11), new DateTime(2011, 03, 10))));
            Assert.That(range.GetIntersection(new DateRange(new DateTime(2004, 09, 11), new DateTime(2012, 07, 31))), // Ends after the end date
                        Is.EqualTo(new DateRange(new DateTime(2004, 09, 11), new DateTime(2011, 03, 10))));
        }
    }
}
