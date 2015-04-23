using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace BitLogic {

    public class SquareBitArray {

        public static readonly Point NullPoint = new Point(-1, -1);

        private BitArray[] rows;

        public readonly int Width;
        public readonly int Height;

        //^E unsafe
        public SquareBitArray(BitArray[] rows) {
            this.rows = rows;
            if (rows.Length > 0) {
                this.Width = rows[0].Length;
                this.Height = rows.Length;
            } else {
                this.Width = 0;
                this.Height = 0;
            }
        }

        public SquareBitArray(int Width, int Height, bool fill = false) {
            this.Width = Width;
            this.Height = Height;
            rows = new BitArray[Height];
            for (int i = 0; i < Height; i++)
                rows[i] = new BitArray(Width, fill);
        }

        public bool this[int x, int y] {
            get {
                return rows[y][x];
            }
            set {
                rows[y][x] = value;
            }
        }

        public bool this[Point xy] {
            get {
                return rows[xy.Y][xy.X];
            }
            set {
                rows[xy.Y][xy.X] = value;
            }
        }

        public static SquareBitArray operator ~(SquareBitArray a) {
            BitArray[] r = new BitArray[a.Height];
            for (int y = a.Height - 1; y >= 0; y--)
                r[y] = ~a.rows[y];
            return new SquareBitArray(r);
        }

        public static SquareBitArray operator &(SquareBitArray a, SquareBitArray b) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new InvalidOperationException();
            BitArray[] r = new BitArray[a.Height];
            for (int y = a.Height - 1; y >= 0; y--)
                r[y] = a.rows[y] & b.rows[y];
            return new SquareBitArray(r);
        }

        public static SquareBitArray operator |(SquareBitArray a, SquareBitArray b) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new InvalidOperationException();
            BitArray[] r = new BitArray[a.Height];
            for (int y = a.Height - 1; y >= 0; y--)
                r[y] = a.rows[y] | b.rows[y];
            return new SquareBitArray(r);
        }

        public static SquareBitArray operator ^(SquareBitArray a, SquareBitArray b) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new InvalidOperationException();
            BitArray[] r = new BitArray[a.Height];
            for (int y = a.Height - 1; y >= 0; y--)
                r[y] = a.rows[y] ^ b.rows[y];
            return new SquareBitArray(r);
        }
        
        public static SquareBitArray And(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by) { 
            return And(a, ax, ay, b, bx, by, Math.Min(a.Width - ax, b.Width - bx), Math.Min(a.Height - ay, b.Height - by)); }
        public static SquareBitArray And(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by, int width, int height) {
            SquareBitArray r = new SquareBitArray(width, height);
            for (int y = 0; y < height; y++) {
                r.rows[y] = a.rows[y + ay].subarray(ax, width);
                r.rows[y].AndEquals(b.rows[y + by].subarray(bx, width));
            }
            return r;
        }
        
        public static void AndEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by) { 
            AndEquals(a, ax, ay, b, bx, by, Math.Min(a.Width - ax, b.Width - bx), Math.Min(a.Height - ay, b.Height - by)); }
        public static void AndEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by, int width, int height) {
            for (int y = 0; y < height; y++)
                BitArray.AndEquals(a.rows[y + ay], ax, b.rows[y + by], bx, width);
        }
        
        public static void AndNotEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by) { 
            AndNotEquals(a, ax, ay, b, bx, by, Math.Min(a.Width - ax, b.Width - bx), Math.Min(a.Height - ay, b.Height - by)); }
        public static void AndNotEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by, int width, int height) {
            for (int y = 0; y < height; y++)
                BitArray.AndNotEquals(a.rows[y + ay], ax, b.rows[y + by], bx, width);
        }
        
        public static void OrEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by) { 
            OrEquals(a, ax, ay, b, bx, by, Math.Min(a.Width - ax, b.Width - bx), Math.Min(a.Height - ay, b.Height - by)); }
        public static void OrEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by, int width, int height) {
            for (int y = 0; y < height; y++)
                BitArray.OrEquals(a.rows[y + ay], ax, b.rows[y + by], bx, width);
        }
        
        public static void XorEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by) { 
            XorEquals(a, ax, ay, b, bx, by, Math.Min(a.Width - ax, b.Width - bx), Math.Min(a.Height - ay, b.Height - by)); }
        public static void XorEquals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by, int width, int height) {
            for (int y = 0; y < height; y++)
                BitArray.XorEquals(a.rows[y + ay], ax, b.rows[y + by], bx, width);
        }

        public bool Contains(SquareBitArray other) {
            if (Width != other.Width || Height != other.Height)
                throw new InvalidOperationException();
            for (int y = Height - 1; y >= 0; y--)
                if (!rows[y].Contains(other.rows[y]))
                    return false;
            return true;
        }

        public SquareBitArray expand(int W, int N, int E, int S, bool fill = false) {
            BitArray[] r; // = new BitArray[Height + N + S]
            int rWidth = Width + W + E;

            if (E == 0 && W == 0) {
                r = common.expand_linear(rows, N, S);
            } else {
                //r = new BitArray[Height + N + S];
                r = new BitArray[rows.Length];
                for (int i = 0; i < rows.Length; i++)
                    r[i] = rows[i].expand(W, E, fill);
                r = common.expand_linear(r, N, S);
            }

            if (N > 0)
                for (int i = 0; i < N; i++)
                    r[i] = new BitArray(rWidth, fill);
            if (S > 0)
                //for (int i = 0, ri = Height - S - 1; i < S; i++, ri++)
                for (int i = 0, ri = r.Length - 1; i < S; i++, ri--)
                    r[ri] = new BitArray(rWidth, fill);
            return new SquareBitArray(r);
        }

        public SquareBitArray Scale(int by) {
            BitArray[] rows = new BitArray[Height * by];
            for (int i = 0, rowsi = 0; i < Height; i++) {
                BitArray row = this.rows[i].Scale(by);
                rows[rowsi++] = row;
                for (int j = 1; j < by; j++) {
                    rows[rowsi++] = row.Clone();
                }
            }
            return new SquareBitArray(rows);
        }

        public override bool Equals(object obj) {
            if (obj is SquareBitArray) {
                SquareBitArray a = this;
                SquareBitArray b = (SquareBitArray) obj;
                for (int y = Height - 1; y >= 0; y--)
                    if (!a.rows[y].Equals(b.rows[y]))
                        return false;
                return true;
            }
            return false;
        }
        
        public static bool Equals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by) {
            return Equals(a, ax, ay, b, bx, by, Math.Min(a.Width - ax, b.Width - bx), Math.Min(a.Height - ay, b.Height - by));
        }
        public static bool Equals(SquareBitArray a, int ax, int ay, SquareBitArray b, int bx, int by, int width, int height) {
            for (int y = 0; y < height; y++)
                if (!BitArray.Equals(a.rows[ay + y], ax, b.rows[by + y], bx, width))
                    return false;
            return true;
        }

        public SquareBitArray Clone() {
            BitArray[] r = new BitArray[rows.Length];
            for (int i = 0; i < Height; i++)
                r[i] = rows[i].Clone();
            return new SquareBitArray(r);
        }
        
        public SquareBitArray Trim() { return Trim(false); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="removevalue">value to be removed</param>
        /// <returns></returns>
        public SquareBitArray Trim(bool removevalue) {
            int W,N,E,S = 0;
            bool keep = !removevalue;
            // N=3 => remove rows < 3
            for (N = 0; N < Height; N++)
                if (rows[N].IndexOf(keep) >= 0)
                    break;
            if (N == Height) return new SquareBitArray(0, 0);
            // S=20 => remove rows > 20
            for (S = Height - 1; S > N; S--)
                if (rows[S].IndexOf(keep) >= 0)
                    break;
            E = 0;
            W = Width;
            for (int i = N; i <= S; i++) {
                int min = rows[i].IndexOf(keep);
                if (min > 0 && min < W) {
                    W = min;
                    if (W == 0)
                        break;
                }
            }
            for (int i = N; i <= S; i++) {
                int max = rows[i].LastIndexOf(keep);
                if (max > E) {
                    E = max;
                    if (E == Width - 1) break;
                }
            }
            SquareBitArray r = new SquareBitArray(E - W + 1, S - N + 1);
            Copy(this, r, W, N, 0, 0);
            return r;
        }
        
        public static void Copy(SquareBitArray from, SquareBitArray to) { Copy(from, to, 0, 0, 0, 0, Math.Min(from.Width, to.Width), Math.Min(from.Height, to.Height), null); }
        public static void Copy(SquareBitArray from, SquareBitArray to, int fromX, int fromY, int toX, int toY) {
            Copy(from, to, fromX, fromY, toX, toY, Math.Min(from.Width - fromX, to.Width - toX), Math.Min(from.Height - fromY, to.Height - toY), null); }
        public static void Copy(SquareBitArray from, SquareBitArray to, int fromX, int fromY, int toX, int toY, int width, int height) {
            Copy(from, to, fromX, fromY, toX, toY, width, height, null); }


        internal static void Copy(SquareBitArray from, SquareBitArray to, int fromX, int fromY, int toX, int toY, int width, int height, Func<ulong, ulong, ulong> op) {
            for (int y = 0; y < height; y++) {
                BitArray.Copy(from.rows[y + fromY].raw, to.rows[y + toY].raw, fromX, toX, width, op);
            }
        }

        // used by FloodFind()
        internal struct line {
            internal int x, y, length;
            internal int right { get { return x + length - 1; }}
            internal line(int x, int y, int length) { this.x = x; this.y = y; this.length = length; }
            IEnumerator<int> loopX() {
                int mX = x + length;
                for (int i = x; i < mX; i++) yield return i;
            }
            IEnumerator<int> loopY() {
                int mY = y + length;
                for (int i = y; i < mY; i++) yield return i;
            }
            internal line offset(int dx, int dy) { return new line(x+dx, y+dy, length); }
        }
        
        //public static SquareBitArray FloodFind<T>(T[,] search, Point at, Func<T, bool> is_ok) {
        public static SquareBitArray FloodFind<T>(Func<int, int, T> search, int Width, int Height, Point at, Func<T, bool> is_ok) {
            
            //int Width  = search.GetLength(0);
            //int Height = search.GetLength(1);
            
            SquareBitArray r = new SquareBitArray(Width, Height);

            if (!is_ok(search(at.X, at.Y)))
                return r;
            
            List<line> todoN = new List<line>();
            List<line> todoS = new List<line>();

            Func<List<line>, line> pop = delegate(List<line> list) {
                line rline = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return rline;
            };
            Func<line> popN = delegate { return pop(todoN); };
            Func<line> popS = delegate { return pop(todoS); };

            // finds line to the right of x,y
            // does not check x,y
            Func<int, int, bool, line> findR = delegate(int x, int y, bool okvalue) {
                int rx = x;
                while (++rx < Width)
                    if (is_ok(search(rx,y)) != okvalue)
                        break;
                return new line(x, y, rx-x);
            };
            
            Func<int, int, bool, line> findL = delegate(int x, int y, bool okvalue) {
                int lx = x;
                while (--lx >= 0)
                    if (is_ok(search(lx,y)) != okvalue)
                        break;
                return new line(lx + 1, y, x - lx);
            };

            ////int start_left  = at.X - rows[at.Y].right_run(value, at.X);
            ////int start_right = at.X + rows[at.Y] .left_run(value, at.X);
            ////line start = new line(start_left + 1, at.Y, start_right - start_left - 1);
            ////line start = findR(at.X, at.Y, true);
            //int lx = at.X;
            //while (lx-- >= 0)
            //    if (!is_ok(search[lx, at.Y]))
            //        break;
            //start = new line(lx + 1, at.Y, at.X + start.length - lx - 1);
            line startR = findR(at.X, at.Y, true);
            line startL = findL(at.X, at.Y, true);
            line start = new line(startL.x, at.Y, startL.length + startR.length - 1);
            r.rows[at.Y].SetRange(true, start.x, start.x + start.length - 1);
            todoS.Add(start);
            todoN.Add(start);

            Action<line, int, List<line>, List<line>> dolook = delegate(line From, int looky, List<line> A, List<line> B) {
                if (looky < 0 || looky >= Height) 
                    return;
                int lookx;
                //if (rows[looky][From.x] == value) {
                if (is_ok(search(From.x, looky))) {
                    line goleft = findL(From.x, looky, true);
                    line goright = findR(From.x, looky, true);
                    //int goleft = rows[looky].right_run(value, From.x);
                    //int goright = rows[looky].left_run(value, From.x);
                    if (!r.rows[looky][From.x]) {
                        r.rows[looky].SetRange(true, From.x - goleft.length + 1, From.x + goright.length - 1);
                        line add = new line(From.x - goleft.length + 1, looky, goright.length + goleft.length - 1);
                        A.Add(add);
                    }
                    if (goleft.x < From.x - 1) // W overhang
                        B.Add(new line(goleft.x, looky, From.x - goleft.x - 1));
                    
                    if (goright.length > From.length + 1)// E overhang
                        B.Add(new line(From.x + From.length + 1, looky, goright.length - From.length - 1));
                    if (goright.length >= From.length - 1)
                        return;
                    //lookx = From.x + goright + rows[looky].left_run(!value, From.x);
                    lookx = From.x + goright.length + findR(From.x + goright.length, looky, false).length;
                    //if (lookx == 123 && looky == 46) 
                    //    throw new Exception();
                } else {
                    //lookx = From.x + rows[looky].left_run(!value, From.x);
                    lookx = From.x + findR(From.x, looky, false).length;
                }
                    
                while (lookx < From.x + From.length) {
                        
                    line right = findR(lookx, looky, true);
                    if (!r.rows[looky][lookx]) {
                        A.Add(right);
                        if (!is_ok(search(right.x, looky)))
                            throw new Exception();
                        r.rows[looky].SetRange(true, right.x, right.x + right.length - 1);
                        if (right.x + right.length > From.x + From.length + 1) // E overhang
                            B.Add(new line(From.x + From.length + 1, looky, (right.x + right.length) - (From.x + From.length) - 1));
                    }
                    if (right.x + right.length > From.x + From.length - 2)
                        return;
                    lookx = lookx + right.length + findR(lookx + right.length, looky, false).length;
                }
            };

            while (todoN.Count > 0 || todoS.Count > 0) {
                while (todoN.Count > 0) {
                    line N = popN();
                    dolook(N, N.y - 1, todoN, todoS);
                }
                while (todoS.Count > 0) {
                    line S = popS();
                    dolook(S, S.y + 1, todoS, todoN);
                }
            }

            return r;
        }

        public SquareBitArray FloodFind(Point at, bool value, out Rectangle area) {

            SquareBitArray r = new SquareBitArray(Width, Height);

            area = new Rectangle(at.X, at.Y, 1, 1);

            if (this[at] != value) return r;
            
            List<line> todoN = new List<line>();
            List<line> todoS = new List<line>();

            Func<List<line>, line> pop = delegate(List<line> list) {
                line rline = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return rline;
            };
            Func<line> popN = delegate { return pop(todoN); };
            Func<line> popS = delegate { return pop(todoS); };

            Func<int, int, bool, line> findR = delegate(int x, int y, bool find) {
                int right = rows[y].left_run(find, x);
                return new line(x, y, right);
            };

            int start_left  = at.X - rows[at.Y].right_run(value, at.X);
            int start_right = at.X + rows[at.Y] .left_run(value, at.X);
            line start = new line(start_left + 1, at.Y, start_right - start_left - 1);
            r.rows[at.Y].SetRange(true, start.x, start.x + start.length - 1);
            todoS.Add(start);
            todoN.Add(start);

            Action<line, int, List<line>, List<line>> dolook = delegate(line From, int looky, List<line> A, List<line> B) {
                int lookx;
                if (rows[looky][From.x] == value) {
                    int goleft = rows[looky].right_run(value, From.x);
                    int goright = rows[looky].left_run(value, From.x);
                    if (!r.rows[looky][From.x]) {
                        
                        r.rows[looky].SetRange(true, From.x - goleft + 1, From.x + goright - 1);
                        A.Add(new line(From.x - goleft + 1, looky, goright + goleft - 1));
                    }
                    if (goright > From.length + 1) // E overhang
                        B.Add(new line(From.x + From.length + 1, looky, goright - 2));
                    if (goright >= From.length - 1)
                        return;
                    lookx = From.x + goright + rows[looky].left_run(!value, From.x);
                } else {
                    lookx = From.x + rows[looky].left_run(!value, From.x);
                }
                    
                while (lookx < From.x + From.length) {
                        
                    line right = findR(lookx, looky, value);
                    if (!r.rows[looky][lookx]) {
                        A.Add(right);
                        r.rows[looky].SetRange(true, right.x, right.x + right.length - 1);
                        if (right.x + right.length > From.x + From.length + 1) // E overhang
                            B.Add(new line(From.x + From.length + 1, looky, (right.x + right.length) - (From.x + From.length) - 1));
                    }
                    if (right.x + right.length > From.x + From.length - 2)
                        return;
                    lookx = lookx + right.length + findR(lookx + right.length, looky, !value).length;
                }
            };

            while (todoN.Count > 0 || todoS.Count > 0) {
                while (todoN.Count > 0) {
                    line N = popN();
                    dolook(N, N.y - 1, todoN, todoS);
                }
                while (todoS.Count > 0) {
                    line S = popS();
                    dolook(S, S.y + 1, todoS, todoN);
                }
                //while (todoN.Count > 0) {
                //    line N = popN();
                //    int lookx;
                //    int looky = N.y - 1;

                //    if (rows[looky][N.x] == value) {
                //        int goleft = rows[looky].right_run(value, N.x);
                //        int goright = rows[looky].left_run(value, N.x);
                //        if (!r.rows[looky][N.x]) {
                //            r.rows[looky].SetRange(true, N.x - goleft + 1, N.x + goright - 1);
                //            todoN.Add(new line(N.x - goleft + 1, looky, goright + goleft - 1));
                //        }
                //        if (goright > N.length + 1) // ES overhang
                //            todoS.Add(new line(N.x + N.length + 1, looky, goright - 2));
                //        if (goright >= N.length - 1)
                //            goto EXIT_N;
                //        lookx = N.x + goright + rows[looky].left_run(!value, N.x);
                //    } else {
                //        lookx = N.x + rows[looky].left_run(!value, N.x);
                //    }
                    
                //    while (lookx < N.x + N.length) {
                        
                //        line right = findR(lookx, looky, value);
                //        if (!r.rows[looky][lookx]) {
                //            todoN.Add(right);
                //            r.rows[looky].SetRange(true, right.x, right.x + right.length - 1);
                //            if (right.x + right.length > N.x + N.length + 1) // ES overhang
                //                todoS.Add(new line(N.x + N.length + 1, looky, (right.x + right.length) - (N.x + N.length) - 1));
                //        }
                //        if (right.x + right.length > N.x + N.length - 2)
                //            goto EXIT_N;
                //        lookx = lookx + right.length + findR(lookx + right.length, looky, !value).length;
                //    }
                //    EXIT_N:;
                //}

                //while (todoS.Count > 0) {
                //    line S = popS();
                //    int lookX;
                //    int lookY = S.y + 1;

                //    if (rows[lookY][S.x] == value) {
                //        int goleft = rows[lookY].right_run(value, S.x);
                //        int goright = rows[lookY].left_run(value, S.x);
                //        if (!r.rows[lookY][S.x]) {
                //            r.rows[lookY].SetRange(true, S.x - goleft + 1, S.x + goright - 1);
                //            todoS.Add(new line(S.x - goleft + 1, lookY, goright + goleft - 1));
                //        }
                //        if (goright > S.length + 1) // EN overhang
                //            todoN.Add(new line(S.x + S.length + 1, lookY, goright - 2));
                //        if (goright >= S.length - 1)
                //            goto EXIT_S;
                //        lookX = S.x + goright + rows[lookY].left_run(!value, S.x);
                //    } else {
                //        lookX = S.x + rows[lookY].left_run(!value, S.x);
                //    }
                    
                //    while (lookX < S.x + S.length) {
                        
                //        line right = findR(lookX, lookY, value);
                //        if (!r.rows[lookY][lookX]) {
                //            todoS.Add(right);
                //            r.rows[lookY].SetRange(true, right.x, right.x + right.length - 1);
                //            if (right.x + right.length > S.x + S.length + 1) // ES overhang
                //                todoN.Add(new line(S.x + S.length + 1, lookY, (right.x + right.length) - (S.x + S.length) - 1));
                //        }
                //        if (right.x + right.length > S.x + S.length - 2)
                //            goto EXIT_S;
                //        lookX = lookX + right.length + findR(lookX + right.length, lookY, !value).length;
                //    }
                //    EXIT_S:;
                //}
            }

            return r;
        }

        //public bool EqualsAt(SquareBitArray subarray, int x, int y) {
        //    for (int dy = 0; dy < subarray.rows.Length; dy++)
        //        if (!this.rows[y + dy].EqualsAt(subarray.rows[y], x)) 
        //            return false;
        //    return true;
        //}
        
        public Point Search(SquareBitArray find) {  return Search(find, Point.Empty); }
        public Point Search(SquareBitArray find, Point after) {


            int maxy = rows.Length - find.rows.Length;
            for (int y = after.Y; y <= maxy; y++) {
                int x = (y == after.Y) 
                    ? rows[y].Search(find.rows[0], after.X + 1, rows[y].Length) 
                    : rows[y].Search(find.rows[0]);
                while (x >= 0) {
                    for (int y2 = 1; y2 < find.rows.Length; y2++) {
                        if (!rows[y + y2].subarray(x, find.rows[y2].Length).Equals(find.rows[y2])) {
                            goto NOT_EQUAL;
                        }
                    }

                    return new Point(x, y);

                NOT_EQUAL:
                    x = rows[y].Search(find.rows[0], x + find.rows[0].Length, rows[y].Length - 1);
                }
            }

            return NullPoint;
        }

        public IEnumerable<Point> ForEach(bool value) {
            Point r = Point.Empty;
            for (int y = 0; y < Height; y++) {
                r.Y = y;
                int x = 0;
                while (x < Width) {
                    int runV = rows[y].left_run(value, x);
                    for (int offx = 0; offx < runV; offx++) {
                        r.X = x + offx;
                        yield return r;
                    }
                    x += runV;
                    if (x >= Width)
                        break;
                    x += rows[y].left_run(!value, x);
                }
            }
        }

        public Point[] IndexOfAll(SquareBitArray find) {
            List<Point> r = new List<Point>();
            Point p = Search(find);

            while (p != SquareBitArray.NullPoint) {
                r.Add(p);
                p = Search(find, p);
            }
            return r.ToArray();
        }
        
        public Point IndexOf(bool value) {
            for (int y = 0; y < rows.Length; y++) {
                int x = rows[y].IndexOf(value);
                if (x >= 0) 
                    return new Point(x, y);
            }
            return NullPoint;
        }
        
        public Point IndexOf(bool value, Point after) {
            int x = rows[after.Y].IndexOf(value, after.X + 1, rows[after.Y].Length - 1);
            if (x >= 0) 
                return new Point(x, after.Y);
            for (int y = after.Y + 1; y < rows.Length; y++) {
                x = rows[y].IndexOf(value);
                if (x >= 0) 
                    return new Point(x, y);
            }
            return NullPoint;
        }
        //public int IndexOf(bool value, int mini, int maxi) { int r = value ? left_0s(mini, maxi) : left_1s(mini, maxi); return r == maxi - mini + 1 ? -1 : r + mini; }
    }
}
