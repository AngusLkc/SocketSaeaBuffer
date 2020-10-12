using System;

namespace SocketIocpBuffer
{
    /// <summary>
    /// C# Socket异步收发专用环形缓冲区,思想来自于Linux Kernel kfifo结构
    /// Put数据前需自行处理数据长度不能大于surplus
    /// Get数据返回的是byte[]的读索引和可读长度,可直接用于Socket异步发送
    /// Socket异步返回时调用Ack方法移动读指针
    /// </summary>
    public class SocketIocpBuffer
    {
        private byte[] buffer;
        private uint size;
        private uint head;
        private uint tail;

        /// <summary>
        /// 空闲长度
        /// </summary>
        public uint surplus
        {
            get
            {
                return size - head + tail;
            }
        }

        /// <summary>
        /// 数据长度
        /// </summary>
        public uint occupied
        {
            get
            {
                return head - tail;
            }
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        public SocketIocpBuffer(int bufferSize)
        {
            uint roundup_pow_of_two(int x)
            {
                int fls(int v)
                {
                    int p;
                    int i;
                    if (v != 0)
                        for (i = v >> 1, p = 0; i != 0; ++p)
                            i >>= 1;
                    else
                        p = -1;
                    return p + 1;
                }
                return 1U << fls(x - 1);
            }
            head = tail = 0;
            size = roundup_pow_of_two(bufferSize);
            buffer = new byte[size];
        }

        /// <summary>
        /// 数据入队
        /// </summary>
        public void PutSize(byte[] data, int dlen)
        {
            if (dlen > surplus)
                throw new Exception("数据太大,无法写入!");
            //写指针
            int index = (int)(head & (size - 1));
            //一次最大可写长度
            int len = (int)Math.Min(dlen, size - index);
            Buffer.BlockCopy(data, 0, buffer, index, len);
            //再次写入剩余数据
            if (dlen > len)
                Buffer.BlockCopy(data, len, buffer, 0, dlen - len);
            head += (uint)dlen;
        }

        /// <summary>
        /// 获取写索引和一次最大可写长度,SocketAsyncEventArgs适用
        /// </summary>
        public void Put(out int index,out int count)
        {
            index = (int)(head & (size - 1));
            count = (int)Math.Min(surplus, size - index);
        }

        /// <summary>
        /// 移动写指针,SocketAsyncEventArgs适用
        /// </summary>
        public void PutAck(int dlen)
        {
            if (dlen <= 0)
                throw new Exception("PutAck:无效长度");
            head += (uint)dlen;
        }

        /// <summary>
        /// 数据出队
        /// </summary>
        public byte[] GetSize(int dlen)
        {
            if (dlen > occupied)
                throw new Exception("数据不足,无法取出!");
            byte[] data = new byte[dlen];
            Get(out int index, out int count);
            int len = Math.Min(dlen, count);
            Buffer.BlockCopy(buffer, index, data, 0, len);
            if (dlen > len)
                Buffer.BlockCopy(buffer, 0, data, len, dlen - len);
            GetAck(dlen);
            return data;
        }

        /// <summary>
        /// 获取读索引和可读长度,SocketAsyncEventArgs适用
        /// </summary>
        public void Get(out int index, out int count)
        {
            //读指针
            index = (int)(tail & (size - 1));
            //一次最大可读长度
            count = (int)Math.Min(occupied, size - index);
        }

        /// <summary>
        /// 移动读指针,SocketAsyncEventArgs适用
        /// </summary>
        public void GetAck(int dlen)
        {
            if (dlen <= 0)
                throw new Exception("GetAck:无效长度");
            tail += (uint)dlen;
        }
    }
}
