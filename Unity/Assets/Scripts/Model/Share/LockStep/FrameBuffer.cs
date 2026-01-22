using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    /// <summary>
    /// 帧缓冲区：用于存储预测回滚或状态同步中帧数据（输入、快照、哈希值）
    /// 采用循环缓冲区设计，通过frame%capcity 实现内存复用
    /// </summary>
    public class FrameBuffer: Object
    {
        /// <summary>
        /// 当前缓冲区-最大帧号
        /// </summary>
        public int MaxFrame { get; private set; }
        /// <summary>
        /// 每一帧玩家输入列表
        /// </summary>
        private readonly List<OneFrameInputs> frameInputs;
        /// <summary>
        /// 每一帧的状态快照列表
        /// 用于回滚时恢复状态
        /// </summary>
        private readonly List<MemoryBuffer> snapshots;
        /// <summary>
        /// 每一帧哈希值列表
        /// 用于多端同步校验
        /// </summary>
        private readonly List<long> hashs;

        public FrameBuffer(int frame = 0, int capacity = LSConstValue.FrameCountPerSecond * 60)
        {
            this.MaxFrame = frame + LSConstValue.FrameCountPerSecond * 30;
            this.frameInputs = new List<OneFrameInputs>(capacity);
            this.snapshots = new List<MemoryBuffer>(capacity);
            this.hashs = new List<long>(capacity);
            
            for (int i = 0; i < this.snapshots.Capacity; ++i)
            {
                this.hashs.Add(0);
                this.frameInputs.Add(OneFrameInputs.Create());
                MemoryBuffer memoryBuffer = new(10240);
                memoryBuffer.SetLength(0);
                memoryBuffer.Seek(0, SeekOrigin.Begin);
                this.snapshots.Add(memoryBuffer);
            }
        }
        /// <summary>
        /// 设置指定帧的哈希值
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="hash"></param>
        public void SetHash(int frame, long hash)
        {
            EnsureFrame(frame);
            this.hashs[frame % this.frameInputs.Capacity] = hash;
        }
        
        /// <summary>
        /// 获取指定帧的哈希值
        /// </summary>
        /// <param name="frame">指定帧号</param>
        /// <returns></returns>
        public long GetHash(int frame)
        {
            EnsureFrame(frame);
            return this.hashs[frame % this.frameInputs.Capacity];
        }
        /// <summary>
        /// 检查帧号是否在合法范围内
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public bool CheckFrame(int frame)
        {
            if (frame < 0)
            {
                return false;
            }

            if (frame > this.MaxFrame)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// 确保帧合法，不合法则抛出异常
        /// </summary>
        /// <param name="frame"></param>
        /// <exception cref="Exception"></exception>
        private void EnsureFrame(int frame)
        {
            if (!CheckFrame(frame))
            {
                throw new Exception($"frame out: {frame}, maxframe: {this.MaxFrame}");
            }
        }
        /// <summary>
        /// 获取指定帧输入数据
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public OneFrameInputs FrameInputs(int frame)
        {
            EnsureFrame(frame);
            OneFrameInputs oneFrameInputs = this.frameInputs[frame % this.frameInputs.Capacity];
            return oneFrameInputs;
        }
        /// <summary>
        /// 向前移动帧
        /// 通常在逻辑层处理完新帧或收到服务器确认帧时调用
        /// </summary>
        /// <param name="frame"></param>
        public void MoveForward(int frame)
        {
            //如果剩余的缓冲空间还足够（超过1秒），则不移动MaxFrame
            if (this.MaxFrame - frame > LSConstValue.FrameCountPerSecond) // 至少留出1秒的空间
            {
                return;
            }
            
            ++this.MaxFrame;
            //获取新一帧的输入对象并清空旧数据（循环利用，必须清空）
            OneFrameInputs oneFrameInputs = this.FrameInputs(this.MaxFrame);
            oneFrameInputs.Inputs.Clear();
        }
        /// <summary>
        /// 获取指定帧的内存快照缓冲区数据
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public MemoryBuffer Snapshot(int frame)
        {
            EnsureFrame(frame);
            MemoryBuffer memoryBuffer = this.snapshots[frame % this.snapshots.Capacity];
            return memoryBuffer;
        }
    }
}