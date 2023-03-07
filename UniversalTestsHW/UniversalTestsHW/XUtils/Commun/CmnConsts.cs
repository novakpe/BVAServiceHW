// .NER
using System;

namespace XUtils.Commun
{
    internal class CmnConsts
    {
        // 'Status' spojeni
        public enum EnStatus { None, Connect, Start, Exception, DisConnect, End }

        // udalost o zmene stavu komunikace
        public delegate void DelStatusChanged(EnStatus status, string prms = null);
        // udalost o prijmu dat
        public delegate void DelReceiveData(string data);
    }
}
