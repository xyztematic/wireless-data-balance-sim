using UnityEngine;
using UnityEngine.Assertions;
using m4ri;

public class Test_m4ri : MonoBehaviour
{
    void Start() {
        Test();
        Debug.Log("Testing of m4ri wrapper complete!");
    }
    public void Test() {
        MatrixGF2 a = new MatrixGF2(1, 50);
        a.Randomize();
        string a_str = a.ToString();
        Assert.AreEqual(a_str.Length, 50+1);

        MatrixGF2 a_copy = a.Copy();
        AssertFullEqual(a, a_copy);

        MatrixGF2 b = a.Copy();
        b[0, 0] = (~a[0, 0]) & 1; // assigns 0/1 at b[0,0] if a[0,0] is 1/0
        AssertFullInequal(a, b);

        // Simple multiplication
        MatrixGF2 c = new MatrixGF2(2, 3);
        c[0,0]=1; c[0,1]=0; c[0,2]=1;
        c[1,0]=1; c[1,1]=1; c[1,2]=0;
        MatrixGF2 d = new MatrixGF2(3, 2);
        d[0,0]=1; d[0,1]=1;
        d[1,0]=0; d[1,1]=0;
        d[2,0]=1; d[2,1]=0;
        MatrixGF2 e = new MatrixGF2(2, 2);
        e[0,0]=0; e[0,1]=1;
        e[1,0]=1; e[1,1]=1;
        AssertFullEqual(c*d, e);

        // Simple addition
        AssertFullEqual(c+c+c, c);
        Assert.IsTrue((c+c).IsZero());
    }

    public void AssertFullEqual(MatrixGF2 M1, MatrixGF2 M2) {

        Assert.IsTrue(M1.IsEqualTo(M2));
        Assert.IsTrue(M2.IsEqualTo(M1));
        Assert.IsTrue(MatrixGF2.AreEqual(M1, M2));
        Assert.IsTrue(MatrixGF2.AreEqual(M2, M1));
    }
    public void AssertFullInequal(MatrixGF2 M1, MatrixGF2 M2) {

        Assert.IsFalse(M1.IsEqualTo(M2));
        Assert.IsFalse(M2.IsEqualTo(M1));
        Assert.IsFalse(MatrixGF2.AreEqual(M1, M2));
        Assert.IsFalse(MatrixGF2.AreEqual(M2, M1));
    }
}
