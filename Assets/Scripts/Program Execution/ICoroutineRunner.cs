using System.Collections;
using UnityEngine;

namespace Program_Execution
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
    }
}