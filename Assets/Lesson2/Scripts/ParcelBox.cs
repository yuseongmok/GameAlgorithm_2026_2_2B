using UnityEngine;

namespace AlgoCourse.Lesson2
{
    /// <summary>
    /// 컨베이어 위 택배 한 개의 번호와 이동 상태를 보관합니다.
    /// </summary>
    public sealed class ParcelBox : MonoBehaviour
    {
        public int ParcelId { get; private set; }
        public bool IsBeingSorted { get; set; }
        public bool IsRejected { get; set; }

        public void Initialize(int parcelId)
        {
            ParcelId = parcelId;
            IsBeingSorted = false;
            IsRejected = false;
            name = $"Parcel_{parcelId}";

        }
    }
}
