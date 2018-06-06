using System;
using System.Collections.Generic;
using System.Text;

namespace XQData
{
    public class Factory<T>
    {
        /// <summary>
        /// 数据操作实例
        /// </summary>
		public static ICommon<T> Instance
		{
			get
			{
				return new SQLData<T>();
			}
		}
    }    
   
}

