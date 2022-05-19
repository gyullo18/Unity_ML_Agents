using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;



// 클래스에는 필요한 모든 환경 매개변수가 포함
// 정의되어 외부 에이전트에 전송

public enum BrainType
{
    Player,
    Heuristic,
    External,
    Internal
}

public enum SpaceType
{
    discrete,
    continuous
}
;




// brain's inpector에서만 수정.
// 카메라의 해상도를 정의
[System.Serializable]
public struct resolution
{
    // 관찰 너비(픽셀)
    public int width;
    // 관찰 높이(픽셀 단위)
    public int height;
    // true : 이미지가 흑백으로 표시
    // false : RGB 색상으로 표시
    public bool blackAndWhite;
}

// Editor Inspector를 통해 수정.
// brain-specific 매개변수 정의
[System.Serializable]
public class BrainParameters
{
    // continuous : 상태를 나타내는 float 벡터의 길이
    // discrete : 상태가 취할 수 있는 가능한 값의 수
    public int vectorObservationSize = 1;

    [Range(1, 50)]
    public int numStackedVectorObservations = 1;

    // continuous : 동작을 나타내는 float 벡터의 길이
    // discrete : 동작이 취할 수 있는 가능한 값의 수
    public int vectorActionSize = 1;

    // brain에 대한 관찰 해상도 목록
    public resolution[] cameraResolutions;

    // 동작(action)이 무엇에 해당하는지 설명하는 문자열 목록
    public string[] vectorActionDescriptions;

    // 동작이 불연속인지 연속인지 정의
    public SpaceType vectorActionSpaceType = SpaceType.discrete;

    // 상태가 불연속인지 연속인지 정의합니다.
    public SpaceType vectorObservationSpaceType = SpaceType.continuous;
}

[HelpURL("https://github.com/Unity-Technologies/ml-agents/blob/master/" +
         "docs/Learning-Environment-Design-Brains.md")]

// 모든 높은 수준의 Brain 논리를 포함.
// 빈 게임 오브젝트에 추가하고 게임 오브젝트를
// 아카데미로 드래그하여 계층 구조의 자식으로 만들어주기.
// Contains a set of CoreBrains, 각각 행동을 결정하는 다른 방법에 해당.
public class Brain : MonoBehaviour
{
    private bool isInitialized = false;

    private Dictionary<Agent, AgentInfo> agentInfos =
        new Dictionary<Agent, AgentInfo>(1024);

    [Tooltip("Brain에 대한 상태, 관찰 및 행동 공간 정의.")]
    // state size와 같은 brain 특정 매개변수를 정의
    public BrainParameters brainParameters = new BrainParameters();


    // brain의 유형이 무엇인지 정의
    // External / Internal / Player / Heuristic
    [Tooltip("Brain이 행동을 결정하는 방법을 설명.")]
    public BrainType brainType;

    //[HideInInspector]
    // brain을 가져가는 에이전트를 추적
    // public Dictionary<int, Agent> agents = new Dictionary<int, Agent>();

    [SerializeField]
    ScriptableObject[] CoreBrains;

    // brain에서 사용하는 현재 CoreBrain에 대한 참조
    public CoreBrain coreBrain;

    // coreBrains가 brain과 중복되지 않도록 함.
    [SerializeField]
    private int instanceID;

    // Brain에 최신 coreBrains 배열이 있는지 확인.
    //인스펙터가 수정되어 InitializeBrain으로 들어갈 때 호출.
    // brain게임 오브젝트가 방금 생성된 경우 coreBrains 목록을 생성.
    // 각 brainType에 대해 하나씩
    public void UpdateCoreBrains()
    {
        // CoreBrains가 null이면,
        // Brain 오브젝트가 인스턴스화되었으며,
        // 각 CoreBrain의 인스턴스를 생성했음을 의미.
        if (CoreBrains == null)
        {
            int numCoreBrains = System.Enum.GetValues(typeof(BrainType)).Length;
            CoreBrains = new ScriptableObject[numCoreBrains];
            foreach (BrainType bt in System.Enum.GetValues(typeof(BrainType)))
            {
                CoreBrains[(int)bt] =
                    ScriptableObject.CreateInstance(
                        "CoreBrain" + bt.ToString());
            }

        }
        else
        {
            foreach (BrainType bt in System.Enum.GetValues(typeof(BrainType)))
            {
                if ((int)bt >= CoreBrains.Length)
                    break;
                if (CoreBrains[(int)bt] == null)
                {
                    CoreBrains[(int)bt] =
                        ScriptableObject.CreateInstance(
                            "CoreBrain" + bt.ToString());
                }
            }
        }

        // CoreBrains의 길이가 BrainTypes의 수와 일치하지 않으면,
        // CoreBrains의 길이를 늘림.
        if (CoreBrains.Length < System.Enum.GetValues(typeof(BrainType)).Length)
        {
            int numCoreBrains = System.Enum.GetValues(typeof(BrainType)).Length;
            ScriptableObject[] new_CoreBrains =
                new ScriptableObject[numCoreBrains];
            foreach (BrainType bt in System.Enum.GetValues(typeof(BrainType)))
            {
                if ((int)bt < CoreBrains.Length)
                {
                    new_CoreBrains[(int)bt] = CoreBrains[(int)bt];
                }
                else
                {
                    new_CoreBrains[(int)bt] =
                        ScriptableObject.CreateInstance(
                            "CoreBrain" + bt.ToString());
                }
            }
            CoreBrains = new_CoreBrains;
        }

        // 저장된 instanceID가 현재 instanceID와 일치하지 않으면,
        // Brain GameObject가 복제되었음을 의미.
        // 각 CoreBrain의 복제를 만들어야 함.
        if (instanceID != gameObject.GetInstanceID())
        {
            foreach (BrainType bt in System.Enum.GetValues(typeof(BrainType)))
            {
                if (CoreBrains[(int)bt] == null)
                {
                    CoreBrains[(int)bt] =
                        ScriptableObject.CreateInstance(
                            "CoreBrain" + bt.ToString());
                }
                else
                {
                    CoreBrains[(int)bt] =
                        ScriptableObject.Instantiate(CoreBrains[(int)bt]);
                }
            }
            instanceID = gameObject.GetInstanceID();
        }

        // 표시할 coreBrain은 brainType에 정의된 것
        coreBrain = (CoreBrain)CoreBrains[(int)brainType];

        coreBrain.SetBrain(this);
    }

    // 환경 시작 시 Academy에서 호출
    public void InitializeBrain(Academy aca, Communicator communicator)
    {
        UpdateCoreBrains();
        coreBrain.InitializeCoreBrain(communicator);
        aca.BrainDecideAction += DecideAction;
        isInitialized = true;
    }

    public void SendState(Agent agent, AgentInfo info)
    {
        // brain이 활성화되지 않거나 제대로 초기화되지 않으면 오류가 발생
        if (!gameObject.activeSelf)
        {
            throw new UnityAgentsException(
                string.Format("Agent {0} tried to request an action " +
                "from brain {1} but it is not active.",
                             agent.gameObject.name, gameObject.name));
        }
        else if (!isInitialized)
        {
            throw new UnityAgentsException(
                string.Format("Agent {0} tried to request an action " +
                "from brain {1} but it was not initialized.",
                             agent.gameObject.name, gameObject.name));
        }
        else
        {
            agentInfos.Add(agent, info);
        }

    }

    void DecideAction()
    {
        coreBrain.DecideAction(agentInfos);
        agentInfos.Clear();
    }
}