using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public Transform[] goals;
    NavMeshAgent agent;
    private int currentGoalIndex = -1;

    void Start()
    {
        FindAllGoalsByTag();
        agent = GetComponent<NavMeshAgent>();
        SetRandomDestination();
    }

    void Update()
    {
        if (agent.hasPath && agent.remainingDistance < agent.stoppingDistance)
        {
            SetRandomDestination();
        }
    }

    // 무작위 목표 지점을 설정하는 메서드
    void SetRandomDestination()
    {
        if (goals.Length == 0)
        {
            Debug.LogError("Goals 배열이 비어 있습니다. 목표 지점을 설정해 주세요.");
            return;
        }

        int newGoalIndex;

        do
        {
            newGoalIndex = Random.Range(0, goals.Length);
        } while (newGoalIndex == currentGoalIndex);

        // 새 목표 인덱스 업데이트
        currentGoalIndex = newGoalIndex;

        // 선택된 목표의 위치로 이동 설정
        agent.SetDestination(goals[currentGoalIndex].position);
    }

    void FindAllGoalsByTag()
    {
        GameObject[] goalObjects = GameObject.FindGameObjectsWithTag("Goal");

        goals = goalObjects.Select(g => g.transform).ToArray();
    }
}