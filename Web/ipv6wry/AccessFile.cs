using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Web.ipv6wry
{
    internal class AccessFile
    {
        private string dbFile; //文件位置
        private Stream stream;
        //public string file;
        public int total; //数据总量
        //public int db4;
        // 索引区
        public int index_start_offset; //开始索引
        public int index_end_offset; //结束索引
        public int offlen; //地址长度（数据记录地址）
        public int iplen; //IP长度
        private string v;
        public AccessFile(string dbFile, string v)
        {
            this.dbFile = dbFile;
            this.v = v;
            this.stream = new FileStream(dbFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            this.index_start_offset = this.read4(16);
            this.total = this.read4(8);
            this.offlen = this.read1(6);
            this.iplen = this.read1(7);
            this.index_end_offset = this.index_start_offset + (this.iplen + this.offlen) * this.total;
        }

        /// <summary>
        /// 跳过的字节数
        /// </summary>
        /// <param name="v"></param>
        public void seek(long v)
        {
            stream.Seek(v, SeekOrigin.Begin);
        }

        internal void write(byte[] vs)
        {
            stream.Write(vs, 0, vs.Length);
        }

        internal void readFully(byte[] dbBinStr, int v, int length)
        {
            stream.Read(dbBinStr, v, length);
        }

        public long length()
        {
            return this.stream.Length;
        }

        internal void close()
        {
            if (stream == null)
            {
                return;
            }
            this.stream.Flush();
            this.stream.Close();
            this.stream = null;
        }

        /// <summary>
        /// 获取指针
        /// </summary>
        /// <returns></returns>
        public long getFilePointer()
        {
            return stream.Position;
        }
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="v"></param>
        internal void write(int v)
        {
            var bytes = BitConverter.GetBytes(v);
            stream.Write(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// 读数据：8字节
        /// </summary>
        /// <returns></returns>
        public long read8(int seek0, int size = 8)
        {
            if (seek0 > 0)
            {
                this.seek(seek0);
            }
            //var bytes = BitConverter.GetBytes(8);
            var bytes = new byte[8];
            stream.Read(bytes, 0, bytes.Length);
            long te = BitConverter.ToInt64(bytes, 0);
            //BigInteger gi = new BigInteger(bytes);
            return te;
        }

        /// <summary>
        /// 读数据：4字节
        /// </summary>
        /// <returns></returns>
        public int read4(int seek0, int size = 4)
        {
            if (seek0 > 0)
            {
                this.seek(seek0);
            }
            var bytes = BitConverter.GetBytes(size);
            stream.Read(bytes, 0, bytes.Length);
            int te = BitConverter.ToInt32(bytes, 0);
            return te;
        }

        /// <summary>
        /// 读数据：4字节
        /// </summary>
        /// <returns></returns>
        public int read3(int seek0, int size = 3)
        {
            if (seek0 > 0)
            {
                this.seek(seek0);
            }
            var bytes = new byte[3];
            stream.Read(bytes, 0, bytes.Length);
            return bytes[0] + bytes[1] * 256 + bytes[2] * 256 * 256;
        }

        /// <summary>
        /// 读数据：1字节
        /// </summary>
        /// <returns></returns>
        public int read1(int seek0)
        {
            if (seek0 > 0)
            {
                this.seek(seek0);
            }

            var bytes = new byte[2];
            stream.Read(bytes, 0, bytes.Length);
            int te = BitConverter.ToUInt16(bytes, 0);
            return te % 256;
        }

        /// <summary>
        /// 地址查找
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string[] read_record(int offset)
        {
            string[] record = new string[] { "", "" };
            int flag = this.read1(offset); //读取offset的1字节
            //if (flag < 0) flag += 256;
            if (flag == 1)
            {
                // 地址重定向
                int location_offset = this.read4(offset + 1, this.offlen);
                return this.read_record(location_offset);
            }
            else
            {
                record[0] = this.read_location(offset); //获取真实地址名称
                if (flag == 2)
                {
                    record[1] = this.read_location(offset + this.offlen + 1); //获取运营商
                }
                else
                {
                    record[1] = this.read_location(offset + (record[0]).ToString().Length + 1); //运营商跟在后面
                }
            }
            return record;
        }

        /// <summary>
        /// 读取地区
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string read_location(int offset)
        {
            if (offset == 0)
            {
                return "";
            }
            int flag = this.read1(offset);
            if (flag < 0) flag += 256;
            // 出错
            if (flag == 0)
            {
                return "";
            }
            // 仍然为重定向
            if (flag == 2)
            {
                //地址ID
                offset = this.read3(offset + 1, this.offlen);
                return this.read_location(offset);
            }
            string location = this.readstr(offset);
            return location;
        }

        // 读取文字
        public string readstr(int offset = -1)
        {
            if (offset > 0)
            {
                this.seek(offset);
            }
            var str = "";
            var chr = this.read1(offset);
            while (chr != 0)
            {
                if (chr < 0)
                {
                    chr += 256;
                }
                string hex = System.Convert.ToString(chr, 16);
                if (hex.Length == 1)
                {
                    hex = "0" + hex;
                }
                str += hex;
                offset++;
                chr = this.read1(offset);
            }
            UTF8Encoding utf8 = new UTF8Encoding();
            str = str.ToUpper();
            string str_aa = Encoding.GetEncoding("UTF-8").GetString(this.HexStringToByteArray(str));
            return str_aa;
        }

        public byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "").Trim().ToUpper();
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        // +-------------------------
        // | 以下是解析部分
        // +-------------------------

        /// <summary>
        /// 查询IP信息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public IPv6Object query(string strIp)
        {
            long aa = this.ip2long64(strIp);
            long ip_num2 = 0;
            int ip_find = this.find(aa, ip_num2, 0, this.total);


            int ip_offset = this.index_start_offset + ip_find * (this.iplen + this.offlen);
            int ip_offset2 = ip_offset + this.iplen + this.offlen;
            long ip0 = this.read8(ip_offset);
            string ip_start = this.long642ip(ip0);
            string ip_end = "";
            try
            {
                long ip1 = this.read8(ip_offset2);
                ip1--;
                ip_end = this.long642ip(ip1);
            }
            catch
            {

            }
            finally
            {

            }

            int ip_record_offset = this.read4(ip_offset + this.iplen, this.offlen);
            string[] ip_addr = this.read_record(ip_record_offset);
            string ip_addr_disp = ip_addr[0] + " " + ip_addr[1];
            IPv6Object i6 = new IPv6Object();

            i6.ip_find = ip_find;
            i6.ip_addr_disp = ip_addr;
            i6.ip_addr = ip_addr[0];
            i6.ip_isp = ip_addr[1];
            i6.ip_start = ip_start;
            i6.ip_end = ip_end;

            return i6;
        }
        /// <summary>
        /// 把IP地址转换为数字 int64
        /// </summary>
        /// <returns></returns>
        public long ip2long64(string strIp)
        {
            IPAddress address = IPAddress.Parse(strIp);
            byte[] ipbyte = address.GetAddressBytes().Reverse().ToArray();
            long tex = BitConverter.ToInt64(ipbyte, 8);
            string te0 = System.Convert.ToString(tex, 16);
            return tex;
        }

        /// <summary>
        /// 把int64转换为IP地址 
        /// </summary>
        /// <returns></returns>
        public string long642ip(long int64ip)
        {

            byte[] ipbyte = BitConverter.GetBytes(int64ip);
            ushort a0 = BitConverter.ToUInt16(ipbyte, 0);
            ushort a1 = BitConverter.ToUInt16(ipbyte, 2);
            ushort a2 = BitConverter.ToUInt16(ipbyte, 4);
            ushort a3 = BitConverter.ToUInt16(ipbyte, 6);
            string strIp = System.Convert.ToString(a3, 16) + ":" +
                            System.Convert.ToString(a2, 16) + ":" +
                            System.Convert.ToString(a1, 16) + ":" +
                            System.Convert.ToString(a0, 16) + "::";
            IPAddress address = IPAddress.Parse(strIp);

            //byte[] ipn64 = new byte[8];
            //byte[] ipn128 = new byte[16];
            //ipbyte.CopyTo(ipn128);
            //ipn64.CopyTo(ipn128);
            //IPAddress address = IPAddress.

            return address.ToString();
        }

        /// <summary>
        /// IPv6位置查找：二分法
        /// </summary>
        /// <param name="ip0">IPv6的前int64</param>
        /// <param name="ip1">IPv6的后int64</param>
        /// <param name="l">数据位置</param>
        /// <param name="r">数据总量</param>
        public int find(long ip_num1, long ip_num2, int l, int r)
        {
            if (l + 1 >= r)
            {
                return l;
            }
            int m = (l + r + 0) / 2;

            long m_ip1 = this.read8(this.index_start_offset + m * (this.iplen + this.offlen), this.iplen);
            long m_ip2 = 0;
            if (this.iplen <= 8)
            {
                m_ip1 <<= 8 * (8 - this.iplen); //左移运算符
            }
            else
            {
                m_ip2 = this.read8(this.index_start_offset + m * (this.iplen + this.offlen) + 8, this.iplen - 8);
                m_ip2 <<= 8 * (16 - this.iplen);
            }
            if (this.uint64cmp(ip_num1, m_ip1) < 0)
            {
                return this.find(ip_num1, ip_num2, l, m);
            }
            else if (this.uint64cmp(ip_num1, m_ip1) > 0)
            {
                return this.find(ip_num1, ip_num2, m, r);
            }
            else if (this.uint64cmp(ip_num2, m_ip2) < 0)
            {
                return this.find(ip_num1, ip_num2, l, m);
            }
            else
            {
                return this.find(ip_num1, ip_num2, m, r);
            }
        }
        /// <summary>
        /// 数据比较
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int uint64cmp(long a, long b)
        {
            if (a >= 0 && b >= 0 || a < 0 && b < 0)
            {
                if (a > b)
                {
                    return 1;
                }
                else if (a == b)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else if (a >= 0 && b < 0)
            {
                return -1;
            }
            else if (a < 0 && b >= 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
