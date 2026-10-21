namespace RP.Math.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="UlongExtension"/> bit-manipulation helpers.</summary>
    [TestClass]
    public class UlongExtensionTests
    {
        #region Bit Tests

        [TestMethod, TestCategory("Bit")]
        public void Bit_LowestBitOfOddValue_ShouldBeOne_Test()
        {
            Assert.AreEqual(1, ((ulong)0b1011).Bit(0));
        }

        [TestMethod, TestCategory("Bit")]
        public void Bit_LowestBitOfEvenValue_ShouldBeZero_Test()
        {
            Assert.AreEqual(0, ((ulong)0b1010).Bit(0));
        }

        [TestMethod, TestCategory("Bit")]
        public void Bit_AtEachPosition_ShouldReturnThatBit_Test()
        {
            // 0b1010 -> bit0=0, bit1=1, bit2=0, bit3=1
            const ulong value = 0b1010;
            Assert.AreEqual(0, value.Bit(0));
            Assert.AreEqual(1, value.Bit(1));
            Assert.AreEqual(0, value.Bit(2));
            Assert.AreEqual(1, value.Bit(3));
        }

        [TestMethod, TestCategory("Bit")]
        public void Bit_HighestBit_ShouldReturnSignBit_Test()
        {
            Assert.AreEqual(1, (1UL << 63).Bit(63));
            Assert.AreEqual(0, (1UL << 63).Bit(62));
        }

        [TestMethod, TestCategory("Bit")]
        public void Bit_OfZero_ShouldAlwaysBeZero_Test()
        {
            for (var bit = 0; bit < 64; bit++)
            {
                Assert.AreEqual(0, ((ulong)0).Bit(bit));
            }
        }

        [TestMethod, TestCategory("Bit")]
        public void Bit_OfAllOnes_ShouldAlwaysBeOne_Test()
        {
            for (var bit = 0; bit < 64; bit++)
            {
                Assert.AreEqual(1, ulong.MaxValue.Bit(bit));
            }
        }

        #endregion

        #region Bits Tests

        [TestMethod, TestCategory("Bits")]
        public void Bits_SingleBitRange_ShouldEqualBit_Test()
        {
            const ulong value = 0b1101;
            for (var bit = 0; bit < 4; bit++)
            {
                Assert.AreEqual((ulong)value.Bit(bit), value.Bits(bit, bit));
            }
        }

        [TestMethod, TestCategory("Bits")]
        public void Bits_FullWidthRange_ShouldReturnWholeValue_Test()
        {
            const ulong value = 0xDEAD_BEEF_0000_1234;
            Assert.AreEqual(value, value.Bits(63, 0));
        }

        [TestMethod, TestCategory("Bits")]
        public void Bits_ContiguousRange_ShouldExtractInclusiveSlice_Test()
        {
            // 0b1101 -> bits [3..1] are 1,1,0 == 0b110 == 6
            Assert.AreEqual(0b110UL, ((ulong)0b1101).Bits(3, 1));
        }

        [TestMethod, TestCategory("Bits")]
        public void Bits_LowOrderRange_ShouldMaskHigherBits_Test()
        {
            // 0b1101 -> bits [1..0] are 0,1 == 0b01 == 1
            Assert.AreEqual(0b01UL, ((ulong)0b1101).Bits(1, 0));
        }

        [TestMethod, TestCategory("Bits")]
        public void Bits_ShouldDeconstructDoubleFields_Test()
        {
            // IEEE-754 layout of 1.0: sign=0, exponent=1023 (0x3FF), mantissa=0.
            var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(1.0));

            Assert.AreEqual(0, bits.Bit(63));          // sign
            Assert.AreEqual(1023UL, bits.Bits(62, 52));  // biased exponent
            Assert.AreEqual(0UL, bits.Bits(51, 0));      // mantissa
        }

        [TestMethod, TestCategory("Bits")]
        public void Bits_ShouldDeconstructNegativeDoubleFields_Test()
        {
            // IEEE-754 layout of -2.0: sign=1, exponent=1024 (0x400), mantissa=0.
            var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(-2.0));

            Assert.AreEqual(1, bits.Bit(63));          // sign
            Assert.AreEqual(1024UL, bits.Bits(62, 52));  // biased exponent
            Assert.AreEqual(0UL, bits.Bits(51, 0));      // mantissa
        }

        #endregion

        #region BitSeq Tests

        [TestMethod, TestCategory("BitSeq")]
        public void BitSeq_ShouldYieldBitsMostSignificantFirst_Test()
        {
            // 0b1101 over [3..0] -> 1,1,0,1
            CollectionAssert.AreEqual(new[] { 1, 1, 0, 1 }, ((ulong)0b1101).BitSeq(3, 0).ToArray());
        }

        [TestMethod, TestCategory("BitSeq")]
        public void BitSeq_Length_ShouldBeInclusiveRangeSize_Test()
        {
            Assert.AreEqual(8, ((ulong)0).BitSeq(7, 0).Count());
            Assert.AreEqual(1, ((ulong)0).BitSeq(5, 5).Count());
        }

        [TestMethod, TestCategory("BitSeq")]
        public void BitSeq_ShouldMatchIndividualBitCalls_Test()
        {
            const ulong value = 0xA5;
            var expected = new List<int>();
            for (var bit = 7; bit >= 0; bit--)
            {
                expected.Add(value.Bit(bit));
            }

            CollectionAssert.AreEqual(expected.ToArray(), value.BitSeq(7, 0).ToArray());
        }

        [TestMethod, TestCategory("BitSeq")]
        public void BitSeq_SingleBitRange_ShouldYieldOneElement_Test()
        {
            CollectionAssert.AreEqual(new[] { 1 }, (1UL << 40).BitSeq(40, 40).ToArray());
            CollectionAssert.AreEqual(new[] { 0 }, (1UL << 40).BitSeq(39, 39).ToArray());
        }

        #endregion
    }
}
