using System;


namespace Frontis.TcpNetwork
{
    /// <summary>
    /// 수신되는 패킷들을 하나의 완성된 패킷으로 만드는 작업을 담당합니다.
    /// </summary>
    public class PacketReader
    {
        public Action<string> LogCallback = null;

        private Action<byte[]> completeReceiveCallback = null;

        private int remainBytes       = 0;  // 남은 크기
        private int currentPosition   = 0;
        private int totalBufferSize   = 0;
        private int currentBufferSize = 0;

        private byte[] totalBuffer    = null;



        public PacketReader(Action<byte[]> completeReceiveCallback)
        {
            this.completeReceiveCallback = completeReceiveCallback;
        }

        public void Receive(byte[] buffer)
        {
            //LogCallback?.Invoke(string.Format($"currentPosition: {currentPosition},  buffer: {buffer.Length}"));

            // 새로운 패킷이라면 전체 크기부터 계산합니다.
            //
            if (currentPosition == 0)
            {
                CalculatePacketSizeToRead(buffer, 0);
            }

            currentBufferSize = buffer.Length;

            int bufferPosition = 0;
            bool isEnd         = false;

            while (!isEnd)
            {
                //LogCallback?.Invoke(string.Format($"remainBytes: {remainBytes},  bufferPosition: {bufferPosition},  currentBufferSize: {currentBufferSize}"));

                if (remainBytes == currentBufferSize)
                {
                    // 읽어야할 패킷의 남은 크기가 현재 버퍼에서 읽어들인 크기와 같다면 전부 복사합니다.
                    //
                    Array.Copy(buffer, bufferPosition, totalBuffer, currentPosition, currentBufferSize);

                    completeReceiveCallback?.Invoke(totalBuffer);  // 패킷 수신 완료 처리

                    remainBytes    -= currentBufferSize;
                    currentPosition = 0;
                    isEnd           = true;
                }
                else if (remainBytes > currentBufferSize)
                {
                    // 읽어야할 패킷의 남은 크기가 현재 버퍼에서 읽어들인 크기보다 크다면 전부 복사합니다.
                    //
                    Array.Copy(buffer, bufferPosition, totalBuffer, currentPosition, currentBufferSize);

                    currentPosition += currentBufferSize;
                    remainBytes     -= currentBufferSize;
                    isEnd            = true;
                }
                else
                {
                    // 현재 읽어들인 버퍼에서 패킷의 남은 크기 만큼만 복사합니다.
                    //
                    Array.Copy(buffer, bufferPosition, totalBuffer, currentPosition, remainBytes);

                    completeReceiveCallback?.Invoke(totalBuffer);  // 패킷 수신 완료 처리
                    
                    bufferPosition    += remainBytes;  // 복사한 크기만큼 다음 읽을 위치를 이동시킵니다.
                    currentBufferSize -= remainBytes;  // 현재 읽어들인 버퍼의 크기를 다시 계산합니다.
                    currentPosition    = 0;

                    CalculatePacketSizeToRead(buffer, bufferPosition);
                }
            }
        }

        /// <summary>
        /// 읽어야할 패킷의 전체 크기를 계산합니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        private void CalculatePacketSizeToRead(byte[] buffer, int startIndex)
        {
            byte[] totalSizeData = new byte[4];

            //LogCallback?.Invoke(string.Format($"buffer: {buffer.Length},  startIndex: {startIndex}"));

            Array.Copy(buffer, startIndex, totalSizeData, 0, totalSizeData.Length);

            totalBufferSize = BitConverter.ToInt32(totalSizeData, 0);
            totalBuffer     = new byte[totalBufferSize];
            remainBytes     = totalBufferSize;
        }


    }


}
