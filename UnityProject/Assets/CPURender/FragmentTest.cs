using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPURender
{
    public struct FragmentPoint
    {
        public int x;
        public int y;
    }
    public class Screen
    {
        public const int width = 72; //720*1280;
        public const int heigth = 128;
    }
    /// <summary>
    /// 模拟模板测试的操作
    /// https://docs.unity3d.com/Manual/SL-Stencil.html
    /// </summary>
    public class StencilTest
    {
        public enum Comparison
        {
            Greater = 0,
            GEqual,
            Less,
            LEqual,
            Equal,
            NotEqual,
            Always,
            Never,
        }
        public enum StencilOp
        {
            Keep = 0,
            Zero,
            Replace,
            IncrSat,
            DecrSat,
            Invert,
            IncrWrap,
            DecrWrap,
        }
        private static byte[,] s_StencilBuff = new byte[Screen.width, Screen.heigth];

        public static void Clear()
        {
            for (int i = 0; i < s_StencilBuff.GetLength(0); ++i)
            {
                for (int j = 0; j < s_StencilBuff.GetLength(1); ++j)
                {
                    s_StencilBuff[i, j] = 0;
                }
            }
        }

        public static bool DoTest(FragmentPoint point, byte refValue, byte readMask, byte writeMask, Comparison comp, StencilOp passOp, StencilOp failOp, StencilOp zfailOp)
        {
            bool passed = CompFunc(point,  refValue, readMask, comp);

            if (passed)
                PassOpFunc(point, refValue, writeMask, passOp);
            else
                FailOpFunc(point, refValue, writeMask, passOp);

            return passed;
        }

        private static bool CompFunc(FragmentPoint point, byte refValue, byte readMask, Comparison comp)
        {
            refValue = (byte)(refValue & readMask);
            byte buffValue = (byte)(GetBuffValue(point) & readMask);
            switch(comp)
            {
                case Comparison.Greater:
                    return refValue > buffValue;
                case Comparison.GEqual:
                    return refValue >= buffValue;
                case Comparison.Less:
                    return refValue < buffValue;
                case Comparison.LEqual:
                    return refValue <= buffValue;
                case Comparison.Equal:
                    return refValue == buffValue;
                case Comparison.NotEqual:
                    return refValue != buffValue;
                case Comparison.Always:
                    return true;
                case Comparison.Never:
                    return false;
            }
            return false;
        }
        private static void PassOpFunc(FragmentPoint point, byte refValue, byte writeMask, StencilOp passOp)
        {
            WriteImp(point, refValue, writeMask, passOp);
        }
        private static void FailOpFunc(FragmentPoint point, byte refValue, byte writeMask, StencilOp failOp)
        {
            WriteImp(point, refValue, writeMask, failOp);
        }
        public static void ZFailOp(FragmentPoint point, byte refValue, byte writeMask, StencilOp zfailOp)
        {
            WriteImp(point, refValue, writeMask, zfailOp);
        }
        private static void WriteImp(FragmentPoint point, byte refValue, byte writeMask, StencilOp op)
        {
            if (writeMask == 0)
                return;
            WriteByOp(point, (byte)(refValue & writeMask), op);
        }
        private static void WriteByOp(FragmentPoint point, byte v, StencilOp op)
        {
            switch(op)
            {
                case StencilOp.Keep:
                    break;
                case StencilOp.Zero:
                    SetBuffValue(point, 0);
                    break;
                case StencilOp.Replace:
                    SetBuffValue(point, v);
                    break;
                case StencilOp.IncrSat:
                    {
                        var curV = GetBuffValue(point);
                        if (curV < 255)
                            SetBuffValue(point, curV++);
                    }
                    break;
                case StencilOp.DecrSat:
                    {
                        var curV = GetBuffValue(point);
                        if (curV > 0)
                            SetBuffValue(point, curV--);
                    }
                    break;
                case StencilOp.Invert:
                    {
                        var curV = GetBuffValue(point);
                        SetBuffValue(point, (byte)~curV);
                    }
                    break;
                case StencilOp.IncrWrap:
                    {
                        var curV = GetBuffValue(point);
                        if (curV == 255)
                            curV = 0;
                        else
                            curV++;
                        SetBuffValue(point, curV);
                    }
                    break;
                case StencilOp.DecrWrap:
                    {
                        var curV = GetBuffValue(point);
                        if (curV == 0)
                            curV = 255;
                        else
                            curV--;
                        SetBuffValue(point, curV);
                    }
                    break;
            }
        }

        private static byte GetBuffValue(FragmentPoint point)
        {
            return s_StencilBuff[point.x, point.y];
        }
        private static void SetBuffValue(FragmentPoint point, byte v)
        {
            s_StencilBuff[point.x, point.y] = v;
        }
    }

    /// <summary>
    /// https://docs.unity3d.com/Manual/SL-CullAndDepth.html
    /// TODO: Offset Factor, Units
    /// </summary>
    public class ZTest
    {
        public enum Comparison
        {
            Less = 0,
            Greater,
            LEqual,
            GEqual,
            Equal,
            NotEqual,
            Always,
        }
        private static float[,] s_ZBuff = new float[Screen.width, Screen.heigth];
        public static bool DoTest(bool zwrite, FragmentPoint point, float z,  Comparison comp, StencilTest.StencilOp zfailOp, byte refValue, byte writeMask)
        {
            bool passed = CompFunc(point, z, comp);
            if (passed)
            {
                if (zwrite)
                    SetBuffValue(point, z);
            }
            else
            {
                StencilTest.ZFailOp(point, refValue, writeMask, zfailOp);
            }
            return true;
        }

        private static bool CompFunc(FragmentPoint point, float z, Comparison comp)
        {
            var buffValue = GetBuffValue(point);
            switch (comp)
            {
                case Comparison.Less:
                    return z < buffValue;
                case Comparison.Greater:
                    return z > buffValue;
                case Comparison.LEqual:
                    return z <= buffValue;
                case Comparison.GEqual:
                    return z >= buffValue;
                case Comparison.Equal:
                    return z == buffValue;
                case Comparison.NotEqual:
                    return z != buffValue;
                case Comparison.Always:
                    return true;
            }
            return false;
        }

        private static float GetBuffValue(FragmentPoint point)
        {
            return s_ZBuff[point.x, point.y];
        }
        private static void SetBuffValue(FragmentPoint point, float v)
        {
            s_ZBuff[point.x, point.y] = v;
        }
    }
}