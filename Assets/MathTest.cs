using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;

public class MathTest : MonoBehaviour
{
    public const int JobCount = 20;
    public const int CalcCount = 50000;

    [SerializeField] bool _useJobs;

    private void Update()
    {
        RunTest();
    }

    private void RunTest()
    {
        if (!_useJobs)
        {
            for (int i = 0; i < JobCount; i++)
            {
                ReallyToughMathTask(Time.deltaTime);
            }
        }
        else
        {
            NativeArray<JobHandle> jobArray = new NativeArray<JobHandle>(JobCount, Allocator.Temp);
            for (int i = 0; i < JobCount; i++)
            {
                jobArray[i] = ReallyToughMathTaskJob(Time.deltaTime);
            }
            JobHandle.CompleteAll(jobArray);
        }
    }

    private float ReallyToughMathTask(float value)
    {
        for (int i = 0; i < CalcCount; i++)
        {
            value = math.exp10(math.sqrt(value));
        }

        return value;
    }

    private JobHandle ReallyToughMathTaskJob(float input)
    {
        var job = new VeryToughJob { Value = input };
        return job.Schedule();
    }
}

[BurstCompile]
public struct VeryToughJob : IJob
{
    public float Value;

    public void Execute()
    {
        for (int i = 0; i < MathTest.CalcCount; i++)
        {
            Value = math.exp10(math.sqrt(Value));
        }
    }
}

