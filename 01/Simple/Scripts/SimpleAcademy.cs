using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAcademy : Academy {

    public override void AcademyReset()
    {


    }

    public override void AcademyStep()
    {


    }
    // Max Steps(최대 단계) : 각 Agent가 실행되기 전에 재설정하도록 하는 행동 횟수를 제한.
    // 0일 경우, Done이 호출될 때까지 에이전트는 행동을 계속.

    // Training Configuration(훈련 환경 구성) : training set + test set
    // 에이전트 모델 구축 > 훈련된 머신러닝을 통해 실제 데이터 셋에서 연습 가능

    // Inference Configuration(추론 구성) : 보이지 않던 환경이나 데이터셋에 대해 모델을 추론하거나 사용.
    // 머신러닝이 이 유형 환경에서 실행될 때 파라미터를 설정하는 곳.
}
