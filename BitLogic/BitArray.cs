using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitLogic {

    // made my own class because 
    //  the C# source for BitArray is poorly optomized
    //   this code is also poorly//^E optomized but it's better//^E than the default
    //  I need more control

    public class BitArray {

        private const ulong x01_1x = long.MaxValue;
        private const ulong x10_0x = ~x01_1x; //(ulong) long.MinValue;
        
        private const ulong x1_1x = ~ (ulong)0;
        private const ulong x0_0x = 0;
        
        internal const int RawSize = 0x40; // 64
        internal const int RawMask = 0x3f; // 63
        internal const int RawBits = 6;

        private int _size;
        public int Length { get { return _size; }}

        // [ 0] => raw[0] & x10_0
        // [63] => raw[0] & 1
        // [64] => raw[1] & x10_0
        internal ulong[] raw;

        public BitArray(int size) {
            this._size = size;
            if ((size & RawMask) == 0) raw = new ulong[size >> RawBits];
            else raw = new ulong[1 + (size >> RawBits)];
        }
        public BitArray(int size, bool fill) : this(size) {
            if (fill)
                for (int i = raw.Length - 1; i >= 0; i--)
                    raw[i] = x1_1x; // all 1s
        }

        public BitArray(bool[] data) : this(data.Length) {
            this._size = data.Length;
            this.raw = compress(data);
            //setRange(data, 0);
        }

        /// <summary>
        /// use:
        ///  new BitArray("xx xx   xx xx", 'x');
        /// </summary>
        /// <param name="data"></param>
        /// <param name="truevalues"></param>
        public BitArray(string data, params char[] truevalues) : this(data.Length) {
            ulong current;
            for (int i = 0; i < raw.Length; i++) {
                int ioff = i << RawBits;
                current = 0;
                int jmax = ioff + RawSize > _size ? _size - ioff : RawSize;
                for (int j = 0; j < jmax; j++) {
                    for (int ci = 0; ci < truevalues.Length; ci++)
                        if (data[ioff + j] == truevalues[ci]) {
                            current |= x10_0x >> j;
                            goto FOUND_MATCH;
                        }
                  FOUND_MATCH:;
                }
                raw[i] = current;
            }
        }

        public BitArray Clone() {
            BitArray r = new BitArray(_size);
            Copy(this.raw, r.raw, 0, 0);
            return r;
        }

        public bool this[int i] {
            get {
                if (i >= _size) throw new IndexOutOfRangeException();
                return (raw[i >> RawBits] & (x10_0x >> (i & RawMask))) > 0;
            }
            set {
                if (i >= _size) throw new IndexOutOfRangeException();
                int rawi = i >> RawBits;
                if (value) { // set bit
                    raw[rawi] = (ulong) (raw[rawi] |  (x10_0x >> (i & RawMask)));
                } else { // clear bit
                    raw[rawi] = (ulong) (raw[rawi] & ~(x10_0x >> (i & RawMask)));
                }
            }
        }

        public void SetRange(bool value, int mini, int maxi) {
            if (value) {
                Copy(raw, raw, mini, mini, maxi - mini + 1, delegate(ulong x, ulong y) { return x1_1x; });
            } else {
                Copy(raw, raw, mini, mini, maxi - mini + 1, delegate(ulong x, ulong y) { return x0_0x; });
            }
        }

        public void SetRange(bool[] data, int index) {
            Copy(compress(data), raw, 0, index);
        }

        ////^E unoptomized
        //private void setRange(bool[] data, int index) {
        //    throw new NotImplementedException();
        //    int imax = index + data.Length;
        //    for (int i = index, di = 0; i < imax; i++, di++) {
        //        int rawi = i >> 6;
        //        if (data[di]) { // set bit
        //            raw[rawi] = (byte) (raw[rawi] |  (x10_0x >> (i & RawMask)));
        //        } else { // clear bit
        //            raw[rawi] = (byte) (raw[rawi] & ~(x10_0x >> (i & RawMask)));
        //        }
        //    }
        //}
        
        private static ulong[] compress(bool[] data) {
            ulong[] r = new ulong[(data.Length - 1) >> RawBits];
            Copy(data, r);
            return r;
        }

        //
        // overload:
        //  ~a
        //  a&b
        //  a|b
        //  a^b (xor)

        public static BitArray operator ~(BitArray a) {
            BitArray r = new BitArray(a._size);
            for (int i = 0; i < a.raw.Length; i++)
                r.raw[i] = ~a.raw[i];
            return r;
        }

        /// <summary>
        /// flips bits
        /// </summary>
        public void NotEqualsSelf() {
            for (int i = 0; i < raw.Length; i++)
                raw[i] = ~raw[i];
        }
        
        public static void NotEquals(BitArray a, int aindex) { NotEquals(a, aindex, a.Length - aindex); }
        public static void NotEquals(BitArray a, int aindex, int length) {
            Copy(a.raw, a.raw, aindex, aindex, length, delegate(ulong x, ulong y) {
                return ~x;
            });
        }
        
        public static BitArray operator &(BitArray a, BitArray b) {
            if (a._size != b._size)
                throw new IndexOutOfRangeException();
            BitArray r = new BitArray(a._size);
            for (int i = 0; i < a.raw.Length; i++)
                r.raw[i] = a.raw[i] & b.raw[i];
            return r;
        }

        public void AndEquals(BitArray other) {
            if (other._size != _size)
                throw new IndexOutOfRangeException();
            for (int i = 0; i < raw.Length; i++)
                raw[i] = raw[i] & other.raw[i];
        }
        
        public static BitArray And(BitArray a, int aindex, BitArray b, int bindex) { return And(a, aindex, b, bindex, Math.Min(a.Length - aindex, b.Length - bindex)); }
        public static BitArray And(BitArray a, int aindex, BitArray b, int bindex, int length) {
            if (aindex != 0 || a._size != length) a = a.subarray(aindex, length);
            if (bindex != 0 || b._size != length) b = b.subarray(bindex, length);
            return a & b;
        }
        
        public static void AndEquals(BitArray a, int aindex, BitArray b, int bindex) { AndEquals(a, aindex, b, bindex, Math.Min(a.Length - aindex, b.Length - bindex)); }
        public static void AndEquals(BitArray a, int aindex, BitArray b, int bindex, int length) {
            Copy(b.raw, a.raw, bindex, aindex, length, delegate(ulong x, ulong y) {
                return x & y;
            });
        }
        
        public static void AndNotEquals(BitArray a, int aindex, BitArray b, int bindex) { AndNotEquals(a, aindex, b, bindex, Math.Min(a.Length - aindex, b.Length - bindex)); }
        public static void AndNotEquals(BitArray a, int aindex, BitArray b, int bindex, int length) {
            Copy(b.raw, a.raw, bindex, aindex, length, delegate(ulong x, ulong y) {
                return x & ~y;
            });
        }
        
        public static BitArray operator |(BitArray a, BitArray b) {
            if (a._size != b._size)
                throw new IndexOutOfRangeException();
            BitArray r = new BitArray(a._size);
            for (int i = 0; i < a.raw.Length; i++)
                r.raw[i] = a.raw[i] | b.raw[i];
            return r;
        }
        
        public void OrEquals(BitArray other) {
            if (other._size != _size)
                throw new IndexOutOfRangeException();
            for (int i = 0; i < raw.Length; i++)
                raw[i] = raw[i] | other.raw[i];
        }
        
        public static BitArray Or(BitArray a, int aindex, BitArray b, int bindex) { return Or(a, aindex, b, bindex, Math.Min(a.Length - aindex, b.Length - bindex)); }
        public static BitArray Or(BitArray a, int aindex, BitArray b, int bindex, int length) {
            if (aindex != 0 || a._size != length) a = a.subarray(aindex, length);
            if (bindex != 0 || b._size != length) b = b.subarray(bindex, length);
            return a | b;
        }
        
        public static void OrEquals(BitArray a, int aindex, BitArray b, int bindex) { OrEquals(a, aindex, b, bindex, Math.Min(a.Length - aindex, b.Length - bindex)); }
        public static void OrEquals(BitArray a, int aindex, BitArray b, int bindex, int length) {
            Copy(b.raw, a.raw, bindex, aindex, length, delegate(ulong x, ulong y) {
                return x | y;
            });
        }
        
        public static BitArray operator ^(BitArray a, BitArray b) {
            if (a._size != b._size)
                throw new IndexOutOfRangeException();
            BitArray r = new BitArray(a._size);
            for (int i = 0; i < a.raw.Length; i++)
                r.raw[i] = a.raw[i] ^ b.raw[i];
            return r;
        }
        
        public void XorEquals(BitArray other) {
            if (other._size != _size)
                throw new IndexOutOfRangeException();
            for (int i = 0; i < raw.Length; i++)
                raw[i] = raw[i] ^ other.raw[i];
        }
        
        public static BitArray Xor(BitArray a, int aindex, BitArray b, int bindex) { return Xor(a, aindex, b, bindex, Math.Min(a.Length - aindex, b.Length - bindex)); }
        public static BitArray Xor(BitArray a, int aindex, BitArray b, int bindex, int length) {
            if (aindex != 0 || a._size != length) a = a.subarray(aindex, length);
            if (bindex != 0 || b._size != length) b = b.subarray(bindex, length);
            return a ^ b;
        }
        
        public static void XorEquals(BitArray a, int aindex, BitArray b, int bindex) { XorEquals(a, aindex, b, bindex, Math.Min(a.Length - aindex, b.Length - bindex)); }
        public static void XorEquals(BitArray a, int aindex, BitArray b, int bindex, int length) {
            Copy(b.raw, a.raw, bindex, aindex, length, delegate(ulong x, ulong y) {
                return x ^ y;
            });
        }

        public bool Contains(BitArray other) {
            if (other._size != _size)
                throw new InvalidOperationException();
            int i = raw.Length - 1;
            int bitpart = _size & RawMask;
            if (bitpart > 0) {
                ulong mask = lmask_of_length(bitpart);
                if (((raw[i] & mask) |~ (other.raw[i] & mask)) != x1_1x)
                    return false;
                --i;
            }
            for (; i >= 0; i--)
                if ((raw[i] |~ other.raw[i]) != x1_1x)
                    return false;
            return true;
        }
        
        public static void Copy(BitArray from, BitArray to) { Copy(from.raw, to.raw, 0, 0, Math.Min(from.Length, to.Length)); }
        public static void Copy(BitArray from, BitArray to, int fromIndex, int toIndex) { Copy(from.raw, to.raw, fromIndex, toIndex, Math.Min(from.Length - fromIndex, to.Length - toIndex)); }
        public static void Copy(BitArray from, BitArray to, int fromIndex, int toIndex, int length) { Copy(from.raw, to.raw, fromIndex, toIndex, length); }
        
        public static void Copy(bool[] from, BitArray to) { Copy(compress(from), to.raw, 0, 0, Math.Min(from.Length, to.Length)); }
        public static void Copy(bool[] from, BitArray to, int fromIndex, int toIndex) { Copy(compress(from), to.raw, fromIndex, toIndex, Math.Min(from.Length - fromIndex, to.Length - toIndex)); }
        public static void Copy(bool[] from, BitArray to, int fromIndex, int toIndex, int length) { Copy(compress(from), to.raw, fromIndex, toIndex, length); }
        
        internal static void Copy(bool[] from, ulong[] to) { Copy(compress(from), to, 0, 0, Math.Min(from.Length, to.Length)); }
        internal static void Copy(bool[] from, ulong[] to, int fromIndex, int toIndex) { Copy(compress(from), to, fromIndex, toIndex, Math.Min(from.Length - fromIndex, to.Length - toIndex)); }
        internal static void Copy(bool[] from, ulong[] to, int fromIndex, int toIndex, int length) { Copy(compress(from), to, fromIndex, toIndex, length); }
        
        internal static void Copy(ulong[] from, ulong[] to) { Copy(from, to, 0, 0, Math.Min(from.Length << RawBits, to.Length << RawBits)); }
        internal static void Copy(ulong[] from, ulong[] to, int fromIndex, int toIndex) { Copy(from, to, fromIndex, toIndex, Math.Min((from.Length << RawBits) - fromIndex, (to.Length << RawBits) - toIndex)); }
        internal static void Copy(ulong[] from, ulong[] to, int fromIndex, int toIndex, int length) { Copy(from, to, fromIndex, toIndex, length, null); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        /// <param name="length"></param>
        /// <param name="op">to[i] = op(to[i], from[i]) ;; if null, to[i] = from[i]</param>
        /// <param name="opfill">to[i] and from[i] are filled with opfill on the sides durring the left and right tail</param>
        internal static void Copy(ulong[] from, ulong[] to, int fromIndex, int toIndex, int length, Func<ulong, ulong, ulong> op) {
            int fromIndex_minrawi = fromIndex >> RawBits;
            int fromIndex_minrawbit = fromIndex & RawMask;
            int fromIndex_maxrawi = (fromIndex + length) >> RawBits;
            int fromIndex_maxrawbit = (fromIndex + length) & RawMask;

            int toIndex_minrawi = toIndex >> RawBits;
            int toIndex_minrawbit = toIndex & RawMask;
            int toIndex_maxrawi = (toIndex + length) >> RawBits;
            int toIndex_maxrawbit = (toIndex + length) & RawMask;

            int fi, ti;

            if (fromIndex_minrawbit == toIndex_minrawbit) { // they line up

                if (toIndex_minrawi == toIndex_maxrawi) { // small copy
                    ulong mask = rmask_of_length(length) << (RawSize - toIndex_maxrawbit); // x0_01_10_0x
                    if (op == null) {
                        to[toIndex_minrawi] = (to[toIndex_minrawi] & ~mask) | (                        from[fromIndex_minrawi]  & mask);
                    } else {
                        to[toIndex_minrawi] = (to[toIndex_minrawi] & ~mask) | (op(to[toIndex_minrawi], from[fromIndex_minrawi]) & mask);
                    }
                } else { // large copy
                    //## copy middle
                    // 0 => 0, 1..64 => 1, etc.
                    int minfull_from = (fromIndex + RawMask) >> RawBits; //fromIndex_rawi + ((fromIndex_rawbit == 0) ? 0 : 1);
                    int minfull_to   = (  toIndex + RawMask) >> RawBits;
                    //// 64..127 => 0, 128..191 => 1, etc.
                    if (op == null) {
                        for (fi = minfull_from, ti = minfull_to; fi < fromIndex_maxrawi; fi++, ti++)
                            to[ti] = from[fi];
                    } else {
                        for (fi = minfull_from, ti = minfull_to; fi < fromIndex_maxrawi; fi++, ti++)
                            to[ti] = op(to[ti], from[fi]);
                    }

                    //## copy left tail
                    if (fromIndex_minrawbit != 0) {
                        // fromIndex_rawbit:
                        //  63 => 0_01, 62 => 0_011, 61 => 0_0111, etc.
                        ulong copy_mask = rmask_of_length(RawSize - fromIndex_minrawbit); // x0_01_1x
                        if (op == null) {
                            to[toIndex_minrawi] = (to[toIndex_minrawi] & ~copy_mask) | (                        from[fromIndex_minrawi]  & copy_mask);
                        } else {
                            to[toIndex_minrawi] = (to[toIndex_minrawi] & ~copy_mask) | (op(to[toIndex_minrawi], from[fromIndex_minrawi]) & copy_mask);
                        }
                    }

                    //## copy right tail
                    if (fromIndex_maxrawbit != 0) {
                        // fromIndex_maxrawbit:
                        //  1 => ~10_0, 2 => ~110_0, 3 => ~1110_0, etc.
                        ulong uncopy_mask = rmask_of_length(RawSize - fromIndex_maxrawbit);
                        if (op == null) {
                            to[ti] = (to[ti] &  uncopy_mask) | (           from[fi]  & ~uncopy_mask) ;
                        } else {
                            to[ti] = (to[ti] &  uncopy_mask) | (op(to[ti], from[fi]) & ~uncopy_mask);
                        }
                    }
                }
            } else { // they don't line up
                
                if (toIndex_minrawi == toIndex_maxrawi) { // small copy
                    if (fromIndex_minrawi == fromIndex_maxrawi) { // to [i] from [j]
                        //^E retest
                        //ulong rmask = rmask_of_length(length);
                        //int to_shift_right = RawSize - toIndex_maxrawbit;
                        //to[toIndex_minrawi] = 
                        //        (to  [  toIndex_minrawi] & ~(rmask << to_shift_right)) | 
                        //    (((from[fromIndex_minrawi] >> (RawSize - fromIndex_maxrawbit)) &  rmask) << to_shift_right) ;
                        ulong tomask   = rmask_of_length(length) << (RawSize -   toIndex_maxrawbit);
                        //ulong frommask = rmask_of_length(length) << (RawSize - fromIndex_maxrawbit);
                        if (op == null) {
                            if (toIndex_minrawbit < fromIndex_minrawbit) {
                                to[toIndex_minrawi] = (to[toIndex_minrawi] & ~tomask) | (                        from[fromIndex_minrawi] << (fromIndex_minrawbit - toIndex_minrawbit)  & tomask);
                            } else {
                                to[toIndex_minrawi] = (to[toIndex_minrawi] & ~tomask) | (                        from[fromIndex_minrawi] >> (toIndex_minrawbit - fromIndex_minrawbit)  & tomask);
                            }
                        } else {
                            if (toIndex_minrawbit < fromIndex_minrawbit) {
                                to[toIndex_minrawi] = (to[toIndex_minrawi] & ~tomask) | (op(to[toIndex_minrawi], from[fromIndex_minrawi] << (fromIndex_minrawbit - toIndex_minrawbit)) & tomask);
                            } else {
                                to[toIndex_minrawi] = (to[toIndex_minrawi] & ~tomask) | (op(to[toIndex_minrawi], from[fromIndex_minrawi] >> (toIndex_minrawbit - fromIndex_minrawbit)) & tomask);
                            }
                        }
                    } else { // to [i] from [j],[j+1]
                        //ulong rmask = rmask_of_length(length);
                        //int to_shift_right = RawSize - toIndex_maxrawbit;
                        ulong mask = rmask_of_length(length) << (RawSize - toIndex_maxrawbit);
                        //to[toIndex_minrawi] = 
                        //     (to  [  toIndex_minrawi] & ~(rmask << to_shift_right)) | 
                        //    ((from[fromIndex_minrawi] & rmask_of_length(RawSize - fromIndex_minrawbit)) << (fromIndex_minrawbit - toIndex_minrawbit)) |
                        //    ((from[fromIndex_maxrawi] & lmask_of_length(fromIndex_maxrawbit)) >> (RawSize - fromIndex_minrawbit + toIndex_minrawbit));
                        if (op == null) {
                            to[toIndex_minrawi] = 
                                 (to  [  toIndex_minrawi] & ~mask) | 
                                ((from[fromIndex_minrawi] & rmask_of_length(RawSize - fromIndex_minrawbit)) << (fromIndex_minrawbit - toIndex_minrawbit)) |
                                ((from[fromIndex_maxrawi] & lmask_of_length(fromIndex_maxrawbit)) >> (RawSize - fromIndex_minrawbit + toIndex_minrawbit)) ;
                        } else {
                            to[toIndex_minrawi] = 
                                 (to  [  toIndex_minrawi] & ~mask) | (op(to[toIndex_minrawi], 
                                ((from[fromIndex_minrawi] & rmask_of_length(RawSize - fromIndex_minrawbit)) << (fromIndex_minrawbit - toIndex_minrawbit)) |
                                ((from[fromIndex_maxrawi] & lmask_of_length(fromIndex_maxrawbit)) >> (RawSize - fromIndex_minrawbit + toIndex_minrawbit))) & mask);
                        }
                    }
                } else { // large copy
                    //## copy middle
                    // 0 => 0, 1..64 => 1, 65..128 => 2, etc.
                    int min_to   = (  toIndex + RawMask) >> RawBits; 
                    int min_from = (fromIndex - 1      ) >> RawBits;
                    int from_shift_left, from_shift_right;
                    if (toIndex_minrawbit < fromIndex_minrawbit) {
                        min_from++;
                        from_shift_left = fromIndex_maxrawbit - toIndex_maxrawbit;
                        from_shift_right = RawSize - from_shift_left;
                    } else {
                        from_shift_right = toIndex_minrawbit - fromIndex_minrawbit;
                        from_shift_left = RawSize - from_shift_right;
                    }
                    if (op == null) {
                        for (fi = min_from, ti = min_to; ti < toIndex_maxrawi; ti++)
                            to[ti] = 
                                (from[  fi] << from_shift_left) | 
                                (from[++fi] >> from_shift_right) ;
                    } else {
                        for (fi = min_from, ti = min_to; ti < toIndex_maxrawi; ti++)
                            to[ti] = op(to[ti], 
                                (from[  fi] << from_shift_left) |
                                (from[++fi] >> from_shift_right));
                    }

                    //## copy left tail
                    if (toIndex_minrawbit != 0) {
                        ulong to_copy_mask = rmask_of_length(RawSize - toIndex_minrawbit);
                        int rawbit_diff = toIndex_minrawbit - fromIndex_minrawbit;
                        if (op == null) {
                            if (rawbit_diff > 0) { // if (fromIndex_minrawbit < toIndex_minrawbit) {
                                to[toIndex_minrawi] = 
                                     (to  [  toIndex_minrawi] & ~to_copy_mask) | 
                                    ((from[fromIndex_minrawi] >> rawbit_diff) & to_copy_mask);
                            } else {
                                //ulong from_copy_mask_0 = rmask_of_length(RawSize - fromIndex_minrawbit);
                                //ulong from_copy_mask_1 = rmask_of_length(-rawbit_diff);
                                //to[toIndex_minrawi] = 
                                //        (to  [  toIndex_minrawi    ] & ~to_copy_mask) | 
                                //        ((from[fromIndex_minrawi    ] & rmask_of_length(RawSize - fromIndex_minrawbit)) << -rawbit_diff) | 
                                //        ((from[fromIndex_minrawi + 1] >> (RawSize + rawbit_diff)) & from_copy_mask_1) ; //^E make it match! vv
                                to[toIndex_minrawi] = 
                                      (to  [  toIndex_minrawi    ] & ~to_copy_mask) | 
                                     ((from[fromIndex_minrawi    ] & rmask_of_length(RawSize - fromIndex_minrawbit)) <<           -rawbit_diff) | 
                                     ((from[fromIndex_minrawi + 1] & lmask_of_length(-rawbit_diff                 )) >> (RawSize + rawbit_diff)) ;
                            }
                        } else {
                            if (rawbit_diff > 0) { // if (fromIndex_minrawbit < toIndex_minrawbit) {
                                to[toIndex_minrawi] = 
                                     (to  [  toIndex_minrawi] & ~to_copy_mask) | (op(to[toIndex_minrawi] ,
                                     (from[fromIndex_minrawi] >> rawbit_diff)) & to_copy_mask);
                            } else {
                                to[toIndex_minrawi] = 
                                      (to  [  toIndex_minrawi    ] & ~to_copy_mask) | (op(to[toIndex_minrawi] , 
                                     ((from[fromIndex_minrawi    ] & rmask_of_length(RawSize - fromIndex_minrawbit)) <<           -rawbit_diff) | 
                                     ((from[fromIndex_minrawi + 1] & lmask_of_length(-rawbit_diff                 )) >> (RawSize + rawbit_diff))) & to_copy_mask);
                            }
                        }
                    }

                    //## copy right tail
                    if (toIndex_maxrawbit != 0) {
                        ulong to_copy_mask = lmask_of_length(toIndex_maxrawbit);
                        int rawbit_diff = toIndex_maxrawbit - fromIndex_maxrawbit;
                        if (op == null) {
                            if (rawbit_diff < 0) { // from_rawbit < to_rawbit
                                to[toIndex_maxrawi] = 
                                     (to  [  toIndex_maxrawi] & ~to_copy_mask) | 
                                    ((from[fromIndex_maxrawi] << -rawbit_diff) & to_copy_mask); 
                            } else { // to_rawbit > from_rawbit
                                to[toIndex_maxrawi] = 
                                     (to  [  toIndex_maxrawi    ] & ~to_copy_mask) | 
                                    ((from[fromIndex_maxrawi - 1] & rmask_of_length(rawbit_diff        )) << (RawSize - rawbit_diff)) | 
                                    ((from[fromIndex_maxrawi    ] & lmask_of_length(fromIndex_maxrawbit)) >>            rawbit_diff );
                            }
                        } else {
                            if (rawbit_diff < 0) { // from_rawbit < to_rawbit
                                to[toIndex_maxrawi] = 
                                     (to  [  toIndex_maxrawi] & ~to_copy_mask) | (op(to[toIndex_maxrawi] ,
                                     (from[fromIndex_maxrawi] << -rawbit_diff)) & to_copy_mask); 
                            } else { // to_rawbit > from_rawbit
                                to[toIndex_maxrawi] = 
                                     (to  [  toIndex_maxrawi    ] & ~to_copy_mask) | (op(to[toIndex_maxrawi],
                                    ((from[fromIndex_maxrawi - 1] & rmask_of_length(rawbit_diff        )) << (RawSize - rawbit_diff)) |
                                    ((from[fromIndex_maxrawi    ] & lmask_of_length(fromIndex_maxrawbit)) >>            rawbit_diff )) & to_copy_mask);
                            }
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj) {
            if (obj is BitArray) {
                BitArray x = (BitArray) obj;
                if (x._size != _size) return false;
                for (int i = raw.Length - 2; i >= 0; i--)
                    if (x.raw[i] != raw[i]) return false;
                ulong lastmask = lmask_of_length(_size & RawMask);
                return (x.raw[raw.Length - 1] & lastmask) == (raw[raw.Length - 1] & lastmask);
            } else if (obj is bool[]) {
                return Equals(new BitArray((bool[]) obj));
            } else {
                return false;
            }
        }
        
        public static bool Equals(BitArray a, int aindex, BitArray b, int bindex) {
            return Equals(a, aindex, b, bindex, Math.Min(a._size - aindex, b._size - bindex)); }
        
        //^E unoptomized
        public static bool Equals(BitArray a, int aindex, BitArray b, int bindex, int length) {
            if (aindex == 0 && length == a._size) {

            } else {
                a = a.subarray(aindex, length);
            }
                
            if (bindex == 0 && length == b.Length) {

            } else {
                b = b.subarray(bindex, length);
            }

            return a.Equals(b);
        }

        //public bool EqualsAt(BitArray array, int thisindex) {
        //    if (thisindex + array._size > _size || thisindex < 0) return false;
        //    return subarray(thisindex, array._size).Equals(array);
        //}

        public BitArray expand(int left, int right, bool fill = false) {
            BitArray r = new BitArray(Length + left + right, fill);
            if (left >= 0)
                Copy(this, r, 0, left);
            else
                Copy(this, r, -left, 0);
            return r;
        }

        public BitArray Scale(int by) {
            BitArray r = new BitArray(_size * by);
            int maxi = raw.Length;
            if (by == 2) {
                ulong left, right;
                int i = 0, ri = 0;
                for (i = 0; i < maxi - 1; i++) {
                    Scale2(raw[i], out left, out right);
                    r.raw[ri++] = left;
                    r.raw[ri++] = right;
                }
                Scale2(raw[i], out left, out right);
                r.raw[ri++] = left;
                if (ri < r.raw.Length)
                    r.raw[ri++] = right;
            } else {
                ulong[] paste = new ulong[by];
                int i = 0, ri = 0;
                for (i = 0, ri = 0; i < maxi - 1; i++) {
                    Scale(raw[i], paste);
                    for (int j = 0; j < by; j++)
                        r.raw[ri++] = paste[j];
                }
                Scale(raw[i], paste);
                for (int j = 0; ri < r.raw.Length; j++)
                    r.raw[ri++] = paste[j];
            }
            return r;
        }
                
        //note: this[i] => (raw[i >> RawBits] & (x10_0x >> (i & RawMask))) > 0;
        private void Scale(ulong x, ulong[] scaled) {
            int by = scaled.Length;
            ulong pastebitsmask = rmask_of_length(by & RawMask);
            int pastefull = by >> RawBits;
            int scaledi = 0;
            int scaledbits = 0;
            for (int i = 0; i < RawSize; i++) {
                if ((x & (x10_0x >> i)) > 0) { // set 1
                    scaled[scaledi] = scaled[scaledi] & lmask_of_length(scaledbits) | (x1_1x >> scaledbits);
                    scaledbits += by;
                    if (scaledbits >= RawSize) { // paste loops
                        int remainder = scaledbits - RawSize;
                        scaledi++;
                        while (remainder < RawSize) {
                            scaled[scaledi++] = x1_1x;
                            remainder -= RawSize;
                        }
                        scaled[scaledi] = x1_1x;
                        scaledbits = remainder;
                    }

                } else { // set 0
                    scaledbits += by;
                    scaled[scaledi] = scaled[scaledi] & lmask_of_length(scaledbits);
                    if (scaledbits >= RawSize) { // paste loops
                        int remainder = scaledbits - RawSize;

                        scaledi += 1 + (remainder >> RawBits);
                        scaledbits = remainder & RawMask;

                        //scaledi++;
                        //while (remainder < RawSize) {
                        //    scaledi++;
                        //    remainder -= RawSize;
                        //}
                        //scaledbits = remainder;
                    }
                }
            }

        }

        private void Scale2(ulong xx, out ulong left, out ulong right) {
            Func<ulong, ulong> rpart = delegate(ulong x) {
                ulong r =
                     (x &          1) | 
                    ((x &          2) <<  1) | 
                    ((x &          4) <<  2) | 
                    ((x &          8) <<  3) | 
                    ((x &       0x10) <<  4) | 
                    ((x &       0x20) <<  5) | 
                    ((x &       0x40) <<  6) | 
                    ((x &       0x80) <<  7) | 
                    ((x &      0x100) <<  8) | 
                    ((x &      0x200) <<  9) | 
                    ((x &      0x400) << 10) | 
                    ((x &      0x800) << 11) | 
                    ((x &     0x1000) << 12) | 
                    ((x &     0x2000) << 13) | 
                    ((x &     0x4000) << 14) | 
                    ((x &     0x8000) << 15) | 
                    ((x &    0x10000) << 16) | 
                    ((x &    0x20000) << 17) | 
                    ((x &    0x40000) << 18) | 
                    ((x &    0x80000) << 19) | 
                    ((x &   0x100000) << 20) | 
                    ((x &   0x200000) << 21) | 
                    ((x &   0x400000) << 22) | 
                    ((x &   0x800000) << 23) | 
                    ((x &  0x1000000) << 24) | 
                    ((x &  0x2000000) << 25) | 
                    ((x &  0x4000000) << 26) | 
                    ((x &  0x8000000) << 27) | 
                    ((x & 0x10000000) << 28) | 
                    ((x & 0x20000000) << 29) | 
                    ((x & 0x40000000) << 30) | 
                    ((x & 0x80000000) << 31);
                return r | r << 1;;
            };
            left = rpart(xx >> 32);
            right = rpart(xx);
        }



        //internal static ulong[] shift_right(ulong[] array, int arraylength, int shiftamount, bool fill) {
        //    if (shiftamount + (arraylength & RawBits) > RawSize) { // size of array will increase

        //        ulong[] r = new ulong[array.Length + 1];

        //        for (int i = 0; i < array.Length; i++) {
        //            r[i] = 

        //    } else { // size of array will NOT increase

        //    }
            
        //}


        public BitArray subarray(int index, int length) {
            if (index < 0) index += _size; 
            if (length < 0) length = _size + length - index;

            if (index > _size) return new BitArray(0);
            if (index + length > _size) length = _size - index;

            BitArray r = new BitArray(length);
            Copy(this, r, index, 0, length);
            return r;
        }

        internal static ulong raw_long_at(ulong[] array, int i) {
            int off = i & RawMask;
            i >>= RawBits;
            return off == 0 ? array[i   ] : (ulong) ((array[i   ] << off) | (array[++i   ] >> (RawSize - off)));
        }
        internal static ulong raw_long_at(ulong[] array, int rawi, int off) {
            return off == 0 ? array[rawi] : (ulong) ((array[rawi] << off) | (array[++rawi] >> (RawSize - off)));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="find"></param>
        /// <returns>index of $find, or -1</returns>
        public int Search(BitArray find) { return Search(find, 0, _size - 1); }
        public int Search(BitArray find, int mini, int maxi) {
            BitArray a = this;
            BitArray b = find;

            if (b._size > a._size)
                return -1;

            if (b._size == 1)
                return IndexOf(b[0]);

            int b_i0, b_len0, b_i1, b_len1;
            b.MaxRun(out b_i0, out b_len0, out b_i1, out b_len1);
            
            if (b_i0 == -1) // b is all 1s
                return IndexOf(a.raw, true , b._size, mini, maxi);
            if (b_i1 == -1) // b is all 0s
                return IndexOf(a.raw, false, b._size, mini, maxi);
            

            if (b_len0 > b_len1) { // search for a run of 0s
                int i = mini;
                int len;
                while (i < maxi) {
                    int found = IndexOf_0s(a.raw, b_len0, i, maxi, out len);
                
                    if (found < 0) return -1;
                    
                    if (b_i0 > 0) {
                        if (a.subarray(found - b_i0, b._size).Equals(b))
                            return     found - b_i0;
                    } else {
                        if (a.subarray(found + len - b_len0, b._size).Equals(b))
                            return     found + len - b_len0;
                    }

                    i = found + len;
                }
                return -1;

            } else { // search for a run of 1s
                int i = mini;
                int len;
                while (i < maxi) {
                    int found = IndexOf_1s(a.raw, b_len1, i, maxi, out len);
                
                    if (found < 0) return -1;
                    
                    if (b_i1 > 0) {
                        if (a.subarray(found - b_i1, b._size).Equals(b))
                            return     found - b_i1;
                    } else {
                        if (a.subarray(found + len - b_len1, b._size).Equals(b))
                            return     found + len - b_len1;
                    }

                    i = found + len;
                }
                return -1;

            }
        }

        public void MaxRun(out int i0, out int len0, out int i1, out int len1) {
            MaxRun(raw, 0, _size - 1, out i0, out len0, out i1, out len1);
        }

        public void MaxRun(int mini, int maxi, out int i0, out int len0, out int i1, out int len1) {
            MaxRun(raw, mini, maxi, out i0, out len0, out i1, out len1);
        }

        internal static int MaxRunOf(ulong[] array, bool value, int mini, int maxi, out int index) {
            int i0, len0, i1, len1;
            MaxRun(array, mini, maxi, out i0, out len0, out i1, out len1);
            index = value ? i1 : i0;
            return value ? len1 : len0;
        }

        internal static int MaxRunOf(ulong[] array, bool value, int mini, int maxi) {
            int i0, len0, i1, len1;
            MaxRun(array, mini, maxi, out i0, out len0, out i1, out len1);
            return value ? len1 : len0;
        }

        internal static void MaxRun(ulong[] array, int mini, int maxi, out int i0, out int len0, out int i1, out int len1) {
            int f0, f1;
            
            f1 = left_0s(array, mini, maxi);
            f0 = left_1s(array, mini, maxi);

            //if (f0 == maxi - mini + 1) { // all 1s
            //    i1 = mini;
            //    len1 = f0;
            //}

            if (f1 == mini) {
            //if (f0 < f1 && f1 != maxi - mini + 1) { // start with run of 1s
                i1 = mini;
                len1 = f0;
                i0 = -1;
                len0 = 0;
                goto NEXT_0;
            } else { // start with run of 0s
                i0 = mini;
                len0 = f1;
                i1 = -1;
                len1 = 0;
                goto NEXT_1;
            }

            WHILE:
                // next 0 run
             NEXT_0:
                
                f1 = f0 + left_0s(array, f0, maxi);
                
                if (f1 - 1 >= maxi) {
                    if (maxi - f0 + 1 > len0) {
                        len0 = maxi - f0 + 1;
                        i0 = f0;
                    }
                    return;
                }
                
                if (f1 - f0 > len0) {
                    len0 = f1 - f0;
                    i0 = f0;
                }

                // next 1 run
             NEXT_1:
                f0 = f1 + left_1s(array, f1, maxi);
                
                if (f0 - 1 >= maxi) {
                    if (maxi - f1 + 1 > len1) {
                        len1 = maxi - f1 + 1;
                        i1 = f1;
                    }
                    return;
                }
                
                if (f0 - f1 > len1) {
                    len1 = f0 - f1;
                    i1 = f1;
                }
                
            goto WHILE;
        }
        
        internal static int IndexOf(ulong[] array, bool value, int length, int mini, int maxi) {
            int actual_legnth;
            return value ? IndexOf_1s(array, length, mini, maxi, out actual_legnth) : IndexOf_0s(array, length, mini, maxi, out actual_legnth);
        }
        internal static int IndexOf(ulong[] array, bool value, int length, int mini, int maxi, out int actual_length) {
            int al;
            int r = value ? IndexOf_1s(array, length, mini, maxi,out al) : IndexOf_0s(array, length, mini, maxi, out al);
            actual_length = al;
            return r;
        }
        internal static int IndexOf_0s(ulong[] array, int minlength, int mini, int maxi, out int actual_length) {
            while (mini <= maxi) {
                int f0 = mini + left_1s(array, mini, maxi);
                if (f0 > maxi) goto NONE;
                int f1 = f0 + left_0s(array, f0, maxi);
                if (f1 > maxi) goto NONE;
                if (f1 - f0 >= minlength) {
                    actual_length = f1 - f0;
                    return f0;
                }
                mini = f1;
            }
            NONE:
            actual_length = 0;
            return -1;
        }
        internal static int IndexOf_1s(ulong[] array, int minlength, int mini, int maxi, out int actual_length) {
            actual_length = 0;
            while (mini <= maxi) {
                int f1 = mini + left_0s(array, mini, maxi);
                if (f1 > maxi) goto NONE;
                int f0 = f1 + left_1s(array, f1, maxi);
                if (f0 > maxi) goto NONE;
                if (f0 - f1 >= minlength) {
                    actual_length = f0 - f1;
                    return f1;
                }
                mini = f0;
            }
            NONE:
            actual_length = 0;
            return -1;
        }


        //public System.Collections.Generic.IEnumerable<int> ForEachTrue() {
        //    System.Collections.Generic.IEnumerable<int> r = Enumerable.
        //}

        //////////////////////////
        // Bit Logic
        //////////////////////////

        public int IndexOf(bool value                    ) { int r = value ? left_0s(          ) : left_1s(          ); return r == _size           ? -1 : r       ; }
        public int IndexOf(bool value, int mini, int maxi) { int r = value ? left_0s(mini, maxi) : left_1s(mini, maxi); return r == maxi - mini + 1 ? -1 : r + mini; }
        
        public int LastIndexOf(bool value                    ) { int r = value ? right_0s(          ) : right_1s(          ); return r == _size           ? -1 : _size - ++r; }
        public int LastIndexOf(bool value, int mini, int maxi) { int r = value ? right_0s(mini, maxi) : right_1s(mini, maxi); return r == maxi - mini + 1 ? -1 : maxi  - ++r; }

        // 0 => 0, 1 => 1, 2 => 3, 3 => 7, etc.
        internal static ulong rmask_of_length(int length) { return ((ulong) 1 << length) - 1; }
        internal static ulong lmask_of_length(int length) { return length == 0 ? 0 : ~(((ulong) x10_0x >> --length) - 1); } //^E simplify if able

        // running from the right
        public int right_run(bool value) { return value ? right_1s() : right_0s(); }
        public int right_run(bool value, int i) { return value ? right_1s(0, i) : right_0s(0, i); }
        public int right_run(bool value, int mini, int maxi) { return value ? right_1s(mini, maxi) : right_0s(mini, maxi); }
        
        // running from the left
        public int left_run(bool value) { return value ? left_1s() : left_0s(); }
        public int left_run(bool value, int i) { return value ? left_1s(i, _size - 1) : left_0s(i, _size - 1); }
        public int left_run(bool value, int mini, int maxi) { return value ? left_1s(mini, maxi) : left_0s(mini, maxi); }

        public int right_0s() { return right_0s(raw, 0, _size - 1); }
        public int right_0s(int mini, int maxi) { return right_0s(raw, mini < 0 ? _size - mini - 1 : mini, maxi < 0 ? _size - maxi - 1 : maxi); }

        public int right_1s() { return right_0s(raw, 0, _size - 1); }
        public int right_1s(int mini, int maxi) { return right_1s(raw, mini < 0 ? _size - mini - 1 : mini, maxi < 0 ? _size - maxi - 1 : maxi); }

        public int left_0s() { return left_0s(raw, 0, _size - 1); }
        public int left_0s(int mini, int maxi) { return left_0s(raw, mini < 0 ? _size - mini - 1 : mini, maxi < 0 ? _size - maxi - 1 : maxi); }

        public int left_1s() { return left_1s(raw, 0, _size - 1); }
        public int left_1s(int mini, int maxi) { return left_1s(raw, mini < 0 ? _size - mini - 1 : mini, maxi < 0 ? _size - maxi - 1 : maxi); }

        internal static int right_1s(ulong[] array, int mini, int maxi) {
            int length = maxi - mini + 1;

            int off = 0;

            int mini_bitpart = mini & RawMask;
            int maxi_bitpart = maxi & RawMask;
            
            int i = maxi >> RawBits;

            // mini: 1..64 => 1, 65..128 => 2,
            int full_arraymin = ((mini - 1) >> RawBits) + 1;
            
            // area is int the middle of a single ulong
            if (full_arraymin > i) {
                ulong look = (~array[i] >> (RawMask - maxi_bitpart)) & rmask_of_length(length);
                if (look == 0) return length;
                return right_0s(look);
            }
            
            // partial rightern area
            if (maxi_bitpart != RawMask) {
                ulong look = (~array[i] & lmask_of_length(maxi_bitpart + 1));
                if (look == 0) {
                    off += maxi_bitpart + 1;
                    i--;
                } else {
                    //return right_0s(look) - RawSize + maxi_bitpart;
                    return right_0s(look) - RawMask + maxi_bitpart;
                }
            }
            
            // full longs
            for (; i >= full_arraymin; i--) {
                if (array[i] == x1_1x) {
                    off += RawSize;
                } else {
                    return off + right_1s(array[i]);
                }
            }

            // partial leftern area
            if (mini_bitpart != 0) {
                ulong look = (~array[i] & rmask_of_length(RawMask - mini_bitpart));
                if (look == 0) {
                    return length;
                } else {
                    return off + right_0s(look);
                }
            }

            return length;
        }

        internal static int right_0s(ulong[] array, int mini, int maxi) {
            int length = maxi - mini + 1;

            int off = 0;

            int mini_bitpart = mini & RawMask;
            int maxi_bitpart = maxi & RawMask;
            
            int i = maxi >> RawBits;

            // mini: 1..64 => 1, 65..128 => 2,
            int full_arraymin = ((mini - 1) >> RawBits) + 1;
            
            // area is int the middle of a single ulong
            if (full_arraymin > i) {
                ulong look = array[i] >> (RawMask - maxi_bitpart) & rmask_of_length(length);
                if (look == 0) return length;
                return right_0s(look);
            }
            
            // partial rightern area
            if (maxi_bitpart != RawMask) {
                ulong look = array[i] & lmask_of_length(maxi_bitpart + 1);
                if (look == 0) {
                    off += maxi_bitpart + 1;
                    i--;
                } else {
                    //return right_0s(look) - RawSize + maxi_bitpart;
                    return right_0s(look) - RawMask + maxi_bitpart;
                }
            }
            
            // full longs
            for (; i >= full_arraymin; i--) {
                if (array[i] == 0) {
                    off += RawSize;
                } else {
                    return off + right_0s(array[i]);
                }
            }

            // partial leftern area
            if (mini_bitpart != 0) {
                ulong look = array[i] & rmask_of_length(RawMask - mini_bitpart);
                if (look == 0) {
                    return length;
                } else {
                    return off + right_0s(look);
                }
            }

            return length;
        }

        internal static int left_1s(ulong[] array, int mini, int maxi) {
            int length = maxi - mini + 1;

            int off = 0;

            int mini_bitpart = mini & RawMask;
            int maxi_bitpart = maxi & RawMask;

            int i = mini >> RawBits;

            // maxi: 63..126 => 0, 127..190 => 1
            int full_arraymax = ((maxi + 1) >> RawBits) - 1;

            // area is int the middle of a single ulong
            if (full_arraymax < i) {
                ulong look = ~((array[i] << mini_bitpart) & lmask_of_length(length));
                if (look == 0) return length;
                return left_0s(look);
            }
            
            // partial leftern area
            if (mini_bitpart > 0) {
                ulong look = (array[i] | lmask_of_length(mini_bitpart));
                //ulong look = ~(array[i] & rmask_of_length(RawSize - mini_bitpart));
                if (look == x1_1x) { // all 1s, continue counting
                    off += RawSize - mini_bitpart;
                    i++;
                } else { // 0s found, return
                    return left_1s(look) - mini_bitpart;
                }
            }
            
            // full longs
            for (; i <= full_arraymax; i++) {
                if (array[i] == x1_1x) {
                    off += RawSize;
                } else {
                    return off + left_0s(~array[i]);
                }
            }
            
            // partial rightern area
            if (maxi_bitpart != RawMask) {
                ulong look = ~(array[i] & lmask_of_length(maxi_bitpart + 1));
                if (look == 0) {
                    return length;
                } else {
                    return off + left_0s(look);
                }
            }

            return length;
        }

        internal static int left_0s(ulong[] array, int mini, int maxi) {
            int length = maxi - mini + 1;

            int off = 0;

            int mini_bitpart = mini & RawMask;
            int maxi_bitpart = maxi & RawMask;

            int i = mini >> RawBits;

            // maxi: 63..126 => 0, 127..190 => 1
            int full_arraymax = ((maxi - 1) >> RawBits) - 1;

            // area is part of a single ulong
            if (full_arraymax < i) {
                ulong look = array[i] << mini_bitpart & lmask_of_length(length);
                if (look == 0) return length;
                return left_0s(look);
            }
            
            // partial leftern area
            if (mini_bitpart > 0) {
                ulong look = array[i] & rmask_of_length(RawSize - mini_bitpart);
                if (look == 0) { // all 0s, continue counting
                    off += RawSize - mini_bitpart;
                    i++;
                } else { // 1s found, return
                    return left_0s(look) - mini_bitpart;
                }
            }
            
            // full longs
            for (; i <= full_arraymax; i++) {
                if (array[i] == 0) {
                    off += RawSize;
                } else {
                    return off + left_0s(array[i]);
                }
            }
            
            // partial rightern area
            if (maxi_bitpart != RawMask) {
                ulong look = array[i] & lmask_of_length(maxi_bitpart + 1);
                if (look == 0) {
                    return length;
                } else {
                    return off + left_0s(look);
                }
            }

            return length;
        }

        internal static int right_0s (ulong x) { return right_1s(~x); }

        internal static int right_1s (ulong x) { return x == 0 ? 64 : 63 - left_0s((x + 1) & (~x)); }

        internal static int left_1s (ulong x) { return left_0s(~x); }

        internal static int left_0s (ulong x) {
            return 
(x <         0x80000000 ? 
(x <             0x8000 ? 
(x <               0x80 ? 
(x <                0x8 ? 
(x <                0x2 ? 
(x <                0x1 ? 64 : 63) : 
(x <                0x4 ? 62 : 61)) : 
(x <               0x20 ? 
(x <               0x10 ? 60 : 59) : 
(x <               0x40 ? 58 : 57))) : 
(x <              0x800 ? 
(x <              0x200 ? 
(x <              0x100 ? 56 : 55) : 
(x <              0x400 ? 54 : 53)) : 
(x <             0x2000 ? 
(x <             0x1000 ? 52 : 51) : 
(x <             0x4000 ? 50 : 49)))) : 
(x <           0x800000 ? 
(x <            0x80000 ? 
(x <            0x20000 ? 
(x <            0x10000 ? 48 : 47) : 
(x <            0x40000 ? 46 : 45)) : 
(x <           0x200000 ? 
(x <           0x100000 ? 44 : 43) : 
(x <           0x400000 ? 42 : 41))) : 
(x <          0x8000000 ? 
(x <          0x2000000 ? 
(x <          0x1000000 ? 40 : 39) : 
(x <          0x4000000 ? 38 : 37)) : 
(x <         0x20000000 ? 
(x <         0x10000000 ? 36 : 35) : 
(x <         0x40000000 ? 34 : 33))))) : 
(x <     0x800000000000 ? 
(x <       0x8000000000 ? 
(x <        0x800000000 ? 
(x <        0x200000000 ? 
(x <        0x100000000 ? 32 : 31) : 
(x <        0x400000000 ? 30 : 29)) : 
(x <       0x2000000000 ? 
(x <       0x1000000000 ? 28 : 27) : 
(x <       0x4000000000 ? 26 : 25))) : 
(x <      0x80000000000 ? 
(x <      0x20000000000 ? 
(x <      0x10000000000 ? 24 : 23) : 
(x <      0x40000000000 ? 22 : 21)) : 
(x <     0x200000000000 ? 
(x <     0x100000000000 ? 20 : 19) : 
(x <     0x400000000000 ? 18 : 17)))) : 
(x <   0x80000000000000 ? 
(x <    0x8000000000000 ? 
(x <    0x2000000000000 ? 
(x <    0x1000000000000 ? 16 : 15) : 
(x <    0x4000000000000 ? 14 : 13)) : 
(x <   0x20000000000000 ? 
(x <   0x10000000000000 ? 12 : 11) : 
(x <   0x40000000000000 ? 10 :  9))) : 
(x <  0x800000000000000 ? 
(x <  0x200000000000000 ? 
(x <  0x100000000000000 ?  8 :  7) : 
(x <  0x400000000000000 ?  6 :  5)) : 
(x < 0x2000000000000000 ? 
(x < 0x1000000000000000 ?  4 :  3) : 
(x < 0x4000000000000000 ?  2 : 
(x < 0x8000000000000000 ?  1 :  0)))))));
        }
    }
}
