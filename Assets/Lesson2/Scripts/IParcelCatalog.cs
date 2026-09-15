namespace AlgoCourse.Lesson2
{
    /// <summary>
    /// 제공 게임과 학생 Dictionary 코드를 연결하는 계약입니다.
    /// </summary>
    public interface IParcelCatalog
    {
        int BucketCount { get; }
        int Count { get; }
        void Initialize();
        bool Register(int id, string productName, int destinationIndex);
        bool Contains(int id);
        bool TryFind(int id, out ParcelProductInfo product);
        int GetHash(int id);
        int GetBucketIndex(int id);
        bool HasCollision(int id);
        bool ChangeDestination(int id, int newDestinationIndex);
        int CountByDestination(int destinationIndex);
        bool Remove(int id);
    }
}
