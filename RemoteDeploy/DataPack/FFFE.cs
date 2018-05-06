using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.DataPack
{
    enum FFFEState
    {
        WAIT_FF_C1,
        WAIT_FF_C2,
        WAIT_NO_FF,
        WAIT_FF_POS,
        ABNORMAL_STATE
    }

    /// <summary>
    /// FFFE协议
    /// </summary>
    public static class FFFE
    {
        /// <summary>
        /// FFFE数据组包（添加帧头FFFE  帧尾FFFD）
        /// </summary>
        /// <param name="Data">数据指针</param>
        /// <returns>添加了FFFE后的数据包</returns>
        public static byte[] FFFEPack(byte[] Data) 
        {
            //初始化返回变量
            byte[] resultData=new byte[0];

            try
            {

            //FFFE转译
            //resultData = TransFFFE(Data, Data.Length);
            //重新获取转换后的长度 @ 4.23
            //int newLength = resultData.Length;
            //resultData[1] = BitConverter.GetBytes(newLength)[3];
            //resultData[0] = BitConverter.GetBytes(newLength)[2];

            //帧头
            byte[] frameHead = new byte[2];
            frameHead[0] = 0xFF;
            frameHead[1] = 0xFE;

            //帧尾
            byte[] frameBottom = new byte[2];
            frameBottom[0] = 0xFF;
            frameBottom[1] = 0xFD;

            //添加FFFE帧头数据
            resultData = frameHead.Concat(resultData).ToArray() ;

            //添加FFFE帧尾数据
            resultData = resultData.Concat(frameBottom).ToArray();

            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogProcError("FFFE", "FFFEAnalysis", ex.Message);
                resultData= new byte[0];
            }

            return resultData;

        }

        /// <summary>
        /// FFFE数据解包（去除帧头FFFE  帧尾FFFD）
        /// </summary>
        /// <param name="Data">数据（含FFFE转义）</param>
        /// <returns>去除了FFFE后的数据包长度，0为处理失败</returns>
        public static Int32 FFFEAnalysis(ref byte[] Data)
        {
            //处理结果回执
            Int32 excuteResult = 0;

            try
            {
                //判定帧头FFFE  帧尾FFFD
                if (Data[0] == 0xFF && Data[1] == 0xFE
                    && Data[Data.Length - 1] == 0xFD && Data[Data.Length - 2] == 0xFF)
                {
                    //取数据（不包含FFFE  FFFD）
                    Data = Data.Skip(2).Take(Data.Length - 4).ToArray();
                    Data = TranslateFromFFFE(Data, Data.Length);
                    //重新获取转译后的长度 @ 4.23
                    //excuteResult = Data.Length;
                    //Data[1] = BitConverter.GetBytes(excuteResult)[3];
                    //Data[0] = BitConverter.GetBytes(excuteResult)[2];

                }
                else
                {
                    excuteResult = 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogProcError("FFFE", "FFFEAnalysis", ex.Message);
                excuteResult = 0;
            }

            return excuteResult;

        }

        public static byte[] TransFFFE(byte[] src,Int32 lenth)
        {
            int SrcPos = 0;
            int TgtPos = 0;
            int Pos1 = 0;/*前一个FF的位置*/
            int Pos2 = 0;/*后一个FF的位置*/
            int iii = 0;/*临时计数变量*/
            int Gap = 0;/*两个FF之间的字节数*/
            int Got1stFF = 0;/*1-获得前一个FF；0-未获得前一个FF*/
            int Got2ndFF = 0;/*1-获得后一个FF；0-未获得后一个FF*/
            byte[] tar = new byte[1024];
            for (SrcPos = 0; SrcPos < lenth; SrcPos++)
            {
                if (1 == Got1stFF)
                {
                    if (0xFF == src[SrcPos])
                    {
                        /*已遇到前一个FF，且当前又遇到FF*/
                        Got2ndFF = 1;
                        Pos2 = SrcPos;
                        tar[TgtPos] = Convert.ToByte(Pos2 - Pos1);
                        TgtPos++;

                        for (iii = Pos1 + 1; iii < Pos2; iii++)
                        {
                            tar[TgtPos] = src[iii];
                            TgtPos++;
                        }

                        Got1stFF = 0;
                        Got2ndFF = 0;
                        Pos1 = 0;
                        Pos2 = 0;
                        Gap = 0;
                    }
                    else
                    {
                        /*已遇到前一个FF，且当前遇到非FF*/
                        Gap++;
                        if (252 == Gap)
                        {
                            Got1stFF = 0;
                            Gap = 0;
                            tar[TgtPos] = 0;
                            TgtPos++;
                            for (iii = Pos1 + 1; iii <= SrcPos; iii++)
                            {
                                tar[TgtPos] = src[iii];
                                TgtPos++;
                            }
                        }
                        else
                        {
                        }
                    }
                }
                else
                {
                    /*尚未遇到前一个FF*/
                    tar[TgtPos] = src[SrcPos];
                    TgtPos++;
                    if (0xFF == src[SrcPos])
                    {
                        /*遇到前一个FF*/
                        Got1stFF = 1;
                        Pos1 = SrcPos;
                        Gap = 0;
                    }
                    else
                    {
                    }
                }
            }
            /*已经遇到了前一个FF, 且源数据已到了末尾仍未遇到后一个FF*/
            if (1 == Got1stFF)
            {
                tar[TgtPos] = 0;
                TgtPos++;

                for (iii = Pos1 + 1; iii < lenth; iii++)
                {
                    tar[TgtPos] = src[iii];
                    TgtPos++;
                }
            }
            else
            {
            }
            tar = tar.Take(TgtPos).ToArray();
            return tar;
        }

        public static byte[] TranslateFromFFFE(byte[] pSrc,int SrcLen)
        {
            byte[] pTgt = new byte[1024];
            int SrcPos = 0;
            int TgtPos = 0;
            int NonFFCount = 0;/*进入WAIT_NO_FF状态的次数*/
            int NextFFPos = 0;/*下一个FF字符的位置*/
            byte Char = 0;/*当前字符值*/
            FFFEState State = FFFEState.WAIT_FF_C1;/*状态*/

            for(SrcPos=0;SrcPos<SrcLen;SrcPos++)
            {
                Char=pSrc[SrcPos];
                switch(State)
                {
                case FFFEState.WAIT_FF_C1:
                    /*等待FF序列第一FF字符*/

                    if(0xFF == Char)  
                    {
                        /*收到FF字符*/
                        pTgt[TgtPos]=Char;
                        TgtPos++;
                        State = FFFEState.WAIT_FF_C2;
                    }
                    else
                    {
                        /*当前非FF字符*/
                        pTgt[TgtPos]=Char;
                        TgtPos++;
                    }

                    break;
                case FFFEState.WAIT_FF_C2:
                    /*等待FF序列第二FF字符*/

                    if( (0xFD==Char) || (0xFE==Char) || (0xFF==Char) )
                    {
                        State = FFFEState.ABNORMAL_STATE;
                    }
                    else
                    {
                        if(0 == Char)
                        {
                            State = FFFEState.WAIT_NO_FF;
                        }
                        else
                        {
                            if(1 == Char)
                            {
                                pTgt[TgtPos]=0xff;
                                TgtPos++;
                                State = FFFEState.WAIT_FF_C1;
                            }
                            else
                            {
                                NextFFPos=SrcPos+Char-1;
                                State = FFFEState.WAIT_FF_POS;
                            }
                        }
                    }
                    break;
                case FFFEState.WAIT_NO_FF:
                    /*消息中不再有FF字符需要处理，*/

                    NonFFCount++;
                    if( (0xFF==Char) && (NonFFCount<=252) )
                    {
                        /*如果在252个字符之内存在FF，解码失败*/
                        State = FFFEState.ABNORMAL_STATE;
                    }
                    else
                    {
                        /*接受非FF字符*/
                        pTgt[TgtPos]=Char;
                        TgtPos++;

                        if(252 == NonFFCount)
                        {
                            NonFFCount=0;
                            State = FFFEState.WAIT_FF_C1;
                        }
                        else
                        {
                        }
                    }
                    break;
                case FFFEState.WAIT_FF_POS:
                    /*等待插入FF字符的位置*/

                    if(0xFF == Char)
                    {
                        /*当前字符是FF字符*/

                        State = FFFEState.ABNORMAL_STATE;
                    }
                    else
                    {
                        /*当前字符非FF字符*/

                        pTgt[TgtPos]=Char;
                        TgtPos++;
                        if(SrcPos == NextFFPos)
                        {
                            pTgt[TgtPos]=0xff;
                            TgtPos++;
                            State = FFFEState.WAIT_FF_C1;
                        }
                        else
                        {
                        }
                    }
                    break;
                default:
                    State = FFFEState.ABNORMAL_STATE;
                    break;
                }
                if(State== FFFEState.ABNORMAL_STATE)
                {
                    /*非法状态*/
                    TgtPos=0;
                    break;
                }
                else
                {
                }
            }

            /*退出时的状态判断*/
            if((State == FFFEState.WAIT_FF_C2) ||(State == FFFEState.WAIT_FF_POS))
            {
                TgtPos = 0;
            }
            else
            {

            }
            return pTgt.Take(TgtPos).ToArray();
        }
    }
}
