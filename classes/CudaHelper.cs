using ManagedCuda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedCuda.NPP.NPPsExtensions;
namespace nn.classes
{
    public static class CudaHelper
    {
        private static CudaContext context;

        static CudaHelper()
        {
            context = new CudaContext();            
        }

        public static double[] Subtract(double[] a, double[] b)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaB = b;
            CudaDeviceVariable<double> cudaResult = new CudaDeviceVariable<double>(a.Length);
            cudaA.Sub(cudaB, cudaResult);
            double[] result = cudaResult;
            cudaA.Dispose();
            cudaB.Dispose();
            cudaResult.Dispose();
            return result;
        }

        public static double[] Sqr(double[] a)
        {
            CudaDeviceVariable<double> cudaA = a;            
            CudaDeviceVariable<double> cudaResult = new CudaDeviceVariable<double>(a.Length);
            cudaA.Sqr(cudaResult);
            double[] result = cudaResult;
            cudaA.Dispose();            
            cudaResult.Dispose();
            return result;
        }

        public static void SqrInPlace(ref double[] a)
        {
            CudaDeviceVariable<double> cudaA = a;            
            cudaA.Sqr();
            a = cudaA;
            cudaA.Dispose();            
        }

        public static void SubtractInPlace(ref double[] a, double[] b)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaB = b;            
            cudaA.Sub(cudaB);
            a = cudaA;
            cudaA.Dispose();
            cudaB.Dispose();
        }

        public static double[] Subtract(double a, double[] b)
        {
            CudaDeviceVariable<double> cudaA = new CudaDeviceVariable<double>(b.Length);
            cudaA.Set(a);
            CudaDeviceVariable<double> cudaB = b;            
            cudaA.Sub(cudaB);
            double[] result = cudaA;
            cudaA.Dispose();
            cudaB.Dispose();
            return result;
        }

        public static void AddInPlace(ref double[] a, double[] b)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaB = b;            
            cudaA.Add(cudaB);
            a = cudaA;
            cudaA.Dispose();
            cudaB.Dispose();            
        }

        public static double[] Add(double[] a, double[] b)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaB = b;
            CudaDeviceVariable<double> cudaResult = new CudaDeviceVariable<double>(a.Length);
            cudaA.Add(cudaB, cudaResult);
            double[] result = cudaResult;
            cudaA.Dispose();
            cudaB.Dispose();
            cudaResult.Dispose();
            return result;
        }

        public static double[] Multiply(double[] a, double[] b)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaB = b;
            CudaDeviceVariable<double> cudaResult = new CudaDeviceVariable<double>(a.Length);
            cudaA.Mul(cudaB, cudaResult);
            double[] result = cudaResult;
            cudaA.Dispose();
            cudaB.Dispose();
            cudaResult.Dispose();
            return result;
        }

        public static void MultiplyInPlace(ref double[] a, double[] b)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaB = b;            
            cudaA.Mul(cudaB);
            a = cudaA;                
            cudaA.Dispose();
            cudaB.Dispose();
        }

        public static void MultiplyInPlace(ref double[] a, double c)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaC = c;            
            cudaA.MulC(cudaC);
            a = cudaA;
            cudaA.Dispose();
            cudaC.Dispose();                        
        }

        public static double[] Multiply(double[] a, double c)
        {
            CudaDeviceVariable<double> cudaA = a;
            CudaDeviceVariable<double> cudaC = c;
            CudaDeviceVariable<double> cudaResult = new CudaDeviceVariable<double>(a.Length);
            cudaA.MulC(cudaResult, cudaC);
            double[] result = cudaResult;
            cudaA.Dispose();
            cudaC.Dispose();
            cudaResult.Dispose();
            return result;
        }
    }
}
