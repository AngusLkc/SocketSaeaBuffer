//入队
unsigned int kfifo_put(struct kfifo *fifo, unsigned char *buffer, unsigned int len)
{
    unsigned int l;
    //fifo->size - fifo->in + fifo->out:空闲长度
    len = min(len, fifo->size - fifo->in + fifo->out);//计算可写长度
    //fifo->size - (fifo->in & (fifo->size - 1)):写指针到数组末尾长度
    l = min(len, fifo->size - (fifo->in & (fifo->size - 1)));
    memcpy(fifo->buffer + (fifo->in & (fifo->size - 1)), buffer, l);
    memcpy(fifo->buffer, buffer + l, len - l);
    fifo->in += len;
    return len;
}

//出队
unsigned int kfifo_get(struct kfifo *fifo, unsigned char *buffer, unsigned int len)
{
    unsigned int l;
    //fifo->in - fifo->out:数据长度
    len = min(len, fifo->in - fifo->out);
    //fifo->size - (fifo->out & (fifo->size - 1)):读指针到数组末尾长度
    l = min(len, fifo->size - (fifo->out & (fifo->size - 1)));
    memcpy(buffer, fifo->buffer + (fifo->out & (fifo->size - 1)), l);
    memcpy(buffer + l, fifo->buffer, len - l);
    smp_mb();
    fifo->out += len;
    return len;
}
