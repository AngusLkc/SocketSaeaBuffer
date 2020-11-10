# SocketSaeaBuffer
C# Socket异步收发专用环形缓冲区,思想来自于Linux Kernel kfifo结构  
Put数据前需自行处理数据长度不能大于surplus  
Get数据返回的是byte[]的读索引和可读长度,可直接用于Socket异步发送  
Socket异步返回时调用Ack方法移动读指针  
