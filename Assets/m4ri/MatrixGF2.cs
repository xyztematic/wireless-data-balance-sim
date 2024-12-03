using System;
using System.Collections;
using System.Text;
using UnityEngine;
using static Interface_m4ri;

namespace m4ri {
    public class MatrixGF2
    {
        public IntPtr headPtr;
        public int nrows, ncols;
        public static int PRINT_LIMIT = 100;

        public MatrixGF2(int rows, int cols) {
            headPtr = mzd_init(rows, cols);
            nrows = rows;
            ncols = cols;
        }
        private MatrixGF2(int rows, int cols, IntPtr ptrToInitializedMatrix) {
            headPtr = ptrToInitializedMatrix;
            nrows = rows;
            ncols = cols;
        }
        ~MatrixGF2() {
            mzd_free(headPtr);
        }

        // Overrides the [] operator on MatrixGF2 instances, allowing reading/writing of an entry with "mat[row, col]"
        public int this[int row, int col] {
            
            get => read_bit(headPtr, row, col);
            set => write_bit(headPtr, row, col, value);
        }
        // Overrides the + operator on MatrixGF2 instances, allowing adding two matrices with "mat1 + mat2"
        public static MatrixGF2 operator+(MatrixGF2 left, MatrixGF2 right) {
            if (left.nrows == right.nrows && left.ncols == right.ncols) {
                MatrixGF2 sum = new MatrixGF2(left.nrows, left.ncols);
                mzd_add(sum.headPtr, left.headPtr, right.headPtr);
                return sum;
            }
            Debug.LogError("Tried to add matrices with different dimensions");
            return null;
        }
        // Overrides the * operator on MatrixGF2 instances, allowing multiplying two matrices with "mat1 * mat2"
        public static MatrixGF2 operator*(MatrixGF2 left, MatrixGF2 right) {
            if (left.ncols == right.nrows) {
                MatrixGF2 prod = new MatrixGF2(left.nrows, right.ncols);
                mzd_mul_m4rm(prod.headPtr, left.headPtr, right.headPtr);
                return prod;
            }
            Debug.LogError("Tried to multiply matrices with mismatching dimensions");
            return null;
        }
        // Overrides the == operator on MatrixGF2 instances, allowing checking equality of two matrices
        public static bool operator==(MatrixGF2 left, MatrixGF2 right) {
            if (left.nrows != right.nrows || left.ncols != right.ncols) return false;
            else return left.IsEqualTo(right);
        }
        // Overrides the != operator on MatrixGF2 instances, allowing checking inequality of two matrices
        public static bool operator!=(MatrixGF2 left, MatrixGF2 right) {
            return !(left==right);
        }
        // Overrides ToString() for printing the whole matrix, if rows and cols are both at most PRINT_LIMIT
        public override string ToString() {
            if (nrows > PRINT_LIMIT || ncols > PRINT_LIMIT) return nrows+" x "+ncols+" Matrix";
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
        // Randomizes all entries of the matrix "in place"
        public void Randomize() {
            mzd_randomize(headPtr);
        }
        // Returns a "by value" copy of the matrix
        public MatrixGF2 Copy() {
            return new MatrixGF2(nrows, ncols, mzd_copy(IntPtr.Zero, headPtr));
        }
        // Checks for equality of two matrices
        public bool IsEqualTo(MatrixGF2 toCompare) {
            return mzd_equal(headPtr, toCompare.headPtr) == 1;
        }
        public static bool AreEqual(MatrixGF2 M1, MatrixGF2 M2) {
            return mzd_equal(M1.headPtr, M2.headPtr) == 1;
        }
        // Checks if the matrix is a zero matrix
        public bool IsZero() {
            return mzd_is_zero(headPtr) == 1;
        }
        // Computes the inverse matrix, if matrix has full rank
        public MatrixGF2 Inverse() {
            return new MatrixGF2(nrows, ncols, mzd_inv_m4ri(IntPtr.Zero, headPtr));
        }
        // Computes the reduced row echelon form of the matrix "in place" and returns the rank of the matrix
        public int Rref() {
            return mzd_echelonize_m4ri(headPtr, 1);
        }
        // Computes the NON-REDUCED row echelon form (or upper triangular form) of the matrix "in place" and returns the rank of the matrix
        public int Ref() {
            return mzd_echelonize_m4ri(headPtr, 0);
        }
        // Returns the identity matrix of the given size
        public static MatrixGF2 Identity(int size) {
            MatrixGF2 m = new MatrixGF2(size, size);
            mzd_set_ui(m.headPtr, 1);
            return m;
        }
        // Returns the submatrix from any row/column to (NOT INCLUDING) any other row/column
        public MatrixGF2 Submatrix(int rowStart, int colStart, int rowEnd, int colEnd) {
            if (rowStart >= rowEnd || colStart >= colEnd || rowStart < 0 || colStart < 0 || rowEnd > nrows || colEnd > ncols) {
                Debug.LogError("Parameter(s) out of bounds for submatrix creation");
                return null;
            }
            return new MatrixGF2(rowEnd - rowStart, colEnd - colStart, mzd_submatrix(IntPtr.Zero, headPtr, rowStart, colStart, rowEnd, colEnd));
        }
        // Stacks two matrices on top of each other and writes the result to a newly allocated matrix
        public MatrixGF2 StackOnto(MatrixGF2 toStackOnto) {
            if (this.ncols != toStackOnto.ncols) {
                Debug.LogError("Tried to stack matrices of different width (columns)");
                return null;
            }
            return new MatrixGF2(this.nrows, this.ncols, mzd_stack(IntPtr.Zero, headPtr, toStackOnto.headPtr));
        }
        // Returns the index of the first full zero row. If there is none, return amount of rows
        public int FirstZeroRow() {
            return mzd_first_zero_row(headPtr);
        }
        // Overwrites the specified row with the specified values. Ignores all values besides 0 and 1
        public void WriteRow(int rowIndex, int[] toWrite) {
            if (toWrite.Length < ncols || rowIndex >= nrows || rowIndex < 0) return;
            for (int i = 0; i < ncols; i++) {
                this[rowIndex, i] = toWrite[i];
            }
        }
        // Not important
        public override bool Equals(object obj) => obj.GetType().IsEquivalentTo(typeof(MatrixGF2)) && this == (MatrixGF2) obj;
        public override int GetHashCode() => base.GetHashCode();
    }
}
