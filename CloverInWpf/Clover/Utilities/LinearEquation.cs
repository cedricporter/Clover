using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover.Utilities
{
    class LinearEquation
    {
        double[] x;
       
        /// <summary>
        /// 初始化类
        /// </summary>
        /// <param name="unkownnum">未知数的个数</param>
        /// <param name="g">线性方程的增广矩阵</param>
        public void Initialize(int unkownnum, double[,] g)
        {
            x = new double[ unkownnum ];
        }

        /// <summary>
        /// 获得方程解
        /// </summary>
        /// <returns></returns>
        public double[] GetResult ()
        {
            GaussianJordanElimination( g );
            return x;
        }


                /// <summary>
        /// 高斯-约旦消元法；
        /// </summary>
        /// <param name="g"></param>
        void GaussianJordanElimination( double[ , ] g )
        {
            
            int m = g.GetLength( 0 );//获得扩展矩阵的行（方程个数）；
            int n = g.GetLength( 1 );//获得扩展矩阵的列（未知数个数+1）；

            //直接消元变形为对角矩阵；
            for ( int i = 1; i < m; i++ )
            {
                for ( int j = i; j < m; j++ )
                {
                    for ( int k = n - 1; k > i - 2; k-- )
                        g[ j, k ] = g[ j, k ] - ( g[ j, i - 1 ] / g[ i - 1, i - 1 ] ) * g[ i - 1, k ];
                }

                for ( int j = 0; j <i; j++ )
                {
                    for ( int k = n - 1; k > i - 1; k-- )
                        g[ j, k ] = g[ j, k ] - ( g[ j, i ] / g[ m - 1, i ] ) * g[ m - 1, k ];
                }
            }

            //取结果（这里是正序结果哦）；
            for ( int i = 0; i < m; i++ )
                x[ i ] = g[ i, n - 1 ] / g[ i, i ];
        }
    
    }
}
