using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stuff : MonoBehaviour
{
    Matrix4x4 coeffMat;
    Matrix4x4 L;
    Matrix4x4 U;

    // Start is called before the first frame update
    void Start()
    {
        coeffMat.SetRow(0, new Vector4(2.0f, -1.0f, -2.0f));
        coeffMat.SetRow(1, new Vector4(-4.0f, 6.0f, 3.0f)); 
        coeffMat.SetRow(2, new Vector4(-4.0f, -2.0f, 8.0f)); 
        coeffMat.SetRow(3, Vector4.zero); 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            decomposeLU(coeffMat, out L, out U);
        }
    }

    // Doolittle algorithm
    void decomposeLU(Matrix4x4 coeffMat, out Matrix4x4 L, out Matrix4x4 U)
    {
        const int N = 3;
        float sum = 0.0f;
        L = Matrix4x4.identity;
        L[3, 3] = 0.0f;
        U = Matrix4x4.zero;

        // Each row in L and A.
        for (int i = 0; i < N; ++i)
        {
            // Upper triangular matrix.
            // Each column in U and A.
            for (int k = i; k < N; ++k)
            {
                sum = 0.0f;

                // Each column in L and row in U.
                for (int j = 0; j < i; ++j)
                {
                    sum += L[i, j] * U[j, k];
                }

                // Evaluate kth row of U.
                U[i, k] = coeffMat[i, k] - sum;
            }

            // Lower triangular matrix.
            // Each row in L and A.
            for (int k = i; k < N; ++k)
            {
                // Lower triangle matrix has a diagonal of 1's.
                if (i == k)
                    L[i, k] = 1.0f;
                else
                {
                    sum = 0.0f;

                    // Each column in L and row in U.
                    for (int j = 0; j < i; ++j)
                    {
                        sum += L[k, j] * U[j, i];
                    }

                    // Evaluate kth column of L.
                    L[k, i] = (coeffMat[k, i] - sum) / U[i, i];
                }
            }
        }
    }
}

