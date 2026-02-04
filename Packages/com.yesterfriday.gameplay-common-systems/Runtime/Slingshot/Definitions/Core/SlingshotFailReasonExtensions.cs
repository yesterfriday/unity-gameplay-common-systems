using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot
{
    public static class SlingshotFailReasonExtensions
    {
        public static string ToMessage(this SlingshotFailReason reason)
        {
            switch (reason)
            {
                case SlingshotFailReason.None:
                    return string.Empty;
                case SlingshotFailReason.NullDefinition:
                    return "슬링샷 정의(Definition)가 설정되지 않았습니다";
                case SlingshotFailReason.NullPrefab:
                    return "발사체 프리팹이 지정되지 않았습니다.";
                case SlingshotFailReason.NullOrigin:
                    return "Origin(발사 기준점)이 설정되지 않았습니다.";
                case SlingshotFailReason.NullCamera:
                    return "입력 카메라가 설정되지 않았습니다.";
                case SlingshotFailReason.CoolingDown:
                    return "쿨타임 중입니다. 잠시만 기다려주세요.";
                case SlingshotFailReason.NoPlaneHit:
                    return "입력을 월드 평면으로 변환할 수 없습니다.";
                case SlingshotFailReason.NotPulling:
                    return "당김 상태가 아닙니다.";
                case SlingshotFailReason.PullTooSmall:
                    return "당기는 힘이 너무 작습니다. 조금 더 당겨주세요.";
                case SlingshotFailReason.InvalidParams:
                    return "설정 값이 유효하지 않습니다.";
                case SlingshotFailReason.NoRigidbody2D:
                    return "발사체 프리팹에 Rigidbody2D가 필요합니다.";
                case SlingshotFailReason.AlreadyPulling:
                    return "이미 당기는 중입니다.";
                default:
                    return "알 수 없는 실패 원인입니다.";
            }
        }
    }
}