using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;



// Ŭ�������� �ʿ��� ��� ȯ�� �Ű������� ����
// ���ǵǾ� �ܺ� ������Ʈ�� ����

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




// brain's inpector������ ����.
// ī�޶��� �ػ󵵸� ����
[System.Serializable]
public struct resolution
{
    // ���� �ʺ�(�ȼ�)
    public int width;
    // ���� ����(�ȼ� ����)
    public int height;
    // true : �̹����� ������� ǥ��
    // false : RGB �������� ǥ��
    public bool blackAndWhite;
}

// Editor Inspector�� ���� ����.
// brain-specific �Ű����� ����
[System.Serializable]
public class BrainParameters
{
    // continuous : ���¸� ��Ÿ���� float ������ ����
    // discrete : ���°� ���� �� �ִ� ������ ���� ��
    public int vectorObservationSize = 1;

    [Range(1, 50)]
    public int numStackedVectorObservations = 1;

    // continuous : ������ ��Ÿ���� float ������ ����
    // discrete : ������ ���� �� �ִ� ������ ���� ��
    public int vectorActionSize = 1;

    // brain�� ���� ���� �ػ� ���
    public resolution[] cameraResolutions;

    // ����(action)�� ������ �ش��ϴ��� �����ϴ� ���ڿ� ���
    public string[] vectorActionDescriptions;

    // ������ �ҿ������� �������� ����
    public SpaceType vectorActionSpaceType = SpaceType.discrete;

    // ���°� �ҿ������� �������� �����մϴ�.
    public SpaceType vectorObservationSpaceType = SpaceType.continuous;
}

[HelpURL("https://github.com/Unity-Technologies/ml-agents/blob/master/" +
         "docs/Learning-Environment-Design-Brains.md")]

// ��� ���� ������ Brain ���� ����.
// �� ���� ������Ʈ�� �߰��ϰ� ���� ������Ʈ��
// ��ī���̷� �巡���Ͽ� ���� ������ �ڽ����� ������ֱ�.
// Contains a set of CoreBrains, ���� �ൿ�� �����ϴ� �ٸ� ����� �ش�.
public class Brain : MonoBehaviour
{
    private bool isInitialized = false;

    private Dictionary<Agent, AgentInfo> agentInfos =
        new Dictionary<Agent, AgentInfo>(1024);

    [Tooltip("Brain�� ���� ����, ���� �� �ൿ ���� ����.")]
    // state size�� ���� brain Ư�� �Ű������� ����
    public BrainParameters brainParameters = new BrainParameters();


    // brain�� ������ �������� ����
    // External / Internal / Player / Heuristic
    [Tooltip("Brain�� �ൿ�� �����ϴ� ����� ����.")]
    public BrainType brainType;

    //[HideInInspector]
    // brain�� �������� ������Ʈ�� ����
    // public Dictionary<int, Agent> agents = new Dictionary<int, Agent>();

    [SerializeField]
    ScriptableObject[] CoreBrains;

    // brain���� ����ϴ� ���� CoreBrain�� ���� ����
    public CoreBrain coreBrain;

    // coreBrains�� brain�� �ߺ����� �ʵ��� ��.
    [SerializeField]
    private int instanceID;

    // Brain�� �ֽ� coreBrains �迭�� �ִ��� Ȯ��.
    //�ν����Ͱ� �����Ǿ� InitializeBrain���� �� �� ȣ��.
    // brain���� ������Ʈ�� ��� ������ ��� coreBrains ����� ����.
    // �� brainType�� ���� �ϳ���
    public void UpdateCoreBrains()
    {
        // CoreBrains�� null�̸�,
        // Brain ������Ʈ�� �ν��Ͻ�ȭ�Ǿ�����,
        // �� CoreBrain�� �ν��Ͻ��� ���������� �ǹ�.
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

        // CoreBrains�� ���̰� BrainTypes�� ���� ��ġ���� ������,
        // CoreBrains�� ���̸� �ø�.
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

        // ����� instanceID�� ���� instanceID�� ��ġ���� ������,
        // Brain GameObject�� �����Ǿ����� �ǹ�.
        // �� CoreBrain�� ������ ������ ��.
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

        // ǥ���� coreBrain�� brainType�� ���ǵ� ��
        coreBrain = (CoreBrain)CoreBrains[(int)brainType];

        coreBrain.SetBrain(this);
    }

    // ȯ�� ���� �� Academy���� ȣ��
    public void InitializeBrain(Academy aca, Communicator communicator)
    {
        UpdateCoreBrains();
        coreBrain.InitializeCoreBrain(communicator);
        aca.BrainDecideAction += DecideAction;
        isInitialized = true;
    }

    public void SendState(Agent agent, AgentInfo info)
    {
        // brain�� Ȱ��ȭ���� �ʰų� ����� �ʱ�ȭ���� ������ ������ �߻�
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