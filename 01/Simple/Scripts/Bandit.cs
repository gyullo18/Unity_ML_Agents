using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 간단한 four armed bandit 구현
public class Bandit : MonoBehaviour
{
    [Header("보상값")]
    /// <summary>
    /// 보상 값 public속성으로 정의
    /// </summary>
    public Material Gold;
    public Material Silver;
    public Material Bronze;

    [Header("Other")]
    /// <summary>
    /// 자리 표시자(placeholder)
    /// </summary>
    private MeshRenderer mesh;
    /// <summary>
    /// 재설정 - 원래의 Material
    /// </summary>
    private Material reset;

    void Start()
    {
        //MeshRenderer기반 private필드 값 정의
        mesh = GetComponent<MeshRenderer>();
        reset = mesh.material;
    }

    // 적절한 Material과 보상을 설정하는 메서드
    public int PullArm(int arm)
    {
        var reward = 0;
        switch (arm)
        {
            case 1:
                mesh.material = Gold;
                reward = 3;
                break;
            case 2:
                mesh.material = Bronze;
                reward = 1;
                break;
            case 3:
                mesh.material = Bronze;
                reward = 1;
                break;
            case 4:
                mesh.material = Silver;
                reward = 2;
                break;
        }
        return reward;
    }

    // 원래의 속성으로 재설정하는 메서드
    public void Reset()
    {
        mesh.material = reset;
    }
}
