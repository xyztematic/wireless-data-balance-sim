using System;
using System.Text;
using static Interface_m4ri;

namespace m4ri {
    public class MatrixGF2
    {
        public IntPtr headPtr;
        public int nrows, ncols;

        public MatrixGF2(int rows, int cols) {
            headPtr = mzd_init(rows, cols);
            nrows = rows;
            ncols = cols;
        }
        public MatrixGF2(int rows, int cols, IntPtr ptrToInitializedMatrix) {
            headPtr = ptrToInitializedMatrix;
            nrows = rows;
            ncols = cols;
        }
        ~MatrixGF2() {
            mzd_free(headPtr);
        }

        public int this[int row, int col] {
            
            get => read_bit(headPtr, row, col);
            set => write_bit(headPtr, row, col, value);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Capacity = nrows * (ncols + 1);
            for (int row = 0; row < nrows; row++) {
                for (int col = 0; col < ncols; col++) {
                    sb.Append(read_bit(headPtr, row, col));
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        public void Randomize() {
            mzd_randomize(headPtr);
        }
        public MatrixGF2 Copy() {
            return new MatrixGF2(nrows, ncols, mzd_copy(IntPtr.Zero, headPtr));
        }
        public bool IsEqualTo(MatrixGF2 toCompare) {
            return mzd_equal(headPtr, toCompare.headPtr) == 1;
        }
        public static bool AreEqual(MatrixGF2 M1, MatrixGF2 M2) {
            return mzd_equal(M1.headPtr, M2.headPtr) == 1;
        }
    }
}
