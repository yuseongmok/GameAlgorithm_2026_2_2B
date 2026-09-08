// Lesson1 패키지와 학생 코드 사이의 약속입니다.
// 이 파일에는 자료구조 구현이 없습니다.
public interface IWarehouseData
{
    int IncomingCount { get; }
    int OutgoingCount { get; }

    void Clear();

    void EnqueueIncoming(ForkliftBox box);
    ForkliftBox PeekIncoming();
    ForkliftBox DequeueIncoming();
    ForkliftBox[] GetIncomingItems();

    void EnqueueOutgoing(ForkliftBox box);
    ForkliftBox PeekOutgoing();
    ForkliftBox DequeueOutgoing();
    ForkliftBox[] GetOutgoingItems();

    int GetStackCount(int stackIndex);
    void PushStack(int stackIndex, ForkliftBox box);
    ForkliftBox PeekStack(int stackIndex);
    ForkliftBox PopStack(int stackIndex);
}
