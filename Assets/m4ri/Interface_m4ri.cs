using System;
using System.Runtime.InteropServices;

public class Interface_m4ri
{
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_init(int rows, int cols);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern void mzd_free(IntPtr M);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern void mzd_randomize(IntPtr M);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern int read_bit(IntPtr M, int row, int col);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern int write_bit(IntPtr M, int row, int col, int bit);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_copy(IntPtr dest, IntPtr M);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern int mzd_equal(IntPtr M1, IntPtr M2);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_add(IntPtr SUM, IntPtr M1, IntPtr M2);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_mul_m4rm(IntPtr PROD, IntPtr M1, IntPtr M2, int k = 0);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_inv_m4ri(IntPtr dest, IntPtr src, int k = 0);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern int mzd_is_zero(IntPtr M);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern int mzd_first_zero_row(IntPtr M);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern int mzd_gauss_delayed(IntPtr M, int start_col, int full = 0);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_set_ui(IntPtr M, uint zero_or_one);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern int mzd_echelonize_m4ri(IntPtr M, int full_if_one, int k = 0);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_submatrix(IntPtr S, IntPtr M, int start_row, int start_col, int end_row, int end_col);
    [DllImport("libm4ri-0.0.20200125.so")]
    public static extern IntPtr mzd_stack(IntPtr dest, IntPtr M1, IntPtr M2);

}