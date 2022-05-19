using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 환경을 관찰하고 관츠간 내용을 수집하는데 사용하는 스크립트
// 단, 이 예제에서는 항상 관측한 내용이 없다고 가정.
public class SimpleAgent : Agent {

    /// <summary>
    /// Agent가 환경을 관측하는 대상을 설정하기 위해 호출하는 메서드
    /// 모든 에이전트 단계 또는 활동에서 호출
    /// </summary>
    public override void CollectObservations()
    {
        // 0값을 에이전트 관측 컬렉션에 추가.
        AddVectorObs(0);
    }

   
    public Bandit bandit;

    /// <summary>
    /// 현재 행동을 취해 PullArm메서드가 있는 Bandit에 적용, 팔을 당기도록 전달
    /// </summary>
    /// <param name="vectorAction">현재 행동 배열</param>
    /// <param name="textAction">현재 행동을 나타내는 문자열</param>
    public override void AgentAction(float[] vectorAction, string textAction)
	{
        var action = (int)vectorAction[0];
        // 밴딧으로부터 반환된 보상 추가.
        AddReward(bandit.PullArm(action));
    }

    /// <summary>
    /// 밴딧을 다시 시작 상태로 재설정하는 메서드.
    /// 에이전트가 행동을 마치거나 완성, 단계를 벗어날 때 호출
    /// </summary>    
    public override void AgentReset()
    {
        bandit.Reset();
    }

    /// <summary>
    ///  밴딧이 단일 상태 또는 행동
    /// </summary>
    public override void AgentOnDone()
    {

    }

    // -------------------------------------------------------------------------------------
    public Academy academy;
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;
    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    /// <summary>
    /// 플레이어 입력을 혀용하기 위해서 필요한 메서드.
    /// 브레인이 플레이어의 결정을 수용할 수 있을 만큼 오래 기다리게 하기 위함.
    /// </summary>
    private void WaitTimeInference()
    {
        if (!academy.GetIsInference())
        {
            RequestDecision();
        }
        else
        {
            if (timeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                timeSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                timeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }
}
