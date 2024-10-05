using System;
using System.Text;
using UnityEngine;
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
        // Randomizes all entries of the matrix
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
        public static bool IsZero(MatrixGF2 toCheck) {
            return mzd_is_zero(toCheck.headPtr) == 1;
        }
        // Computes the reduced row echelon form of the matrix "in place" and returns the rank of the matrix
        public int Rref() {
            return mzd_echelonize_m4ri(headPtr, 1);
        }
        // Computes the NON-REDUCED row echelon form (or upper triangular form) of the matrix "in place" and returns the rank of the matrix
        public int Ref() {
            return mzd_echelonize_m4ri(headPtr, 0);
        }
    }
}
