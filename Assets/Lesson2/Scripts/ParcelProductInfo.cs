using System;

namespace AlgoCourse.Lesson2
{
    /// <summary>
    /// 택배 번호를 조회했을 때 얻는 상품 정보입니다.
    /// </summary>
    [Serializable]
    public readonly struct ParcelProductInfo
    {
        public ParcelProductInfo(int id, string productName, int destinationIndex)
        {
            Id = id;
            ProductName = productName;
            DestinationIndex = destinationIndex;
        }

        public int Id { get; }
        public string ProductName { get; }
        public int DestinationIndex { get; }
    }
}
