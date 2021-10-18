using ipdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Web.ipv6wry
{
    internal class IPv6Object
    {
        //索引ID
        public int ip_find { get; set; }

        //偏移量byte
        public int ip_offset { get; set; }

        //地址记录位置
        public int ip_record_offset { get; set; }

        public string ip_start { get; set; }

        public string ip_end { get; set; }

        public string ip_addr { get; set; }

        public string[] ip_addr_disp { get; set; }

        public string ip_isp { get; set; }

        public int ip_isp_id { get; set; }


        public static implicit operator CityInfo(IPv6Object info)
        {
            return new CityInfo(info.ip_addr_disp);
        }
    }
}
